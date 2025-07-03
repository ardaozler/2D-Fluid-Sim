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

    public Cell(Vector2 position, float size, Color color, bool listenToMouse)
    {
        Position = position;
        Size = size;
        Color = color;

        if (listenToMouse)
        {
            MouseTracker.Instance.OnMouseLeftClickOrHold += OnMoseLeftClickOrHold;
            MouseTracker.Instance.OnMouseRightClickOrHold += OnMouseRightClickOrHold;
        }
    }

    private void OnMoseLeftClickOrHold(Vector2 mousePosition)
    {
        if (Vector2.Distance(mousePosition, Center) < Size / 2)
        {
            Velocity = 2 * (mousePosition - Center); // Set velocity towards the mouse position
        }
    }

    private void OnMouseRightClickOrHold(Vector2 mousePosition)
    {
        if (Vector2.Distance(mousePosition, Center) < Size / 2)
        {
            Density = Mathf.Clamp(Density + 0.1f, 0f, 1f); // Increase density on right click
        }
    }
}

public class StamSolver : MonoBehaviour
{
    public int Rows = 10;
    public int Columns = 10;
    public float CellSize = 1.0f;
    public Color CellColor = Color.white;
    private Cell[,] cells;
    private Cell[,] sourceCells;

    public float DiffusionRate = 0.01f;
    public int AdvectionCount = 20;

    public float SourceCellAddingRate = 100f; // Rate at which source cells add density and velocity

    //buffers for projection
    private float[,] div;
    private float[,] p;


