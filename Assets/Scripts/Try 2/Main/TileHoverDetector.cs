using UnityEngine;
using UnityEngine.InputSystem;

public class TileHoverDetector
{
    private Camera mainCamera;
    private Mouse mouse;
    private Vector2 lastMousePosition;
    public Vector2 LastMousePosition => lastMousePosition;

    public TileHoverDetector(Camera camera, Mouse mouse)
    {
        mainCamera = camera;
        this.mouse = mouse;
    }

    public void SendRaycast(out Vector3? hitLocation)
    {
        hitLocation = null;
        Vector2 currentMousePosition = mouse.position.ReadValue();
        if (currentMousePosition != lastMousePosition)
        {
            lastMousePosition = currentMousePosition;
        }

        Ray ray = mainCamera.ScreenPointToRay(currentMousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            hitLocation = hit.point;
        }
    }
}
