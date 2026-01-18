-- PostgreSQL Migration: Batch Import Functions for Nutrition Facts
-- Adds functions to efficiently bulk load nutrition data

-- Batch import nutrition facts from a JSONB array
-- Much faster than calling import_nutrition_facts in a loop
CREATE OR REPLACE FUNCTION cooktime.batch_import_nutrition_facts(
    p_nutrition_array jsonb,
    p_dataset text
)
RETURNS TABLE(imported_count integer, skipped_count integer) AS $$
DECLARE
    v_imported integer := 0;
    v_skipped integer := 0;
BEGIN
    -- Validate dataset
    IF p_dataset NOT IN ('usda_sr_legacy', 'usda_branded') THEN
        RAISE EXCEPTION 'Unknown dataset: %', p_dataset;
    END IF;

    -- Use a CTE to handle the batch insert with conflict detection
    WITH input_data AS (
        SELECT 
            elem,
            CASE 
                WHEN p_dataset = 'usda_sr_legacy' THEN 
                    jsonb_build_object(
                        'ndbNumber', elem->>'ndbNumber',
                        'fdcId', elem->>'fdcId'
                    )
                WHEN p_dataset = 'usda_branded' THEN 
                    jsonb_build_object(
                        'gtinUpc', elem->>'gtinUpc',
                        'fdcId', elem->>'fdcId'
                    )
            END AS source_ids,
            ARRAY[elem->>'description'] AS names
        FROM jsonb_array_elements(p_nutrition_array) AS elem
    ),
    existing AS (
        SELECT source_ids 
        FROM cooktime.nutrition_facts nf
        WHERE nf.source_ids IN (SELECT source_ids FROM input_data)
    ),
    to_insert AS (
        SELECT * FROM input_data
        WHERE source_ids NOT IN (SELECT source_ids FROM existing)
    ),
    inserted AS (
        INSERT INTO cooktime.nutrition_facts (
            source_ids,
            names,
            unit_mass,
            density,
            nutrition_data,
            count_regex,
            dataset
        )
        SELECT 
            source_ids,
            names,
            NULL,
            NULL,
            elem,
            NULL,
            p_dataset
        FROM to_insert
        RETURNING 1
    )
    SELECT COUNT(*)::integer INTO v_imported FROM inserted;
    
    SELECT COUNT(*)::integer INTO v_skipped 
    FROM jsonb_array_elements(p_nutrition_array);
    v_skipped := v_skipped - v_imported;
    
    RETURN QUERY SELECT v_imported, v_skipped;
END;
$$ LANGUAGE plpgsql;

-- Batch import using unnest for even better performance with prepared arrays
-- Call with arrays of equal length for each column
CREATE OR REPLACE FUNCTION cooktime.batch_import_nutrition_facts_arrays(
    p_source_ids jsonb[],
    p_names text[][],
    p_nutrition_data jsonb[],
    p_dataset text
)
RETURNS TABLE(imported_count integer, skipped_count integer) AS $$
DECLARE
    v_imported integer := 0;
    v_skipped integer := 0;
    v_total integer;
BEGIN
    v_total := array_length(p_source_ids, 1);
    
    IF v_total IS NULL OR v_total = 0 THEN
        RETURN QUERY SELECT 0, 0;
        RETURN;
    END IF;

    -- Insert all at once, skipping duplicates
    WITH input_data AS (
        SELECT 
            unnest(p_source_ids) AS source_ids,
            unnest(p_names) AS names,
            unnest(p_nutrition_data) AS nutrition_data
    ),
    inserted AS (
        INSERT INTO cooktime.nutrition_facts (
            source_ids,
            names,
            unit_mass,
            density,
            nutrition_data,
            count_regex,
            dataset
        )
        SELECT 
            source_ids,
            names,
            NULL,
            NULL,
            nutrition_data,
            NULL,
            p_dataset
        FROM input_data
        ON CONFLICT ((source_ids)) DO NOTHING
        RETURNING 1
    )
    SELECT COUNT(*)::integer INTO v_imported FROM inserted;
    
    v_skipped := v_total - v_imported;
    
    RETURN QUERY SELECT v_imported, v_skipped;
END;
$$ LANGUAGE plpgsql;

-- Create a unique index on source_ids if not exists (needed for ON CONFLICT)
CREATE UNIQUE INDEX IF NOT EXISTS idx_nutrition_facts_source_ids_unique 
ON cooktime.nutrition_facts ((source_ids));

-- Optimized function to get nutrition facts by source_ids in batch
CREATE OR REPLACE FUNCTION cooktime.batch_get_nutrition_facts_by_source_ids(
    p_source_ids jsonb[]
)
RETURNS TABLE(
    id uuid,
    source_ids jsonb,
    names text[],
    density double precision,
    dataset text
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        nf.id,
        nf.source_ids,
        nf.names,
        nf.density,
        nf.dataset
    FROM cooktime.nutrition_facts nf
    WHERE nf.source_ids = ANY(p_source_ids);
END;
$$ LANGUAGE plpgsql;

-- Batch update densities
CREATE OR REPLACE FUNCTION cooktime.batch_update_nutrition_facts_density(
    p_ids uuid[],
    p_densities double precision[]
)
RETURNS integer AS $$
DECLARE
    v_updated integer;
BEGIN
    WITH updates AS (
        SELECT 
            unnest(p_ids) AS id,
            unnest(p_densities) AS density
    )
    UPDATE cooktime.nutrition_facts nf
    SET density = u.density
    FROM updates u
    WHERE nf.id = u.id;
    
    GET DIAGNOSTICS v_updated = ROW_COUNT;
    RETURN v_updated;
END;
$$ LANGUAGE plpgsql;
