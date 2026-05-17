# LOTR Mod for Vintage Story — Comprehensive Development Plan

> Project root: `/Users/max/Projects/lotr`
> Current mod id: `lotr`
> Target game/runtime: Vintage Story 1.22.x, .NET 10, C# 13
> Canonical game path: `/Applications/Vintage Story.app`
> Canonical mods deploy path: `/Users/max/Library/Application Support/VintagestoryData/Mods/lotr`

## 1. Purpose of this document

This file is the master development plan for the LOTR mod.
It consolidates ideas from:
- `AGENT_CONTEXT.md`
- `LOTR_MOD_SETUP.md`
- `STARTER_FILES.md`
- the actual current repository state

It is written for the real current project, not the older `lotrmod` template layout shown in some of the imported notes.

The goal is to turn the current minimal scaffold into a full Middle-earth total-conversion-style content mod for Vintage Story, with a phased plan that is realistic, testable, and safe to build incrementally.

---

## 2. Current project state

The actual repository currently contains:
- `lotr.csproj`
- `modinfo.json` with mod id `lotr`
- `LotrSystem.cs` with a minimal `ModSystem`
- `assets/lotr/lang/en.json`
- `Properties/launchSettings.json`

The current repo does NOT yet contain the large code/content structure described in the imported notes, such as:
- `src/entities/`
- `src/systems/`
- `src/worldgen/`
- `src/items/`
- `assets/lotr/entities/`
- `assets/lotr/blocktypes/`
- `assets/lotr/itemtypes/`
- `assets/lotr/worldgen/`
- `CakeBuild/`

So this plan starts from the real current scaffold and treats the imported documents as design intent and source material, not as the current truth.

---

## 3. Critical inconsistencies discovered in the imported documents

Before writing features, the project must resolve documentation drift.

### 3.1 Naming drift
Imported docs frequently use:
- mod id: `lotrmod`
- namespace: `LotrMod`
- paths: `assets/lotrmod/...`
- entity/item/block codes prefixed with `lotrmod:`

Actual current project uses:
- mod id: `lotr`
- namespace: `Lotr`
- paths: `assets/lotr/...`
- codes should therefore use `lotr:`

Decision for this plan:
- Keep the real project identity as `lotr`
- Treat all `lotrmod` references in the imported docs as legacy material that must be migrated

### 3.2 Game path drift
Imported docs repeatedly use:
- `/Applications/Vintagestory/`

Actual verified working path is:
- `/Applications/Vintage Story.app`

Decision for this plan:
- Standardize all docs and future code comments on `/Applications/Vintage Story.app`

### 3.3 Build workflow drift
Imported docs mention Cake build and zip packaging.
Actual current repo is a plain .NET project and already builds correctly with:
- `dotnet build lotr.csproj -c Debug`

Decision for this plan:
- Use plain `dotnet build` and loose-file deployment as the default dev workflow now
- Add packaging automation later only if needed

### 3.4 Output artifact drift
This project was recently renamed. Old artifacts such as `lotrmod.dll` can reappear if build output is not cleaned.

Decision for this plan:
- Any phase that changes assembly naming, output structure, or deployment should verify there are no stale `lotrmod.*` files in either:
  - `bin/Debug/Mods/mod/`
  - `/Users/max/Library/Application Support/VintagestoryData/Mods/lotr`

---

## 4. Product vision

The mod should gradually add a recognizable, explorable Middle-earth experience to Vintage Story.

The final target includes:
- iconic regions: Shire, Rivendell, Moria, Lothlorien, Rohan, Gondor, Isengard, Mordor
- important NPCs: Frodo, Bilbo, Gandalf, Aragorn, Legolas, Gimli, Boromir, Sauron-aligned enemies
- faction/alignment gameplay
- quest progression
- themed items, food, weapons, armor, blocks, decorative sets
- world generation for landmark regions and structures
- unique progression and exploration goals

The mod should be built in playable vertical slices, not as one giant content dump.

---

## 5. Design principles

1. Build in vertical slices
   - Every phase should produce something playable in-game.

2. Keep `lotr` canonical everywhere
   - File paths, asset ids, code ids, translations, logs, docs.

3. Prefer stable scaffolding before content explosion
   - Faction/alignment/quest architecture should exist before dozens of NPCs depend on it.

4. Server/client separation must be respected
   - Server systems for logic and persistence
   - Client systems only for rendering/UI/client-only behavior

