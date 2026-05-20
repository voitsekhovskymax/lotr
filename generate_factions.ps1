$factions = @(
    @{ folder = "gondor"; code = "gondorian"; class = "EntityHuman"; hitbox = @{x=0.6; y=1.9}; eye = 1.7; size = 1.0; health = 25; faction = "gondor" },
    @{ folder = "rohan"; code = "rohirrim"; class = "EntityHuman"; hitbox = @{x=0.6; y=1.9}; eye = 1.7; size = 1.0; health = 25; faction = "rohan" },
    @{ folder = "bree"; code = "bree-man"; class = "EntityHuman"; hitbox = @{x=0.6; y=1.9}; eye = 1.7; size = 1.0; health = 20; faction = "bree" },
    @{ folder = "dale"; code = "daleman"; class = "EntityHuman"; hitbox = @{x=0.6; y=1.9}; eye = 1.7; size = 1.0; health = 20; faction = "dale" },
    @{ folder = "esgaroth"; code = "lakeman"; class = "EntityHuman"; hitbox = @{x=0.6; y=1.9}; eye = 1.7; size = 1.0; health = 20; faction = "esgaroth" },
    @{ folder = "beornings"; code = "beorning"; class = "EntityHuman"; hitbox = @{x=0.7; y=2.2}; eye = 1.9; size = 1.15; health = 40; faction = "beornings" },
    @{ folder = "rangers-north"; code = "ranger"; class = "EntityHuman"; hitbox = @{x=0.6; y=1.9}; eye = 1.7; size = 1.0; health = 30; faction = "rangers-north" },
    @{ folder = "druedain"; code = "wose"; class = "EntityHuman"; hitbox = @{x=0.5; y=1.5}; eye = 1.3; size = 0.8; health = 20; faction = "druedain" },
    
    @{ folder = "rivendell"; code = "rivendell-elf"; class = "EntityElf"; hitbox = @{x=0.55; y=2.1}; eye = 1.95; size = 1.1; health = 30; faction = "rivendell" },
    @{ folder = "lothlorien"; code = "galadhrim"; class = "EntityElf"; hitbox = @{x=0.55; y=2.1}; eye = 1.95; size = 1.1; health = 30; faction = "lothlorien" },
    @{ folder = "mirkwood"; code = "wood-elf"; class = "EntityElf"; hitbox = @{x=0.55; y=2.1}; eye = 1.95; size = 1.1; health = 25; faction = "mirkwood" },
    @{ folder = "lindon"; code = "lindon-elf"; class = "EntityElf"; hitbox = @{x=0.55; y=2.1}; eye = 1.95; size = 1.1; health = 30; faction = "lindon" },

    @{ folder = "erebor"; code = "erebor-dwarf"; class = "EntityDwarf"; hitbox = @{x=0.5; y=1.4}; eye = 1.2; size = 0.74; health = 35; faction = "erebor" },
    @{ folder = "iron-hills"; code = "iron-hills-dwarf"; class = "EntityDwarf"; hitbox = @{x=0.5; y=1.4}; eye = 1.2; size = 0.74; health = 35; faction = "iron-hills" },
    @{ folder = "ered-luin"; code = "ered-luin-dwarf"; class = "EntityDwarf"; hitbox = @{x=0.5; y=1.4}; eye = 1.2; size = 0.74; health = 30; faction = "ered-luin" },
    @{ folder = "moria"; code = "moria-dwarf"; class = "EntityDwarf"; hitbox = @{x=0.5; y=1.4}; eye = 1.2; size = 0.74; health = 30; faction = "moria" },

    @{ folder = "shire"; code = "shire-hobbit"; class = "EntityHobbit"; hitbox = @{x=0.4; y=1.1}; eye = 0.95; size = 0.58; health = 15; faction = "shire" },

    @{ folder = "mordor"; code = "mordor-orc"; class = "EntityAgent"; hitbox = @{x=0.5; y=1.6}; eye = 1.4; size = 0.85; health = 15; faction = "mordor-orcs" },
    @{ folder = "mordor-uruks"; code = "black-uruk"; class = "EntityAgent"; hitbox = @{x=0.6; y=1.9}; eye = 1.7; size = 1.0; health = 30; faction = "mordor-uruks" },
    @{ folder = "olog-hai"; code = "olog-hai"; class = "EntityAgent"; hitbox = @{x=1.5; y=4.0}; eye = 3.5; size = 2.1; health = 150; faction = "olog-hai" },
    @{ folder = "nazgul"; code = "ringwraith"; class = "EntityAgent"; hitbox = @{x=0.6; y=2.0}; eye = 1.85; size = 1.05; health = 200; faction = "nazgul" },

    @{ folder = "isengard"; code = "isengard-uruk"; class = "EntityAgent"; hitbox = @{x=0.6; y=1.9}; eye = 1.7; size = 1.0; health = 25; faction = "isengard" },
    @{ folder = "isengard-orcs"; code = "isengard-orc"; class = "EntityAgent"; hitbox = @{x=0.5; y=1.6}; eye = 1.4; size = 0.85; health = 15; faction = "isengard-orcs" },
    @{ folder = "dunland"; code = "dunlending"; class = "EntityHuman"; hitbox = @{x=0.6; y=1.9}; eye = 1.7; size = 1.0; health = 20; faction = "dunland" },
    @{ folder = "half-orcs"; code = "half-orc"; class = "EntityAgent"; hitbox = @{x=0.6; y=1.8}; eye = 1.6; size = 0.95; health = 20; faction = "isengard-men" },

    @{ folder = "goblins-moria"; code = "moria-goblin"; class = "EntityAgent"; hitbox = @{x=0.4; y=1.4}; eye = 1.2; size = 0.74; health = 12; faction = "goblins-moria" },
    @{ folder = "gundabad"; code = "gundabad-orc"; class = "EntityAgent"; hitbox = @{x=0.5; y=1.6}; eye = 1.4; size = 0.85; health = 18; faction = "gundabad" },
    @{ folder = "goblin-town"; code = "goblin-town-goblin"; class = "EntityAgent"; hitbox = @{x=0.4; y=1.4}; eye = 1.2; size = 0.74; health = 10; faction = "goblin-town" },
    @{ folder = "angmar"; code = "angmar-orc"; class = "EntityAgent"; hitbox = @{x=0.5; y=1.6}; eye = 1.4; size = 0.85; health = 18; faction = "angmar" },

    @{ folder = "harad"; code = "haradrim"; class = "EntityHuman"; hitbox = @{x=0.6; y=1.9}; eye = 1.7; size = 1.0; health = 22; faction = "harad" },
    @{ folder = "rhun"; code = "easterling"; class = "EntityHuman"; hitbox = @{x=0.6; y=1.9}; eye = 1.7; size = 1.0; health = 22; faction = "rhun" },
    @{ folder = "umbar"; code = "corsair"; class = "EntityHuman"; hitbox = @{x=0.6; y=1.9}; eye = 1.7; size = 1.0; health = 20; faction = "umbar" },

    @{ folder = "fangorn"; code = "ent"; class = "EntityAgent"; hitbox = @{x=2.0; y=6.0}; eye = 5.0; size = 3.1; health = 300; faction = "fangorn" },
    @{ folder = "eagles"; code = "great-eagle"; class = "EntityAgent"; hitbox = @{x=2.5; y=2.0}; eye = 1.5; size = 1.5; health = 100; faction = "eagles" },
    @{ folder = "wargs"; code = "warg"; class = "EntityAgent"; hitbox = @{x=1.0; y=1.2}; eye = 1.0; size = 0.8; health = 30; faction = "wargs" },
    @{ folder = "mirkwood-spiders"; code = "giant-spider"; class = "EntityAgent"; hitbox = @{x=1.5; y=1.5}; eye = 1.0; size = 1.0; health = 40; faction = "mirkwood-spiders" },
    @{ folder = "oathbreakers"; code = "oathbreaker"; class = "EntityAgent"; hitbox = @{x=0.6; y=1.9}; eye = 1.7; size = 1.0; health = 50; faction = "oathbreakers" },
    @{ folder = "balrog"; code = "balrog"; class = "EntityAgent"; hitbox = @{x=3.0; y=8.0}; eye = 7.0; size = 4.2; health = 1000; faction = "balrog" },
    @{ folder = "bombadil"; code = "tom-bombadil"; class = "EntityAgent"; hitbox = @{x=0.6; y=1.9}; eye = 1.7; size = 1.0; health = 9999; faction = "bombadil" }
)

