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

    public int mapHeight;
    public int mapWidth;

    private MouseSelection mouseSelection;
    private UIController uiController;

    void Start()
    {
        cameraPanButtonPressed = false;
        mouseSelection = GameObject.Find("Main Camera").GetComponent<MouseSelection>();
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Camera Pan") && mouseSelection.isOverDispArea) {
            cameraPanButtonPressed = true;
            mousePosition = Input.mousePosition;
        }
        if (Input.GetButtonUp("Camera Pan")) {
            cameraPanButtonPressed = false;
        }

        if (cameraPanButtonPressed) {

            Vector2 newMousePosition = Input.mousePosition;
            Vector3 diffPos = (mousePosition - newMousePosition) * panSpeed * -transform.position.z;

            float viewportWorldWidth = camUI.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1, 0, Mathf.Abs(camUI.transform.position.z))).x - camUI.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(camUI.transform.position.z))).x;
            Vector3 targetPosition = camUI.transform.position + diffPos;
            //Debug.Log(viewportWorldWidth);
            //Debug.Log(diffPos);

            CheckNSetPosition(targetPosition);

            //Debug.Log((mousePosition - newMousePosition) * panSpeed * -transform.position.z);
            // todo this "* -transform.position.z" kinda works for maintaining the same speed with different zoom levels, but I'd like to have something more elegant

            mousePosition = newMousePosition;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0 && mouseSelection.isOverDispArea || scroll != 0 && uiController.isOverMinimap)
        {
            float zPositionDiff = scroll * zoomSpeed * -transform.position.z;
            Vector3 targetPosition = camUI.transform.position + new Vector3(0, 0, zPositionDiff);
            CheckNSetPosition(targetPosition);
        }
    }

    public void CheckNSetPosition(Vector3 targetPosition2)
    {
        Vector3 targetPosition = targetPosition2;

        float viewportWorldWidth = camUI.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1, 0, Mathf.Abs(targetPosition.z))).x - camUI.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(targetPosition.z))).x;
        
        if ((int)viewportWorldWidth > mapWidth || (int)viewportWorldWidth > mapHeight)
        {
            if (viewportWorldWidth >= mapHeight)
            {
                targetPosition = new Vector3(camUI.transform.position.x, mapHeight / 2, -(float)CalculateCamDistance(mapHeight / 2));
                viewportWorldWidth = mapHeight;
            }
            else
            {
                targetPosition = new Vector3(mapWidth / 2, camUI.transform.position.y, -(float)CalculateCamDistance(mapWidth / 2));
                viewportWorldWidth = mapWidth;
            }

            

            if ((targetPosition).x - viewportWorldWidth / 2 < 0)
            {
                targetPosition = new Vector3(viewportWorldWidth / 2, targetPosition.y, targetPosition.z);
            }

            if ((targetPosition).x + viewportWorldWidth / 2 > mapWidth)
            {
                targetPosition = new Vector3(mapWidth - (viewportWorldWidth / 2), targetPosition.y, targetPosition.z);
            }

            if ((targetPosition).y - viewportWorldWidth / 2 < 0)
            {
                targetPosition = new Vector3(targetPosition.x, viewportWorldWidth / 2, targetPosition.z);
            }

            if ((targetPosition).y + viewportWorldWidth / 2 > mapHeight)
            {
                targetPosition = new Vector3(targetPosition.x, mapHeight - (viewportWorldWidth / 2), targetPosition.z);
            }

            transform.position = targetPosition;
        }
        else
        {
            //Debug.Log(viewportWorldWidth);
            if ((targetPosition).x - viewportWorldWidth / 2 < 0)
            {
                targetPosition = new Vector3(viewportWorldWidth / 2, targetPosition.y, targetPosition.z);
            }

            if ((targetPosition).x + viewportWorldWidth / 2 > mapWidth)
            {
                targetPosition = new Vector3(mapWidth - (viewportWorldWidth / 2), targetPosition.y, targetPosition.z);
            }

            if ((targetPosition).y - viewportWorldWidth / 2 < 0)
            {
                targetPosition = new Vector3(targetPosition.x, viewportWorldWidth / 2, targetPosition.z);
            }

            if ((targetPosition).y + viewportWorldWidth / 2 > mapHeight)
            {
                targetPosition = new Vector3(targetPosition.x, mapHeight - (viewportWorldWidth / 2), targetPosition.z);
            }

            transform.position = targetPosition;  // todo it's probably better to change FoV instead of moving the camera  // no it's not (DK)
        }
    }

    public double ConvertToRadians(double angle)
    {
        return (System.Math.PI / 180) * angle;
    }

    public double CalculateCamDistance(double mapDim)
    {
        double distance = mapDim / System.Math.Tan(ConvertToRadians(30));
        Debug.Log(distance);
        return distance;
    }
}
