using System.Collections.Generic;
using UnityEngine;

public class GridGenerator
{
    [SerializeField] private List<TileSO> tileTypes;
    private int[,] gridArray;

    public int[,] GridArray => gridArray;

    private float cellSize;
    private Vector3 originPosition;

    public GridGenerator(int tilesX, int tilesY, float cellSize, Vector3 originPosition = default(Vector3))
    {
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new int[tilesX, tilesY];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = 0;
            }
        }
    }

    public string PositionToString(int x, int y)
    {
        return $"({x}, {y})";
    }

    public bool IsValidCell(int x, int y)
    {
        return x >= 0 && x < gridArray.GetLength(0) && y >= 0 && y < gridArray.GetLength(1);
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, 0, y) * cellSize + originPosition;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        if (worldPosition.x < originPosition.x || worldPosition.z < originPosition.z)
        {
            x = -1;
            y = -1;
            return;
        }
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize);
    }
}