5. Data-driven where possible
   - JSON for content definitions
   - C# for systems, AI, persistence, hooks, and custom behavior

6. Avoid overbuilding too early
   - Phase 1 should not require complete worldgen, full dialogue trees, or every canon character.

7. Every phase must have verification steps
   - Build passes
   - Mod loads
   - Spawn/test commands work
   - No stale renamed artifacts remain

---

## 6. Target architecture for the current repo

This is the intended structure for the actual `lotr` project:

```text
/Users/max/Projects/lotr/
├── readme.md
├── AGENT_CONTEXT.md
├── LOTR_MOD_SETUP.md
├── STARTER_FILES.md
├── lotr.csproj
├── modinfo.json
├── LotrSystem.cs                     # temporary; may become src/LotrModSystem.cs later
├── Properties/
│   └── launchSettings.json
├── assets/
│   └── lotr/
│       ├── lang/
│       │   ├── en.json
│       │   └── uk.json
│       ├── blocktypes/
│       ├── itemtypes/
│       ├── entities/
│       │   └── humanoid/
│       ├── worldgen/
│       │   ├── blockpatches/
│       │   ├── structures/
│       │   ├── regions/
│       │   └── spawnconditions/
│       ├── recipes/
│       ├── textures/
│       │   ├── block/
│       │   ├── item/
│       │   └── entity/
│       ├── shapes/
│       ├── sounds/
│       ├── dialogue/
│       ├── tradelists/
│       └── config/
├── src/
│   ├── LotrModSystem.cs
│   ├── Constants/
│   ├── Integration/
│   ├── Entities/
│   │   ├── Humanoids/
│   │   ├── Monsters/
│   │   ├── Bosses/
│   │   ├── Behaviors/
│   │   └── AI/
│   ├── Systems/
│   │   ├── Alignment/
│   │   ├── Factions/
│   │   ├── Quests/
│   │   ├── Dialogue/
│   │   ├── Regions/
│   │   ├── Spawn/
│   │   ├── World/
│   │   └── SaveData/
│   ├── WorldGen/
│   ├── Items/
│   ├── Blocks/
│   ├── Commands/
│   ├── Config/
│   └── Utilities/
└── docs/
    ├── design/
    ├── plans/
    ├── lore/
    └── testing/
```

Note: `LotrSystem.cs` is acceptable for now, but long-term the project should move to `src/LotrModSystem.cs` once the source tree is expanded.

---

## 7. Major development streams

The project naturally breaks into 8 streams:

1. Foundation and normalization
2. Core code scaffolding
3. NPC/entity pipeline
4. Factions and alignment
5. Dialogue and quest systems
6. World generation and region identity
7. Items, blocks, combat, and progression
8. Polish, balancing, packaging, release, and mod compatibility

Each stream appears in the roadmap below.

---

## 8. Roadmap overview

### Phase 0 — Normalize the project and documents
Goal: align all docs and starter ideas with the actual current `lotr` project.

### Phase 1 — Build the minimal playable vertical slice: The Shire
Goal: load the mod, spawn Frodo/Bilbo, place at least one themed block/item, and prove the project direction in-game.

### Phase 2 — Introduce reusable gameplay systems
Goal: create stable faction, alignment, dialogue, and quest foundations.

### Phase 3 — Expand to a second region with stronger identity
Goal: add Gandalf, a first quest chain, and one more region or landmark.

### Phase 4 — World generation framework
Goal: establish region mapping, structure placement, and biome/theming rules.

### Phase 5 — Add major region packs
Goal: Moria, Rivendell/Lothlorien, Rohan, Gondor, Isengard, Mordor.

### Phase 6 — Combat, enemies, bosses, and progression
Goal: orcs, uruks, trolls, Balrog/Sauron-aligned threats, gear, rewards.

### Phase 7 — Polish and release engineering
Goal: performance, balancing, translations, testing passes, packaging, release docs.

---

## 9. Detailed phase-by-phase plan

## Phase 0 — Foundation and normalization

### Objectives
- Make project naming and docs internally consistent
- Define the real canonical architecture
- Remove `lotrmod` legacy references from live development docs
- Prepare the repo for multi-file expansion

### Why this phase matters
The imported docs are useful, but they describe an older project identity and an older path layout. If this drift remains, the AI agent and future coding work will constantly regenerate wrong file paths, wrong ids, and wrong asset namespaces.

