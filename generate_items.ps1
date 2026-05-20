$factions = @(
    # Gondor
    @{ name = "gondor"; group = "gondor"; prefix = "gondorian"; type = "human"; desc = "Gondorian" },
    # Rohan
    @{ name = "rohan"; group = "rohan"; prefix = "rohirrim"; type = "human"; desc = "Rohirrim" },
    # Bree
    @{ name = "bree"; group = "bree"; prefix = "bree"; type = "hobbit-human"; desc = "Bree" },
    # Dale
    @{ name = "dale"; group = "dale"; prefix = "dale"; type = "human"; desc = "Dale" },
    # Esgaroth
    @{ name = "esgaroth"; group = "esgaroth"; prefix = "laketown"; type = "human"; desc = "Lake-town" },
    # Beornings
    @{ name = "beornings"; group = "beornings"; prefix = "beorning"; type = "beorning"; desc = "Beorning" },
    # Rangers of the North
    @{ name = "rangers-north"; group = "rangers-north"; prefix = "ranger"; type = "human"; desc = "Ranger" },
    # Druedain
    @{ name = "druedain"; group = "druedain"; prefix = "wose"; type = "primitive"; desc = "Wose" },
    
    # Elves
    @{ name = "rivendell"; group = "rivendell"; prefix = "rivendell-elf"; type = "elf"; desc = "Rivendell Elf" },
    @{ name = "lothlorien"; group = "lothlorien"; prefix = "galadhrim"; type = "elf"; desc = "Galadhrim" },
    @{ name = "mirkwood"; group = "mirkwood"; prefix = "wood-elf"; type = "elf"; desc = "Mirkwood Elf" },
    @{ name = "lindon"; group = "lindon"; prefix = "lindon-elf"; type = "elf"; desc = "Lindon Elf" },

    # Dwarves
    @{ name = "erebor"; group = "erebor"; prefix = "erebor-dwarf"; type = "dwarf"; desc = "Erebor Dwarf" },
    @{ name = "iron-hills"; group = "iron-hills"; prefix = "iron-hills-dwarf"; type = "dwarf"; desc = "Iron Hills Dwarf" },
    @{ name = "ered-luin"; group = "ered-luin"; prefix = "ered-luin-dwarf"; type = "dwarf"; desc = "Blue Mountains Dwarf" },
    @{ name = "moria"; group = "moria"; prefix = "moria-dwarf"; type = "dwarf"; desc = "Moria Dwarf" },

    # Shire
    @{ name = "shire"; group = "shire"; prefix = "shire"; type = "hobbit"; desc = "Shire Hobbit" },

    # Evil
    @{ name = "mordor"; group = "mordor"; prefix = "mordor-orc"; type = "orc"; desc = "Mordor Orc" },
    @{ name = "mordor-uruks"; group = "mordor-uruks"; prefix = "black-uruk"; type = "uruk"; desc = "Black Uruk" },
    @{ name = "isengard"; group = "isengard"; prefix = "isengard-uruk"; type = "uruk"; desc = "Isengard Uruk-hai" },
    @{ name = "isengard-orcs"; group = "isengard-orcs"; prefix = "isengard-orc"; type = "orc"; desc = "Isengard Orc" },
    @{ name = "dunland"; group = "dunland"; prefix = "dunlending"; type = "human"; desc = "Dunlending" },
    @{ name = "half-orcs"; group = "half-orcs"; prefix = "half-orc"; type = "uruk"; desc = "Half-orc" },
    @{ name = "goblins-moria"; group = "goblins-moria"; prefix = "moria-goblin"; type = "goblin"; desc = "Moria Goblin" },
    @{ name = "gundabad"; group = "gundabad"; prefix = "gundabad-orc"; type = "orc"; desc = "Gundabad Orc" },
    @{ name = "goblin-town"; group = "goblin-town"; prefix = "goblin-town-goblin"; type = "goblin"; desc = "Goblin-town Goblin" },
    @{ name = "angmar"; group = "angmar"; prefix = "angmar-orc"; type = "orc"; desc = "Angmar Orc" },

    # Evil Men
    @{ name = "harad"; group = "harad"; prefix = "haradrim"; type = "human"; desc = "Haradrim" },
    @{ name = "rhun"; group = "rhun"; prefix = "easterling"; type = "human"; desc = "Easterling" },
    @{ name = "umbar"; group = "umbar"; prefix = "corsair"; type = "human"; desc = "Umbar Corsair" }
)

$basePath = "d:\projects\lotr\lotr\assets\lotr\itemtypes"