$basePath = "d:\projects\lotr\lotr\assets\lotr\entities"

foreach ($f in $factions) {
    $dir = Join-Path $basePath $f.folder
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }
    
    $filePath = Join-Path $dir "$($f.code).json"
    
    $json = @"
{
  "code": "$($f.code)",
  "class": "$($f.class)",
  "hitboxSize": { "x": $($f.hitbox.x), "y": $($f.hitbox.y) },
  "eyeHeight": $($f.eye),
  "client": {
    "size": $($f.size),
    "renderer": "Shape",
    "shape": { "base": "game:entity/humanoid/seraph-faceless" },
    "behaviors": [
      { "code": "repulseagents" },
      { "code": "controlledphysics", "stepHeight": 0.6 },
      { "code": "floatupwhenstuck", "onlyWhenDead": true },
      { "code": "interpolateposition" }
    ]
  },
  "server": {
    "behaviors": [
      { "code": "repulseagents" },
      { "code": "controlledphysics", "stepHeight": 0.6 },
      { "code": "health", "currenthealth": $($f.health), "maxhealth": $($f.health) },
      { "code": "taskai", "aitasks": [
        { "code": "lookaround", "priority": 0.5 },
        { "code": "wander", "priority": 0.3, "movespeed": 0.015 }
      ]}
    ]
  },
  "attributes": {
    "faction": "lotr:faction-$($f.faction)"
  }
}
"@

    Set-Content -Path $filePath -Value $json -Encoding UTF8
}