### Tasks

#### 0.1 Normalize the documentation set
Update:
- `AGENT_CONTEXT.md`
- `LOTR_MOD_SETUP.md`
- `STARTER_FILES.md`

Required changes:
- `lotrmod` -> `lotr`
- `LotrMod` -> `Lotr`
- `assets/lotrmod/` -> `assets/lotr/`
- `lotrmod:` -> `lotr:`
- `/Applications/Vintagestory/` -> `/Applications/Vintage Story.app`
- nested old template layout -> current flat repo layout

#### 0.2 Define code organization
Create directories:
- `src/`
- `src/Entities/`
- `src/Entities/AI/`
- `src/Systems/`
- `src/Items/`
- `src/Blocks/`
- `src/WorldGen/`
- `src/Utilities/`
- `docs/design/`
- `docs/testing/`

#### 0.3 Move or replace the minimal entry point
Current:
- `LotrSystem.cs`

Target:
- `src/LotrModSystem.cs`

Decision:
- either keep `LotrSystem` temporarily and expand it
- or replace it with `LotrModSystem` under `src/`

Recommended:
- migrate to `src/LotrModSystem.cs` to match the imported design intent and keep all code under `src/`

#### 0.4 Establish coding constants
Create constants/config definitions for:
- mod id `lotr`
- log prefix `[LOTR]`
- faction ids
- dialogue ids
- quest ids naming conventions
- region ids

#### 0.5 Define backlog documents
Add:
- `docs/design/factions.md`
- `docs/design/quests.md`
- `docs/design/regions.md`
- `docs/design/entities.md`
- `docs/testing/manual-test-checklist.md`

### Verification
- `dotnet build lotr.csproj -c Debug` succeeds
- docs no longer instruct use of `lotrmod` as the active mod id
- docs use `/Applications/Vintage Story.app`
- no stale `lotrmod.*` files remain in build/deploy output

---

## Phase 1 — Shire MVP (first playable slice)

### Objectives
Create the first genuinely playable LOTR content slice with:
- a stable mod entry point
- Frodo and Bilbo as basic NPCs
- one region marker block
- one signature item
- minimal translations
- manual spawn/testing workflow

### Deliverables
- `src/LotrModSystem.cs`
- `src/Entities/Humanoids/EntityHobbit.cs`
- `assets/lotr/entities/humanoid/frodo.json`
- `assets/lotr/entities/humanoid/bilbo.json`
- `assets/lotr/blocktypes/shire-grass.json`
- `src/Items/ItemLembas.cs`
- `assets/lotr/itemtypes/lembas.json`
- expanded `assets/lotr/lang/en.json`

### Tasks

#### 1.1 Expand the mod entry point
Responsibilities:
- startup logging
- registration of custom items/blocks/entities/tasks
- registration of any server systems introduced in this phase

#### 1.2 Implement a reusable Hobbit base class
Behavior for MVP:
- loads successfully
- can exist as a small humanoid NPC
- optional debug logging only
- no complex combat or quest logic yet

#### 1.3 Add Frodo and Bilbo JSON definitions
Requirements:
- `code` values use `lotr:frodo` and `lotr:bilbo`
- smaller hitbox per imported design intent
- simple wander/look-at-player style behavior
- faction attribute points to Shire

#### 1.4 Add Shire Grass block
Purpose:
- visual marker for early region identity
- useful for testing assets and world placement

#### 1.5 Add Lembas item
MVP behavior:
- loads as a usable food item
- can later gain buffs/effects in more advanced phases

Recommended incremental implementation:
- phase 1a: item loads and has translation/icon
- phase 1b: food values work
- phase 1c: custom buff/effect logic is added only after the item framework is stable

#### 1.6 Expand translation assets
At minimum:
- mod name
- Frodo
- Bilbo
- Lembas
- Shire Grass
- debug/admin text if any commands are added

#### 1.7 Create a basic manual test checklist
Cover:
- mod loads cleanly
- no asset errors on startup
- spawn commands work
- entities render or at least load without crashing
- item/block names localize correctly

### In-game verification targets
- mod appears in the mod list
- `/entity spawn lotr:frodo`
- `/entity spawn lotr:bilbo`
- custom item loads without asset error
- custom block loads without asset error
- `/reload` works without crashing the save

### Exit criteria
The project is now more than a scaffold: it visibly contains LOTR-themed content and can be iterated on in-game.

