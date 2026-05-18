# Regions Design

## Overview

Regions are named geographic areas in the mod world that have visual identity,
associated factions, and eventually custom world generation.

## Region List

| Region ID | Faction | Status |
|---|---|---|
| lotr:region-shire | lotr:faction-shire | Phase 1 — basic marker |
| lotr:region-rivendell | lotr:faction-rivendell | Phase 3+ |
| lotr:region-moria | lotr:faction-moria | Phase 5+ |
| lotr:region-rohan | lotr:faction-rohan | Phase 5+ |
| lotr:region-gondor | lotr:faction-gondor | Phase 5+ |
| lotr:region-isengard | lotr:faction-isengard | Phase 5+ |
| lotr:region-mordor | lotr:faction-mordor | Phase 5+ |

## Phase 1

- Shire Grass block as a visual marker for the Shire region
- No actual region boundaries in Phase 1

## Phase 4+

- `RegionSystem` maps world coordinates to named regions
- Drives spawn conditions, music, ambient effects
- Region data in `assets/lotr/worldgen/regions/`
