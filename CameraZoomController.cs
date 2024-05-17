using UnityEngine;
using Cinemachine;
using System;

public class CameraZoomController : MonoBehaviour
{
    // Handles zooming the player camera in and out during gameplay
    // Zooming is done by changing the orto size of the Cinemachine virtual camera component

    CinemachineVirtualCamera virtualCamera;

    public float maxZoom = 19f;
    public float minZoom = 5f;

    private void OnEnable()
    {
        PlayerInput.OnCameraZoom += OnCameraZoomDetected;
    }

    private void OnDisable()
    {
        PlayerInput.OnCameraZoom += OnCameraZoomDetected;
    }

    private void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    private void OnCameraZoomDetected(float zoomInput)
    {
        //// Make zoom input 1 or -1 (original zoominput might be 120 or -120)
        //var zoomAmount = zoomInput / Math.Abs(zoomInput);

        // Prevent zooming over or under min and max zoom values
        if (virtualCamera.m_Lens.OrthographicSize - zoomInput > maxZoom)
        {
            virtualCamera.m_Lens.OrthographicSize = maxZoom;
            return;
        }
        if (virtualCamera.m_Lens.OrthographicSize - zoomInput < minZoom)
        {
            virtualCamera.m_Lens.OrthographicSize = minZoom;
            return;
        }

        // zoom
        virtualCamera.m_Lens.OrthographicSize -= zoomInput / Math.Abs(zoomInput);
    }
}
