using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSelection : MonoBehaviour
{
    public GameObject selectionMarker;
    public GameObject pathStepMarker;
    
    private List<Position> pathSteps;
    private List<GameObject> pathMarkers;
    private Position previousPathGoal;

    private Army selectedArmy;  // todo add option to select one army when there are many on the same tile

    private TileMap tileMap;
    private GameController gameController;

    //UI
    public GameObject cam;
    public GameObject displayArea;
    public GameObject gui;
    public Tile highlightedTile;

    void Start()
    {
        tileMap = FindObjectOfType<TileMap>();
        gameController = FindObjectOfType<GameController>();

        pathMarkers = new List<GameObject>();
        cam.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) {
            gameController.Turn();
            if (selectedArmy != null && selectedArmy.owner != gameController.activePlayer) {
                selectedArmy = null;
            }
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            gameController.activePlayer.MoveAll();
        }

        float dispAreaWidth = (displayArea.GetComponent<RectTransform>().anchorMax.x - displayArea.GetComponent<RectTransform>().anchorMin.x) * Screen.width + displayArea.GetComponent<RectTransform>().sizeDelta.x * gui.GetComponent<Canvas>().scaleFactor;
        float dispAreaHeight = (displayArea.GetComponent<RectTransform>().anchorMax.y - displayArea.GetComponent<RectTransform>().anchorMin.y) * Screen.height + displayArea.GetComponent<RectTransform>().sizeDelta.y * gui.GetComponent<Canvas>().scaleFactor;
        float dispAreaPosX = displayArea.GetComponent<RectTransform>().position.x;
        float dispAreaPosY = displayArea.GetComponent<RectTransform>().position.y;
        float dispAreaOriginX= dispAreaPosX - (dispAreaWidth / 2);
        float dispAreaOriginY = dispAreaPosY - (dispAreaHeight / 2);

        float screenOffset = (Screen.width - Screen.height)/2;
        float screenWidth = Screen.width - (Screen.width - Screen.height);

        float mousePosXNormalized = (Input.mousePosition.x - dispAreaOriginX) / dispAreaWidth;
        float mousePosYNormalized = (Input.mousePosition.y - dispAreaOriginY) / dispAreaHeight;

        Vector3 mousePos = new Vector3((mousePosXNormalized * screenWidth)+screenOffset, (mousePosYNormalized * Screen.height), 0);

        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo)) {
            selectionMarker.SetActive(true);

            Position hitPosition = new Position((int)hitInfo.point.x, (int)hitInfo.point.y);

            selectionMarker.transform.position = hitPosition;

            highlightedTile = tileMap.GetTile(hitPosition);

            //Debug.Log(highlightedTile.data.name);

            if (Input.GetButtonDown("Select")) {
                if (highlightedTile.contents != null && highlightedTile.contents.armies != null) {
                    selectedArmy = highlightedTile.contents.armies[0];
                    if (selectedArmy.owner != gameController.activePlayer) {
                        selectedArmy = null;
                    }
                } else {
                    selectedArmy = null;
                }
            }

            if (selectedArmy != null) {
                if (Input.GetButtonUp("Move")) {
                    if (pathSteps != null && pathSteps[0] != selectedArmy.position) {
                        pathSteps = tileMap.FindPath(selectedArmy.position, hitPosition, selectedArmy);
                    }
                    selectedArmy.SetPath(pathSteps);
                    selectedArmy.Move();

                    ClearPath();

                    previousPathGoal = new Position(-1, -1);
                    // kinda hacky, but this is to ensure the next path after move will always be calculated, no matter if the position is the same or not

                } else if (Input.GetButton("Move")) {
                    if (previousPathGoal != hitPosition || pathSteps != null && pathSteps[0] != selectedArmy.position) {
                        ClearPath();
                        pathSteps = tileMap.FindPath(selectedArmy.position, hitPosition, selectedArmy);
                        DrawPath();
                        previousPathGoal = hitPosition;
                    }
                }
            } else {
                if (Input.GetButtonDown("Info")) {
                   Debug.Log(highlightedTile);
                   if (highlightedTile.contents != null && highlightedTile.contents.armies != null) {
                       pathSteps = highlightedTile.contents.armies[0].path;
                       DrawPath();
                   }
                } else if (Input.GetButtonUp("Info")) {
                    ClearPath();
                }
            }

            if (Input.GetKeyDown(KeyCode.R) && highlightedTile.contents != null && highlightedTile.contents.city != null) {
                if (highlightedTile.contents.city.owner == gameController.activePlayer) {
                    highlightedTile.contents.city.Raze();
                }
            }
        } else {
            selectionMarker.SetActive(false);

            if (pathSteps != null) {
                ClearPath();

                previousPathGoal = new Position(-1, -1);
                // kinda hacky, but this is to ensure the next path after mouse returns to legal position (over TileMap collider) will always be calculated
            }
        }
    }

    private void DrawPath()
    {
        if (pathSteps != null) {
            foreach (Position step in pathSteps) {
                pathMarkers.Add(Instantiate(pathStepMarker, step, Quaternion.identity));
            }
        }
    }

    private void ClearPath()
    {
        foreach (GameObject go in pathMarkers) {
            Destroy(go);
        }
        pathMarkers.Clear();
        pathSteps = null;
    }
}
