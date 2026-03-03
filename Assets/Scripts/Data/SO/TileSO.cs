using UnityEngine;

public enum TileType
{
    Grass,
    Stone,
    Water
}

[CreateAssetMenu(fileName = "New Tile", menuName = "ScriptableObjects/Tile")]
public class TileSO : ScriptableObject
{
    public TileType tileType;
    public Color tileColor;
}
