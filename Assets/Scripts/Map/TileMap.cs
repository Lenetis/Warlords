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


    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    private Tile[,] tiles;


    public string[] jsons;

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
        List<int> tmpUnitList = new List<int>();
        tmpUnitList.Add(2);
        tmpUnitList.Add(4);
        tmpUnitList.Add(5);
        Army tmpArmy;
        tmpArmy = new Army(tmpUnitList, new Position(1, 1));
        tiles[1, 1].contents.AddArmy(tmpArmy);

        GameObject sprite = GameObject.Find("warlord");
        tmpArmy.mapSprite = sprite;
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
                Color[] pixels = tiles[x, y].data.texture.GetPixels(0, 0, tileSize, tileSize);
                texture.SetPixels(x * tileSize, y * tileSize, tileSize, tileSize, pixels);
            }
        }
        texture.Apply();

        meshRenderer.material.mainTexture = texture;
    }

    private List<Position> ReconstructPath(Dictionary<Position, Position> cameFrom, Position current)
    {
        List<Position> completePath = new List<Position>();
        completePath.Add(current);
        while (cameFrom.ContainsKey(current)) {
            current = cameFrom[current];
            completePath.Insert(0, current);
        }
        return completePath;
    }

    private int Heuristic(Position start, Position goal) {  // todo maybe replace ints with floats?
        return Max(Abs(start.x - goal.x), Abs(start.y - goal.y));
    }

    public List<Position> FindPath(Position start, Position goal, string pathfindingType="Land")
    {
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
            Position current = openList[0];
            foreach (Position pos in openList) {
                if (fScore[pos] < minScore) {
                    minScore = fScore[pos];
                    current = pos;
                }
            }  // todo check pathfinding type
            
            if (current == goal) {
                return ReconstructPath(cameFrom, current);
            }

            openList.Remove(current);
            foreach (Position neighbour in GetNeighbouringPositions(current)) {
                // d(current,neighbor) is the weight of the edge from current to neighbor
                // tentativeGScore is the distance from start to the neighbor through current
                int tentativeGScore = gScore[current] + (int)GetTile(neighbour).data.moveCost;
                if (!gScore.ContainsKey(neighbour) || tentativeGScore < gScore[neighbour]) {
                    // This path to neighbor is better than any previous one. Record it!
                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentativeGScore;
                    fScore[neighbour] = tentativeGScore + Heuristic(neighbour, goal);
                    if (!openList.Contains(neighbour)) {
                        openList.Add(neighbour);
                    }

                    if (current == goal) {
                        return ReconstructPath(cameFrom, current);
                    }
                }
            }
        }

        // Open set is empty but goal was never reached        
        return null;


        /*
        List<Position> pathSteps = new List<Position>();
        Position step = start;
        while(step != end){
            if (step.x > end.x) {
                step.x -= 1;
            } else if (step.x < end.x) {
                step.x += 1;
            }

            if (step.y > end.y) {
                step.y -= 1;
            } else if (step.y < end.y) {
                step.y += 1;
            }

            pathSteps.Add(step);
        }
        return pathSteps;
        */
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
        return tiles[position.x, position.y];  // todo check if not outside of bounds
    }
}