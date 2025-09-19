$types = @(0, 1, 2, 3)
$heightMax = 10

function GenerateMap {
  param(
    $MapName,
    $Width,
    $Height,
    $Variance
  )

  $map = @{
    Name = $MapName
    Width = $Width
    Height = $Height
    MapTilesData = @()
  }

  for ($i = 0; $i -lt $Height; $i++) {
    for ($k = 0; $k -lt $Width; $k++) {
      $Tile = @{
        TileType = Get-Random -Minimum 0 -Maximum 3
        TilePosition = [ordered]@{
          x = $k 
          y = Get-Random -Minimum 0 -Maximum 3
          z = $i
        }
      }
      $map.MapTilesData += $Tile
    }
  }

  return $map
}