foreach ($f in $factions) {
    $dir = Join-Path $basePath $f.group
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }

    # Set parameters based on faction type
    $durabilityMult = 1.0
    $speedMult = 1.0
    $tier = 2
    $dmg = 4.0
    
    if ($f.type -eq "dwarf") {
        $durabilityMult = 2.0
        $speedMult = 1.2
        $tier = 3
        $dmg = 4.5
    } elseif ($f.type -eq "elf") {
        $durabilityMult = 1.5
        $speedMult = 1.3
        $tier = 3
        $dmg = 5.0
    } elseif ($f.type -eq "human" -or $f.type -eq "uruk") {
        $durabilityMult = 1.2
        $speedMult = 1.0
        $tier = 2
        $dmg = 4.5
    } elseif ($f.type -eq "beorning") {
        $durabilityMult = 1.3
        $speedMult = 1.1
        $tier = 2
        $dmg = 4.8
    } elseif ($f.type -eq "hobbit" -or $f.type -eq "hobbit-human") {
        $durabilityMult = 0.7
        $speedMult = 0.9
        $tier = 1
        $dmg = 3.0
    } elseif ($f.type -eq "orc" -or $f.type -eq "goblin") {
        $durabilityMult = 0.8
        $speedMult = 1.1
        $tier = 2
        $dmg = 4.0
        if ($f.type -eq "goblin") {
            $durabilityMult = 0.5
            $tier = 1
            $dmg = 3.5
        }
    } elseif ($f.type -eq "primitive") {
        $durabilityMult = 0.4
        $speedMult = 0.7
        $tier = 1
        $dmg = 3.5
    }

    # 1. Axe
    $axeDur = [int](350 * $durabilityMult)
    $axeSpeed = 3.0 * $speedMult
    $axeTier = $tier
    if ($f.type -eq "dwarf") { $axeTier = 3 }
    
    $json = @"
{
  "code": "$($f.prefix)-axe",
  "class": "ItemTool",
  "durability": $axeDur,
  "tool": "axe",
  "miningspeed": {
    "wood": $($axeSpeed),
    "leaves": $($axeSpeed * 0.7)
  },
  "attributes": {
    "tooltier": $axeTier,
    "attackpower": $($dmg * 0.4),
    "attackrange": 1.0
  },
  "textures": {
    "all": { "base": "game:item/tool/axe" }
  },
  "creativeInventory": {
    "lotr": ["*"]
  }
}
"@
    Set-Content -Path (Join-Path $dir "$($f.prefix)-axe.json") -Value $json -Encoding UTF8

    # 2. Pickaxe
    $pickDur = [int](400 * $durabilityMult)
    if ($f.type -eq "dwarf") { $pickDur = [int]($pickDur * 1.5) }
    $pickSpeed = 4.0 * $speedMult
    $pickTier = $tier
    if ($f.type -eq "dwarf") { $pickTier = 4 }
    
    $json = @"
{
  "code": "$($f.prefix)-pickaxe",
  "class": "ItemTool",
  "durability": $pickDur,
  "tool": "pickaxe",
  "miningspeed": {
    "stone": $($pickSpeed),
    "ore": $($pickSpeed),
    "basemetal": $($pickSpeed * 0.8)
  },
  "attributes": {
    "tooltier": $pickTier,
    "attackpower": $($dmg * 0.3),
    "attackrange": 1.0
  },
  "textures": {
    "all": { "base": "game:item/tool/pickaxe" }
  },
  "creativeInventory": {
    "lotr": ["*"]
  }
}
"@
    Set-Content -Path (Join-Path $dir "$($f.prefix)-pickaxe.json") -Value $json -Encoding UTF8

    # 3. Knife
    $knifeDur = [int](150 * $durabilityMult)
    if ($f.type -eq "elf") { $knifeDur = [int]($knifeDur * 1.8) }
    $knifeSpeed = 3.0 * $speedMult
    $knifeDmg = $($dmg * 0.3)
    if ($f.type -eq "elf") { $knifeDmg = $($dmg * 0.5) }
    
    $json = @"
{
  "code": "$($f.prefix)-knife",
  "class": "ItemTool",
  "durability": $knifeDur,
  "tool": "knife",
  "miningspeed": {
    "plant": $($knifeSpeed),
    "cloth": $($knifeSpeed * 0.8)
  },
  "attributes": {
    "tooltier": $tier,
    "attackpower": $($knifeDmg),
    "attackrange": 0.8
  },
  "textures": {
    "all": { "base": "game:item/tool/knife" }
  },
  "creativeInventory": {
    "lotr": ["*"]
  }
}
"@
    Set-Content -Path (Join-Path $dir "$($f.prefix)-knife.json") -Value $json -Encoding UTF8

    # 4. Saw
    $sawDur = [int](250 * $durabilityMult)
    $sawSpeed = 2.5 * $speedMult
    
    $json = @"
{
  "code": "$($f.prefix)-saw",
  "class": "ItemTool",
  "durability": $sawDur,
  "tool": "saw",
  "miningspeed": {
    "wood": $($sawSpeed)
  },
  "attributes": {
    "tooltier": $tier
  },
  "textures": {
    "all": { "base": "game:item/tool/saw" }
  },
  "creativeInventory": {
    "lotr": ["*"]
  }
}
"@
    Set-Content -Path (Join-Path $dir "$($f.prefix)-saw.json") -Value $json -Encoding UTF8

    # 5. Sword
    $swordDur = [int](600 * $durabilityMult)
    if ($f.name -eq "gondor" -or $f.name -eq "mordor" -or $f.name -eq "mordor-uruks" -or $f.name -eq "isengard") {
        $swordDur = [int]($swordDur * 1.5)
    }
    $swordDmg = $dmg * 1.2
    $swordRange = 1.8
    if ($f.type -eq "elf") { $swordRange = 2.0 }
    if ($f.type -eq "hobbit") { $swordRange = 1.3 }
    
    $json = @"
{
  "code": "$($f.prefix)-sword",
  "class": "ItemTool",
  "durability": $swordDur,
  "tool": "sword",
  "attributes": {
    "attackpower": $($swordDmg),
    "attackrange": $($swordRange),
    "tooltier": $tier
  },
  "textures": {
    "all": { "base": "game:item/tool/sword" }
  },
  "creativeInventory": {
    "lotr": ["*"]
  }
}
"@
    Set-Content -Path (Join-Path $dir "$($f.prefix)-sword.json") -Value $json -Encoding UTF8

    # 6. Shovel
    $shovDur = [int](300 * $durabilityMult)
    if ($f.type -eq "hobbit") { $shovDur = [int]($shovDur * 2.0) }
    $shovSpeed = 3.5 * $speedMult
    if ($f.type -eq "hobbit") { $shovSpeed = $shovSpeed * 1.5 }
    
    $json = @"
{
  "code": "$($f.prefix)-shovel",
  "class": "ItemTool",
  "durability": $shovDur,
  "tool": "shovel",
  "miningspeed": {
    "soil": $($shovSpeed),
    "gravel": $($shovSpeed * 0.8),
    "sand": $($shovSpeed * 0.8)
  },
  "attributes": {
    "tooltier": $tier
  },
  "textures": {
    "all": { "base": "game:item/tool/shovel" }
  },
  "creativeInventory": {
    "lotr": ["*"]
  }
}
"@
    Set-Content -Path (Join-Path $dir "$($f.prefix)-shovel.json") -Value $json -Encoding UTF8
}

