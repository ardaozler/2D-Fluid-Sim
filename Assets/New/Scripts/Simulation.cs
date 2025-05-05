using UnityEngine;


public class Simulation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        DisplayShapes.DrawCircle(Vector3.zero, 2, DisplayShapes.ColorOptions.Blue, DisplayShapes.Space2D.XY);
    }
}