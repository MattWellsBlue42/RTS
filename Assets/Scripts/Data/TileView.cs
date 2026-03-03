using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TileView
{
    public Tile TileData { get; private set; }

    public TileView(Tile tileData)
    {
        TileData = tileData;
    }
}
