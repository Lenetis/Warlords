using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static System.Math;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TileMap : MonoBehaviour
{
    public int width;
    public int height;
    
    public int tileSize;

    public string[] jsons;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    private Tile[,] tiles;

    public Texture2D miniMapTexture;

    void Start()
    {
        tiles = new Tile[width, height];

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        GenerateMap();

        GenerateMesh();

        meshCollider.sharedMesh = meshFilter.mesh;

        GenerateTexture();
    }

    void GenerateMap()
    {
        // the real actual map generation is a todo, this here is just completely random

        TileData[] availableTiles = new TileData[jsons.Length];

        for (int i = 0; i < jsons.Length; i += 1) {
            
            TileData tile = new TileData(jsons[i]);
            availableTiles[i] = tile;
        }

        for (int x = 0; x < width; x += 1) {
            for (int y = 0; y < height; y += 1) {
                tiles[x, y] = new Tile(availableTiles[Random.Range(0, availableTiles.Length)]);
            }
        }

        // todo tmp
        Player player1 = new Player("Assets/Resources/Players/defaultPlayer.json", "Summoners", Color.cyan);
        tiles[1, 1] = new Tile(availableTiles[1]);
        List<Unit> tmpUnitList = new List<Unit>();
        tmpUnitList.Add(new Unit("Assets/Resources/Units/scout.json"));
        tmpUnitList.Add(new Unit("Assets/Resources/Units/knight.json"));
        tmpUnitList.Add(new Unit("Assets/Resources/Units/scout.json"));
        Army tmpArmy;
        tmpArmy = new Army(tmpUnitList, new Position(1, 1), player1);
        tiles[1, 1].contents.AddArmy(tmpArmy);

        Player player2 = new Player("Assets/Resources/Players/defaultPlayer.json", "Magicians", Color.white);
        tiles[2, 2] = new Tile(availableTiles[0]);
        List<Unit> tmpUnitList2 = new List<Unit>();
        tmpUnitList2.Add(new Unit("Assets/Resources/Units/okoń.json"));
        tmpUnitList2.Add(new Unit("Assets/Resources/Units/okoń.json"));
        Army tmpArmy2;
        tmpArmy2 = new Army(tmpUnitList2, new Position(2, 2), player2);
        tiles[2, 2].contents.AddArmy(tmpArmy2);

        Player player3 = new Player("Assets/Resources/Players/defaultPlayer.json", "Necromancers", Color.red);
        tiles[5, 5] = new Tile(availableTiles[1]);
        List<Unit> tmpUnitList3 = new List<Unit>();
        tmpUnitList3.Add(new Unit("Assets/Resources/Units/scout.json"));
        Army tmpArmy3;
        tmpArmy3 = new Army(tmpUnitList3, new Position(5, 5), player3);
        tiles[5, 5].contents.AddArmy(tmpArmy3);

        City city = new City("Assets/Resources/Cities/city.json", new Position(10, 10), player3);
    }

    void GenerateMesh()
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

    void GenerateTexture()
    {
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.color = new Color(1f, 1f, 1f);
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

    private List<Position> ReconstructPath(Dictionary<Position, Position> cameFrom, Position currentPosition)
    {
        List<Position> completePath = new List<Position>();
        completePath.Add(currentPosition);
        while (cameFrom.ContainsKey(currentPosition)) {
            currentPosition = cameFrom[currentPosition];
            completePath.Insert(0, currentPosition);
        }
        return completePath;
    }

    private int Heuristic(Position pos, Position goal) {
        return Max(Abs(pos.x - goal.x), Abs(pos.y - goal.y));
    }

    public List<Position> FindPath(Position start, Position goal, Army army)
    {
        if (!GetTile(start).pathfindingTypes.Overlaps(army.pathfindingTypes)){
            return null;
        }
        if (!GetTile(goal).pathfindingTypes.Overlaps(army.pathfindingTypes)){
            return null;
        }
        // todo maybe add checks if the goal is not on a very small unreachable island (BFS from goal position with max radius=3 for example)

        // A* Pathfinding

        // The set of discovered nodes that may need to be (re-)expanded.
        // Initially, only the start node is known.
        // This is usually implemented as a min-heap or priority queue rather than a hash-set.
        List<Position> openList = new List<Position>();
        List<Position> closedList = new List<Position>();  // todo change it to one of the above, lists are awful
        openList.Add(start);

        // For node n, cameFrom[n] is the node immediately preceding it on the cheapest path from start
        // to n currently known
        Dictionary<Position, Position> cameFrom = new Dictionary<Position, Position>();
        // todo would it be possible to reuse it when finding paths to different goals from the same start position?

        // For node n, gScore[n] is the cost of the cheapest path from start to n currently known.
        Dictionary<Position, int> gScore = new Dictionary<Position, int>();
        gScore[start] = 0;

        // For node n, fScore[n] := gScore[n] + h(n). fScore[n] represents our current best guess as to
        // how cheap a path could be from start to finish if it goes through n.
        Dictionary<Position, int> fScore = new Dictionary<Position, int>();
        fScore[start] = Heuristic(start, goal);

        while (openList.Count != 0) {
            // This operation can occur in O(Log(N)) time if openSet is a min-heap or a priority queue -- todo            
            // choose position with the lowest fScore
            int minScore = int.MaxValue;
            Position currentPosition = openList[0];
            foreach (Position pos in openList) {
                if (fScore[pos] < minScore) {
                    minScore = fScore[pos];
                    currentPosition = pos;
                }
            }
            
            if (currentPosition == goal) {
                return ReconstructPath(cameFrom, currentPosition);
            }

            openList.Remove(currentPosition);
            foreach (Position neighbourPosition in GetNeighbouringPositions(currentPosition)) {
                Tile neighbourTile = GetTile(neighbourPosition);
                if ((neighbourTile.owner == null || neighbourTile.owner == army.owner) && neighbourTile.pathfindingTypes.Overlaps(army.pathfindingTypes)) {

                    // tentativeGScore is the distance from start to the neighbor through current
                    int tentativeGScore = gScore[currentPosition] + neighbourTile.moveCost;
                    if (!gScore.ContainsKey(neighbourPosition) || tentativeGScore < gScore[neighbourPosition]) {
                        // This path to neighbor is better than any previous one. Record it!
                        cameFrom[neighbourPosition] = currentPosition;
                        gScore[neighbourPosition] = tentativeGScore;
                        fScore[neighbourPosition] = tentativeGScore + Heuristic(neighbourPosition, goal);
                        if (!openList.Contains(neighbourPosition)) {
                            openList.Add(neighbourPosition);
                        }
                    }
                }
            }
        }
        // Open set is empty but goal was never reached        
        return null;
    }

    public List<Position> GetNeighbouringPositions(Position position)
    {
        List<Position> positions = new List<Position>();
        for (int x = Max(position.x - 1, 0); x <= Min(position.x + 1, width - 1); x += 1) {
            for (int y = Max(position.y - 1, 0); y <= Min(position.y + 1, height - 1); y += 1) {
                Position neighbour = new Position(x, y);
                if (neighbour != position) {
                    positions.Add(neighbour);
                }
            }
        }
        return positions;
    }

    public Tile GetTile(Position position)
    {
        return tiles[position.x, position.y];
    }
}