# Generate Legendary Items
$legendaryDir = Join-Path $basePath "legendary"
if (-not (Test-Path $legendaryDir)) {
    New-Item -ItemType Directory -Force -Path $legendaryDir | Out-Null
}

$legendaries = @(
    @{ code = "sting"; dmg = 5.0; range = 1.4; dur = 2500; desc = "Sting" },
    @{ code = "anduril"; dmg = 8.5; range = 2.2; dur = 5000; desc = "Anduril" },
    @{ code = "glamdring"; dmg = 7.5; range = 2.0; dur = 4000; desc = "Glamdring" },
    @{ code = "orcrist"; dmg = 7.5; range = 2.0; dur = 4000; desc = "Orcrist" },
    @{ code = "herugrim"; dmg = 6.8; range = 1.9; dur = 3200; desc = "Herugrim" },
    @{ code = "gurthang"; dmg = 9.5; range = 2.1; dur = 4500; desc = "Gurthang" },
    @{ code = "durin-axe"; dmg = 9.0; range = 1.8; dur = 6000; desc = "Axe of Durin"; tool = "axe" },
    @{ code = "morgul-blade"; dmg = 4.0; range = 1.3; dur = 500; desc = "Morgul Blade" },
    @{ code = "aeglos"; dmg = 8.0; range = 2.5; dur = 3800; desc = "Aeglos" }
)

foreach ($l in $legendaries) {
    $toolType = "sword"
    if ($l.tool) { $toolType = $l.tool }
    
    $json = @"
{
  "code": "$($l.code)",
  "class": "ItemTool",
  "durability": $($l.dur),
  "tool": "$toolType",
  "attributes": {
    "attackpower": $($l.dmg),
    "attackrange": $($l.range),
    "tooltier": 5,
    "isLegendary": true
  },
  "textures": {
    "all": { "base": "game:item/tool/sword" }
  },
  "creativeInventory": {
    "lotr": ["*"]
  }
}
"@
    Set-Content -Path (Join-Path $legendaryDir "$($l.code).json") -Value $json -Encoding UTF8
}
