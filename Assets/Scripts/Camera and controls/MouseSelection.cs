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
    public Position highlightedPosition;

    private ArmyManagement armyManagement;
    private CityManagement cityManagement;

    public bool isOverDispArea;

    public GameObject gameMenu;

    private UIController uiController;
    private CameraController cameraController;

    [SerializeField]
    private int moveMode = 0;

    void Awake()
    {
        EventManager.ArmyMovedEvent += ArmyMovedHandler;
        EventManager.ArmyDestroyedEvent += ArmyDestroyedHandler;
        EventManager.BattleStartedEvent += BattleStartedHandler;
    }

    void OnDestroy()
    {
        EventManager.ArmyMovedEvent -= ArmyMovedHandler;
        EventManager.ArmyDestroyedEvent -= ArmyDestroyedHandler;
        EventManager.BattleStartedEvent -= BattleStartedHandler;
    }

    void Start()
    {
        tileMap = FindObjectOfType<TileMap>();
        gameController = FindObjectOfType<GameController>();

        pathMarkers = new List<GameObject>();
        cam.SetActive(true);

        armyManagement = GameObject.Find("Main").GetComponent<ArmyManagement>();
        cityManagement = GameObject.Find("Main").GetComponent<CityManagement>();
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();

        previousPathGoal = new Position(-1, -1);
        // kinda hacky, but this is to ensure the first path goal will always be calculated (otherwise path for (0,0) tile wouldn't be searched)
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!gameMenu.activeSelf)
            {
                gameMenu.SetActive(true);
            }
            else
            {
                gameMenu.SetActive(false);
            }
            
        }
        if (Input.GetKeyDown(KeyCode.T) && uiController.controllsAvailable())
        {
            EndTurn();
        }
        if (Input.GetKeyDown(KeyCode.M) && uiController.controllsAvailable())
        {
            gameController.activePlayer.MoveAll();
        }
        if (Input.GetKeyDown(KeyCode.A) && selectedArmy != null && uiController.controllsAvailable())
        {
            MergeTileUnits(selectedArmy);
        }
        if (Input.GetKeyDown(KeyCode.S) && selectedArmy != null && uiController.controllsAvailable())
        {
            SplitTileUnits(selectedArmy);
        }
        if (Input.GetKeyDown(KeyCode.P) && selectedArmy != null && selectedArmy.heroes.Count != 0 && uiController.controllsAvailable())
        {
            if (gameController.tileMap.GetTile(selectedArmy.position).items != null)
            {
                Item itemToPickUp = gameController.tileMap.GetTile(selectedArmy.position).items[0];
                selectedArmy.heroes[0].heroData.PickUpItem(itemToPickUp);
            }
        }
        if (Input.GetKeyDown(KeyCode.D) && selectedArmy != null && selectedArmy.heroes.Count != 0 && uiController.controllsAvailable())
        {
            if (selectedArmy.heroes[0].heroData.items.Count != 0)
            {
                selectedArmy.heroes[0].heroData.DropItem(selectedArmy.heroes[0].heroData.items[0], selectedArmy.position);
            }
        }
        if (Input.GetKeyDown(KeyCode.Z) && selectedArmy != null && selectedArmy.heroes.Count != 0 && uiController.controllsAvailable())
        {
            Tile exploreTile = gameController.tileMap.GetTile(selectedArmy.position);
            if (exploreTile.structure as IExplorable != null) {
                ((IExplorable)exploreTile.structure).Explore(selectedArmy.heroes[0]);
            }
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

        if (Physics.Raycast(ray, out hitInfo) && uiController.controllsAvailable())
        {
            selectionMarker.SetActive(true);

            highlightedPosition = new Position((int)hitInfo.point.x, (int)hitInfo.point.y);

            selectionMarker.transform.position = highlightedPosition;

            highlightedTile = tileMap.GetTile(highlightedPosition);

            if (Input.GetButtonDown("Select") && isOverDispArea && uiController.controllsAvailable() && moveMode!=1)
            {
                if (highlightedTile != null)
                {
                    if (highlightedTile.armies != null && (selectedArmy == null || selectedArmy.position != highlightedPosition))
                    {
                        selectedArmy = highlightedTile.armies[0];

                        if (selectedArmy.owner == gameController.activePlayer)
                        {
                            selectedArmyMarker.transform.SetParent(selectedArmy.mapSprite.transform);
                            selectedArmyMarker.transform.localPosition = Vector3.zero;
                            selectedArmyMarker.SetActive(true);

                            ClearPath();
                            pathSteps = selectedArmy.path;
                            DrawPath();

                            armyManagement.SelectArmy(selectedArmy);
                        }
                        else
                        {  
                            DeselectArmy();
                        }
                    }
                    else
                    {
                        DeselectArmy();
                        
                        if (highlightedTile.structure as City != null)
                        {
                            cityManagement.SelectCity((City)highlightedTile.structure);
                        }
                    }
                }
            }

            if (selectedArmy != null)
            {
                if (Input.GetButtonUp("Move") && uiController.controllsAvailable() && moveMode==0 || Input.GetButton("Select") && uiController.controllsAvailable() && isOverDispArea && moveMode==1)
                {
                    moveMode = 0;
                    selectedArmy.SetPath(pathSteps);
                    gameController.StartArmyMove(selectedArmy);

                    previousPathGoal = new Position(-1, -1);
                    // kinda hacky, but this is to ensure the next path after move will always be calculated, no matter if the position is the same or not
                }
                else if (Input.GetButton("Move") && isOverDispArea && uiController.controllsAvailable() && moveMode==0 || moveMode == 1 && isOverDispArea && uiController.controllsAvailable())
                {
                    if (previousPathGoal != highlightedPosition)
                    {
                        ClearPath();
                        pathSteps = tileMap.FindPath(selectedArmy.position, highlightedPosition, selectedArmy);
                        DrawPath();
                        previousPathGoal = highlightedPosition;
                    }
                }
            }
        }
        else
        {
            selectionMarker.SetActive(false);

            if (pathSteps != null)
            {
                previousPathGoal = new Position(-1, -1);
                // kinda hacky, but this is to ensure the next path after mouse returns to legal position (over TileMap collider) will always be calculated
            }
        }
    }

    private void ArmyMovedHandler(object sender, ArmyMovedEventData eventData)
    {
        Army movedArmy = (Army)sender;
        if (movedArmy == selectedArmy) {
            ClearPath();
            pathSteps = movedArmy.path;        
            DrawPath();
        }
    }

    private void ArmyDestroyedHandler(object sender, System.EventArgs args)
    {
        if ((Army)sender == selectedArmy) {
           DeselectArmy();
        }
    }

    private void BattleStartedHandler(object sender, System.EventArgs args)
    {
        ClearPath();
    }
    
    private void DeselectArmy()
    {
        ClearPath();

        selectedArmy = null;
        selectedArmyMarker.SetActive(false);
        selectedArmyMarker.transform.SetParent(null);

        armyManagement.DeselectArmy();
        moveMode = 0;
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
        if (pathMarkers != null) {
            foreach (GameObject go in pathMarkers)
            {
                Destroy(go);
            }
            pathMarkers.Clear();
        }
        pathSteps = null;
    }

    public void MergeTileUnits(Army selectedArmy)
    {
        Tile mergeTile = tileMap.GetTile(selectedArmy.position);

        int i = 0;
        while (i < mergeTile.armies.Count)
        {
            if (mergeTile.armies[i] != selectedArmy) {
                mergeTile.armies[i].Merge(selectedArmy);
            }
            else {
                i += 1;
            }
        }
        
        armyManagement.DeselectArmy();
        armyManagement.SelectArmy(selectedArmy);
    }

    public void SplitTileUnits(Army selectedArmy)
    {
        selectedArmy.Split();
        Tile splitTile = tileMap.GetTile(selectedArmy.position);
        armyManagement.SelectArmy(selectedArmy);
    }

    public void EndTurn()
    {
        gameController.Turn();
        if (selectedArmy != null && selectedArmy.owner != gameController.activePlayer)
        {
            selectedArmy = null;
            armyManagement.DeselectArmy();
            DeselectArmy();
        }
    }

    public void ButtonMove()
    {
        if (selectedArmy != null)
        {
            moveMode = 1;
        }
    }

    public void NextUnits()
    {
        int currentIndex = 0;

        if (selectedArmy != null)
        {
            currentIndex = gameController.activePlayer.armies.FindIndex(x => x == selectedArmy);
        }

        int index=currentIndex;

        highlightedTile = tileMap.GetTile(gameController.activePlayer.armies[index].position);
        selectedArmy = highlightedTile.armies[0];

        Vector2 unitsPosition = selectedArmy.position;
        for(int i= 0; i < gameController.activePlayer.armies.Count; i++)
        {
            if (selectedArmy.position == gameController.activePlayer.armies[index].position)
            {
                index++;
                if(index>= gameController.activePlayer.armies.Count)
                {
                    index = 0;
                }
            }
            else
            {
                ShowUnits(index)
;           }

            if (unitsPosition == gameController.activePlayer.armies[index].position && i == gameController.activePlayer.armies.Count - 1)
            {
                ShowUnits(index);
            }
        }
    }

    public void ShowUnits(int index)
    {
        cameraController.CheckNSetPosition(new Vector3(gameController.activePlayer.armies[index].position.x, gameController.activePlayer.armies[index].position.y, cam.transform.parent.gameObject.transform.position.z));

        highlightedTile = tileMap.GetTile(gameController.activePlayer.armies[index].position);

        if (highlightedTile != null)
        {
            if (highlightedTile.armies != null && (selectedArmy == null || selectedArmy.position != highlightedPosition))
            {
                selectedArmy = highlightedTile.armies[0];

                if (selectedArmy.owner == gameController.activePlayer)
                {
                    selectedArmyMarker.transform.SetParent(selectedArmy.mapSprite.transform);
                    selectedArmyMarker.transform.localPosition = Vector3.zero;
                    selectedArmyMarker.SetActive(true);

                    ClearPath();
                    pathSteps = selectedArmy.path;
                    DrawPath();

                    armyManagement.SelectArmy(selectedArmy);
                }
                else
                {
                    DeselectArmy();
                }
            }
            else
            {
                DeselectArmy();
            }
        }
    }
}
