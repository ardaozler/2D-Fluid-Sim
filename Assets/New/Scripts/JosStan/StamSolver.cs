using UnityEngine;


public class Cell
{
    public Vector2 Position { get; private set; }
    public float Size { get; private set; }
    public Color Color { get; private set; }

    public Vector2 Center => Position + new Vector2(Size / 2, Size / 2);

    public Vector2 Velocity { get; set; } = Vector2.zero;
    public float Density { get; set; } = 0f;

    private float _mouseSize;


    public Cell(Vector2 position, float size, Color color, bool listenToMouse, float mouseBrushSize)
    {
        Position = position;
        Size = size;
        Color = color;
        _mouseSize = mouseBrushSize;

        if (listenToMouse)
        {
            MouseTracker.Instance.OnMouseLeftClickOrHold += OnMoseLeftClickOrHold;
            MouseTracker.Instance.OnMouseRightClickOrHold += OnMouseRightClickOrHold;
        }
    }

    private void OnMoseLeftClickOrHold(Vector2 mousePosition, float mouseSpeed)
    {
        if (Vector2.Distance(mousePosition, Center) < _mouseSize)
        {
            Velocity = mouseSpeed * 5 * (mousePosition - Center); // Set velocity towards the mouse position
        }
    }

    private void OnMouseRightClickOrHold(Vector2 mousePosition)
    {
        if (Vector2.Distance(mousePosition, Center) < _mouseSize)
        {
            Density = Mathf.Clamp(Density + 0.1f, 0f, 1f); // Increase density on right click
        }
    }
}

public class StamSolver : MonoBehaviour
{
    public float mouseBrushSize = 0.1f; // Size of the mouse brush for density and velocity changes

    public int Columns = 10;
    public int Rows = 10;
    public float CellSize = 1.0f;
    public Color CellColor = Color.white;
    private Cell[,] cells;
    private Cell[,] sourceCells;

    public float DiffusionRate = 0.01f;
    public int AdvectionCount = 20;
    public float SourceCellAddingRate = 100f;

    // flattened buffers for projection and diffusion
    private float[] div;
    private float[] p;
    private float[] oldDensity;
    private Vector2[] oldVelocity;

