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

    public int mapWidth = 50;
    public int mapHeight = 80;

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
            Vector3 diffPos = (mousePosition -newMousePosition) *panSpeed * - transform.position.z;

            float viewportWorldWidth = camUI.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1, 0, Mathf.Abs(camUI.transform.position.z))).x - camUI.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(camUI.transform.position.z))).x;
            Vector3 targetPosition= camUI.transform.position + diffPos;
            Debug.Log(viewportWorldWidth);
            //Debug.Log(diffPos);

            if ((camUI.transform.position + diffPos).x- viewportWorldWidth/2 < 0)
            {
                targetPosition = new Vector3(viewportWorldWidth / 2, targetPosition.y, targetPosition.z + diffPos.z);
            }

            if ((camUI.transform.position + diffPos).x + viewportWorldWidth / 2 > mapHeight)
            {
                targetPosition = new Vector3(mapHeight - (viewportWorldWidth / 2), targetPosition.y, targetPosition.z + diffPos.z);
            }

            if ((camUI.transform.position + diffPos).y - viewportWorldWidth / 2 < 0)
            {
                targetPosition = new Vector3(targetPosition.x, viewportWorldWidth / 2, targetPosition.z + diffPos.z);
            }

            if ((camUI.transform.position + diffPos).y + viewportWorldWidth / 2 > mapWidth)
            {
                targetPosition = new Vector3(targetPosition.x, mapWidth - (viewportWorldWidth / 2), targetPosition.z + diffPos.z);
            }

            transform.position = targetPosition;

            //Debug.Log((mousePosition - newMousePosition) * panSpeed * -transform.position.z);
            // todo this "* -transform.position.z" kinda works for maintaining the same speed with different zoom levels, but I'd like to have something more elegant

            mousePosition = newMousePosition;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0){
            float zPositionDiff = scroll * zoomSpeed * -transform.position.z;
            Vector3 targetPosition = camUI.transform.position + new Vector3(0, 0, zPositionDiff);
            float viewportWorldWidth = camUI.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1, 0, Mathf.Abs(targetPosition.z))).x - camUI.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(targetPosition.z))).x;
            
            if(viewportWorldWidth>=mapHeight*0.75|| viewportWorldWidth >= mapWidth*0.75)
            {
                targetPosition = camUI.transform.position;
            }
            else
            {
                Debug.Log(viewportWorldWidth);
                if ((camUI.transform.position).x - viewportWorldWidth / 2 < 0)
                {
                    targetPosition = new Vector3(viewportWorldWidth / 2, targetPosition.y, targetPosition.z);
                }

                if ((camUI.transform.position).x + viewportWorldWidth / 2 > mapHeight)
                {
                    targetPosition = new Vector3(mapHeight - (viewportWorldWidth / 2), targetPosition.y, targetPosition.z);
                }

                if ((camUI.transform.position).y - viewportWorldWidth / 2 < 0)
                {
                    targetPosition = new Vector3(targetPosition.x, viewportWorldWidth / 2, targetPosition.z);
                }

                if ((camUI.transform.position).y + viewportWorldWidth / 2 > mapWidth)
                {
                    targetPosition = new Vector3(targetPosition.x, mapWidth - (viewportWorldWidth / 2), targetPosition.z);
                }

                transform.position = targetPosition;  // todo it's probably better to change FoV instead of moving the camera  // no it's not (DK)
            }

            
        }
    }
}
