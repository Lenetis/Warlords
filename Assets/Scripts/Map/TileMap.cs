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
                Texture2D tileTexture = tiles[x, y].GetRelativeTexture(GetNeighbouringTiles(new Position(x, y)));
                Color[] pixels = tileTexture.GetPixels(0, 0, tileSize, tileSize);
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

        PathfindingNode startNode = PathfindingNode.Get(start, army.pathfindingTypes);

        Heap<PathfindingNode> openSet = new Heap<PathfindingNode>(width * height * (Constants.pathfindingMaxDistinctTransitions + 1));
        HashSet<PathfindingNode> closedSet = new HashSet<PathfindingNode>();
        openSet.Add(startNode);

        Dictionary<PathfindingNode, PathfindingNode> cameFrom = new Dictionary<PathfindingNode, PathfindingNode>();
        // todo would it be possible to reuse it when finding paths to different goals from the same start position?

        Dictionary<PathfindingNode, int> gScore = new Dictionary<PathfindingNode, int>();
        Dictionary<PathfindingNode, int> fScore = new Dictionary<PathfindingNode, int>();
        gScore[startNode] = 0;
        fScore[startNode] = Heuristic(startNode, goal);

        while (openSet.Count > 0) {
            PathfindingNode currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode.position == goal) {
                return ReconstructPath(cameFrom, currentNode);
            }

            Tile currentTile = GetTile(currentNode.position);

            foreach (Position neighbourPosition in GetNeighbouringPositions(currentNode.position)) {
                Tile neighbourTile = GetTile(neighbourPosition);

                if (neighbourPosition == goal && neighbourTile.owner != null && neighbourTile.owner != army.owner) {
                    // attacking tiles owned by other players
                    PathfindingNode goalNode = PathfindingNode.Get(neighbourPosition, currentNode.pathfindingTypes);
                    cameFrom[goalNode] = currentNode;
                    return ReconstructPath(cameFrom, goalNode);
                }

                HashSet<string>[] neighbourPathfindingTypes;
                if (currentNode.pathfindingTypes == army.basePathfindingTypes) {
                    // army is not transitioned
                    if (currentTile.transition != null && currentNode.pathfindingTypes.IsSupersetOf(currentTile.transition.from)) {
                        neighbourPathfindingTypes = new HashSet<string>[] {
                            currentNode.pathfindingTypes,
                            currentTile.transition.to
                        };
                    }
                    else {
                        neighbourPathfindingTypes = new HashSet<string>[] {
                            currentNode.pathfindingTypes
                        };
                    }
                }
                else {
                    // army is transitioned
                    if (neighbourTile.transitionReturn != null && currentNode.pathfindingTypes.IsSupersetOf(neighbourTile.transitionReturn.from)) {
                        neighbourPathfindingTypes = new HashSet<string>[] {
                            currentNode.pathfindingTypes,
                            army.basePathfindingTypes
                        };
                    } else {
                        neighbourPathfindingTypes = new HashSet<string>[] {
                            currentNode.pathfindingTypes
                        };
                    }
                }

                foreach (HashSet<string> pathfindingType in neighbourPathfindingTypes) {
                    if (neighbourTile.pathfindingTypes.Overlaps(pathfindingType)) {
                        PathfindingNode neighbourNode = PathfindingNode.Get(neighbourPosition, pathfindingType);
                    
                        if (!closedSet.Contains(neighbourNode) && (neighbourTile.owner == null || neighbourTile.owner == army.owner)) {
                            int tentativeGScore = currentNode.gCost + neighbourTile.moveCost;
                            if (neighbourNode.pathfindingTypes != currentNode.pathfindingTypes) {
                                tentativeGScore += Constants.pathfindingTransitionCost;
                            }
                            if (tentativeGScore < neighbourNode.gCost || !openSet.Contains(neighbourNode)) {
                                neighbourNode.gCost = tentativeGScore;
                                neighbourNode.hCost = Heuristic(neighbourNode, goal);

                                cameFrom[neighbourNode] = currentNode;

                                if (!openSet.Contains(neighbourNode)) {
                                    openSet.Add(neighbourNode);
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

    /// Returns a Neighbours<Tile> struct with all tiles around the given position
    public Neighbours<Tile> GetNeighbouringTiles(Position position)
    {
        Neighbours<Tile> neighbours = new Neighbours<Tile>();
        int x = position.x;
        int y = position.y;

        if (x - 1 >= 0) {
            neighbours.left = GetTile(new Position(x - 1, y));
            if (y - 1 >= 0) {
                neighbours.bottomLeft = GetTile(new Position(x - 1, y - 1));
            }
            if (y + 1 < height) {
                neighbours.topLeft = GetTile(new Position(x - 1, y + 1));
            }
        }

        if (x + 1 < width) {
            neighbours.right = GetTile(new Position(x + 1, y));
            if (y - 1 >= 0) {
                neighbours.bottomRight = GetTile(new Position(x + 1, y - 1));
            }
            if (y + 1 < height) {
                neighbours.topRight = GetTile(new Position(x + 1, y + 1));
            }
        }

        if (y - 1 >= 0) {
            neighbours.bottom = GetTile(new Position(x, y - 1));
        }
        if (y + 1 < height) {
            neighbours.top = GetTile(new Position(x, y + 1));
        }

        return neighbours;
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

    /// Sets the tile data (not its contents) at the given position to the provided tile
    /// If applyTexture is set to true, automatically updates the texture. If not, you need to call ApplyTexture() afterwards
    public void SetTileData(TileData tileData, Position position, bool applyTexture = false)
    {
        tiles[position.x, position.y].data = tileData;

        List<Position> updateTexturesPositions = GetNeighbouringPositions(position);
        updateTexturesPositions.Add(position);

        Texture2D texture = (Texture2D)meshRenderer.material.mainTexture;

        foreach (Position pos in updateTexturesPositions) {
            Texture2D tileTexture = tiles[pos.x, pos.y].GetRelativeTexture(GetNeighbouringTiles(pos));
            Color[] pixels = tileTexture.GetPixels(0, 0, tileSize, tileSize);
            texture.SetPixels(pos.x * tileSize, pos.y * tileSize, tileSize, tileSize, pixels);
        }

        if (applyTexture) {
            texture.Apply();
        }
    }

    /// Creates a new tileMap from JObject   // todo maybe return new TileMap instead of void?
    public void FromJObject(JObject attributes)
    {
        ResourceManager.ExpandWithBaseFile(attributes);

        TileMapResizedEventData eventData;
        eventData.oldWidth = width;
        eventData.oldHeight = height;

        width = (int)attributes.GetValue("width");
        height = (int)attributes.GetValue("height");
        
        eventData.newWidth = width;
        eventData.newHeight = height;

        tiles = new Tile[width, height];

        string mapString = (string)attributes.GetValue("mapString");
        List<TileData> tileTypes = new List<TileData>();
        foreach (string tilePath in attributes.GetValue("tileTypes")) {
            tileTypes.Add(new TileData(ResourceManager.LoadResource(tilePath)));
        }

        if (mapString.Length / digitsPerTile != width * height) {
            Debug.LogWarning("Warning: invalid mapString length.");

            // the map will be loaded incorrectly, but fill it with tiles before loading so at least it won't throw a NullReferenceException later on
            for (int y = 0; y < height; y += 1) {
                for (int x = 0; x < width; x += 1) {
                    tiles[x, y] = new Tile(tileTypes[0]);
                }
            }
        }

        for (int i = 0; i < Min(mapString.Length / digitsPerTile, width * height); i += 1) {
            int index = System.Convert.ToInt32(mapString.Substring(i * digitsPerTile, digitsPerTile), 16);
            int x = i % width;
            int y = i / width;

            tiles[x, y] = new Tile(tileTypes[index]);
        }

        GenerateMesh();

        meshCollider.sharedMesh = meshFilter.mesh;

        GenerateTexture();

        EventManager.OnTileMapResized(this, eventData);
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

        for (int y = 0; y < height; y += 1) {
            for (int x = 0; x < width; x += 1) {
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