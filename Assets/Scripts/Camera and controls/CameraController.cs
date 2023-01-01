using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed;
    public float sidePanSpeed;
    public float zoomSpeed;

    private bool cameraPanButtonPressed;
    private Vector2 mousePosition;


    void Start()
    {
        cameraPanButtonPressed = false;
    }


    void Update()
    {
        if (Input.GetButtonDown("Camera Pan")){
            cameraPanButtonPressed = true;
            mousePosition = Input.mousePosition;
        }
        if (Input.GetButtonUp("Camera Pan")){
            cameraPanButtonPressed = false;
        }

        if (cameraPanButtonPressed){
            Vector2 newMousePosition = Input.mousePosition;
            transform.Translate((mousePosition - newMousePosition) * panSpeed * -transform.position.z);
            // todo this * -transform.position.z kinda works for maintaining the same speed with different zoom levels, but I'd like to have something more elegant

            mousePosition = newMousePosition;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0){
            transform.Translate(0, 0, scroll * zoomSpeed * -transform.position.z);  // todo it's probably better to change FoV instead of moving the camera // no it's not (DK)
        }
    }
}
