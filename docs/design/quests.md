# Quests Design

## Overview

Quests are stored per-player and tracked as active or completed.
Quest IDs follow the format: `lotr:quest-<region>-<name>`

## Quest ID Naming

- `lotr:quest-shire-long-expected-party`
- `lotr:quest-shire-concerning-hobbits`
- `lotr:quest-rivendell-council`
- `lotr:quest-moria-fellowship`

## Quest State Storage

Stored in player WatchedAttributes:
- Active: `lotr_quest_active_<quest_id>` = true/false
- Done: `lotr_quest_done_<quest_id>` = true/false

## Phase 1 Quests (Shire MVP)

- No quests yet in Phase 1, just NPC placeholders

## Phase 2+ Quest Architecture

- `QuestSystem` manages loading, starting, progressing, completing quests
- Quest definitions live in `assets/lotr/config/quests/`
- Quest rewards can include alignment changes, items, unlocks
