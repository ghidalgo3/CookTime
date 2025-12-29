-- PostgreSQL Migration Tracker
-- Tracks which migrations have been applied

CREATE SCHEMA IF NOT EXISTS cooktime;

CREATE TABLE IF NOT EXISTS cooktime.schema_migrations (
    id serial PRIMARY KEY,
    script_name text UNIQUE NOT NULL,
    applied_at timestamptz DEFAULT now(),
    checksum text
);
