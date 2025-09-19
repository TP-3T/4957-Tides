$types = @(0, 1, 2, 3)
$heightMax = 10

function GenerateMap {
  param(
    $MapName,
    $Width,
    $Height,
    $Variance
  )
  
  $MapDetails = @{}
  $Tiles = @()

  for ($i = 0; $i -lt $Height; $i++) {
    for ($k = 0; $k -lt $Width; $k++) {
      $Tile = @{
        TileType = Get-Random -Minimum 0 -Maximum 3
        TilePosition = [ordered]@{
          x = $k 
          y = Get-Random -Minimum 0 -Maximum $Variance
          z = $i
        }
      }
      $Tiles += $Tile
    }
  }

  $MapDetails.MapName = $MapName
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
      -Depth 3 > "$PSScriptRoot\..\Assets\Maps\$($Map.MapName).json"
  }
}


