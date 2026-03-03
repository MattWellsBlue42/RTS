using UnityEngine;

public struct WorldTile
{
    public string id;
    public Vector2Int gridPosition;
    public bool isUnitOccupied;
    public bool isWalkable;
    public bool isBuildingOccupied;
    public bool isResourceOccupied;
    public TileView tileView;
}

public class WorldManager : MonoBehaviour
{
    [Min(1)]
    [SerializeField] public Vector2Int gridSize = new(5, 5);
    [Min(1)]
    [SerializeField] public Vector2Int tileSize = new(2, 2);
    public float meshSize = 1f;


    private GridGenerator gridGenerator;

    private void Awake()
    {
        gridGenerator = GetComponent<GridGenerator>();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) return;

        RebuildMesh();
    }

    private void RebuildMesh()
    {
        if (gridGenerator == null) return;
        gridGenerator.StartGen(gridSize, tileSize, meshSize);
    }

    private void Start()
    {
        gridGenerator.StartGen(gridSize, tileSize, meshSize);
    }

}
