import { MeasureUnit } from "./CookTime";

export type UnitPreference = "recipe" | "imperial" | "metric";

const IMPERIAL_VOLUME = [
  "teaspoon",
  "tablespoon",
  "fluid_ounce",
  "cup",
  "pint",
  "quart",
  "gallon",
];

const METRIC_VOLUME = ["milliliter", "liter"];

const IMPERIAL_WEIGHT = ["ounce", "pound"];
const METRIC_WEIGHT = ["gram", "kilogram"];

const UNIT_DISPLAY_NAME: Record<string, string> = {
  fluid_ounce: "fluid ounce",
};

export function formatUnitName(unitName: string): string {
  if (!unitName || unitName === "count") {
    return "";
  }
  if (UNIT_DISPLAY_NAME[unitName]) {
    return UNIT_DISPLAY_NAME[unitName];
  }
  return unitName.replace(/_/g, " ").toLowerCase();
}

export function formatNumber(value: number, maxDecimals = 2): string {
  if (Number.isInteger(value)) {
    return value.toString();
  }
  const fixed = value.toFixed(maxDecimals);
  return fixed.replace(/\.?0+$/, "");
}

type ConversionResult = {
  quantity: number;
  unitName: string;
  displayName: string;
  converted: boolean;
};

function getUnit(units: MeasureUnit[] | undefined, name: string): MeasureUnit | undefined {
  return units?.find((unit) => unit.name === name);
}

function pickBestUnit(baseSiValue: number, candidates: MeasureUnit[]): MeasureUnit | null {
  if (candidates.length === 0) {
    return null;
  }
  const sorted = [...candidates].sort((a, b) => a.siValue - b.siValue);
  if (baseSiValue <= 0) {
    return sorted[0];
  }
  let best = sorted[0];
  for (const unit of sorted) {
    if (baseSiValue / unit.siValue >= 1) {
      best = unit;
    }
  }
  return best;
}

function getCandidates(
  units: MeasureUnit[] | undefined,
  siType: string,
  preference: UnitPreference
): MeasureUnit[] {
  const normalizedType = siType.toLowerCase();
  const isVolume = normalizedType === "volume";
  const isWeight = normalizedType === "weight";

  if (!isVolume && !isWeight) {
    return [];
  }

  const names =
    preference === "metric"
      ? isVolume
        ? METRIC_VOLUME
        : METRIC_WEIGHT
      : isVolume
        ? IMPERIAL_VOLUME
        : IMPERIAL_WEIGHT;

  return names
    .map((name) => getUnit(units, name))
    .filter((unit): unit is MeasureUnit => unit != null);
}

export function convertQuantity({
  quantity,
  unitName,
  units,
  preference,
}: {
  quantity: number;
  unitName: string;
  units?: MeasureUnit[];
  preference: UnitPreference;
}): ConversionResult {
  const unit = getUnit(units, unitName);

  if (!unit || preference === "recipe" || unit.siType === "count") {
    return {
      quantity,
      unitName,
      displayName: formatUnitName(unitName),
      converted: false,
    };
  }

  const candidates = getCandidates(units, unit.siType, preference);
  const best = pickBestUnit(quantity * unit.siValue, candidates);

  if (!best) {
    return {
      quantity,
      unitName,
      displayName: formatUnitName(unitName),
      converted: false,
    };
  }

  const convertedQuantity = (quantity * unit.siValue) / best.siValue;
  return {
    quantity: convertedQuantity,
    unitName: best.name,
    displayName: formatUnitName(best.name),
    converted: true,
  };
}
