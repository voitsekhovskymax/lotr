$basePath = "d:\projects\lotr\lotr\assets\lotr"

# 1. Ensure Directories exist
$dirs = @(
    "blocktypes",
    "itemtypes/metal",
    "recipes/grid"
)

foreach ($d in $dirs) {
    $path = Join-Path $basePath $d
    if (-not (Test-Path $path)) {
        New-Item -ItemType Directory -Force -Path $path | Out-Null
    }
}

# 2. Write Block Definitions

# stone-gondor
$gondorStone = @{
  code = "stone-gondor"
  class = "Block"
  material = "Stone"
  blockmaterial = "Stone"
  drawtype = "Cube"
  replaceable = 0
  resistance = 15
  sounds = @{
    walk = "game:walk/stone"
    break = "game:break/stone"
    place = "game:place/stone"
  }
  sidessolid = @{ all = $true }
  sidesopaque = @{ all = $true }
  textures = @{ all = @{ base = "game:block/stone/rock/marble" } }
  lightAbsorption = 99
  creativeInventory = @{ lotr = @("*") }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "blocktypes/stone-gondor.json") -Value $gondorStone -Encoding UTF8

# stone-mordor
$mordorStone = @{
  code = "stone-mordor"
  class = "Block"
  material = "Stone"
  blockmaterial = "Stone"
  drawtype = "Cube"
  replaceable = 0
  resistance = 20
  sounds = @{
    walk = "game:walk/stone"
    break = "game:break/stone"
    place = "game:place/stone"
  }
  sidessolid = @{ all = $true }
  sidesopaque = @{ all = $true }
  textures = @{ all = @{ base = "game:block/stone/rock/basalt" } }
  lightAbsorption = 99
  creativeInventory = @{ lotr = @("*") }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "blocktypes/stone-mordor.json") -Value $mordorStone -Encoding UTF8

# stone-orthanc
$orthancStone = @{
  code = "stone-orthanc"
  class = "Block"
  material = "Stone"
  blockmaterial = "Stone"
  drawtype = "Cube"
  replaceable = 0
  resistance = 100
  sounds = @{
    walk = "game:walk/stone"
    break = "game:break/stone"
    place = "game:place/stone"
  }
  sidessolid = @{ all = $true }
  sidesopaque = @{ all = $true }
  textures = @{ all = @{ base = "game:block/stone/rock/obsidian" } }
  lightAbsorption = 99
  creativeInventory = @{ lotr = @("*") }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "blocktypes/stone-orthanc.json") -Value $orthancStone -Encoding UTF8

# ore-mithril
$mithrilOre = @{
  code = "ore-mithril"
  class = "Block"
  material = "Stone"
  blockmaterial = "Stone"
  drawtype = "Cube"
  replaceable = 0
  resistance = 10
  sounds = @{
    walk = "game:walk/stone"
    break = "game:break/stone"
    place = "game:place/stone"
  }
  sidessolid = @{ all = $true }
  sidesopaque = @{ all = $true }
  textures = @{ all = @{ base = "game:block/stone/rock/granite" } }
  drops = @(
    @{
      type = "item"
      code = "lotr:nugget-mithril"
      quantity = @{ avg = 2; var = 1 }
    }
  )
  lightAbsorption = 99
  creativeInventory = @{ lotr = @("*") }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "blocktypes/ore-mithril.json") -Value $mithrilOre -Encoding UTF8

# ore-morguliron
$morgulOre = @{
  code = "ore-morguliron"
  class = "Block"
  material = "Stone"
  blockmaterial = "Stone"
  drawtype = "Cube"
  replaceable = 0
  resistance = 8
  sounds = @{
    walk = "game:walk/stone"
    break = "game:break/stone"
    place = "game:place/stone"
  }
  sidessolid = @{ all = $true }
  sidesopaque = @{ all = $true }
  textures = @{ all = @{ base = "game:block/stone/rock/basalt" } }
  drops = @(
    @{
      type = "item"
      code = "lotr:nugget-morguliron"
      quantity = @{ avg = 2; var = 1 }
    }
  )
  lightAbsorption = 99
  creativeInventory = @{ lotr = @("*") }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "blocktypes/ore-morguliron.json") -Value $morgulOre -Encoding UTF8


# 3. Write Custom Nuggets & Ingots

$metals = @("mithril", "morguliron", "galvorn", "dwarvensteel")

