import csv
import json
import re
from pathlib import Path
from statistics import mean
from typing import Dict, List, Tuple


ROOT_DIR = Path(__file__).resolve().parents[1]
ASSETS_DIR = ROOT_DIR / "Assets"
ITEM_DB_PATH = ROOT_DIR / "Assets" / "Scripts" / "Item Database.asset"
ENEMY_DB_PATH = ROOT_DIR / "Assets" / "Databases" / "Enemy Database.asset"
REPORT_DIR = ROOT_DIR / "reports"

GUID_ENTRY_RE = re.compile(r"-\s*\{[^}]*guid:\s*([0-9a-f]{32})", re.IGNORECASE)

ITEM_TYPE_NAMES = {
    0: "Montura",
    1: "Casco",
    2: "Collar",
    3: "Arma",
    4: "Armadura",
    5: "Escudo",
    6: "Guantes",
    7: "Cinturón",
    8: "Anillo",
    9: "Botas",
    10: "Otros",
}

ITEM_NUMERIC_FIELDS = [
    "price",
    "hp",
    "mana",
    "ataque",
    "defensa",
    "velocidadAtaque",
    "ataqueCritico",
    "danoCritico",
    "suerte",
    "destreza",
    "nivel",
]

ENEMY_NUMERIC_FIELDS = [
    "hp",
    "ataque",
    "defensa",
    "velocidadAtaque",
    "ataqueCritico",
    "danoCritico",
    "suerte",
    "destreza",
    "rewardCoins",
    "experienceReward",
    "requiredLevel",
    "level",
]

COMMON_COMPARISON_FIELDS = [
    "hp",
    "ataque",
    "defensa",
    "velocidadAtaque",
    "ataqueCritico",
    "danoCritico",
    "suerte",
    "destreza",
]


def build_guid_map() -> Dict[str, Path]:
    guid_map: Dict[str, Path] = {}
    for meta_path in ASSETS_DIR.rglob("*.meta"):
        asset_path = meta_path.with_suffix("")
        if not asset_path.exists() or asset_path.is_dir():
            continue
        try:
            with meta_path.open("r", encoding="utf-8", errors="ignore") as meta_file:
                for line in meta_file:
                    stripped = line.strip()
                    if stripped.startswith("guid:"):
                        guid = stripped.split("guid:", 1)[1].strip()
                        if guid:
                            guid_map[guid] = asset_path
                        break
        except OSError:
            continue
    return guid_map


def extract_guids(asset_path: Path) -> List[str]:
    contents = asset_path.read_text(encoding="utf-8", errors="ignore")
    return GUID_ENTRY_RE.findall(contents)


def parse_yaml_fields(asset_path: Path) -> Dict[str, str]:
    data: Dict[str, str] = {}
    with asset_path.open("r", encoding="utf-8", errors="ignore") as asset_file:
        for raw_line in asset_file:
            if ":" not in raw_line:
                continue
            key, value = raw_line.split(":", 1)
            key = key.strip()
            if not key:
                continue
            data[key] = value.strip()
    return data


def clean_value(raw: str | None):
    if raw is None:
        return None
    raw = raw.strip()
    if not raw:
        return None
    if raw.startswith('"') and raw.endswith('"'):
        raw = raw[1:-1]
    lowered = raw.lower()
    if lowered in {"true", "false"}:
        return lowered == "true"
    try:
        return int(raw)
    except ValueError:
        return raw


def get_int_field(parsed: Dict[str, str], key: str):
    value = clean_value(parsed.get(key))
    return value if isinstance(value, int) else None


def build_item_records(guid_map: Dict[str, Path]) -> Tuple[List[Dict], List[str]]:
    records: List[Dict] = []
    missing_guids: List[str] = []

    for guid in extract_guids(ITEM_DB_PATH):
        asset_path = guid_map.get(guid)
        if not asset_path:
            missing_guids.append(guid)
            continue

        parsed = parse_yaml_fields(asset_path)
        record: Dict[str, object] = {
            "guid": guid,
            "asset_path": str(asset_path.relative_to(ROOT_DIR)),
            "itemName": clean_value(parsed.get("itemName")),
            "rareza": clean_value(parsed.get("rareza")),
        }

        item_type_value = clean_value(parsed.get("itemType"))
        if isinstance(item_type_value, int):
            record["itemTypeIndex"] = item_type_value
            record["itemType"] = ITEM_TYPE_NAMES.get(
                item_type_value, f"Desconocido ({item_type_value})"
            )
        else:
            record["itemTypeIndex"] = item_type_value
            record["itemType"] = item_type_value

        for field in ITEM_NUMERIC_FIELDS:
            record[field] = get_int_field(parsed, field)

        records.append(record)

    records.sort(key=lambda r: (r.get("itemName") or "").lower())
    return records, missing_guids


