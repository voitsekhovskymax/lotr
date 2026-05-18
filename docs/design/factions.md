# Factions Design

## Overview

The LOTR mod uses a faction system where each region has an associated faction.
Players gain or lose alignment with factions through actions, quests, and combat.

## Faction List

| Faction ID | Name | Default Alignment | Notes |
|---|---|---|---|
| lotr:faction-shire | The Shire (Hobbits) | +200 | Friendly default |
| lotr:faction-gondor | Gondor (Humans) | 0 | Neutral |
| lotr:faction-rohan | Rohan (Rohirrim) | 0 | Neutral |
| lotr:faction-rivendell | Rivendell (Elves) | 0 | Neutral |
| lotr:faction-moria | Moria (Dwarves) | 0 | Neutral |
| lotr:faction-isengard | Isengard (Uruk-hai) | -500 | Hostile |
| lotr:faction-mordor | Mordor (Orcs/Sauron) | -800 | Very hostile |

## Alignment Thresholds

| Range | Label |
|---|---|
| 750..1000 | Exalted |
| 500..749 | Revered |
| 250..499 | Honored |
| 0..249 | Friendly |
| -249..-1 | Unfriendly |
| -499..-250 | Hostile |
| -749..-500 | Hated |
| -1000..-750 | Nemesis |

## Architecture Notes

- `AlignmentSystem` is the single source of truth for per-player alignment data
- `FactionSystem` reads from `AlignmentSystem` to determine NPC hostility
- Do NOT duplicate alignment state inside FactionSystem
- Alignment is stored in player WatchedAttributes with keys: `lotr_alignment_<faction_id>`

## Future: Inter-faction Relations

Each faction can have ally/enemy relationships with other factions.
Example: gaining alignment with Gondor slightly improves Rohan alignment.
This will be implemented in Phase 2.
