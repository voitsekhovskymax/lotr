# -*- coding: utf-8 -*-
"""
Extends all biome JSONs in assets/lotr/worldgen/biomes/ with worldgen params:
  surfaceBlock (real VS block code), subSurfaceBlock, decorationChance,
  decorations[], treeChance, trees[], oreBoost{}, flora[], fauna[]

Single source of truth becomes the JSON — C# LotrWorldGen reads these fields.
Run:  python tools/extend_biomes.py
"""
import json, os, sys

BIOME_DIR = os.path.join(os.path.dirname(__file__), "..", "assets", "lotr", "worldgen", "biomes")

# ── Real VS 1.22 block codes ─────────────────────────────────────────────
SOIL_RICH = "game:soil-rich-normal"
SOIL_MED  = "game:soil-medium-normal"
SOIL_LOW  = "game:soil-low-normal"
STONE     = "game:rock-granite"
GRAVEL    = "game:gravel-granite"
GRAVEL_B  = "game:gravel-basalt"
SAND_ASH  = "game:sand-ash"          # volcanic ash (Mordor)
SAND_WARM = "game:sand-sandstone"    # yellow desert/beach sand
BASALT    = "game:rock-basalt"
ICE       = "game:glacierice"

# VS 1.22 flowers (verified in worldproperties/block/flower.json)
F_DAISY    = "game:flower-wilddaisy-free"
F_POPPY    = "game:flower-goldenpoppy-free"
F_CORN     = "game:flower-cornflower-free"
F_HEATHER  = "game:flower-heather-free"
F_GORSE    = "game:flower-westerngorse-free"
F_EDELW    = "game:flower-edelweiss-free"
F_FORGET   = "game:flower-forgetmenot-free"
F_BLUEBELL = "game:flower-bluebell-free"
F_DAFFODIL = "game:flower-daffodil-free"
F_COWPARS  = "game:flower-cowparsley-free"
F_CATMINT  = "game:flower-catmint-free"
F_LILY     = "game:flower-lilyofthevalley-free"
F_MUGWORT  = "game:flower-mugwort-free"
F_GHOSTRED = "game:flower-ghostpipered-free"
F_WOAD     = "game:flower-woad-free"
F_MALLOW   = "game:flower-orangemallow-free"

G_TALL   = "game:tallgrass-tall-free"
G_MED    = "game:tallgrass-medium-free"
G_SHORT  = "game:tallgrass-short-free"
G_VSHORT = "game:tallgrass-veryshort-free"
G_VTALL  = "game:tallgrass-verytall-free"

BUSH_RED   = "game:fruitingbush-wild-redcurrant-free"
BUSH_BLACK = "game:fruitingbush-wild-blackcurrant-free"
BUSH_BLUE  = "game:fruitingbush-wild-blueberry-free"
BUSH_RASP  = "game:fruitingbush-wild-raspberry-free"
BUSH_STRAW = "game:fruitingbush-wild-strawberry-free"
BUSH_CRAN  = "game:fruitingbush-wild-cranberry-free"
BUSH_CLOUD = "game:fruitingbush-wild-cloudberry-free"
FIRE       = "game:fire"

# Tree generator codes (assets/survival/worldgen/treegen/*.json)
T_OAK      = "game:englishoak"
T_OLDOAK   = "game:oldenglishoak"
T_BIRCH    = "game:silverbirch"
T_RBIRCH   = "game:riverbirch"
T_DBIRCH   = "game:dwarfbirch"
T_HBIRCH   = "game:himalayanbirch"
T_WALNUT   = "game:walnut"
T_PINE     = "game:scotspine"
T_MPINE    = "game:mountainpine"
T_LARCH    = "game:larch"
T_DLARCH   = "game:deepforestlarch"
T_FIR      = "game:fir"
T_ACACIA   = "game:acacia"
T_DACACIA  = "game:deadacacia"
T_KAPOK    = "game:kapok"
T_LKAPOK   = "game:largekapok"
T_DKAPOK   = "game:deadkapok"
T_EBONY    = "game:ebony"
T_MAPLE_S  = "game:sugarmaple"        # golden — Lothlórien mallorn stand-in
T_MAPLE_N  = "game:norwaymaple"
T_MAPLE_CK = "game:crimsonkingmaple"
T_CYPRESS  = "game:greenspirecypress"
T_BALD     = "game:baldcypress"
T_SWAMP    = "game:baldcypressswamp"
T_MULGA    = "game:truemulga"
T_MOSES    = "game:pricklymoses"
T_REDWOOD  = "game:redwoodpine"
T_BRISTLE  = "game:bristleconepine"
T_PURPLE   = "game:purpleheart"

# Fauna (entity code stems — verified against vanilla in Phase 10 spawn impl)
FA_HARE, FA_FOX, FA_WOLF = "game:hare", "game:fox", "game:wolf"
FA_PIG, FA_SHEEP, FA_CHICKEN = "game:pig-wild", "game:bighornsheep", "game:chicken"
FA_MOOSE, FA_BEAR, FA_RACCOON = "game:moose", "game:bear", "game:raccoon"
FA_GAZELLE, FA_HYENA = "game:gazelle", "game:hyena"
ORC, ORC_URUK, SPIDER, WARG = "lotr:orc-mordor", "lotr:uruk-hai", "lotr:mirkwood-spider", "lotr:warg"


def deco(chance, *pairs):
    """decorationChance + weighted decoration list: deco(0.2, (G_TALL,10), (F_DAISY,2))"""
    return chance, [{"block": b, "weight": w} for b, w in pairs]