def build_enemy_records(guid_map: Dict[str, Path]) -> Tuple[List[Dict], List[str]]:
    records: List[Dict] = []
    missing_guids: List[str] = []

    for guid in extract_guids(ENEMY_DB_PATH):
        asset_path = guid_map.get(guid)
        if not asset_path:
            missing_guids.append(guid)
            continue

        parsed = parse_yaml_fields(asset_path)
        record: Dict[str, object] = {
            "guid": guid,
            "asset_path": str(asset_path.relative_to(ROOT_DIR)),
            "enemyName": clean_value(parsed.get("enemyName")),
            "description": clean_value(parsed.get("description")),
        }

        for field in ENEMY_NUMERIC_FIELDS:
            record[field] = get_int_field(parsed, field)

        records.append(record)

    records.sort(key=lambda r: (r.get("enemyName") or "").lower())
    return records, missing_guids


def write_csv(path: Path, fieldnames: List[str], rows: List[Dict]):
    with path.open("w", newline="", encoding="utf-8") as csv_file:
        writer = csv.DictWriter(csv_file, fieldnames=fieldnames)
        writer.writeheader()
        for row in rows:
            writer.writerow({field: row.get(field) for field in fieldnames})


def compute_numeric_summary(records: List[Dict], fields: List[str]) -> Dict[str, Dict]:
    summary: Dict[str, Dict] = {}
    for field in fields:
        values = [r.get(field) for r in records if isinstance(r.get(field), int)]
        if not values:
            continue
        summary[field] = {
            "count": len(values),
            "min": min(values),
            "max": max(values),
            "avg": round(mean(values), 2),
        }
    return summary


def main():
    REPORT_DIR.mkdir(exist_ok=True)
    guid_map = build_guid_map()

    item_records, missing_item_guids = build_item_records(guid_map)
    enemy_records, missing_enemy_guids = build_enemy_records(guid_map)

    item_csv_path = REPORT_DIR / "item_database_stats.csv"
    enemy_csv_path = REPORT_DIR / "enemy_database_stats.csv"

    write_csv(
        item_csv_path,
        [
            "itemName",
            "itemType",
            "itemTypeIndex",
            "rareza",
            "price",
            "hp",
            "mana",
            "ataque",
            "defensa",
            "velocidadAtaque",
            "ataqueCritico",
            "danoCritico",
            "suerte",
            "destreza",
            "nivel",
            "asset_path",
            "guid",
        ],
        item_records,
    )

    write_csv(
        enemy_csv_path,
        [
            "enemyName",
            "description",
            "hp",
            "ataque",
            "defensa",
            "velocidadAtaque",
            "ataqueCritico",
            "danoCritico",
            "suerte",
            "destreza",
            "rewardCoins",
            "experienceReward",
            "requiredLevel",
            "level",
            "asset_path",
            "guid",
        ],
        enemy_records,
    )

    item_summary = compute_numeric_summary(item_records, ITEM_NUMERIC_FIELDS)
    enemy_summary = compute_numeric_summary(enemy_records, ENEMY_NUMERIC_FIELDS)

    comparison_summary = {
        field: {
            "items": item_summary.get(field),
            "enemies": enemy_summary.get(field),
        }
        for field in COMMON_COMPARISON_FIELDS
    }

    summary_payload = {
        "item_count": len(item_records),
        "enemy_count": len(enemy_records),
        "item_numeric_summary": item_summary,
        "enemy_numeric_summary": enemy_summary,
        "comparison": comparison_summary,
        "missing_item_guids": missing_item_guids,
        "missing_enemy_guids": missing_enemy_guids,
        "item_csv": str(item_csv_path.relative_to(ROOT_DIR)),
        "enemy_csv": str(enemy_csv_path.relative_to(ROOT_DIR)),
    }

    summary_path = REPORT_DIR / "database_stats_summary.json"
    summary_path.write_text(json.dumps(summary_payload, indent=2, ensure_ascii=False), encoding="utf-8")

    print("Item records:", len(item_records))
    print("Enemy records:", len(enemy_records))
    if missing_item_guids:
        print("Missing item GUIDs:", missing_item_guids)
    if missing_enemy_guids:
        print("Missing enemy GUIDs:", missing_enemy_guids)
    print("Item CSV:", summary_payload["item_csv"])
    print("Enemy CSV:", summary_payload["enemy_csv"])
    print("Summary JSON:", str(summary_path.relative_to(ROOT_DIR)))


if __name__ == "__main__":
    main()