---

## Phase 2 — Core gameplay systems foundation

### Objectives
Build the reusable systems that all future content depends on:
- factions
n- alignment
- quest state persistence
- dialogue framework
- region metadata

### Deliverables
- `src/Systems/Alignment/AlignmentSystem.cs`
- `src/Systems/Factions/FactionSystem.cs`
- `src/Systems/Quests/QuestSystem.cs`
- `src/Systems/Dialogue/DialogueSystem.cs`
- `src/Systems/Regions/RegionSystem.cs`
- initial JSON/config assets under `assets/lotr/config/`

### Tasks

#### 2.1 Implement faction definitions
Initial faction list from imported docs, migrated to `lotr:` ids:
- `lotr:faction-shire`
- `lotr:faction-gondor`
- `lotr:faction-rohan`
- `lotr:faction-rivendell`
- `lotr:faction-moria`
- `lotr:faction-isengard`
- `lotr:faction-mordor`

Each faction should define:
- display name key
- hostility defaults
- alignment defaults
- optional ally/enemy metadata
- region association

#### 2.2 Implement alignment persistence
Requirements:
- per-player storage
- clamped range: `-1000 .. +1000`
- helpers for set/get/modify
- alignment labels and thresholds

Potential thresholds:
- Exalted
- Revered
- Honored
- Friendly
- Unfriendly
- Hostile
- Hated
- Nemesis

#### 2.3 Refactor faction logic to consume alignment, not duplicate it
Important architectural note:
The imported starter logic creates `AlignmentSystem` inside `FactionSystem`. That is acceptable as a placeholder but not as final architecture.

Final direction:
- one authoritative alignment service
- faction logic reads from it or is injected with it
- no duplicate initialization side effects

#### 2.4 Implement quest state storage model
Need support for:
- active quests
- completed quests
- failed quests
- quest progress counters
- prerequisite checks
- faction/alignment reward hooks

Even if only one quest exists initially, the storage model should be generic.

#### 2.5 Implement dialogue framework skeleton
Not full UI first; start with data structures and triggers.

Dialogue system should support:
- NPC dialogue id lookup
- conditional lines by quest state
- conditional lines by alignment or faction standing
- reward/quest triggers
- future branching support

#### 2.6 Add debug/admin commands
Useful commands:
- inspect player alignment
- set alignment
- grant/complete/reset quest
- print active region id
- spawn important NPCs

### Verification
- server starts without null-reference issues
- new players get default faction alignment values
- alignment changes persist across rejoin/reload
- one debug command proves each system is functional

---

## Phase 3 — First narrative slice

### Objectives
Move from static content to narrative gameplay.

### Deliverables
- Gandalf entity
- first dialogue definitions
- first quest chain
- first reward loop tied to alignment

### Suggested quest slice
A practical first quest chain:
1. Speak with Bilbo or Frodo in the Shire
2. Receive a simple delivery/exploration/gathering objective
3. Return to quest giver
4. Gain Shire alignment and a themed reward
5. Gandalf unlocks as a more important guide NPC after completion

### Tasks

#### 3.1 Add Gandalf entity
Need:
- custom class `EntityGandalf`
- JSON definition under `assets/lotr/entities/humanoid/`
- faction and dialogue attributes
- optional protection/immunity rules later

#### 3.2 Implement quest giver AI task
The imported notes propose a `questgiver` AI task.
That is a reasonable path, but it should be implemented carefully:
- only run on server side
- avoid brittle pseudo-timers inside `ShouldExecute()`
- clearly separate proximity detection from dialogue/quest activation

#### 3.3 Add first dialogue files
Examples:
- `assets/lotr/dialogue/frodo.json`
- `assets/lotr/dialogue/bilbo.json`
- `assets/lotr/dialogue/gandalf.json`

#### 3.4 Add first quest data
Examples:
- `assets/lotr/config/quests/shire-introduction.json`
- `assets/lotr/config/quests/road-from-the-shire.json`

#### 3.5 Add first reward integration
Rewards may include:
- alignment increase with Shire
- item reward such as Lembas
- unlock of next dialogue state

### Verification
- player can talk to at least one named NPC
- player can start and finish one quest
- quest completion persists after reload
- reward and alignment update correctly

---

## Phase 4 — Region and world generation framework

