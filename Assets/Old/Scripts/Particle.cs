using System;
using Unity.VisualScripting;
using UnityEngine;

public class Particle
{
    public float Rad;
    public DisplayShapes.ColorOptions Color;
    public readonly Vector2 CurrentPosition;
    public readonly Vector2 Velocity;

    public Particle(float r, DisplayShapes.ColorOptions c)
    {
        Rad = r;
        Color = c;
    }

    public void UpdatePosition()
    {
        throw new NotImplementedException();
    }

    public void UpdateVelocity()
    {
        throw new NotImplementedException();
    }
}