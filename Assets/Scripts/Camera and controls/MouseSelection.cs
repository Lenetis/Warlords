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

    void Start()
    {
        tileMap = FindObjectOfType<TileMap>();
        gameController = FindObjectOfType<GameController>();

        pathMarkers = new List<GameObject>();
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

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo)) {
            selectionMarker.SetActive(true);

            Position hitPosition = new Position((int)hitInfo.point.x, (int)hitInfo.point.y);

            selectionMarker.transform.position = hitPosition;

            Tile highlightedTile = tileMap.GetTile(hitPosition);

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
