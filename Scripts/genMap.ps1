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
        TileType = Get-Random -Minimum 0 -Maximum 3
        Height = Get-Random -Minimum 0 -Maximum $Variance
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


