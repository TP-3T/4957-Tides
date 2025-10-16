$TurnTheTides = @"
  `e[48;5;21m              `e[0m
`e[48;5;19m     `e[0m `e[48;5;20m `e[0m`e[48;5;21m     `e[0m `e[48;5;19m     `e[0m
 `e[48;5;17m `e[0m`e[48;5;19m  `e[0m  `e[48;5;20m `e[0m`e[48;5;21m     `e[0m  `e[48;5;17m `e[0m`e[48;5;19m  `e[0m
 `e[48;5;17m `e[0m`e[48;5;19m  `e[0m  `e[48;5;20m `e[0m`e[48;5;21m     `e[0m  `e[48;5;17m `e[0m`e[48;5;19m  `e[0m
"@
$NumberTileType = @{
  0 = "plains";
  1 = "desert";
  2 = "coastal";
}

function EpicTitle {
  Write-Host @"

$TurnTheTides

GenerateMap
  -Name     # the name of the map
  -Width    # the x plane limit
  -Heigth   # the z plane limit
  -Variance # How much the terrain hight will varry

GenerateMap
  -MapDetails # Array of hashmaps having the same keys

"@
}

function GetRandomTile {
  $max    = $NumberTileType.Count
  $number = ( Get-Random -Minimum 0 -Maximum $max  )

  return $number
}

function GenerateMap {
  param(
    $Name,
    $Width,
    $Height,
    $Variance
  )
  
  $MapDetails = @{}
  $Tiles = @()

  for ($i = 0; $i -lt $Height; $i++) {
    for ($k = 0; $k -lt $Width; $k++) {
      $Tile = @{
        TileType  = $NumberTileType[( GetRandomTile )]
        Height    = ( Get-Random -Minimum 0 -Maximum $Variance )
        OffsetCoordinates = [ordered]@{
          x = $k 
          z = $i
        }
      }
      $Tiles += $Tile
    }
  }

  $MapDetails.Name = $Name
  $MapDetails.Width = $Width
  $MapDetails.Height = $Height
  $MapDetails.MapTilesData = $Tiles

  return $MapDetails
}

function GenerateMaps {
  <# Auto dump the map files generated into ../Assets/Maps #>
  param(
    $MapDetails
  )
  foreach ($MapDetail in $MapDetails) {

    Write-Host $MapDetail

    $Map = GenerateMap @MapDetail

    ConvertTo-Json `
      -InputObject $Map `
      -Depth 3 > "$PSScriptRoot\..\Assets\Maps\$($Map.Name).json"
  }
}

EpicTitle


