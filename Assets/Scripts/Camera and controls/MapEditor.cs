using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class MapEditor : MonoBehaviour
{
    private GameController gameController;
    private TileMap tileMap;
    private MouseSelection mouseSelection;

    public string[] tilePaths;
    public string[] unitPaths;
    public string[] cityPaths;

    private TileData[] availableTiles;
    private int activeTileIndex = 0;
    private int activeUnitIndex = 0;
    private int activeCityIndex = 0;
    
    private int brushSize = 0;

    private bool enabledSymmetryX = false;
    private bool enabledSymmetryY = false;
    private bool enabledSymmetryPoint = false;

    /// Start is called before the first frame update
    void Start()
    {
        gameController = FindObjectOfType<GameController>();
        tileMap = gameController.tileMap;
        mouseSelection = GetComponent<MouseSelection>();

        // todo remove code repetition
        availableTiles = new TileData[tilePaths.Length];
        for (int i = 0; i < tilePaths.Length; i += 1) {
            TileData tile = new TileData(ResourceManager.LoadResource(tilePaths[i]));
            availableTiles[i] = tile;
        }
    }

    /// Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1)) {
            Draw();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            PlaceUnits();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            PlaceCities();
        }
        if (Input.GetKey(KeyCode.Alpha4)) {
            PlaceRoads();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            PlaceSignposts();
        }
        if (Input.GetKey(KeyCode.Delete)) {
            ClearTiles();
        }

        if (Input.GetKeyDown(KeyCode.X)) {
            enabledSymmetryPoint = false;
            enabledSymmetryX = !enabledSymmetryX;
            Debug.Log($"X Symmetry Enabled: {enabledSymmetryX}");
        }
        if (Input.GetKeyDown(KeyCode.Y)) {
            enabledSymmetryPoint = false;
            enabledSymmetryY = !enabledSymmetryY;
            Debug.Log($"Y Symmetry Enabled: {enabledSymmetryY}");
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            enabledSymmetryX = false;
            enabledSymmetryY = false;
            enabledSymmetryPoint = !enabledSymmetryPoint;
            Debug.Log($"Point Symmetry Enabled: {enabledSymmetryPoint}");
        }

        if (Input.GetKeyDown(KeyCode.Period)) {
            activeTileIndex = MathUtilities.Modulo(activeTileIndex + 1, availableTiles.Length);
            Debug.Log(availableTiles[activeTileIndex].baseFile);
        }
        if (Input.GetKeyDown(KeyCode.Comma)) {
            activeTileIndex = MathUtilities.Modulo(activeTileIndex - 1, availableTiles.Length);
            Debug.Log(availableTiles[activeTileIndex].baseFile);
        }

        if (Input.GetKeyDown(KeyCode.RightBracket)) {
            activeUnitIndex = MathUtilities.Modulo(activeUnitIndex + 1, unitPaths.Length);
            Debug.Log(unitPaths[activeUnitIndex]);
        }
        if (Input.GetKeyDown(KeyCode.LeftBracket)) {
            activeUnitIndex = MathUtilities.Modulo(activeUnitIndex - 1, unitPaths.Length);
            Debug.Log(unitPaths[activeUnitIndex]);
        }

        if (Input.GetKeyDown(KeyCode.Equals)) {
            ChangeBrushSize(1);
        }
        if (Input.GetKeyDown(KeyCode.Minus)) {
            ChangeBrushSize(-1);
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            tileMap.Resize(200, 200);  // todo - tmp, this should be moved elsewhere and the size should be provided by user ofc
        }
    }

    /// Changes the brush size by the specified delta. Sets brushSize to 0 if lower or equal than 0
    private void ChangeBrushSize(int delta)
    {
        brushSize += delta;
        if (brushSize < 0) {
            brushSize = 0;
        }
    }

    /// Draws on tileMap with the selected tile. Works with symmetry
    private void Draw()
    {
        List<Position> positions = tileMap.GetNeighbouringPositions(mouseSelection.highlightedPosition, brushSize);
        positions.Add(mouseSelection.highlightedPosition);
        foreach (Position position in positions) {
            foreach (Position symmetryPosition in GetSymmetryPositions(position)) {
                tileMap.SetTileData(availableTiles[activeTileIndex], symmetryPosition);
            }
        }
        tileMap.ApplyTexture();
    }

    /// Places selected unit on the tileMap. Works with symmetry
    private void PlaceUnits()
    {
        foreach (Position symmetryPosition in GetSymmetryPositions(mouseSelection.highlightedPosition)) {
            Unit unit = Unit.FromJObject(ResourceManager.LoadResource(unitPaths[activeUnitIndex]));
            List<Unit> unitList = new List<Unit>();
            unitList.Add(unit);
            Army newArmy = new Army(unitList, symmetryPosition, gameController.activePlayer);
            newArmy.AddToGame();
        }
    }

    /// Places selected road on the tileMap. Works with symmetry
    private void PlaceRoads()
    {
        foreach (Position symmetryPosition in GetSymmetryPositions(mouseSelection.highlightedPosition)) {
            Road road = new Road(ResourceManager.LoadResource("Assets/Resources/Structures/road.json"), symmetryPosition);
            if (road.CanAddToGame()) {
                road.AddToGame();
            }
        }
    }

    /// Places selected signpost on the tileMap. Works with symmetry
    private void PlaceSignposts()
    {
        foreach (Position symmetryPosition in GetSymmetryPositions(mouseSelection.highlightedPosition)) {
            Signpost signpost = new Signpost(ResourceManager.LoadResource("Assets/Resources/Structures/signpost.json"), symmetryPosition, "Editor Signpost", $"{symmetryPosition}");
            if (signpost.CanAddToGame()) {
                signpost.AddToGame();
            }
        }
    }

    /// Places city on the tileMap. Works with symmetry
    private void PlaceCities()
    {
        List<Position> positions = new List<Position>();
        Position position = mouseSelection.highlightedPosition;
        positions.Add(position);
        if (enabledSymmetryX) {
            positions.Add(new Position(tileMap.width - position.x - 1 - 1, position.y));
        }
        if (enabledSymmetryY) {
            positions.Add(new Position(position.x, tileMap.height - position.y - 1 - 1));
        }
        if ((enabledSymmetryX && enabledSymmetryY) || enabledSymmetryPoint) {
            positions.Add(new Position(tileMap.width - position.x - 1 - 1, tileMap.height - position.y - 1 - 1));
        }
        // cities occupy more than one tile and the city position is its bottom left, not in the center, so the normal symmetry wouldn't work 
        // probably a //todo

        foreach (Position symmetryPosition in positions) {
            City city = new City(ResourceManager.LoadResource(cityPaths[activeCityIndex]), gameController.activePlayer, "Editor City", "No description", symmetryPosition);
            if (city.CanAddToGame()) {
                city.AddToGame();
            }
        }
    }

    /// Destroys the contents of tiles (armies and cities, in the future also roads, temples, etc). Works with symmetry
    private void ClearTiles()
    {
        foreach (Position symmetryPosition in GetSymmetryPositions(mouseSelection.highlightedPosition)) {
            Tile symmetryTile = tileMap.GetTile(symmetryPosition);
            symmetryTile.Clear();
        }
    }

    /// Returns a list of positions symmetrical to position. The symmetry depends on variables: enabledSymmetryX,  enabledSymmetryY,  enabledSymmetryPoint
    private List<Position> GetSymmetryPositions(Position position)
    {
        List<Position> positions = new List<Position>();
        positions.Add(position);
        if (enabledSymmetryX) {
            positions.Add(new Position(tileMap.width - position.x - 1, position.y));
        }
        if (enabledSymmetryY) {
            positions.Add(new Position(position.x, tileMap.height - position.y - 1));
        }
        if ((enabledSymmetryX && enabledSymmetryY) || enabledSymmetryPoint) {
            positions.Add(new Position(tileMap.width - position.x - 1, tileMap.height - position.y - 1));
        }

        return positions;
    }
}