def trees(chance, *pairs):
    """treeChance + weighted tree list: trees(0.01, (T_OAK,10,1.0), ...) (code, weight, size)"""
    return chance, [{"code": c, "weight": w, "size": s} for c, w, s in pairs]


# ── Archetype groups ─────────────────────────────────────────────────────
LUSH_DECO    = deco(0.22, (G_TALL, 8), (G_MED, 8), (F_DAISY, 2), (F_POPPY, 2), (F_COWPARS, 1))
GRASS_DECO   = deco(0.25, (G_MED, 10), (G_SHORT, 8), (G_TALL, 3), (F_CORN, 1), (F_DAISY, 1))
STEPPE_DECO  = deco(0.20, (G_SHORT, 10), (G_VSHORT, 6), (G_MED, 2))
FOREST_DECO  = deco(0.14, (G_MED, 6), (G_SHORT, 4), (F_BLUEBELL, 2), (F_LILY, 1), (BUSH_RASP, 1))
MOOR_DECO    = deco(0.18, (F_HEATHER, 8), (F_GORSE, 4), (G_SHORT, 5), (G_VSHORT, 3))
ALPINE_DECO  = deco(0.03, (F_EDELW, 3), (G_VSHORT, 2))
TUNDRA_DECO  = deco(0.06, (G_VSHORT, 6), (BUSH_CLOUD, 1), (BUSH_CRAN, 1))
MARSH_DECO   = deco(0.20, (G_VTALL, 6), (G_TALL, 6), (F_FORGET, 1), (F_MUGWORT, 1))
DESERT_DECO  = deco(0.015, (G_VSHORT, 3))
DARK_DECO    = deco(0.06, (F_MUGWORT, 4), (F_GHOSTRED, 2), (G_SHORT, 2))
WASTE_DECO   = deco(0.0)
JUNGLE_DECO  = deco(0.20, (G_VTALL, 6), (G_TALL, 5), (F_MALLOW, 2))

NO_TREES      = trees(0.0)
EU_FOREST     = trees(0.012, (T_OAK, 8, 1.0), (T_BIRCH, 5, 1.0), (T_PINE, 2, 1.0))
EU_PARK       = trees(0.003, (T_OAK, 8, 1.0), (T_BIRCH, 4, 1.0))
PLAINS_TREES  = trees(0.0012, (T_OAK, 5, 0.9), (T_BIRCH, 2, 0.9))
TAIGA_TREES   = trees(0.014, (T_LARCH, 8, 1.0), (T_FIR, 6, 1.0), (T_PINE, 5, 1.0))
MTN_TREES     = trees(0.003, (T_MPINE, 6, 0.9), (T_DBIRCH, 4, 0.8), (T_BRISTLE, 1, 0.8))
MED_TREES     = trees(0.006, (T_OAK, 6, 1.0), (T_CYPRESS, 4, 1.0), (T_WALNUT, 3, 1.0))
SAVANNA_TREES = trees(0.0025, (T_ACACIA, 8, 1.0), (T_MULGA, 3, 0.9), (T_DACACIA, 2, 0.9))
JUNGLE_TREES  = trees(0.022, (T_KAPOK, 8, 1.0), (T_LKAPOK, 3, 1.1), (T_PURPLE, 3, 1.0), (T_EBONY, 2, 1.0))
SWAMP_TREES   = trees(0.008, (T_BALD, 6, 1.0), (T_SWAMP, 4, 1.0), (T_RBIRCH, 3, 1.0))

FAUNA_EU      = [FA_HARE, FA_FOX, FA_PIG, FA_CHICKEN]
FAUNA_FOREST  = [FA_HARE, FA_FOX, FA_WOLF, FA_PIG, FA_BEAR, FA_RACCOON]
FAUNA_PLAINS  = [FA_HARE, FA_FOX, FA_WOLF, FA_SHEEP]
FAUNA_MTN     = [FA_SHEEP, FA_WOLF, FA_BEAR]
FAUNA_NORTH   = [FA_MOOSE, FA_WOLF, FA_BEAR, FA_HARE]
FAUNA_DESERT  = [FA_GAZELLE, FA_HYENA]
FAUNA_NONE    = []

# ── Per-biome table ──────────────────────────────────────────────────────
# id-suffix: dict(surface, sub, deco, trees, ore, fauna)  — omitted keys get archetype defaults
# surface "" = no override (ocean/rivers)

def B(surface, sub="", d=None, t=None, ore=None, fauna=None):
    d = d or WASTE_DECO
    t = t or NO_TREES
    return {
        "surface": surface, "sub": sub,
        "decoChance": d[0], "decorations": d[1],
        "treeChance": t[0], "trees": t[1],
        "ore": ore or {}, "fauna": fauna if fauna is not None else [],
    }

ORE_EREBOR = {"quartz_nativegold": 3.0, "galena": 2.5, "hematite": 2.5, "nativecopper": 2.0, "cassiterite": 2.0}
ORE_MORIA  = {"galena": 3.0, "hematite": 2.0, "nativecopper": 1.5, "bismuthinite": 1.5}
ORE_IRON   = {"hematite": 3.0, "magnetite": 3.0, "limonite": 2.5}
ORE_BLUE   = {"cassiterite": 2.5, "nativecopper": 2.0, "sphalerite": 1.5}
ORE_MISTY  = {"nativecopper": 1.5, "sphalerite": 1.5, "galena": 1.5}
ORE_MTN    = {"nativecopper": 1.5, "hematite": 1.3}
ORE_MORDOR = {"bismuthinite": 1.8, "sphalerite": 1.5, "hematite": 1.5}

