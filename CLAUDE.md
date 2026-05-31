# LOTR Mod — Claude Code Project Context

## Stack
- Game: Vintage Story 1.22, .NET 10, C# 13
- Build: `dotnet run --project CakeBuild`
- Output: `Releases/lotr_*.zip` → copy to ~/Library/Application Support/VintagestoryData/Mods/

## Project Structure
- src/           C# source code
- assets/lotr JSON assets (entities, blocks, items, worldgen)
- .claude/skills/ project-specific skills

## Active Skills
- vs-api         Vintage Story API rules
- lotr-lore      Tolkien lore for dialogue and content
- blockbench-vs  3D model creation via Blockbench MCP

## Current Phase: MVP — The Shire
Focus on: Frodo + Bilbo NPCs, basic Shire terrain, hobbit hole structure

## Mod ID
Always use prefix: `lotr:` for all asset codes