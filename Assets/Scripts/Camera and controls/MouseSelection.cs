using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSelection : MonoBehaviour
{
    public GameObject selectionMarker;
    public GameObject selectedArmyMarker;
    public GameObject pathStepMarker;

    private List<Position> pathSteps;
    private List<GameObject> pathMarkers;
    private Position previousPathGoal;

    public Army selectedArmy { get; private set; }

    private TileMap tileMap;
    private GameController gameController;

    //UI
    public GameObject cam;
    public GameObject displayArea;
    public GameObject gui;
    public Tile highlightedTile;

    private ArmyManagement armyManagement;
    private CityManagement cityManagement;

    public bool isOverDispArea;
    public bool isSelected;

    public List<Army> selectedArmies;

    void Start()
    {
        tileMap = FindObjectOfType<TileMap>();
        gameController = FindObjectOfType<GameController>();

        pathMarkers = new List<GameObject>();
        cam.SetActive(true);

        armyManagement = GameObject.Find("Main").GetComponent<ArmyManagement>();
        cityManagement = GameObject.Find("Main").GetComponent<CityManagement>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            gameController.Turn();
            if (selectedArmy != null && selectedArmy.owner != gameController.activePlayer)
            {
                selectedArmy = null;
                armyManagement.DeselectArmy();
            }
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            gameController.activePlayer.MoveAll();
        }
        if (Input.GetKeyDown(KeyCode.A) && selectedArmy != null)
        {
            Tile mergeTile = tileMap.GetTile(selectedArmy.position);
            while (mergeTile.armies.Count > 1)
            {
                mergeTile.armies[0].Merge(mergeTile.armies[1]);
            }
            selectedArmy = mergeTile.armies[0];
            selectedArmies = mergeTile.armies;
            armyManagement.DeselectArmy();
            armyManagement.SelectArmy(selectedArmies);
        }
        if (Input.GetKeyDown(KeyCode.S) && selectedArmy != null)
        {
            selectedArmy.Split();
            armyManagement.RefreshSelection();
        }
        if(Input.GetKeyDown(KeyCode.F5))
        {
            ResourceManager.SaveGame("save.json");
        }
        if(Input.GetKeyDown(KeyCode.F9))
        {
            ResourceManager.LoadGame("save.json");
        }

        float dispAreaWidth = (displayArea.GetComponent<RectTransform>().anchorMax.x - displayArea.GetComponent<RectTransform>().anchorMin.x) * Screen.width + displayArea.GetComponent<RectTransform>().sizeDelta.x * gui.GetComponent<Canvas>().scaleFactor;
        float dispAreaHeight = (displayArea.GetComponent<RectTransform>().anchorMax.y - displayArea.GetComponent<RectTransform>().anchorMin.y) * Screen.height + displayArea.GetComponent<RectTransform>().sizeDelta.y * gui.GetComponent<Canvas>().scaleFactor;
        float dispAreaPosX = displayArea.GetComponent<RectTransform>().position.x;
        float dispAreaPosY = displayArea.GetComponent<RectTransform>().position.y;
        float dispAreaOriginX = dispAreaPosX - (dispAreaWidth / 2);
        float dispAreaOriginY = dispAreaPosY - (dispAreaHeight / 2);
        float dispAreaEndX = dispAreaPosX + (dispAreaWidth / 2);
        float dispAreaEndY = dispAreaPosY + (dispAreaHeight / 2);

        if (Input.mousePosition.x >= dispAreaOriginX && Input.mousePosition.x <= dispAreaEndX)
        {
            if (Input.mousePosition.y >= dispAreaOriginY && Input.mousePosition.y <= dispAreaEndY)
            {
                isOverDispArea = true;
            }
            else
            {
                isOverDispArea = false;
            }
        }
        else
        {
            isOverDispArea = false;
        }

        float screenOffset = (Screen.width - Screen.height) / 2;
        float screenWidth = Screen.width - (Screen.width - Screen.height);

        float mousePosXNormalized = (Input.mousePosition.x - dispAreaOriginX) / dispAreaWidth;
        float mousePosYNormalized = (Input.mousePosition.y - dispAreaOriginY) / dispAreaHeight;

        Vector3 mousePos = new Vector3((mousePosXNormalized * screenWidth) + screenOffset, (mousePosYNormalized * Screen.height), 0);

        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo))
        {
            selectionMarker.SetActive(true);

            Position hitPosition = new Position((int)hitInfo.point.x, (int)hitInfo.point.y);

            selectionMarker.transform.position = hitPosition;

            highlightedTile = tileMap.GetTile(hitPosition);

            //Debug.Log(highlightedTile.data.name);

            if (Input.GetButtonDown("Select") && isOverDispArea)
            {
                if (highlightedTile != null)
                {
                    if (highlightedTile.armies != null && (selectedArmy == null || selectedArmy.position != hitPosition))
                    {
                        selectedArmy = highlightedTile.armies[0];
                        selectedArmies = highlightedTile.armies;

                        if (selectedArmy.owner == gameController.activePlayer)
                        {
                            selectedArmyMarker.transform.SetParent(selectedArmy.mapSprite.transform);
                            selectedArmyMarker.transform.localPosition = Vector3.zero;
                            selectedArmyMarker.SetActive(true);

                            armyManagement.SelectArmy(selectedArmies);
                            isSelected = true;
                        }
                        else
                        {
                            selectedArmy = null;
                            selectedArmyMarker.SetActive(false);
                            selectedArmyMarker.transform.SetParent(null);

                            armyManagement.DeselectArmy();
                        }
                    }
                    else
                    {
                        selectedArmy = null;
                        selectedArmyMarker.SetActive(false);
                        selectedArmyMarker.transform.SetParent(null);

                        armyManagement.DeselectArmy();
                        isSelected = false;

                        if (highlightedTile.city != null)
                        {
                            cityManagement.SelectCity(highlightedTile.city);
                        }
                    }
                }
            }

            if (selectedArmy != null)
            {
                if (Input.GetButtonUp("Move"))
                {
                    if (pathSteps != null && pathSteps[0] != selectedArmy.position)
                    {
                        pathSteps = tileMap.FindPath(selectedArmy.position, hitPosition, selectedArmy);
                    }
                    selectedArmy.SetPath(pathSteps);
                    gameController.StartArmyMove(selectedArmy);

                    ClearPath();

                    previousPathGoal = new Position(-1, -1);
                    // kinda hacky, but this is to ensure the next path after move will always be calculated, no matter if the position is the same or not
                }
                else if (Input.GetButton("Move") && isOverDispArea)
                {
                    if (previousPathGoal != hitPosition || pathSteps != null && pathSteps[0] != selectedArmy.position)
                    {
                        ClearPath();
                        pathSteps = tileMap.FindPath(selectedArmy.position, hitPosition, selectedArmy);
                        DrawPath();
                        previousPathGoal = hitPosition;
                    }
                }
            }
            else
            {
                if (Input.GetButtonDown("Info") && isOverDispArea)
                {
                    Debug.Log(highlightedTile);
                    if (highlightedTile != null && highlightedTile.armies != null)
                    {
                        pathSteps = highlightedTile.armies[0].path;
                        DrawPath();
                    }
                }
                else if (Input.GetButtonUp("Info"))
                {
                    ClearPath();
                }
            }

            if (Input.GetKeyDown(KeyCode.R) && highlightedTile != null && highlightedTile.city != null)
            {
                if (highlightedTile.city.owner == gameController.activePlayer)
                {
                    highlightedTile.city.Raze();
                }
            }
        }
        else
        {
            selectionMarker.SetActive(false);

            if (pathSteps != null)
            {
                ClearPath();

                previousPathGoal = new Position(-1, -1);
                // kinda hacky, but this is to ensure the next path after mouse returns to legal position (over TileMap collider) will always be calculated
            }
        }
    }

    private void DrawPath()
    {
        if (pathSteps != null)
        {
            foreach (Position step in pathSteps)
            {
                pathMarkers.Add(Instantiate(pathStepMarker, step, Quaternion.identity));
            }
        }
    }

    private void ClearPath()
    {
        foreach (GameObject go in pathMarkers)
        {
            Destroy(go);
        }
        pathMarkers.Clear();
        pathSteps = null;
    }
}
