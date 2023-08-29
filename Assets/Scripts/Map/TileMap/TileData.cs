using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class TileData
{
    public string baseFile {get;}
    public string name {get;}
    public string description {get;}
    public Texture2D texture
    {
        get {
            // todo I don't like it too much that tileData.texture may not be equal to tileData.texture (the same object!)
            //      but I'm not sure how else to allow multiple textures per tileData without changing lots of things in other places
            return textures[Random.Range(0, textures.Count)];
        }
    }
    private List<Texture2D> textures;

    private List<(Neighbours<string>, string)> relativeTextures;

    public int moveCost {get;}
    public HashSet<string> pathfindingTypes {get;}
    public HashSet<string> tileTypes {get;}
    
    public TileData(JObject attributes)
    {
        baseFile = (string)attributes.GetValue("baseFile");
        
        name = (string)attributes.GetValue("name");

        description = (string)attributes.GetValue("description");

        textures = new List<Texture2D>();
        foreach (string texturePath in attributes.GetValue("textures")) {
            textures.Add(ResourceManager.LoadTexture(texturePath));
        }

        relativeTextures = new List<(Neighbours<string>, string)>();
        if (attributes.ContainsKey("relativeTextures")) {
            foreach (JObject relativeTexture in attributes.GetValue("relativeTextures")) {
                Neighbours<string> neighboursCondition;
                neighboursCondition.left = (string)((JObject)relativeTexture.GetValue("neighbourTiles")).GetValue("left");
                neighboursCondition.right = (string)((JObject)relativeTexture.GetValue("neighbourTiles")).GetValue("right");
                neighboursCondition.top = (string)((JObject)relativeTexture.GetValue("neighbourTiles")).GetValue("top");
                neighboursCondition.bottom = (string)((JObject)relativeTexture.GetValue("neighbourTiles")).GetValue("bottom");

                neighboursCondition.topLeft = (string)((JObject)relativeTexture.GetValue("neighbourTiles")).GetValue("topLeft");
                neighboursCondition.topRight = (string)((JObject)relativeTexture.GetValue("neighbourTiles")).GetValue("topRight");
                neighboursCondition.bottomLeft = (string)((JObject)relativeTexture.GetValue("neighbourTiles")).GetValue("bottomLeft");
                neighboursCondition.bottomRight = (string)((JObject)relativeTexture.GetValue("neighbourTiles")).GetValue("bottomRight");
                
                string neighboursTexturePath = (string)relativeTexture.GetValue("texture");

                relativeTextures.Add((neighboursCondition, neighboursTexturePath));
            }
        }
        
        pathfindingTypes = new HashSet<string>();
        foreach (string pathfindingType in attributes.GetValue("pathfindingTypes")) {
            pathfindingTypes.Add(pathfindingType);
        }

        tileTypes = new HashSet<string>();
        foreach (string tileType in attributes.GetValue("tileTypes")) {
            tileTypes.Add(tileType);
        }

        moveCost = (int)attributes.GetValue("moveCost");
    }

    /// Returns texture depending on the neighbouring tiles. If no neighbour condition has been matched, returns default texture
    public Texture2D GetRelativeTexture(Neighbours<Tile> neighbouringTiles)
    {
        Texture2D relativeTexture = TextureUtilities.GetRelativeTexture(relativeTextures, neighbouringTiles);
        if (relativeTexture != null) {
            return relativeTexture;
        }
        return texture;
    }

    public override string ToString()
    {
        return $"TileData(name = {name}, description = {description}, pathfindingTypes = [{string.Join(", ", pathfindingTypes)}], moveCost = {moveCost})";
    }
}