foreach ($m in $metals) {
    # Nuggets (Mithril and Morgul Iron only, others are alloyed/crafted directly into ingots)
    if ($m -eq "mithril" -or $m -eq "morguliron") {
        $nugget = @{
          code = "nugget-$m"
          class = "Item"
          maxstacksize = 64
          textures = @{ all = @{ base = "game:item/resource/nugget" } }
          creativeInventory = @{ lotr = @("*") }
        } | ConvertTo-Json -Depth 5
        Set-Content -Path (Join-Path $basePath "itemtypes/metal/nugget-$m.json") -Value $nugget -Encoding UTF8
    }

    # Ingots
    $ingot = @{
      code = "ingot-$m"
      class = "Item"
      maxstacksize = 64
      textures = @{ all = @{ base = "game:item/resource/ingot" } }
      creativeInventory = @{ lotr = @("*") }
    } | ConvertTo-Json -Depth 5
    Set-Content -Path (Join-Path $basePath "itemtypes/metal/ingot-$m.json") -Value $ingot -Encoding UTF8
}


# 4. Write Smelting/Refinement Recipes

# Mithril Ingot Recipe
$mithrilSmelt = @{
  ingredientPattern = "NNN,NNN,NNN"
  width = 3
  height = 3
  ingredients = @{
    N = @{ type = "item"; code = "lotr:nugget-mithril" }
  }
  output = @{ type = "item"; code = "lotr:ingot-mithril"; quantity = 1 }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "recipes/grid/smelt-mithril.json") -Value $mithrilSmelt -Encoding UTF8

# Morgul Iron Ingot Recipe
$morgulSmelt = @{
  ingredientPattern = "NNN,NNN,NNN"
  width = 3
  height = 3
  ingredients = @{
    N = @{ type = "item"; code = "lotr:nugget-morguliron" }
  }
  output = @{ type = "item"; code = "lotr:ingot-morguliron"; quantity = 1 }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "recipes/grid/smelt-morguliron.json") -Value $morgulSmelt -Encoding UTF8

# Galvorn Ingot Alloy Recipe (5 Iron + 3 Silver + 2 Meteoric Iron)
$galvornSmelt = @{
  ingredientPattern = "III,SSS,MMM"
  width = 3
  height = 3
  ingredients = @{
    I = @{ type = "item"; code = "game:nugget-iron" }
    S = @{ type = "item"; code = "game:nugget-silver" }
    M = @{ type = "item"; code = "game:nugget-meteoriciron" }
  }
  output = @{ type = "item"; code = "lotr:ingot-galvorn"; quantity = 1 }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "recipes/grid/smelt-galvorn.json") -Value $galvornSmelt -Encoding UTF8

# Dwarven Steel Ingot Alloy Recipe (8 Iron Nuggets + 1 Coal)
$dwarvenSmelt = @{
  ingredientPattern = "III,ICI,III"
  width = 3
  height = 3
  ingredients = @{
    I = @{ type = "item"; code = "game:nugget-iron" }
    C = @{ type = "item"; code = "game:coal-lignite" } # Accept standard coal
  }
  output = @{ type = "item"; code = "lotr:ingot-dwarvensteel"; quantity = 1 }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "recipes/grid/smelt-dwarvensteel.json") -Value $dwarvenSmelt -Encoding UTF8


# 5. Write Tool & Weapon Crafting Recipes for Factions

