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

    private TileMap tileMap;

    private Army selectedArmy;  // todo add option to select one army when there are many on the same tile
    void Start()
    {
        tileMap = FindObjectOfType<TileMap>();

        pathMarkers = new List<GameObject>();
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo)) {
            Position hitPosition = new Position((int)hitInfo.point.x, (int)hitInfo.point.y);

            selectionMarker.transform.position = hitPosition;

            Tile highlightedTile = tileMap.GetTile(hitPosition);

            if (Input.GetButtonDown("Select")) {
                if (highlightedTile.contents != null && highlightedTile.contents.armies != null) {
                    selectedArmy = highlightedTile.contents.armies[0];
                } else {
                    selectedArmy = null;
                }
            }

            if (selectedArmy != null) {
                if (Input.GetButtonUp("Move")) {
                    ClearPath();
                    selectedArmy.Move(hitPosition);

                    previousPathGoal = new Position(-1, -1);
                    // kinda hacky, but this is to ensure the next path after move will always be calculated, no matter if the position is the same or not

                } else if (Input.GetButton("Move")) {
                    if (previousPathGoal != hitPosition) {
                        pathSteps = tileMap.FindPath(selectedArmy.position, hitPosition, selectedArmy.pathfindingTypes);
                        ClearPath();
                        DrawPath();
                        previousPathGoal = hitPosition;
                    }
                }
            } else {
                if (Input.GetButtonDown("Info")) {
                   Debug.Log(highlightedTile);
                }
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
    }
}
