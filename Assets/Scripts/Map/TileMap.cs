using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static System.Math;

using System.Linq;
using Newtonsoft.Json.Linq;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TileMap : MonoBehaviour  // todo remove MonoBehaviour maybe? change into a static class?
{
    private const int digitsPerTile = 2;  // how many hex digits represent a single tile in save file

    public int width;
    public int height;
    
    public static double textureScale = 1;
    public static int tileSize = (int)(32 * textureScale);

    public string[] jsons;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    private Tile[,] tiles;

    public Texture2D miniMapTexture;


    private readonly struct PathfindingNode
    {
        public readonly Position position;
        public readonly HashSet<string> pathfindingTypes;  // todo change to ImmutableHashSet maybe?
        public PathfindingNode(Position position, HashSet<string> pathfindingTypes)
        {
            this.position = position;
            this.pathfindingTypes = pathfindingTypes;
        }
    }

    /// Start is called before the first frame update
    void Start()
    {
        tiles = new Tile[width, height];

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    /// Initializes the tile map - generates map, mesh, texture and sets some test units
    public void Initialize()
    {
        GenerateMap();

        GenerateMesh();

        meshCollider.sharedMesh = meshFilter.mesh;

        GenerateTexture();
    }

    /// TMP method that generates a completely random map //todo
    private void GenerateMap()
    {
        // the real actual map generation is a todo, this here is just completely random

        TileData[] availableTiles = new TileData[jsons.Length];

        for (int i = 0; i < jsons.Length; i += 1) {
            
            TileData tile = new TileData(ResourceManager.LoadResource(jsons[i]));
            availableTiles[i] = tile;
        }

        for (int x = 0; x < width; x += 1) {
            for (int y = 0; y < height; y += 1) {
                tiles[x, y] = new Tile(availableTiles[Random.Range(0, availableTiles.Length)]);
            }
        }
    }

    /// Generates mesh that the texture will be displayed on. Must be called after map generation
    private void GenerateMesh()
    {
        Vector3[] vertices = new Vector3[4];
        int[] triangles = new int[2 * 3];
        Vector3[] normals = new Vector3[4];
        Vector2[] uv = new Vector2[4];

        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(width, 0, 0);
        vertices[2] = new Vector3(0, height, 0);
        vertices[3] = new Vector3(width, height, 0);

        triangles[0] = 3;
        triangles[1] = 1;
        triangles[2] = 0;

        triangles[3] = 2;
        triangles[4] = 3;
        triangles[5] = 0;

        normals[0] = Vector3.back;
        normals[1] = Vector3.back;
        normals[2] = Vector3.back;
        normals[3] = Vector3.back;

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;

        meshFilter.mesh = mesh;
    }

    /// Generates map texture to display on the map mesh
    private void GenerateTexture()
    {
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.color = new Color(1f, 1f, 1f);
        meshRenderer.material.shader = Shader.Find("Sprites/Default");
        // todo maybe copy a base template material instead of generating a new material here

        Texture2D texture = new Texture2D(width * tileSize, height * tileSize);
        texture.filterMode = FilterMode.Point;

        for (int x = 0; x < width; x += 1) {
            for (int y = 0; y < height; y += 1) {
                Color[] pixels = tiles[x, y].texture.GetPixels(0, 0, tileSize, tileSize);
                texture.SetPixels(x * tileSize, y * tileSize, tileSize, tileSize, pixels);
            }
        }
        texture.Apply();

        meshRenderer.material.mainTexture = texture;
        miniMapTexture = texture;
    }

    /// Applies the modified texture. Useful when you want to SetTile more than one tile at the same time without applying the texture after each SetTile call
    public void ApplyTexture()
    {
        Texture2D texture = (Texture2D)meshRenderer.material.mainTexture;
        texture.Apply();
    }

    /// Changes the size of the tileMap to new size. Leaves the old tiles the same where applicable and fills the new tiles with the same tile type as at [0, 0]
    public void Resize(int newWidth, int newHeight)
    {
        Tile[,] newTiles = new Tile[newWidth, newHeight];

        for (int x = 0; x < newWidth; x += 1) {
            for (int y = 0; y < newHeight; y += 1) {
                newTiles[x, y] = new Tile(tiles[0, 0].data);
            }
        }

        for (int x = 0; x < Min(width, newWidth); x += 1) {
            for (int y = 0; y < Min(height, newHeight); y += 1) {
                newTiles[x, y] = tiles[x, y];
            }
        }

        TileMapResizedEventData eventData;
        eventData.oldWidth = width;
        eventData.oldHeight = height;
        eventData.newWidth = newWidth;
        eventData.newHeight = newHeight;

        tiles = newTiles;
        width = newWidth;
        height = newHeight;

        GenerateMesh();

        meshCollider.sharedMesh = meshFilter.mesh;

        GenerateTexture();

        // todo remove this when handling of TileMapResizedEvent is added
        Minimap minimapUI = GameObject.Find("Main").GetComponent<Minimap>();
        minimapUI.width = newWidth;
        minimapUI.height = newHeight;
        minimapUI.isTileMapLoaded = false;
        CameraController controller = Camera.main.GetComponent<CameraController>();
        controller.mapWidth = newWidth;
        controller.mapHeight = newHeight;

        EventManager.OnTileMapResized(this, eventData);
    }

    /// Returns path from the start position of pathfinding to currentPosition
    private List<Position> ReconstructPath(Dictionary<PathfindingNode, PathfindingNode> cameFrom, PathfindingNode currentNode)
    {
        List<Position> completePath = new List<Position>();
        completePath.Add(currentNode.position);
        while (cameFrom.ContainsKey(currentNode)) {
            currentNode = cameFrom[currentNode];
            completePath.Insert(0, currentNode.position);
        }
        completePath.RemoveAt(0);  // remove the first position because we don't need to move to where we already are
        return completePath;
    }

    /// Returns estimated cost of moving from currentPosition to goal
    private int Heuristic(PathfindingNode currentNode, Position goal) {
        return Max(Abs(currentNode.position.x - goal.x), Abs(currentNode.position.y - goal.y));
    }

    /// Returns a shortest path from start to goal for a given army. Returns null if no path exists
    public List<Position> FindPath(Position start, Position goal, Army army)
    {
        if (start == goal) {
            return null;
        }

        PathfindingNode startNode = new PathfindingNode(start, army.pathfindingTypes);

        // A* Pathfinding

        // The set of discovered nodes that may need to be (re-)expanded.
        // Initially, only the start node is known.
        // This is usually implemented as a min-heap or priority queue rather than a hash-set.
        List<PathfindingNode> openList = new List<PathfindingNode>();
        List<PathfindingNode> closedList = new List<PathfindingNode>();  // todo change it to one of the above, lists are awful
        openList.Add(startNode);

        // For node n, cameFrom[n] is the node immediately preceding it on the cheapest path from start
        // to n currently known
        Dictionary<PathfindingNode, PathfindingNode> cameFrom = new Dictionary<PathfindingNode, PathfindingNode>(); // todo would it be possible to reuse it when finding paths to different goals from the same start position?

        // For node n, gScore[n] is the cost of the cheapest path from start to n currently known.
        Dictionary<PathfindingNode, int> gScore = new Dictionary<PathfindingNode, int>();
        gScore[startNode] = 0;

        // For node n, fScore[n] := gScore[n] + h(n). fScore[n] represents our current best guess as to
        // how cheap a path could be from start to finish if it goes through n.
        Dictionary<PathfindingNode, int> fScore = new Dictionary<PathfindingNode, int>();
        fScore[startNode] = Heuristic(startNode, goal);

        Tile startTile = GetTile(start);
        if (army.isTransitioned) {
            if (startTile.transitionReturn != null && army.pathfindingTypes.IsSupersetOf(startTile.transitionReturn.from)) {
                PathfindingNode alternativeStartNode = new PathfindingNode(start, army.basePathfindingTypes);
                openList.Add(alternativeStartNode);
                gScore[alternativeStartNode] = 0;
                fScore[alternativeStartNode] = Heuristic(alternativeStartNode, goal);
            }
        }
        else {
            if (startTile.transition != null && army.basePathfindingTypes.IsSupersetOf(startTile.transition.from)) {
                PathfindingNode alternativeStartNode = new PathfindingNode(start, startTile.structure.pathfinding.transition.to);
                openList.Add(alternativeStartNode);
                gScore[alternativeStartNode] = 0;
                fScore[alternativeStartNode] = Heuristic(alternativeStartNode, goal);
            }            
        }

        while (openList.Count != 0) {
            // This operation can occur in O(Log(N)) time if openSet is a min-heap or a priority queue -- todo            
            // choose position with the lowest fScore
            int minScore = int.MaxValue;
            PathfindingNode currentNode = openList[0];
            foreach (PathfindingNode node in openList) {
                if (fScore[node] < minScore) {
                    minScore = fScore[node];
                    currentNode = node;
                }
            }
            
            if (currentNode.position == goal) {
                return ReconstructPath(cameFrom, currentNode);
            }

            openList.Remove(currentNode);
            foreach (Position neighbourPosition in GetNeighbouringPositions(currentNode.position)) {
                Tile neighbourTile = GetTile(neighbourPosition);


                HashSet<string>[] neighbourPathfindingTypes;
                if (currentNode.pathfindingTypes == army.basePathfindingTypes) {
                    // army is not transitioned
                    if (neighbourTile.transition != null && currentNode.pathfindingTypes.IsSupersetOf(neighbourTile.transition.from)) {
                        neighbourPathfindingTypes = new HashSet<string>[] {currentNode.pathfindingTypes, neighbourTile.transition.to};
                    } else {
                        neighbourPathfindingTypes = new HashSet<string>[] {currentNode.pathfindingTypes};
                    }
                }
                else {
                    // army is transitioned
                    if (neighbourTile.transitionReturn != null && currentNode.pathfindingTypes.IsSupersetOf(neighbourTile.transitionReturn.from)) {
                        neighbourPathfindingTypes = new HashSet<string>[] {currentNode.pathfindingTypes, army.basePathfindingTypes};
                    } else {
                        neighbourPathfindingTypes = new HashSet<string>[] {currentNode.pathfindingTypes};
                    }
                }

                foreach (HashSet<string> pathfindingType in neighbourPathfindingTypes) {

                    if (neighbourTile.pathfindingTypes.Overlaps(pathfindingType)) {
                        PathfindingNode neighbourNode = new PathfindingNode(neighbourPosition, pathfindingType);

                        if (neighbourPosition == goal) {
                            cameFrom[neighbourNode] = currentNode;
                            return ReconstructPath(cameFrom, neighbourNode);
                        }
                        else if ((neighbourTile.owner == null || neighbourTile.owner == army.owner)) {

                            // tentativeGScore is the distance from start to the neighbor through current
                            int tentativeGScore = gScore[currentNode] + neighbourTile.moveCost;
                            if (!gScore.ContainsKey(neighbourNode) || tentativeGScore < gScore[neighbourNode]) {
                                // This path to neighbor is better than any previous one. Record it!
                                cameFrom[neighbourNode] = currentNode;
                                gScore[neighbourNode] = tentativeGScore;
                                fScore[neighbourNode] = tentativeGScore + Heuristic(neighbourNode, goal);
                                if (!openList.Contains(neighbourNode)) {
                                    openList.Add(neighbourNode);
                                }
                            }
                        }
                    }
                }
            }
        }
        // Open set is empty but goal was never reached        
        return null;
    }

    /// Returns a list of positions around the given position within the specified distance
    public List<Position> GetNeighbouringPositions(Position position, int distance = 1)
    {
        List<Position> positions = new List<Position>();
        if (distance == 1) {
            // if distance == 1, we probably want to use these positions for pathfinding.
            //     It feels more natural to move only horizontally/vertically instead of diagonally (if the paths are otherwise equal), so we need to get the tiles in a specific order
            int x = position.x;
            int y = position.y;

            if (x - 1 >= 0) {
                positions.Add(new Position(x - 1, y));
            }
            if (x + 1 < width) {
                positions.Add(new Position(x + 1, y));
            }
            if (y - 1 >= 0) {
                positions.Add(new Position(x, y - 1));
            }
            if (y + 1 < height) {
                positions.Add(new Position(x, y + 1));
            }

            if (x - 1 >= 0) {
                if(y - 1 >= 0) {
                    positions.Add(new Position(x - 1, y - 1));
                }
                if (y + 1 < height) {
                   positions.Add(new Position(x - 1, y + 1));
                }
            }
            if (x + 1 < width) {
                if (y + 1 < height) {
                    positions.Add(new Position(x + 1, y + 1));
                }
                if (y - 1 >= 0) {
                   positions.Add(new Position(x + 1, y - 1));
                }
            }
        }
        else {
            for (int x = Max(position.x - distance, 0); x <= Min(position.x + distance, width - 1); x += 1) {
                for (int y = Max(position.y - distance, 0); y <= Min(position.y + distance, height - 1); y += 1) {
                    Position neighbour = new Position(x, y);
                    if (neighbour != position) {
                        positions.Add(neighbour);
                    }
                }
            }
        }
        return positions;
    }

    /// Returns true if startPosition is adjacent to endPosition
    public bool Adjacent(Position startPosition, Position endPosition)
    {
        if (!GetNeighbouringPositions(startPosition).Contains(endPosition)) {
            return false;
        }
        return true;
    }

    /// Returns the tile at the given position
    public Tile GetTile(Position position)
    {
        return tiles[position.x, position.y];
    }

    /// Sets the tile at the given position to the provided tile
    /// If applyTexture is set to true, automatically updates the texture. If not, you need to call ApplyTexture() afterwards
    public void SetTile(Tile tile, Position position, bool applyTexture = false)
    {
        tiles[position.x, position.y] = tile;
        Color[] pixels = tile.texture.GetPixels(0, 0, tileSize, tileSize);
        Texture2D texture = (Texture2D)meshRenderer.material.mainTexture;
        texture.SetPixels(position.x * tileSize, position.y * tileSize, tileSize, tileSize, pixels);

        if (applyTexture) {
            texture.Apply();
        }
    }

    /// Sets the tile data (not its contents) at the given position to the provided tile
    /// If applyTexture is set to true, automatically updates the texture. If not, you need to call ApplyTexture() afterwards
    public void SetTileData(TileData tileData, Position position, bool applyTexture = false)
    {
        tiles[position.x, position.y].data = tileData;
        Color[] pixels = tileData.texture.GetPixels(0, 0, tileSize, tileSize);
        Texture2D texture = (Texture2D)meshRenderer.material.mainTexture;
        texture.SetPixels(position.x * tileSize, position.y * tileSize, tileSize, tileSize, pixels);

        if (applyTexture) {
            texture.Apply();
        }
    }

    /// Creates a new tileMap from JObject   // todo maybe return new TileMap instead of void?
    public void FromJObject(JObject attributes)
    {
        ResourceManager.ExpandWithBaseFile(attributes);

        width = (int)attributes.GetValue("width");
        height = (int)attributes.GetValue("height");

        tiles = new Tile[width, height];

        string mapString = (string)attributes.GetValue("mapString");
        List<TileData> tileTypes = new List<TileData>();
        foreach (string tilePath in attributes.GetValue("tileTypes")) {
            tileTypes.Add(new TileData(ResourceManager.LoadResource(tilePath)));
        }

        if (mapString.Length / digitsPerTile != width * height) {
            Debug.LogWarning("Warning: invalid mapString length.");

            // the map will be loaded incorrectly, but fill it with tiles before loading so at least it won't throw a NullReferenceException later on
            for (int x = 0; x < width; x += 1) {
                for (int y = 0; y < height; y += 1) {
                    tiles[x, y] = new Tile(tileTypes[0]);
                }
            }
        }

        for (int i = 0; i < Min(mapString.Length / digitsPerTile, width * height); i += 1) {
            int index = System.Convert.ToInt32(mapString.Substring(i * digitsPerTile, digitsPerTile), 16);
            int x = i / height;
            int y = i % height;

            tiles[x, y] = new Tile(tileTypes[index]);
        }

        GenerateMesh();

        meshCollider.sharedMesh = meshFilter.mesh;

        GenerateTexture();
    }

    /// Serializes the TileMap into a JObject
    public JObject ToJObject()
    {
        JObject tileMapJObject = new JObject();

        tileMapJObject.Add("width", width);
        tileMapJObject.Add("height", height);

        List<string> tileTypes = new List<string>();
        // todo maybe use something else than a list (O(1) access that keeps ordering)

        string mapString = "";

        // todo digitsPerTile could be calculated based on tileTypes.Count, but that would require iterating over all tiles two times

        for (int x = 0; x < width; x += 1) {
            for (int y = 0; y < height; y += 1) {
                Tile tile = tiles[x, y];
                if (!tileTypes.Contains(tile.data.baseFile)) {
                    tileTypes.Add(tile.data.baseFile);
                }
                mapString += tileTypes.IndexOf(tile.data.baseFile).ToString($"X{digitsPerTile}");
            }
        }

        tileMapJObject.Add("tileTypes", new JArray(tileTypes));
        tileMapJObject.Add("mapString", mapString);

        return tileMapJObject;
    }
}