    void Start()
    {
        div = new float[Rows, Columns];
        p = new float[Rows, Columns];

        cells = new Cell[Rows, Columns];
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                Vector2 position = new Vector2(j * CellSize, i * CellSize);
                cells[i, j] = new Cell(position, CellSize, CellColor, true);
                cells[i, j].Velocity = Vector2.up;
            }
        }

        sourceCells = new Cell[Rows, Columns];
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                sourceCells[i, j] = new Cell(cells[i, j].Position, cells[i, j].Size, cells[i, j].Color, false);
            }
        }

        //sourceCells[5, 5].Velocity = new Vector2(0f, .5f);

        DrawGrid();
    }

    private void FixedUpdate()
    {
        AddSource(Time.deltaTime);
        Diffuse(Time.deltaTime);
        Project();
        Advect(Time.deltaTime);
        Project();
    }


    private void AddSource(float dt)
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                Cell cell = cells[i, j];
                Cell sourceCell = sourceCells[i, j];

                //add density
                if (sourceCell.Density > 0f && cell.Density < sourceCell.Density)
                {
                    cell.Density += sourceCell.Density * dt * SourceCellAddingRate;
                }

                //add velocity
                if (sourceCell.Velocity.magnitude > 0f && cell.Velocity.magnitude < sourceCell.Velocity.magnitude)
                {
                    cell.Velocity += sourceCell.Velocity * (dt * SourceCellAddingRate);
                }
            }
        }
    }

    private void Diffuse(float dt)
    {
        float a = dt * DiffusionRate;

        float[][] oldDensity = new float[Rows][];
        for (int index = 0; index < Rows; index++)
        {
            oldDensity[index] = new float[Columns];
        }

        Vector2[][] oldVelocity = new Vector2[Rows][];
        for (int index = 0; index < Rows; index++)
        {
            oldVelocity[index] = new Vector2[Columns];
        }

        for (int i = 0; i < Rows; i++)
        for (int j = 0; j < Columns; j++)
        {
            oldDensity[i][j] = cells[i, j].Density;
            oldVelocity[i][j] = cells[i, j].Velocity;
        }

        for (int k = 0; k < AdvectionCount; k++)
        {
            for (int i = 1; i < Rows - 1; i++)
            {
                for (int j = 1; j < Columns - 1; j++)
                {
                    //diffuse density
                    cells[i, j].Density = (oldDensity[i][j] + a * (cells[i - 1, j].Density
                                                                   + cells[i + 1, j].Density
                                                                   + cells[i, j - 1].Density
                                                                   + cells[i, j + 1].Density)) / (1 + 4 * a);
                    //diffuse velocity
                    cells[i, j].Velocity = (oldVelocity[i][j] + a * (cells[i - 1, j].Velocity
                                                                     + cells[i + 1, j].Velocity
                                                                     + cells[i, j - 1].Velocity
                                                                     + cells[i, j + 1].Velocity)) / (1 + 4 * a);
                }
            }

            BoundaryConditions();
        }
    }

    private void Advect(float dt)
    {
        float[][] oldDensity = new float[Rows][];
        for (int index = 0; index < Rows; index++)
        {
            oldDensity[index] = new float[Columns];
        }

        Vector2[][] oldVelocity = new Vector2[Rows][];
        for (int index = 0; index < Rows; index++)
        {
            oldVelocity[index] = new Vector2[Columns];
        }

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                oldDensity[i][j] = cells[i, j].Density;
                oldVelocity[i][j] = cells[i, j].Velocity;
            }
        }


        for (int i = 1; i < Rows - 1; i++)
        {
            for (int j = 1; j < Columns - 1; j++)
            {
                float x = j - dt * oldVelocity[i][j].x;
                float y = i - dt * oldVelocity[i][j].y;

                x = Mathf.Clamp(x, 0f, Columns - 1f);
                y = Mathf.Clamp(y, 0f, Rows - 1f);

                int j0 = Mathf.FloorToInt(x);
                int i0 = Mathf.FloorToInt(y);
                int j1 = Mathf.Min(j0 + 1, Columns - 1);
                int i1 = Mathf.Min(i0 + 1, Rows - 1);


                float s1 = x - j0;
                float s0 = 1f - s1;
                float t1 = y - i0;
                float t0 = 1f - t1;

                // Interpolate density
                cells[i, j].Density = s0 * (t0 * oldDensity[i0][j0] + t1 * oldDensity[i1][j0]) +
                                      s1 * (t0 * oldDensity[i0][j1] + t1 * oldDensity[i1][j1]);

                // Interpolate velocity
                cells[i, j].Velocity = s0 * (t0 * oldVelocity[i0][j0] + t1 * oldVelocity[i1][j0]) +
                                       s1 * (t0 * oldVelocity[i0][j1] + t1 * oldVelocity[i1][j1]);
            }
        }

        BoundaryConditions();
    }

    private void Project()
    {
        //compute divergence and initialize pressure to 0
        for (int i = 1; i < Rows - 1; i++)
        {
            for (int j = 1; j < Columns - 1; j++)
            {
                div[i, j] = -0.5f * (
                    cells[i, j + 1].Velocity.x - cells[i, j - 1].Velocity.x
                    + cells[i + 1, j].Velocity.y - cells[i - 1, j].Velocity.y
                );

                p[i, j] = 0f;
            }
        }

        //boundary conditions for divergence
        for (int i = 1; i < Rows - 1; i++)
        {
            // mirror div at left/right
            div[i, 0] = div[i, 1];
            div[i, Columns - 1] = div[i, Columns - 2];

            //mirror pressure at left/right
            p[i, 0] = p[i, 1]; // left
            p[i, Columns - 1] = p[i, Columns - 2]; // right
        }

        for (int j = 1; j < Columns - 1; j++)
        {
            // mirror div at bottom/top
            div[0, j] = div[1, j];
            div[Rows - 1, j] = div[Rows - 2, j];

            //mirror pressure at bottom/top
            p[0, j] = p[1, j]; // bottom
            p[Rows - 1, j] = p[Rows - 2, j]; // top
        }

        // Gauss Seidel iteration for pressure TODO: separate this maybe
        for (int iter = 0; iter < AdvectionCount; iter++)
        {
            for (int i = 1; i < Rows - 1; i++)
            {
                for (int j = 1; j < Columns - 1; j++)
                {
                    p[i, j] = (div[i, j] + p[i - 1, j] + p[i + 1, j] + p[i, j - 1] + p[i, j + 1]) / 4f;
                }
            }

            // Boundary conditions for pressure
            for (int i = 1; i < Rows - 1; i++)
            {
                p[i, 0] = p[i, 1]; // left
                p[i, Columns - 1] = p[i, Columns - 2]; // right
            }

            for (int j = 1; j < Columns - 1; j++)
            {
                p[0, j] = p[1, j]; // bottom
                p[Rows - 1, j] = p[Rows - 2, j]; // top
            }
        }

        // Update velocities based on pressure
        for (int i = 1; i < Rows - 1; i++)
        {
            for (int j = 1; j < Columns - 1; j++)
            {
                cells[i, j].Velocity = new Vector2(
                    cells[i, j].Velocity.x - 0.5f * (p[i, j + 1] - p[i, j - 1]),
                    cells[i, j].Velocity.y - 0.5f * (p[i + 1, j] - p[i - 1, j])
                );
            }
        }

        BoundaryConditions();
    }

    private void BoundaryConditions()
    {
        //density
        for (int i = 1; i < Rows - 1; i++)
        {
            cells[i, 0].Density = cells[i, 1].Density; // left
            cells[i, Columns - 1].Density = cells[i, Columns - 2].Density; // right
        }

        for (int j = 1; j < Columns - 1; j++)
        {
            cells[0, j].Density = cells[1, j].Density; // bottom
            cells[Rows - 1, j].Density = cells[Rows - 2, j].Density; // top
        }

        //velocities
        for (int j = 1; j < Columns - 1; j++)
        {
            // bottom (i = 0): invert y
            cells[0, j].Velocity = new Vector2(
                cells[1, j].Velocity.x,
                -cells[1, j].Velocity.y
            );
            // top (i = Rows-1): invert y
            cells[Rows - 1, j].Velocity = new Vector2(
                cells[Rows - 2, j].Velocity.x,
                -cells[Rows - 2, j].Velocity.y
            );
        }

        for (int i = 1; i < Rows - 1; i++)
        {
            // left (j = 0): invert x
            cells[i, 0].Velocity = new Vector2(
                -cells[i, 1].Velocity.x,
                cells[i, 1].Velocity.y
            );
            // right (j = Columns-1): invert x
            cells[i, Columns - 1].Velocity = new Vector2(
                -cells[i, Columns - 2].Velocity.x,
                cells[i, Columns - 2].Velocity.y
            );
        }

        //corner velocities
        cells[0, 0].Velocity = Vector2.zero;
        cells[0, Columns - 1].Velocity = Vector2.zero;
        cells[Rows - 1, 0].Velocity = Vector2.zero;
        cells[Rows - 1, Columns - 1].Velocity = Vector2.zero;
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

        Color color = CellColor;

        foreach (var cell in cells)
        {
            color.a = cell.Density;
            Gizmos.color = color;
            Gizmos.DrawCube(cell.Center, new Vector3(cell.Size, cell.Size, 0.1f));
            //Gizmos.color = Color.green;
            //Gizmos.DrawLine(cell.Center, cell.Center + cell.Velocity * cell.Size / 2);
            //Gizmos.DrawSphere(cell.Center + cell.Velocity * cell.Size / 2, cell.Size / 32);
        }
    }
}