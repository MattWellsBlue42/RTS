using UnityEngine;

public class Tile
{
    private string id;
    public TileSO tileInfo;
    public Vector2Int size;

    public Tile(TileSO tileInfo, Vector2Int size)
    {
        this.tileInfo = tileInfo;
        this.size = size;
        this.id = System.Guid.NewGuid().ToString();
    }

    public string GetId()
    {
        return id;
    }
}
