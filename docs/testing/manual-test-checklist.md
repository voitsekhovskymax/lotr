# Manual Test Checklist

## Phase 0 — Foundation

- [ ] `dotnet build lotr.csproj -c Debug` succeeds with zero errors
- [ ] No stale `lotrmod.*` files in `bin/Debug/Mods/mod/`
- [ ] No stale `lotrmod.*` files in `/Users/max/Library/Application Support/VintagestoryData/Mods/lotr/`
- [ ] Mod loads in game (appears in mod list)
- [ ] Log shows `[LOTR] Initializing Middle-earth...`

## Phase 1 — Shire MVP

- [ ] `/entity spawn lotr:frodo` — entity spawns without crash
- [ ] `/entity spawn lotr:bilbo` — entity spawns without crash
- [ ] Frodo and Bilbo have correct smaller hitbox (visible via debug wireframe)
- [ ] Lembas item appears in creative inventory
- [ ] Shire Grass block appears in creative inventory
- [ ] `en.json` translations display correctly for all Phase 1 content
- [ ] `/reload` does not crash the save

## Build Verification

Before any deploy, run:
```bash
cd /Users/max/Projects/lotr
dotnet build lotr.csproj -c Debug
ls bin/Debug/Mods/mod/
# Should show: lotr.dll  lotr.deps.json  lotr.pdb  (NOT lotrmod.*)
```