### Objectives
Lay the technical foundation for Middle-earth geography instead of hardcoding content into one test area.

### Inputs from imported docs
The notes provide relative region placements such as:
- Shire
- Rivendell
- Moria
- Lothlorien
- Edoras/Rohan
- Minas Tirith
- Mordor
- Isengard

These coordinates should not be hardcoded directly into gameplay code yet, but they are useful as design references.

### Deliverables
- `src/WorldGen/MiddleEarthWorldGen.cs`
- `src/WorldGen/StructurePlacer.cs`
- region config assets
- structure registration/loading pipeline
- terrain/placement rules

### Tasks

#### 4.1 Define region metadata
Each region should define:
- id
- biome flavor
- preferred terrain
- palette/theme blocks
- encounter tables
- structures allowed
- safe/neutral/hostile tag

#### 4.2 Create region assignment logic
Need a way to answer:
- what region is this position in?
- what faction owns this region?
- what spawn table should apply here?

#### 4.3 Implement structure placement pipeline
Target structures:
- Hobbit hole
- watch post
- ruins
- region markers

Later structures:
- Edoras
- Minas Tirith
- Orthanc
- Moria halls
- Barad-dur

#### 4.4 Create content-friendly worldgen rules
Worldgen should be authored in a way that supports later tuning by data, not only by code.

#### 4.5 Add schematic workflow docs
Document how to:
- build a structure in creative
- save schematic
- place/test schematic
- tune spawn/placement constraints

### Verification
- region system returns sensible ids in debug mode
- at least one test structure can generate or be placed programmatically
- no severe chunk generation lag or crashes

---

## Phase 5 — Major content packs by region

This phase should be broken into separate mini-releases. Do not implement all regions at once.

### 5A — The Shire expansion
Add:
- more hobbit NPC variants
- decorative blocks and props
- food items
- village generation improvements
- calmer ambient encounter table

### 5B — Rivendell / Lothlorien pack
Add:
- elf NPCs
- elegant architecture assets
- bows, cloaks, elven food/gear
- neutral/high-alignment interactions
- specialized flora/lighting/ambience

### 5C — Moria pack
Add:
- dwarf-associated remnants or living dwarves depending on chosen lore direction
- underground structures
- hostile cave encounters
- mithril-themed materials
- first dungeon-like progression segment

### 5D — Rohan pack
Add:
- horse-oriented NPC culture if practical within VS systems
- patrol routes
- mead hall / settlement generation
- Rohirrim faction logic

### 5E — Gondor pack
Add:
- human soldier NPCs
- white-stone architecture set
- Minas Tirith as long-term capstone structure
- stronger questline progression hub

### 5F — Isengard pack
Add:
- industrial/war-torn region theming
- Uruk-hai enemies
- patrol/aggression behavior improvements
- siege or outpost structures

### 5G — Mordor pack
Add:
- harsh hostile biome theming
- orcs, trolls, elite enemies
- Mount Doom / volcanic region work
- endgame threat loop

### Verification per region pack
Each region pack must add all of the following together:
- region identity
- at least one structure
- at least one NPC or enemy type
- at least one block/item set
- translations
- faction/alignment hooks if applicable

---

## Phase 6 — Combat, enemies, bosses, and progression

### Objectives
Give the world danger, stakes, and reward loops.

### Deliverables
- hostile AI variants
- enemy factions
- combat items/gear
- boss encounter framework
- progression rewards

### Enemy ladder
Suggested escalation:
1. low-tier orcs
2. stronger Mordor orcs
3. Uruk-hai
4. trolls
5. elite captains
6. Balrog or major boss encounter

### Key tasks

#### 6.1 Hostility integration
NPCs and enemies should react based on:
- faction ownership
- player alignment
- quest state
- region danger level

#### 6.2 Equipment and special items
Major items to eventually support:
- Sting
- One Ring
- mithril gear
- Gondor/Rohan/Elven weapons and armor
- faction-specific food or utility items

#### 6.3 Boss framework
Need generic support for:
- custom health/phase logic
- dialogue/introduction hooks
- reward drops
- area-specific encounter conditions

#### 6.4 Reward/progression loop
Possible systems:
- alignment unlocks
- special crafting recipes
- region access recommendations
- rare item acquisition
- quest chain advancement

### Verification
- enemies spawn and behave predictably
- faction hostility reacts to alignment thresholds
- boss encounters are testable with admin commands

---