$factions = @(
    @{ prefix = "gondorian"; metal = "game:ingot-steel" },
    @{ prefix = "rohirrim"; metal = "game:ingot-iron" },
    @{ prefix = "bree"; metal = "game:ingot-iron" },
    @{ prefix = "dale"; metal = "game:ingot-iron" },
    @{ prefix = "laketown"; metal = "game:ingot-iron" },
    @{ prefix = "beorning"; metal = "game:ingot-iron" },
    @{ prefix = "ranger"; metal = "game:ingot-steel" },
    @{ prefix = "wose"; metal = "game:ingot-copper" },
    
    @{ prefix = "rivendell-elf"; metal = "lotr:ingot-galvorn" },
    @{ prefix = "galadhrim"; metal = "lotr:ingot-galvorn" },
    @{ prefix = "wood-elf"; metal = "lotr:ingot-galvorn" },
    @{ prefix = "lindon-elf"; metal = "lotr:ingot-galvorn" },

    @{ prefix = "erebor-dwarf"; metal = "lotr:ingot-dwarvensteel" },
    @{ prefix = "iron-hills-dwarf"; metal = "lotr:ingot-dwarvensteel" },
    @{ prefix = "ered-luin-dwarf"; metal = "lotr:ingot-dwarvensteel" },
    @{ prefix = "moria-dwarf"; metal = "lotr:ingot-dwarvensteel" },

    @{ prefix = "shire"; metal = "game:ingot-copper" },

    @{ prefix = "mordor-orc"; metal = "lotr:ingot-morguliron" },
    @{ prefix = "black-uruk"; metal = "lotr:ingot-morguliron" },
    @{ prefix = "isengard-uruk"; metal = "game:ingot-iron" },
    @{ prefix = "isengard-orc"; metal = "game:ingot-iron" },
    @{ prefix = "dunlending"; metal = "game:ingot-copper" },
    @{ prefix = "half-orc"; metal = "game:ingot-iron" },
    @{ prefix = "moria-goblin"; metal = "game:ingot-copper" },
    @{ prefix = "gundabad-orc"; metal = "game:ingot-iron" },
    @{ prefix = "goblin-town-goblin"; metal = "game:ingot-copper" },
    @{ prefix = "angmar-orc"; metal = "game:ingot-iron" },

    @{ prefix = "haradrim"; metal = "game:ingot-iron" },
    @{ prefix = "easterling"; metal = "game:ingot-iron" },
    @{ prefix = "corsair"; metal = "game:ingot-iron" }
)

foreach ($f in $factions) {
    $p = $f.prefix
    $m = $f.metal

    # Axe
    $axe = @{
      ingredientPattern = "MM,MS,_S"
      width = 2
      height = 3
      ingredients = @{
        M = @{ type = "item"; code = $m }
        S = @{ type = "item"; code = "game:stick" }
      }
      output = @{ type = "item"; code = "lotr:$p-axe"; quantity = 1 }
    } | ConvertTo-Json -Depth 5
    Set-Content -Path (Join-Path $basePath "recipes/grid/$p-axe.json") -Value $axe -Encoding UTF8

    # Pickaxe
    $pick = @{
      ingredientPattern = "MMM,_S_,_S_"
      width = 3
      height = 3
      ingredients = @{
        M = @{ type = "item"; code = $m }
        S = @{ type = "item"; code = "game:stick" }
      }
      output = @{ type = "item"; code = "lotr:$p-pickaxe"; quantity = 1 }
    } | ConvertTo-Json -Depth 5
    Set-Content -Path (Join-Path $basePath "recipes/grid/$p-pickaxe.json") -Value $pick -Encoding UTF8

    # Knife
    $knife = @{
      ingredientPattern = "M,S"
      width = 1
      height = 2
      ingredients = @{
        M = @{ type = "item"; code = $m }
        S = @{ type = "item"; code = "game:stick" }
      }
      output = @{ type = "item"; code = "lotr:$p-knife"; quantity = 1 }
    } | ConvertTo-Json -Depth 5
    Set-Content -Path (Join-Path $basePath "recipes/grid/$p-knife.json") -Value $knife -Encoding UTF8

    # Saw
    $saw = @{
      ingredientPattern = "MM,SS"
      width = 2
      height = 2
      ingredients = @{
        M = @{ type = "item"; code = $m }
        S = @{ type = "item"; code = "game:stick" }
      }
      output = @{ type = "item"; code = "lotr:$p-saw"; quantity = 1 }
    } | ConvertTo-Json -Depth 5
    Set-Content -Path (Join-Path $basePath "recipes/grid/$p-saw.json") -Value $saw -Encoding UTF8

    # Sword
    $sword = @{
      ingredientPattern = "M,M,S"
      width = 1
      height = 3
      ingredients = @{
        M = @{ type = "item"; code = $m }
        S = @{ type = "item"; code = "game:stick" }
      }
      output = @{ type = "item"; code = "lotr:$p-sword"; quantity = 1 }
    } | ConvertTo-Json -Depth 5
    Set-Content -Path (Join-Path $basePath "recipes/grid/$p-sword.json") -Value $sword -Encoding UTF8

    # Shovel
    $shovel = @{
      ingredientPattern = "M,S,S"
      width = 1
      height = 3
      ingredients = @{
        M = @{ type = "item"; code = $m }
        S = @{ type = "item"; code = "game:stick" }
      }
      output = @{ type = "item"; code = "lotr:$p-shovel"; quantity = 1 }
    } | ConvertTo-Json -Depth 5
    Set-Content -Path (Join-Path $basePath "recipes/grid/$p-shovel.json") -Value $shovel -Encoding UTF8
}


