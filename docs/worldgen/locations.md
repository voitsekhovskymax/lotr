# LOTR Mod — Location & Biome Specifications

> Used by: RegionSystem, WorldGen Phase 6+, SpawnSystem
> Last updated: Phase 4

---

## Location Table

| Location     | Region ID              | Faction          | Landform type  | Temp °C   | Rain      | Fertility | Notes                              |
|--------------|------------------------|------------------|----------------|-----------|-----------|-----------|-------------------------------------|
| The Shire    | lotr:region-shire      | lotr:faction-shire     | rolling hills  | +15..+20  | 0.6..0.8  | 0.7..0.9  | Green, lush, gentle pag hills      |
| Bree         | lotr:region-bree       | lotr:faction-gondor    | flat lowlands  | +10..+15  | 0.5..0.7  | 0.5..0.7  | Crossroads town, moderate climate  |
| Rivendell    | lotr:region-rivendell  | lotr:faction-rivendell | deep valley    | +10..+15  | 0.6..0.8  | 0.6..0.8  | Hidden valley, waterfalls, elven   |
| Blue Mountains| lotr:region-blue-mountains| lotr:faction-moria  | high mountains | +0..+8    | 0.4..0.6  | 0.2..0.4  | Snow peaks, dwarven tunnels        |
| Moria        | lotr:region-moria      | lotr:faction-moria     | underground    | +5..+10   | 0.0       | 0.0       | Caves only, no surface flora       |
| Minas Tirith | lotr:region-minas-tirith| lotr:faction-gondor   | cliff plateau  | +15..+20  | 0.3..0.5  | 0.4..0.6  | White city on mountain spur        |
| Isengard     | lotr:region-isengard   | lotr:faction-isengard  | flat lowlands  | +10..+15  | 0.2..0.4  | 0.1..0.3  | Deforested, industrial wasteland   |
| Mordor       | lotr:region-mordor     | lotr:faction-mordor    | volcanic waste | +20..+30  | 0.0..0.1  | 0.0..0.05 | Ash, lava, dead rock, no flora     |

---

## Biome Details

### The Shire
- Flora: oak/apple trees, tall grass, berry bushes, wildflowers
- Fauna: rabbits, birds, deer
- Special: hobbit hole structures (Phase 6)
- Surface blocks: fertile soil with normal grass, moss

### Bree
- Flora: mixed forest, hedgerows
- Fauna: horses, cattle
- Special: inn structure, market
- Surface blocks: standard soil

### Rivendell
- Flora: birch/beech forest, ferns, flowers
- Fauna: deer, birds
- Special: elven buildings along valley walls
- Surface blocks: fertile soil, moss-covered stone
- Water: multiple waterfalls and streams

### Blue Mountains (Ered Luin)
- Flora: sparse pine, alpine flowers above snowline
- Fauna: goats, mountain eagles
- Special: dwarven mine entrances
- Surface blocks: stone, gravel, thin soil; snow above Y~0.7

### Moria (Khazad-dûm)
- Flora: none (underground)
- Fauna: cave fish (future), goblins (Phase 6)
- Special: massive cave complex, bridges, pillars
- Surface blocks: obsidian, basalt, glowstone accents

### Minas Tirith
- Flora: olive trees, dry grassland, cypress
- Fauna: horses, hawks
- Special: city walls, 7 tiers (Phase 6)
- Surface blocks: limestone/white stone, flagstone

### Isengard
- Flora: almost none (deforested), dead stumps
- Fauna: none naturally
- Special: Orthanc tower, pit mines
- Surface blocks: scorched earth, gravel, mud

### Mordor
- Flora: none (ash plains), occasional dead tree
- Fauna: none
- Special: Mount Doom, Barad-dûr (Phase 6), lava rivers
- Surface blocks: volcanic ash (gravel-basalt), basalt, lava pockets
- Climate note: extreme heat, near-zero rain and fertility

---

## Coordinate Plan (World Center = 0,0)

Approximate layout in world blocks (subject to revision in Phase 6 when PNG map arrives):

```
                     [Blue Mountains -4000,0]
                           |
  [Shire -2000,-1000] --- [Bree -1000,0] --- [Rivendell 0,-800]
                                |
                         [Minas Tirith 2000,500]
                                |
                    [Isengard 500,2000]
                                |
                    [Mordor 2000,3000]
                         [Moria 500,1000 underground]
```

---

## Phase Roadmap for World Generation

| Phase | Scope                                                              |
|-------|--------------------------------------------------------------------|
| 4     | RegionSystem (AABB-based), /lotr region command, no worldgen yet   |
| 5     | Custom landforms JSON for shire/mordor, custom block layers        |
| 6     | Structure schematics (hobbit holes, Orthanc, city walls)           |
| 7     | PNG-driven map — pixel=region, color=biome, full world layout      |
| 8     | Rivers, lava flows, waterfalls via custom ChunkColumnGeneration    |
