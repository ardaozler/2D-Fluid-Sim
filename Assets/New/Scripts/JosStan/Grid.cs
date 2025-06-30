using System;
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

    void Start()
    {
        cells = new Cell[Rows, Columns];
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                Vector2 position = new Vector2(j * CellSize, i * CellSize);
                cells[i, j] = new Cell(position, CellSize, CellColor);
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

        sourceCells[0, 0].Density = 0.8f;

        DrawGrid();
    }


    private void AddSource(float dt)
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                Cell cell = cells[i, j];
                Cell sourceCell = sourceCells[i, j];

                if (sourceCell.Density > 0f)
                {
                    cell.Density += sourceCell.Density * dt;
                    cell.Velocity += sourceCell.Velocity * dt;
                }
            }
        }
    }

    private void Diffuse(float dt, float diffusionRate)
    {
        float a = dt * diffusionRate * Rows * Columns;
        for (int k = 0; k < 20; k++)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    //x[IX(i,j)] = (x0[IX(i,j)] + a*(x[IX(i-1,j)]+x[IX(i+1,j)]+ x[IX(i,j-1)]+x[IX(i,j+1)]))/(1+4*a);
                    cells[i, j].Density += a * (cells[i - 1, j].Density
                                                  + cells[i + 1, j].Density
                                                  + cells[i, j - 1].Density 
                                                  + cells[i, j + 1].Density);
                }
            }
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