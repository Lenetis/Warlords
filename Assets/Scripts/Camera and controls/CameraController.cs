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

    public GameObject camUI;

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
            Vector2 diffPos = (mousePosition -newMousePosition) *panSpeed * - transform.position.z;

            if (camUI.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(camUI.transform.position.z))).x <= 0)
            {
                //Debug.Log("-X");
                if(diffPos.x<0)
                {
                    diffPos.x = 0;
                }
            }
                
            if (camUI.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1, 0, Mathf.Abs(camUI.transform.position.z))).x >= 80)
            {
                //Debug.Log("X");
                if (diffPos.x > 0)
                {
                    diffPos.x = 0;
                }
            }
                
            if (camUI.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(camUI.transform.position.z))).y <= 0)
            {
                //Debug.Log("-Y");
                if (diffPos.y < 0)
                {
                    diffPos.y = 0;
                }
            }
                
            if (camUI.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0, 1, Mathf.Abs(camUI.transform.position.z))).y >= 50)
            {
                //Debug.Log("Y");
                if (diffPos.y > 0)
                {
                    diffPos.y = 0;
                }
            }
            
            transform.Translate(diffPos);
            //Debug.Log((mousePosition - newMousePosition) * panSpeed * -transform.position.z);
            // todo this "* -transform.position.z" kinda works for maintaining the same speed with different zoom levels, but I'd like to have something more elegant

            mousePosition = newMousePosition;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0){
            transform.Translate(0, 0, scroll * zoomSpeed * -transform.position.z);  // todo it's probably better to change FoV instead of moving the camera  // no it's not (DK)
        }
    }
}