## Phase 7 — UI, UX, and presentation

### Objectives
Make the mod legible and polished instead of only technically functional.

### Areas
- localized text quality
- quest text quality
- alignment feedback UI/messages
- dialogue readability
- region discovery notifications
- lore-friendly item/block naming
- audio and ambience hooks

### Tasks
- add `uk.json` alongside `en.json`
- standardize naming style for all entities/items/blocks
- add clear log messages with `[LOTR]`
- reduce debug spam before release
- add simple player-facing feedback when alignment changes or quests progress

---

## Phase 8 — Performance, testing, packaging, and release

### Objectives
Make the mod safe to distribute and maintain.

### Testing layers

#### Manual smoke tests
For every release candidate:
- mod loads in singleplayer
- mod loads on server
- `/reload` works
- no missing assets
- no missing translations on core content
- no stale renamed assemblies

#### Scenario tests
- new world creation
- new player join
- old save reload
- quest completion and save persistence
- region traversal
- structure placement stress test

#### Regression tests
Keep a checklist for previously fixed failures such as:
- wrong asset prefix (`lotrmod:` vs `lotr:`)
- wrong game path
- missing entity registration
- stale `lotrmod.dll` deployment

### Packaging
Short term:
- continue loose-file deployment for development

Medium term:
- add repeatable packaging flow for release zips
- ensure versioned release artifacts
- validate manifest fields before packaging

### Release checklist
- version bump in `modinfo.json`
- changelog entry
- clean build
- deploy test
- translation sanity pass
- multiplayer sanity pass if supported
- archive package

---

## 10. Concrete implementation backlog

## 10.1 Core code backlog
High priority:
- `src/LotrModSystem.cs`
- `src/Utilities/LotrConstants.cs`
- `src/Systems/Alignment/AlignmentSystem.cs`
- `src/Systems/Factions/FactionSystem.cs`
- `src/Systems/Quests/QuestSystem.cs`
- `src/Systems/Dialogue/DialogueSystem.cs`
- `src/Commands/LotrDebugCommands.cs`

## 10.2 Entity backlog
Friendly/neutral NPCs:
- Frodo
- Bilbo
- Gandalf
- Aragorn
- Legolas
- Gimli
- Boromir
- generic Hobbit
- generic Gondor guard
- generic Rohirrim rider/guard
- generic Elf
- generic Dwarf

Hostile NPCs/enemies:
- Mordor orc
- Uruk-hai
- troll
- elite captain
- Balrog

## 10.3 Block backlog
- Shire Grass
- Hobbit house decorative blocks
- Gondor stone variants
- Rohan wood/thatch set
- Isengard industrial stone/metal set
- Mordor ash/black stone set
- mithril block/material set

## 10.4 Item backlog
- Lembas
- Sting
- One Ring
- mithril ingot
- mithril armor pieces
- faction banners/tokens
- books/maps/quest items

## 10.5 Worldgen backlog
- Hobbit hole schematic
- Shire village pieces
- Rivendell decorative structures
- Moria halls/entrances
- Rohan hall/outpost
- Minas Tirith tier pieces
- Isengard tower/outworks
- Mordor fortress/outpost
- Mount Doom landmark

## 10.6 Content data backlog
- dialogues
- quest definitions
- tradelists
- spawn conditions
- region configs
- structure configs
- translation entries

---

## 11. Recommended implementation order inside the current repo

If development starts immediately, this is the best order:

1. Normalize imported docs to `lotr`
2. Create `src/` tree
3. Replace `LotrSystem.cs` with a real `LotrModSystem.cs`
4. Add constants and logging conventions
5. Add Frodo/Bilbo content
6. Add Shire Grass
7. Add Lembas
8. Expand translations
9. Add alignment system
10. Add faction system
11. Add dialogue system skeleton
12. Add quest system skeleton
13. Add Gandalf
14. Add first quest chain
15. Add region system
16. Add first structure pipeline
17. Expand into second region

This sequence gives the fastest path to a playable prototype without locking the project into bad architecture.

---

## 12. File-by-file immediate next actions

These are the best immediate edits for the next work session.

### Update existing files
- `/Users/max/Projects/lotr/AGENT_CONTEXT.md`
- `/Users/max/Projects/lotr/LOTR_MOD_SETUP.md`
- `/Users/max/Projects/lotr/STARTER_FILES.md`
- `/Users/max/Projects/lotr/modinfo.json`
- `/Users/max/Projects/lotr/assets/lotr/lang/en.json`

