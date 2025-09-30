using UnityEngine;

/// <summary>
/// The terrain type of a tile. Represents climates and helps define regions.
/// </summary>
[CreateAssetMenu(fileName = "TerrainType", menuName = "Scriptable Objects/TerrainType")]
public class TerrainType : ScriptableObject
{
    [Tooltip("The color that represents this terrain.")]
    public ColorReference tileColor;

    [Tooltip("Identifier unique for all terrain types")]
    public string uid;

    // We can add more variables here as we scale. Example:
    // [Tooltip("The multiplier to be applied to get local temperatures")]
    // public float temperatureMultiplier = 1.0f;
}
