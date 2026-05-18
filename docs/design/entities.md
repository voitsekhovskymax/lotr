# Entities Design

## Planned Entity List

### Hobbits (Phase 1)
- `lotr:frodo` — class EntityHobbit, hitbox 0.4x1.1
- `lotr:bilbo` — class EntityHobbit, hitbox 0.4x1.1

### Maiar (Phase 3)
- `lotr:gandalf` — class EntityGandalf, hitbox 0.6x2.0, eyeHeight 1.85

### Humans (Phase 3+)
- `lotr:aragorn` — class EntityHuman, hitbox 0.6x1.9
- `lotr:boromir` — class EntityHuman, hitbox 0.6x1.9

### Elves (Phase 3+)
- `lotr:legolas` — class EntityElf, hitbox 0.55x2.1

### Dwarves (Phase 3+)
- `lotr:gimli` — class EntityDwarf, hitbox 0.5x1.4

### Enemies (Phase 6)
- `lotr:orc-mordor` — hostile
- `lotr:uruk-hai` — hostile
- `lotr:troll` — boss-level, hitbox 1.5x4.0

### Bosses (Phase 6)
- `lotr:balrog` — boss, hitbox 3.0x8.0

## Entity Size Reference

| Type | hitbox x | hitbox y | eyeHeight |
|---|---|---|---|
| Hobbit | 0.4 | 1.1 | 0.95 |
| Dwarf | 0.5 | 1.4 | 1.2 |
| Human | 0.6 | 1.9 | 1.7 |
| Elf | 0.55 | 2.1 | 1.95 |
| Maiar | 0.6 | 2.0 | 1.85 |
| Troll | 1.5 | 4.0 | 3.5 |
| Balrog | 3.0 | 8.0 | 7.0 |

## AI Tasks

- `lotr:questgiver` — look at nearby player, offer quest dialogue (Phase 2+)
- `lotr:patrol` — move between defined waypoints (Phase 3+)
- Built-in tasks used: `lookatentity`, `wander`