### Create near-term code files
- `/Users/max/Projects/lotr/src/LotrModSystem.cs`
- `/Users/max/Projects/lotr/src/Utilities/LotrConstants.cs`
- `/Users/max/Projects/lotr/src/Entities/Humanoids/EntityHobbit.cs`
- `/Users/max/Projects/lotr/src/Items/ItemLembas.cs`
- `/Users/max/Projects/lotr/src/Systems/Alignment/AlignmentSystem.cs`
- `/Users/max/Projects/lotr/src/Systems/Factions/FactionSystem.cs`
- `/Users/max/Projects/lotr/src/Systems/Quests/QuestSystem.cs`

### Create near-term asset files
- `/Users/max/Projects/lotr/assets/lotr/entities/humanoid/frodo.json`
- `/Users/max/Projects/lotr/assets/lotr/entities/humanoid/bilbo.json`
- `/Users/max/Projects/lotr/assets/lotr/entities/humanoid/gandalf.json`
- `/Users/max/Projects/lotr/assets/lotr/blocktypes/shire-grass.json`
- `/Users/max/Projects/lotr/assets/lotr/itemtypes/lembas.json`
- `/Users/max/Projects/lotr/assets/lotr/dialogue/frodo.json`
- `/Users/max/Projects/lotr/assets/lotr/dialogue/bilbo.json`
- `/Users/max/Projects/lotr/assets/lotr/config/factions.json`

---

## 13. Risks and pitfalls to actively avoid

1. Reintroducing `lotrmod` ids into new content
2. Mixing client-only and server-only logic
3. Building too many regions before core systems stabilize
4. Hardcoding too much worldgen logic in C# instead of config/data
5. Adding many entities before a stable dialogue/quest/faction framework exists
6. Keeping outdated documentation that causes future wrong generations
7. Forgetting to clean stale renamed artifacts after refactors
8. Making AI tasks depend on fragile timing logic without proper lifecycle design
9. Overcommitting to giant structures too early before structure tooling is proven
10. Treating lore scope as more important than technical iteration speed

---

## 14. Definition of success by milestone

### Success after Phase 1
- The mod feels real, not just scaffolded
- The Shire exists as an identifiable concept in-game
- Frodo/Bilbo and at least one themed item/block are testable

### Success after Phase 2
- Core systems are reusable and stable
- future regions can plug into shared faction/alignment/quest architecture

### Success after Phase 4
- The mod can express Middle-earth geographically, not just via spawned NPCs

### Success after Phase 6
- The mod has exploration, danger, rewards, and progression loops

### Success after Phase 8
- The mod can be versioned, tested, packaged, and shared safely

---

## 15. Recommended next milestone to execute now

The best next milestone is:

`Phase 0 + Phase 1 combined`

Meaning:
- first normalize docs and naming
- then immediately build the Shire MVP

That gives the project both correctness and momentum.

---

## 16. Practical dev commands

Build:
```bash
cd /Users/max/Projects/lotr
dotnet build lotr.csproj -c Debug
```

Deploy loose files:
```bash
rm -rf "/Users/max/Library/Application Support/VintagestoryData/Mods/lotr"
cp -R "/Users/max/Projects/lotr/bin/Debug/Mods/mod" \
      "/Users/max/Library/Application Support/VintagestoryData/Mods/lotr"
```

Check stale rename artifacts:
```bash
find "/Users/max/Projects/lotr/bin/Debug/Mods/mod" -maxdepth 3 \( -name 'lotrmod.dll' -o -name 'lotrmod.deps.json' -o -name 'lotrmod.pdb' \)
find "/Users/max/Library/Application Support/VintagestoryData/Mods/lotr" -maxdepth 3 \( -name 'lotrmod.dll' -o -name 'lotrmod.deps.json' -o -name 'lotrmod.pdb' \)
```

---

## 17. Final recommendation

Do not start with Minas Tirith, Mordor, or full Middle-earth worldgen.
Start with a disciplined vertical slice:
- clean docs
- stable entry point
- two Hobbit NPCs
- one Shire block
- one signature food item
- one translation pass
- one debug/test workflow

If that slice is built cleanly, the rest of the mod can scale without chaos.

If it is skipped, the project will accumulate naming drift, broken assumptions, and content debt very quickly.