T = {
    # ── Shire ───────────────────────────────────────────────────────────
    "shire": B(SOIL_RICH, SOIL_MED,
        deco(0.26, (G_TALL, 8), (G_MED, 6), (F_DAISY, 3), (F_POPPY, 2), (F_COWPARS, 1),
                   (BUSH_STRAW, 1), (BUSH_RED, 1), (BUSH_RASP, 1)),
        trees(0.004, (T_OAK, 8, 1.0), (T_WALNUT, 4, 1.0), (T_BIRCH, 3, 1.0)),
        fauna=[FA_HARE, FA_CHICKEN, FA_PIG, FA_FOX]),
    "shire-woodlands": B(SOIL_RICH, SOIL_MED, FOREST_DECO,
        trees(0.013, (T_OAK, 8, 1.0), (T_OLDOAK, 2, 1.1), (T_WALNUT, 3, 1.0), (T_BIRCH, 4, 1.0)),
        fauna=FAUNA_EU),
    "white-downs": B(SOIL_RICH, SOIL_MED, GRASS_DECO, EU_PARK, fauna=FAUNA_EU),
    "shire-moors": B(SOIL_LOW, "", MOOR_DECO, NO_TREES, fauna=[FA_HARE, FA_FOX]),
    "shire-marshes": B(SOIL_LOW, "", MARSH_DECO, SWAMP_TREES, fauna=[FA_HARE, FA_RACCOON]),
    "old-forest": B(SOIL_MED, SOIL_LOW, DARK_DECO,
        trees(0.018, (T_OLDOAK, 8, 1.2), (T_OAK, 4, 1.1), (T_WALNUT, 2, 1.0)),
        fauna=[FA_WOLF, FA_BEAR, FA_RACCOON]),
    "barrow-downs": B(SOIL_LOW, "", deco(0.10, (G_SHORT, 6), (F_HEATHER, 3), (F_MUGWORT, 2)),
        NO_TREES, fauna=[]),
    "tower-hills": B(SOIL_LOW, "", GRASS_DECO, EU_PARK, fauna=FAUNA_EU),

    # ── Bree & Eriador ──────────────────────────────────────────────────
    "bree": B(SOIL_LOW, "", GRASS_DECO, EU_PARK, fauna=FAUNA_EU),
    "breeland": B(SOIL_LOW, "", GRASS_DECO, EU_PARK, fauna=FAUNA_EU),
    "chetwood": B(SOIL_MED, SOIL_LOW, FOREST_DECO, EU_FOREST, fauna=FAUNA_FOREST),
    "eriador": B(SOIL_LOW, "", GRASS_DECO, PLAINS_TREES, fauna=FAUNA_PLAINS),
    "eriador-downs": B(SOIL_LOW, "", MOOR_DECO, PLAINS_TREES, fauna=FAUNA_PLAINS),
    "midgewater": B(SOIL_LOW, "", MARSH_DECO, NO_TREES, fauna=[FA_RACCOON]),
    "lone-lands": B(SOIL_LOW, "", STEPPE_DECO, PLAINS_TREES, fauna=[FA_HARE, FA_WOLF]),
    "lone-lands-hills": B(SOIL_LOW, "", MOOR_DECO, PLAINS_TREES, fauna=[FA_HARE, FA_WOLF]),
    "angle": B(SOIL_MED, SOIL_LOW, LUSH_DECO, EU_PARK, fauna=FAUNA_EU),
    "coldfells": B(GRAVEL, "", TUNDRA_DECO, NO_TREES, fauna=[FA_WOLF]),
    "ettenmoors": B(SOIL_LOW, "", MOOR_DECO, NO_TREES, fauna=[FA_WOLF, FA_BEAR]),
    "minhiriath": B(SOIL_MED, SOIL_LOW, GRASS_DECO, EU_PARK, fauna=FAUNA_PLAINS),
    "eryn-vorn": B(SOIL_MED, SOIL_LOW, FOREST_DECO,
        trees(0.016, (T_OAK, 6, 1.0), (T_OLDOAK, 3, 1.1), (T_BIRCH, 3, 1.0)), fauna=FAUNA_FOREST),
    "enedwaith": B(SOIL_LOW, "", STEPPE_DECO, PLAINS_TREES, fauna=FAUNA_PLAINS),
    "dunland": B(SOIL_LOW, "", GRASS_DECO, EU_PARK, fauna=FAUNA_PLAINS),
    "swanfleet": B(SOIL_LOW, "", MARSH_DECO, SWAMP_TREES, fauna=[FA_RACCOON]),

    # ── Rivendell & Eregion ─────────────────────────────────────────────
    "rivendell": B(SOIL_MED, SOIL_LOW,
        deco(0.20, (G_TALL, 6), (F_BLUEBELL, 3), (F_LILY, 2), (F_FORGET, 2), (F_CATMINT, 1)),
        trees(0.010, (T_BIRCH, 6, 1.0), (T_HBIRCH, 4, 1.1), (T_PINE, 3, 1.0), (T_MAPLE_N, 2, 1.0)),
        fauna=[FA_HARE, FA_FOX]),
    "trollshaws": B(SOIL_MED, SOIL_LOW, FOREST_DECO, EU_FOREST, fauna=FAUNA_FOREST),
    "eregion": B(SOIL_MED, SOIL_LOW,
        deco(0.16, (G_MED, 6), (F_HEATHER, 4), (F_DAISY, 1)),  # holly land
        EU_PARK, ore={"nativecopper": 1.5}, fauna=FAUNA_EU),

    # ── Blue Mountains & Lindon ─────────────────────────────────────────
    "blue-mountains": B(STONE, GRAVEL, ALPINE_DECO, MTN_TREES, ore=ORE_BLUE, fauna=FAUNA_MTN),
    "blue-mountains-foothills": B(GRAVEL, "", MOOR_DECO, MTN_TREES, ore={"cassiterite": 1.8, "nativecopper": 1.5}, fauna=FAUNA_MTN),
    "lindon": B(SOIL_MED, SOIL_LOW, LUSH_DECO,
        trees(0.008, (T_BIRCH, 6, 1.0), (T_OAK, 4, 1.0), (T_MAPLE_N, 2, 1.0)), fauna=FAUNA_EU),
    "lindon-woodlands": B(SOIL_MED, SOIL_LOW, FOREST_DECO, EU_FOREST, fauna=FAUNA_FOREST),
    "lindon-coast": B(SAND_WARM, "", deco(0.04, (G_SHORT, 4)), NO_TREES, fauna=[]),

    # ── Rohan ───────────────────────────────────────────────────────────
    "rohan": B(SOIL_MED, SOIL_LOW,
        deco(0.30, (G_TALL, 10), (G_MED, 8), (G_VTALL, 3), (F_CORN, 1), (F_DAISY, 1)),
        PLAINS_TREES, fauna=FAUNA_PLAINS),
    "rohan-woodlands": B(SOIL_MED, SOIL_LOW, FOREST_DECO, EU_FOREST, fauna=FAUNA_FOREST),
    "rohan-uruk-highlands": B(SOIL_LOW, "", STEPPE_DECO, NO_TREES, fauna=[FA_WOLF], ),
    "wold": B(SOIL_LOW, "", GRASS_DECO, PLAINS_TREES, fauna=FAUNA_PLAINS),
    "adornland": B(SOIL_MED, SOIL_LOW, GRASS_DECO, EU_PARK, fauna=FAUNA_PLAINS),
    "entwash-mouth": B(SOIL_LOW, "", MARSH_DECO, SWAMP_TREES, fauna=[FA_RACCOON]),

    # ── Fangorn ─────────────────────────────────────────────────────────
    "fangorn": B(SOIL_MED, SOIL_LOW,
        deco(0.16, (G_MED, 5), (F_LILY, 2), (F_BLUEBELL, 2), (F_GHOSTRED, 1)),
        trees(0.022, (T_OLDOAK, 8, 1.3), (T_OAK, 4, 1.2), (T_WALNUT, 3, 1.1), (T_DLARCH, 2, 1.2)),
        fauna=[FA_BEAR, FA_WOLF]),
    "fangorn-clearing": B(SOIL_MED, SOIL_LOW, LUSH_DECO, EU_PARK, fauna=FAUNA_FOREST),
    "fangorn-wasteland": B(SOIL_LOW, "", deco(0.04, (G_VSHORT, 4), (F_MUGWORT, 1)), NO_TREES, fauna=[]),

    # ── Lothlórien ──────────────────────────────────────────────────────
    "lothlorien": B(SOIL_RICH, SOIL_MED,
        deco(0.28, (F_DAFFODIL, 5), (F_POPPY, 4), (G_TALL, 8), (F_LILY, 2), (F_DAISY, 2)),
        trees(0.016, (T_MAPLE_S, 10, 1.2), (T_MAPLE_N, 3, 1.1), (T_BIRCH, 2, 1.0)),
        fauna=[FA_HARE, FA_FOX]),
    "lothlorien-edge": B(SOIL_MED, SOIL_LOW,
        deco(0.20, (F_DAFFODIL, 3), (G_TALL, 6), (G_MED, 4)),
        trees(0.008, (T_MAPLE_S, 6, 1.0), (T_BIRCH, 4, 1.0)), fauna=FAUNA_EU),
    "celebrant": B(SOIL_RICH, SOIL_MED, LUSH_DECO,
        trees(0.006, (T_RBIRCH, 6, 1.0), (T_BIRCH, 3, 1.0)), fauna=FAUNA_EU),

    # ── Misty Mountains & Moria ─────────────────────────────────────────
    "misty-mountains": B(STONE, GRAVEL, ALPINE_DECO, MTN_TREES, ore=ORE_MISTY, fauna=FAUNA_MTN),
    "misty-mountains-foothills": B(GRAVEL, "", MOOR_DECO, TAIGA_TREES, ore={"nativecopper": 1.3}, fauna=FAUNA_MTN),
    "moria": B(STONE, GRAVEL, ALPINE_DECO, NO_TREES, ore=ORE_MORIA, fauna=[]),

    # ── Gondor ──────────────────────────────────────────────────────────
    "gondor": B(SOIL_LOW, "", GRASS_DECO, MED_TREES, fauna=FAUNA_PLAINS),
    "minas-tirith": B(SOIL_LOW, "", GRASS_DECO, NO_TREES, fauna=[]),
    "pelennor": B(SOIL_LOW, "", deco(0.22, (G_MED, 8), (G_SHORT, 6), (F_CORN, 1)), NO_TREES, fauna=[FA_SHEEP]),
    "ithilien": B(SOIL_MED, SOIL_LOW,
        deco(0.20, (G_MED, 6), (F_DAISY, 2), (F_CATMINT, 2), (F_WOAD, 1), (BUSH_BLACK, 1)),
        MED_TREES, fauna=FAUNA_FOREST),
    "ithilien-hills": B(SOIL_LOW, "", GRASS_DECO, MED_TREES, fauna=FAUNA_PLAINS),
    "ithilien-wasteland": B(SAND_WARM, "", DESERT_DECO, NO_TREES, fauna=[]),
    "pelargir": B(SOIL_LOW, "", GRASS_DECO, MED_TREES, fauna=FAUNA_EU),
    "lossarnach": B(SOIL_MED, SOIL_LOW,
        deco(0.26, (F_DAISY, 4), (F_MALLOW, 2), (F_CATMINT, 2), (G_TALL, 6), (BUSH_STRAW, 1)),
        MED_TREES, fauna=FAUNA_EU),  # vale of flowers
    "imloth-melui": B(SOIL_MED, SOIL_LOW,
        deco(0.28, (F_MALLOW, 4), (F_DAISY, 3), (F_CATMINT, 2), (G_TALL, 5)), MED_TREES, fauna=FAUNA_EU),  # roses
    "lamedon": B(SOIL_LOW, "", GRASS_DECO, MED_TREES, fauna=FAUNA_PLAINS),
    "lamedon-hills": B(SOIL_LOW, "", MOOR_DECO, MTN_TREES, fauna=FAUNA_MTN),
    "blackroot-vale": B(SOIL_MED, SOIL_LOW, GRASS_DECO, MED_TREES, fauna=FAUNA_PLAINS),
    "pinnath-gelin": B(SOIL_LOW, "", deco(0.24, (G_TALL, 8), (G_MED, 6)), EU_PARK, fauna=FAUNA_PLAINS),  # green hills
    "dor-en-ernil": B(SOIL_LOW, "", GRASS_DECO, MED_TREES, fauna=FAUNA_PLAINS),
    "dor-en-ernil-hills": B(SOIL_LOW, "", MOOR_DECO, MED_TREES, fauna=FAUNA_MTN),
    "andrast": B(SOIL_LOW, "", MOOR_DECO, EU_PARK, fauna=FAUNA_PLAINS),
    "lebennin": B(SOIL_MED, SOIL_LOW, LUSH_DECO, MED_TREES, fauna=FAUNA_EU),
    "gondor-woodlands": B(SOIL_MED, SOIL_LOW, FOREST_DECO, MED_TREES, fauna=FAUNA_FOREST),
    "pukel": B(SOIL_LOW, "", DARK_DECO,
        trees(0.016, (T_OLDOAK, 6, 1.1), (T_PINE, 4, 1.0), (T_DLARCH, 3, 1.1)), fauna=FAUNA_FOREST),  # Drúadan
    "harondor": B(SOIL_LOW, "", STEPPE_DECO, SAVANNA_TREES, fauna=FAUNA_DESERT),
    "white-mountains": B(STONE, GRAVEL, ALPINE_DECO, NO_TREES, ore=ORE_MTN, fauna=FAUNA_MTN),
    "white-mountains-foothills": B(GRAVEL, "", MOOR_DECO, MTN_TREES, ore=ORE_MTN, fauna=FAUNA_MTN),
    "tolfalas": B(SOIL_MED, SOIL_LOW, GRASS_DECO, MED_TREES, fauna=[FA_HARE]),

    # ── Isengard ────────────────────────────────────────────────────────
    "isengard": B(SOIL_LOW, "", STEPPE_DECO, NO_TREES, fauna=[ORC_URUK, WARG]),
    "nan-curunir": B(SOIL_MED, SOIL_LOW, GRASS_DECO, EU_PARK, fauna=[ORC_URUK]),

    # ── Anduin Vale & Wilderland ────────────────────────────────────────
    "anduin-vale": B(SOIL_MED, SOIL_LOW, LUSH_DECO,
        trees(0.006, (T_RBIRCH, 5, 1.0), (T_OAK, 4, 1.0)), fauna=FAUNA_EU),
    "anduin-hills": B(SOIL_LOW, "", GRASS_DECO, EU_PARK, fauna=FAUNA_PLAINS),
    "anduin-mouth": B(SOIL_LOW, "", MARSH_DECO, SWAMP_TREES, fauna=[FA_RACCOON]),
    "gladden-fields": B(SOIL_LOW, "", deco(0.24, (G_VTALL, 8), (G_TALL, 6), (F_DAFFODIL, 3)),
        SWAMP_TREES, fauna=[FA_RACCOON]),  # gladden = iris
    "wilderland": B(SOIL_LOW, "", GRASS_DECO, PLAINS_TREES, fauna=FAUNA_PLAINS),
    "wilderland-north": B(SOIL_LOW, "", STEPPE_DECO, TAIGA_TREES, fauna=FAUNA_NORTH),
    "brown-lands": B(SOIL_LOW, "", deco(0.03, (G_VSHORT, 3), (F_MUGWORT, 1)), NO_TREES, fauna=[]),
    "nindalf": B(SOIL_LOW, "", MARSH_DECO, NO_TREES, fauna=[]),
    "long-marshes": B(SOIL_LOW, "", MARSH_DECO, SWAMP_TREES, fauna=[FA_RACCOON]),
    "east-bight": B(SOIL_LOW, "", GRASS_DECO, PLAINS_TREES, fauna=FAUNA_PLAINS),

    # ── Mirkwood ────────────────────────────────────────────────────────
    "woodland-realm": B(SOIL_MED, SOIL_LOW, FOREST_DECO,
        trees(0.018, (T_PINE, 6, 1.1), (T_BIRCH, 4, 1.0), (T_OAK, 3, 1.0), (T_FIR, 3, 1.0)),
        fauna=FAUNA_FOREST),
    "woodland-realm-hills": B(SOIL_LOW, "", FOREST_DECO, TAIGA_TREES, fauna=FAUNA_FOREST),
    "mirkwood": B(SOIL_MED, SOIL_LOW, DARK_DECO,
        trees(0.020, (T_PINE, 6, 1.1), (T_DLARCH, 5, 1.2), (T_FIR, 3, 1.0)), fauna=[SPIDER, FA_WOLF]),
    "mirkwood-corrupted": B(SOIL_LOW, "",
        deco(0.10, (F_MUGWORT, 5), (F_GHOSTRED, 3), (G_SHORT, 2)),
        trees(0.020, (T_DLARCH, 7, 1.2), (T_DKAPOK, 2, 1.0), (T_PINE, 3, 1.1)), fauna=[SPIDER]),
    "mirkwood-north": B(SOIL_MED, SOIL_LOW, FOREST_DECO,
        trees(0.016, (T_BIRCH, 6, 1.0), (T_PINE, 5, 1.0), (T_LARCH, 3, 1.0)), fauna=FAUNA_FOREST),
    "mirkwood-mountains": B(STONE, GRAVEL, ALPINE_DECO, MTN_TREES, ore=ORE_MTN, fauna=FAUNA_MTN),
    "dol-guldur": B(SOIL_LOW, "", deco(0.06, (F_GHOSTRED, 4), (F_MUGWORT, 3)),
        trees(0.010, (T_DLARCH, 6, 1.1), (T_DKAPOK, 2, 1.0)), fauna=[SPIDER, ORC]),

    # ── Erebor, Dale, Iron Hills, Grey Mountains ────────────────────────
    "erebor": B(STONE, GRAVEL, ALPINE_DECO, NO_TREES, ore=ORE_EREBOR, fauna=[]),
    "dale": B(SOIL_LOW, "", GRASS_DECO, EU_PARK, fauna=FAUNA_EU),
    "iron-hills": B(STONE, GRAVEL, ALPINE_DECO, MTN_TREES, ore=ORE_IRON, fauna=FAUNA_MTN),
    "grey-mountains": B(STONE, GRAVEL, ALPINE_DECO, MTN_TREES, ore=ORE_MTN, fauna=FAUNA_MTN),
    "grey-mountains-foothills": B(GRAVEL, "", TUNDRA_DECO, TAIGA_TREES, ore=ORE_MTN, fauna=FAUNA_NORTH),

    # ── Angmar & Far North ──────────────────────────────────────────────
    "angmar": B(GRAVEL, "", TUNDRA_DECO, NO_TREES, ore={"bismuthinite": 1.5}, fauna=[FA_WOLF, ORC]),
    "angmar-mountains": B(STONE, GRAVEL, ALPINE_DECO, NO_TREES, ore=ORE_MTN, fauna=[FA_WOLF]),
    "forodwaith": B(GRAVEL, "", TUNDRA_DECO, NO_TREES, fauna=[FA_WOLF, FA_BEAR]),
    "forodwaith-mountains": B(STONE, GRAVEL, deco(0.01, (F_EDELW, 1)), NO_TREES, fauna=[]),
    "forodwaith-glacier": B(ICE, "", WASTE_DECO, NO_TREES, fauna=[]),
    "forodwaith-coast": B(GRAVEL, "", TUNDRA_DECO, NO_TREES, fauna=[FA_BEAR]),
    "tundra": B(GRAVEL, "", TUNDRA_DECO, trees(0.002, (T_DBIRCH, 6, 0.8)), fauna=FAUNA_NORTH),
    "taiga": B(SOIL_LOW, "", deco(0.10, (G_SHORT, 5), (BUSH_BLUE, 2), (BUSH_CLOUD, 1)),
        TAIGA_TREES, fauna=FAUNA_NORTH),

    # ── Mordor ──────────────────────────────────────────────────────────
    "mordor": B(BASALT, SAND_ASH, deco(0.02, (FIRE, 1)), NO_TREES, ore=ORE_MORDOR, fauna=[ORC]),
    "mordor-mountains": B(STONE, GRAVEL_B, WASTE_DECO, NO_TREES, ore=ORE_MORDOR, fauna=[ORC]),
    "gorgoroth": B(BASALT, SAND_ASH, deco(0.03, (FIRE, 1)), NO_TREES, ore=ORE_MORDOR, fauna=[ORC]),
    "udun": B(BASALT, SAND_ASH, deco(0.02, (FIRE, 1)), NO_TREES, ore=ORE_MORDOR, fauna=[ORC]),
    "morgul-vale": B(BASALT, GRAVEL_B, deco(0.05, (F_GHOSTRED, 3), (F_MUGWORT, 2)), NO_TREES, fauna=[ORC]),
    "nan-ungol": B(GRAVEL_B, "", DARK_DECO, NO_TREES, fauna=[SPIDER]),
    "dagorlad": B(SAND_ASH, "", WASTE_DECO, NO_TREES, fauna=[]),
    "dead-marshes": B(SOIL_LOW, "", deco(0.14, (G_VTALL, 4), (F_GHOSTRED, 2), (F_MUGWORT, 2)),
        NO_TREES, fauna=[]),
    "emyn-muil": B(STONE, GRAVEL, deco(0.02, (G_VSHORT, 2)), NO_TREES, fauna=[]),
    "nurn": B(SOIL_LOW, "", STEPPE_DECO, PLAINS_TREES, fauna=[ORC]),
    "nurnen": B(SOIL_LOW, "", MARSH_DECO, NO_TREES, fauna=[ORC]),
    "nurn-marshes": B(SOIL_LOW, "", MARSH_DECO, NO_TREES, fauna=[]),
    "eastern-desolation": B(BASALT, SAND_ASH, WASTE_DECO, NO_TREES, fauna=[]),

    # ── Rhûn & Dorwinion ────────────────────────────────────────────────
    "rhun": B(SOIL_LOW, "", STEPPE_DECO, PLAINS_TREES, fauna=FAUNA_PLAINS),
    "rhun-land": B(SOIL_LOW, "", STEPPE_DECO, PLAINS_TREES, fauna=FAUNA_PLAINS),
    "rhun-land-steppe": B(SOIL_LOW, "", deco(0.26, (G_SHORT, 10), (G_VSHORT, 8), (G_MED, 3)),
        NO_TREES, fauna=[FA_GAZELLE, FA_WOLF]),
    "rhun-land-hills": B(SOIL_LOW, "", STEPPE_DECO, PLAINS_TREES, fauna=FAUNA_PLAINS),
    "rhun-forest": B(SOIL_MED, SOIL_LOW, FOREST_DECO,
        trees(0.014, (T_MAPLE_N, 5, 1.0), (T_OAK, 4, 1.0), (T_LARCH, 3, 1.0)), fauna=FAUNA_FOREST),
    "rhun-red-forest": B(SOIL_MED, SOIL_LOW, FOREST_DECO,
        trees(0.016, (T_MAPLE_CK, 8, 1.1), (T_MAPLE_S, 3, 1.0), (T_MAPLE_N, 3, 1.0)), fauna=FAUNA_FOREST),
    "rhun-island": B(SOIL_MED, SOIL_LOW, GRASS_DECO, EU_PARK, fauna=[FA_HARE]),
    "rhun-island-forest": B(SOIL_MED, SOIL_LOW, FOREST_DECO, EU_FOREST, fauna=FAUNA_FOREST),
    "dorwinion": B(SOIL_RICH, SOIL_MED,
        deco(0.24, (G_TALL, 6), (F_DAISY, 2), (F_CATMINT, 2), (BUSH_RED, 2), (BUSH_BLACK, 2), (BUSH_STRAW, 1)),
        trees(0.005, (T_WALNUT, 6, 1.0), (T_OAK, 3, 1.0)), fauna=FAUNA_EU),  # wine country
    "dorwinion-hills": B(SOIL_MED, SOIL_LOW,
        deco(0.20, (G_MED, 6), (BUSH_RED, 2), (BUSH_BLACK, 1)), EU_PARK, fauna=FAUNA_EU),
    "red-mountains": B(STONE, GRAVEL, ALPINE_DECO, MTN_TREES, ore={"hematite": 2.0, "rhodochrosite": 2.0}, fauna=FAUNA_MTN),
    "red-mountains-foothills": B(GRAVEL, "", MOOR_DECO, MTN_TREES, ore={"hematite": 1.5}, fauna=FAUNA_MTN),
    "wind-mountains": B(STONE, GRAVEL, ALPINE_DECO, NO_TREES, ore=ORE_MTN, fauna=FAUNA_MTN),
    "wind-mountains-foothills": B(GRAVEL, "", TUNDRA_DECO, TAIGA_TREES, fauna=FAUNA_NORTH),
    "last-desert": B(SAND_WARM, "", DESERT_DECO, NO_TREES, fauna=FAUNA_DESERT),

    # ── Near Harad ──────────────────────────────────────────────────────
    "near-harad": B(SAND_WARM, "", DESERT_DECO, trees(0.0008, (T_DACACIA, 5, 0.9), (T_ACACIA, 2, 0.9)), fauna=FAUNA_DESERT),
    "near-harad-hills": B(SAND_WARM, "", DESERT_DECO, NO_TREES, fauna=FAUNA_DESERT),
    "near-harad-semi-desert": B(SAND_WARM, "", deco(0.05, (G_VSHORT, 5), (G_SHORT, 2)), SAVANNA_TREES, fauna=FAUNA_DESERT),
    "near-harad-red-desert": B(SAND_WARM, "", WASTE_DECO, NO_TREES, fauna=[]),
    "near-harad-fertile": B(SOIL_MED, SOIL_LOW, GRASS_DECO,
        trees(0.005, (T_ACACIA, 5, 1.0), (T_WALNUT, 3, 1.0)), fauna=FAUNA_DESERT + [FA_SHEEP]),
    "near-harad-fertile-forest": B(SOIL_MED, SOIL_LOW, FOREST_DECO, SAVANNA_TREES, fauna=FAUNA_DESERT),
    "near-harad-oasis": B(SOIL_MED, SOIL_LOW, LUSH_DECO,
        trees(0.010, (T_ACACIA, 6, 1.0), (T_KAPOK, 1, 0.9)), fauna=FAUNA_DESERT),
    "near-harad-riverbank": B(SOIL_MED, SOIL_LOW, LUSH_DECO,
        trees(0.006, (T_RBIRCH, 4, 1.0), (T_ACACIA, 4, 1.0)), fauna=FAUNA_DESERT),

    # ── Far Harad ───────────────────────────────────────────────────────
    "far-harad": B(SAND_WARM, "", DESERT_DECO, SAVANNA_TREES, fauna=FAUNA_DESERT),
    "far-harad-arid": B(SAND_WARM, "", WASTE_DECO, trees(0.0006, (T_DACACIA, 5, 0.9)), fauna=FAUNA_DESERT),
    "far-harad-arid-hills": B(SAND_WARM, "", WASTE_DECO, NO_TREES, fauna=FAUNA_DESERT),
    "far-harad-bushland": B(SOIL_LOW, "", deco(0.12, (G_SHORT, 6), (G_VSHORT, 4)),
        trees(0.005, (T_MULGA, 6, 1.0), (T_MOSES, 4, 1.0), (T_ACACIA, 3, 1.0)), fauna=FAUNA_DESERT),
    "far-harad-bushland-hills": B(SOIL_LOW, "", STEPPE_DECO, SAVANNA_TREES, fauna=FAUNA_DESERT),
    "far-harad-forest": B(SOIL_MED, SOIL_LOW, FOREST_DECO,
        trees(0.014, (T_KAPOK, 5, 1.0), (T_EBONY, 4, 1.0), (T_ACACIA, 3, 1.0)), fauna=FAUNA_DESERT),
    "far-harad-jungle": B(SOIL_MED, SOIL_LOW, JUNGLE_DECO, JUNGLE_TREES, fauna=[FA_CHICKEN, FA_RACCOON]),
    "far-harad-jungle-edge": B(SOIL_MED, SOIL_LOW, JUNGLE_DECO,
        trees(0.012, (T_KAPOK, 6, 1.0), (T_ACACIA, 3, 1.0)), fauna=[FA_CHICKEN]),
    "far-harad-jungle-lake": B(SOIL_MED, SOIL_LOW, JUNGLE_DECO, SWAMP_TREES, fauna=[FA_CHICKEN]),
    "far-harad-cloud-forest": B(SOIL_RICH, SOIL_MED,
        deco(0.24, (G_VTALL, 5), (G_TALL, 5), (F_FORGET, 2), (F_LILY, 1)),
        trees(0.018, (T_KAPOK, 5, 1.0), (T_HBIRCH, 4, 1.1), (T_REDWOOD, 2, 1.1)), fauna=[FA_CHICKEN]),
    "far-harad-mangrove": B(SOIL_LOW, "", MARSH_DECO, SWAMP_TREES, fauna=[FA_RACCOON]),
    "far-harad-swamp": B(SOIL_LOW, "", MARSH_DECO, SWAMP_TREES, fauna=[FA_RACCOON]),
    "far-harad-volcano": B(BASALT, SAND_ASH, deco(0.03, (FIRE, 1)), NO_TREES, fauna=[]),
    "far-harad-coast": B(SAND_WARM, "", deco(0.03, (G_SHORT, 3)), trees(0.002, (T_ACACIA, 4, 1.0)), fauna=[]),
    "gulf-harad": B(SAND_WARM, "", DESERT_DECO, SAVANNA_TREES, fauna=FAUNA_DESERT),
    "gulf-harad-forest": B(SOIL_MED, SOIL_LOW, FOREST_DECO, SAVANNA_TREES, fauna=FAUNA_DESERT),
    "harad-mountains": B(STONE, GRAVEL, ALPINE_DECO, NO_TREES, ore=ORE_MTN, fauna=FAUNA_MTN),
    "harnedor": B(SOIL_LOW, "", STEPPE_DECO, SAVANNA_TREES, fauna=FAUNA_DESERT),
    "lostladen": B(SAND_WARM, "", WASTE_DECO, NO_TREES, fauna=[FA_HYENA]),
    "pertorogwaith": B(SAND_WARM, "", DESERT_DECO, NO_TREES, fauna=FAUNA_DESERT),
    "tauredain-clearing": B(SOIL_MED, SOIL_LOW, JUNGLE_DECO,
        trees(0.006, (T_KAPOK, 4, 1.0)), fauna=[FA_CHICKEN]),

    # ── Umbar ───────────────────────────────────────────────────────────
    "umbar": B(SAND_WARM, "", DESERT_DECO, SAVANNA_TREES, fauna=FAUNA_DESERT),
    "umbar-hills": B(SOIL_LOW, "", STEPPE_DECO, SAVANNA_TREES, fauna=FAUNA_DESERT),
    "umbar-forest": B(SOIL_MED, SOIL_LOW, FOREST_DECO,
        trees(0.010, (T_CYPRESS, 5, 1.0), (T_OAK, 3, 1.0), (T_ACACIA, 3, 1.0)), fauna=FAUNA_DESERT),

    # ── Generic / water / misc ──────────────────────────────────────────
    "ocean": B(""), "river": B(""), "lake": B(""),
    "beach": B(SAND_WARM, "", deco(0.02, (G_VSHORT, 2))),
    "beach-gravel": B(GRAVEL, ""),
    "beach-white": B(SAND_WARM, "", deco(0.02, (G_VSHORT, 2))),
    "island": B(SOIL_MED, SOIL_LOW, GRASS_DECO, EU_PARK, fauna=[FA_HARE]),
    "meneltarma": B(SOIL_MED, SOIL_LOW, GRASS_DECO, EU_PARK, fauna=[]),
}