# 6. Crafting Recipes for Legendary Items

# Sting
$sting = @{
  ingredientPattern = "M,S"
  width = 1
  height = 2
  ingredients = @{
    M = @{ type = "item"; code = "lotr:ingot-mithril" }
    S = @{ type = "item"; code = "game:stick" }
  }
  output = @{ type = "item"; code = "lotr:sting"; quantity = 1 }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "recipes/grid/legendary-sting.json") -Value $sting -Encoding UTF8

# Anduril
$anduril = @{
  ingredientPattern = "M,M,S"
  width = 1
  height = 3
  ingredients = @{
    M = @{ type = "item"; code = "lotr:ingot-mithril" }
    S = @{ type = "item"; code = "game:stick" }
  }
  output = @{ type = "item"; code = "lotr:anduril"; quantity = 1 }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "recipes/grid/legendary-anduril.json") -Value $anduril -Encoding UTF8

# Glamdring
$glamdring = @{
  ingredientPattern = "M,I,S"
  width = 1
  height = 3
  ingredients = @{
    M = @{ type = "item"; code = "lotr:ingot-mithril" }
    I = @{ type = "item"; code = "game:ingot-steel" }
    S = @{ type = "item"; code = "game:stick" }
  }
  output = @{ type = "item"; code = "lotr:glamdring"; quantity = 1 }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "recipes/grid/legendary-glamdring.json") -Value $glamdring -Encoding UTF8

# Orcrist
$orcrist = @{
  ingredientPattern = "I,M,S"
  width = 1
  height = 3
  ingredients = @{
    M = @{ type = "item"; code = "lotr:ingot-mithril" }
    I = @{ type = "item"; code = "game:ingot-steel" }
    S = @{ type = "item"; code = "game:stick" }
  }
  output = @{ type = "item"; code = "lotr:orcrist"; quantity = 1 }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "recipes/grid/legendary-orcrist.json") -Value $orcrist -Encoding UTF8

# Herugrim
$herugrim = @{
  ingredientPattern = "I,I,S"
  width = 1
  height = 3
  ingredients = @{
    I = @{ type = "item"; code = "game:ingot-steel" }
    S = @{ type = "item"; code = "game:stick" }
  }
  output = @{ type = "item"; code = "lotr:herugrim"; quantity = 1 }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "recipes/grid/legendary-herugrim.json") -Value $herugrim -Encoding UTF8

# Gurthang
$gurthang = @{
  ingredientPattern = "G,G,S"
  width = 1
  height = 3
  ingredients = @{
    G = @{ type = "item"; code = "lotr:ingot-galvorn" }
    S = @{ type = "item"; code = "game:stick" }
  }
  output = @{ type = "item"; code = "lotr:gurthang"; quantity = 1 }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "recipes/grid/legendary-gurthang.json") -Value $gurthang -Encoding UTF8

# Durin's Axe
$durinAxe = @{
  ingredientPattern = "DM,DS,_S"
  width = 2
  height = 3
  ingredients = @{
    D = @{ type = "item"; code = "lotr:ingot-dwarvensteel" }
    M = @{ type = "item"; code = "lotr:ingot-mithril" }
    S = @{ type = "item"; code = "game:stick" }
  }
  output = @{ type = "item"; code = "lotr:durin-axe"; quantity = 1 }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "recipes/grid/legendary-durin-axe.json") -Value $durinAxe -Encoding UTF8

# Morgul Blade
$morgulBlade = @{
  ingredientPattern = "M,S"
  width = 1
  height = 2
  ingredients = @{
    M = @{ type = "item"; code = "lotr:ingot-morguliron" }
    S = @{ type = "item"; code = "game:stick" }
  }
  output = @{ type = "item"; code = "lotr:morgul-blade"; quantity = 1 }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "recipes/grid/legendary-morgul-blade.json") -Value $morgulBlade -Encoding UTF8

# Aeglos
$aeglos = @{
  ingredientPattern = "M,S,S"
  width = 1
  height = 3
  ingredients = @{
    M = @{ type = "item"; code = "lotr:ingot-galvorn" }
    S = @{ type = "item"; code = "game:stick" }
  }
  output = @{ type = "item"; code = "lotr:aeglos"; quantity = 1 }
} | ConvertTo-Json -Depth 5
Set-Content -Path (Join-Path $basePath "recipes/grid/legendary-aeglos.json") -Value $aeglos -Encoding UTF8

Write-Host "Geology blocks, metal items, and recipes generated successfully."