    void Start()
    {
        int N = Rows * Columns;
        div = new float[N];
        p = new float[N];
        oldDensity = new float[N];
        oldVelocity = new Vector2[N];

        cells = new Cell[Rows, Columns];
        sourceCells = new Cell[Rows, Columns];
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                Vector2 pos = new Vector2(j * CellSize, i * CellSize);
                cells[i, j] = new Cell(pos, CellSize, CellColor, true, mouseBrushSize);
                cells[i, j].Velocity = Vector2.up;
                sourceCells[i, j] = new Cell(pos, CellSize, CellColor, false, mouseBrushSize);
            }
        }

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

    private void Diffuse(float dt)
    {
        float a = dt * DiffusionRate;
        float invDen = 1f / (1f + 4f * a);
        int W = Columns;

        // copy out
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                int idx = i * W + j;
                oldDensity[idx] = cells[i, j].Density;
                oldVelocity[idx] = cells[i, j].Velocity;
            }
        }

        // Gauss–Seidel
        for (int sweep = 0; sweep < AdvectionCount; sweep++)
        {
            for (int i = 1; i < Rows - 1; i++)
            {
                for (int j = 1; j < Columns - 1; j++)
                {
                    int idx = i * W + j;

                    // density
                    float sumN = cells[i - 1, j].Density
                                 + cells[i + 1, j].Density
                                 + cells[i, j - 1].Density
                                 + cells[i, j + 1].Density;
                    cells[i, j].Density = (oldDensity[idx] + a * sumN) * invDen;

                    // velocity
                    Vector2 vSum = cells[i - 1, j].Velocity
                                   + cells[i + 1, j].Velocity
                                   + cells[i, j - 1].Velocity
                                   + cells[i, j + 1].Velocity;
                    cells[i, j].Velocity = (oldVelocity[idx] + a * vSum) * invDen;
                }
            }

            BoundaryConditions();
        }
    }

    private void Advect(float dt)
    {
        int W = Columns;
        // copy out
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                int idx = i * W + j;
                oldDensity[idx] = cells[i, j].Density;
                oldVelocity[idx] = cells[i, j].Velocity;
            }
        }

        for (int i = 1; i < Rows - 1; i++)
        {
            for (int j = 1; j < Columns - 1; j++)
            {
                int idx = i * W + j;
                float x = j - dt * oldVelocity[idx].x;
                float y = i - dt * oldVelocity[idx].y;

                x = Mathf.Clamp(x, 0f, Columns - 1f);
                y = Mathf.Clamp(y, 0f, Rows - 1f);

                int j0 = Mathf.FloorToInt(x), i0 = Mathf.FloorToInt(y);
                int j1 = Mathf.Min(j0 + 1, Columns - 1);
                int i1 = Mathf.Min(i0 + 1, Rows - 1);

                float s1 = x - j0, s0 = 1f - s1;
                float t1 = y - i0, t0 = 1f - t1;

                // density
                int idx00 = i0 * W + j0;
                int idx10 = i1 * W + j0;
                int idx01 = i0 * W + j1;
                int idx11 = i1 * W + j1;
                cells[i, j].Density = s0 * (t0 * oldDensity[idx00] + t1 * oldDensity[idx10])
                                      + s1 * (t0 * oldDensity[idx01] + t1 * oldDensity[idx11]);

                // velocity
                cells[i, j].Velocity = s0 * (t0 * oldVelocity[idx00] + t1 * oldVelocity[idx10])
                                       + s1 * (t0 * oldVelocity[idx01] + t1 * oldVelocity[idx11]);
            }
        }

        BoundaryConditions();
    }

    private void Project()
    {
        int W = Columns;
        // divergence and zero p
        for (int i = 1; i < Rows - 1; i++)
        {
            for (int j = 1; j < Columns - 1; j++)
            {
                int idx = i * W + j;
                div[idx] = -0.5f * (
                    cells[i, j + 1].Velocity.x - cells[i, j - 1].Velocity.x
                    + cells[i + 1, j].Velocity.y - cells[i - 1, j].Velocity.y
                );
                p[idx] = 0f;
            }
        }

        // mirror div and p at boundaries
        for (int i = 1; i < Rows - 1; i++)
        {
            int l = i * W, r = i * W + (W - 1);
            div[l] = div[l + 1];
            div[r] = div[r - 1];
            p[l] = p[l + 1];
            p[r] = p[r - 1];
        }

        for (int j = 1; j < Columns - 1; j++)
        {
            int b = j, t = (Rows - 1) * W + j;
            div[b] = div[b + W];
            div[t] = div[t - W];
            p[b] = p[b + W];
            p[t] = p[t - W];
        }

        // converge
        for (int iter = 0; iter < AdvectionCount; iter++)
        {
            for (int i = 1; i < Rows - 1; i++)
            {
                for (int j = 1; j < Columns - 1; j++)
                {
                    int idx = i * W + j;
                    p[idx] = (div[idx]
                              + p[idx - W] + p[idx + W]
                              + p[idx - 1] + p[idx + 1]) * 0.25f;
                }
            }

            // reapply pressure boundaries
            for (int i = 1; i < Rows - 1; i++)
            {
                int l = i * W, r = i * W + (W - 1);
                p[l] = p[l + 1];
                p[r] = p[r - 1];
            }

            for (int j = 1; j < Columns - 1; j++)
            {
                int b = j, t = (Rows - 1) * W + j;
                p[b] = p[b + W];
                p[t] = p[t - W];
            }
        }

        // subtract p from velocity
        for (int i = 1; i < Rows - 1; i++)
        {
            for (int j = 1; j < Columns - 1; j++)
            {
                int idx = i * W + j;
                cells[i, j].Velocity = new Vector2(
                    cells[i, j].Velocity.x - 0.5f * (p[idx + 1] - p[idx - 1]),
                    cells[i, j].Velocity.y - 0.5f * (p[idx + W] - p[idx - W])
                );
            }
        }

        BoundaryConditions();
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
        //only draw the borders
        Debug.DrawLine(new Vector3(0, 0, 0), new Vector3(Columns * CellSize, 0, 0),
            Color.gray, 10000);
        Debug.DrawLine(new Vector3(0, Rows * CellSize, 0), new Vector3(Columns * CellSize, Rows * CellSize, 0),
            Color.gray, 10000);
        Debug.DrawLine(new Vector3(0, 0, 0), new Vector3(0, Rows * CellSize, 0), Color.gray, 10000);
        Debug.DrawLine(new Vector3(Columns * CellSize, 0, 0), new Vector3(Columns * CellSize, Rows * CellSize, 0),
            Color.gray, 10000);


        return;
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