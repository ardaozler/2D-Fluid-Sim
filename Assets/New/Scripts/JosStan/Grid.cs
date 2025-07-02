using System;
using Unity.VisualScripting;
using UnityEngine;


public class Cell
{
    public Vector2 Position { get; private set; }
    public float Size { get; private set; }
    public Color Color { get; private set; }

    public Vector2 Center => Position + new Vector2(Size / 2, Size / 2);

    public Vector2 Velocity { get; set; } = Vector2.zero;
    public float Density { get; set; } = 0f;

    public Cell(Vector2 position, float size, Color color)
    {
        Position = position;
        Size = size;
        Color = color;
    }
}

public class Grid : MonoBehaviour
{
    public int Rows = 10;
    public int Columns = 10;
    public float CellSize = 1.0f;
    public Color CellColor = Color.white;
    private Cell[,] cells;
    private Cell[,] sourceCells;

    public float DiffusionRate = 0.01f;
    public float AdvectionCount = 20;

    public float SourceCellAddingRate = 100f; // Rate at which source cells add density and velocity

    void Start()
    {
        cells = new Cell[Rows, Columns];
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                Vector2 position = new Vector2(j * CellSize, i * CellSize);
                cells[i, j] = new Cell(position, CellSize, CellColor);
                cells[i, j].Velocity = Vector2.up;
            }
        }

        sourceCells = new Cell[Rows, Columns];
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                sourceCells[i, j] = new Cell(cells[i, j].Position, cells[i, j].Size, cells[i, j].Color);
            }
        }

        sourceCells[5, 5].Density = 0.8f;

        DrawGrid();
    }

    private void FixedUpdate()
    {
        AddSource(Time.deltaTime);
        Diffuse(Time.deltaTime);
    }


    private void AddSource(float dt)
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                Cell cell = cells[i, j];
                Cell sourceCell = sourceCells[i, j];

                if (sourceCell.Density > 0f && cell.Density < sourceCell.Density)
                {
                    cell.Density += sourceCell.Density * dt * SourceCellAddingRate;
                }

                if (sourceCell.Velocity.magnitude > 0f && cell.Velocity.magnitude < sourceCell.Velocity.magnitude)
                {
                    cell.Velocity += sourceCell.Velocity * (dt * SourceCellAddingRate);
                }
            }
        }
    }

    private void Diffuse(float dt)
    {
        float a = dt * DiffusionRate * Rows * Columns;

        var prev = cells;
        for (int k = 0; k < AdvectionCount; k++)
        {
            for (int i = 1; i < Rows - 1; i++)
            {
                for (int j = 1; j < Columns - 1; j++)
                {
                    cells[i, j].Density = (prev[i, j].Density + a * (cells[i - 1, j].Density
                                                                     + cells[i + 1, j].Density
                                                                     + cells[i, j - 1].Density
                                                                     + cells[i, j + 1].Density)) / (1 + 4 * a);
                }
            }
        }

        //Boundaries
        for (int i = 1; i < Rows - 1; i++)
        {
            cells[i, 0].Density = cells[i, 1].Density; // bottom
            cells[i, Rows - 1].Density = cells[i, Rows - 2].Density; // top
        }

        for (int i = 0; i < Columns; i++)
        {
            cells[0, i].Density = cells[1, i].Density; // left
            cells[Columns - 1, i].Density = cells[Columns - 2, i].Density; // right
        }
    }

    private void DrawGrid()
    {
        for (int i = 0; i <= Rows; i++)
        {
            Debug.DrawLine(new Vector3(0, i * CellSize, 0), new Vector3(Columns * CellSize, i * CellSize, 0),
                Color.gray, 10000);
        }

        for (int j = 0; j <= Columns; j++)
        {
            Debug.DrawLine(new Vector3(j * CellSize, 0, 0), new Vector3(j * CellSize, Rows * CellSize, 0), Color.gray,
                10000);
        }
    }

    private void OnDrawGizmos()
    {
        if (cells == null)
        {
            return;
        }

        foreach (var cell in cells)
        {
            Gizmos.color = cell.Color;
            Gizmos.DrawWireSphere(cell.Center, cell.Density * cell.Size / 4);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(cell.Center, cell.Center + cell.Velocity * cell.Size / 2);
        }
    }
}