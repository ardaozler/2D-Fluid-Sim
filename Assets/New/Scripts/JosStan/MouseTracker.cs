using System;
using UnityEngine;

public class MouseTracker : MonoBehaviour
{
    public static MouseTracker Instance { get; private set; }
    private Vector2 _mousePosition;

    public Action<Vector2> OnMouseRightClickOrHold;
    public Action<Vector2> OnMouseLeftClickOrHold;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(0)) // Left mouse button
        {
            OnMouseLeftClickOrHold?.Invoke(_mousePosition);
        }

        if (Input.GetMouseButton(1)) // Right mouse button
        {
            OnMouseRightClickOrHold?.Invoke(_mousePosition);
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_mousePosition, 0.1f);
    }
}