def main():
    files = sorted(f for f in os.listdir(BIOME_DIR) if f.endswith(".json"))
    missing = []
    for fn in files:
        suffix = fn[:-5]
        path = os.path.join(BIOME_DIR, fn)
        with open(path, encoding="utf-8-sig") as f:
            data = json.load(f)

        row = T.get(suffix)
        if row is None:
            missing.append(suffix)
            continue

        data["surfaceBlock"]     = row["surface"]
        data["subSurfaceBlock"]  = row["sub"]
        data["decorationChance"] = row["decoChance"]
        data["decorations"]      = row["decorations"]
        data["treeChance"]       = row["treeChance"]
        data["trees"]            = row["trees"]
        data["oreBoost"]         = row["ore"]
        data["fauna"]            = row["fauna"]
        data["flora"]            = sorted({d["block"] for d in row["decorations"]} |
                                          {t["code"] for t in row["trees"]})

        with open(path, "w", encoding="utf-8", newline="\n") as f:
            json.dump(data, f, indent=2, ensure_ascii=False)
            f.write("\n")

    print(f"Updated {len(files) - len(missing)}/{len(files)} biomes.")
    if missing:
        print("NO TABLE ENTRY (left untouched):", ", ".join(missing))
    # sanity: table entries with no file
    orphan = [k for k in T if f"{k}.json" not in files]
    if orphan:
        print("TABLE ENTRY WITHOUT FILE:", ", ".join(orphan))


if __name__ == "__main__":
    main()
