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

    public int moveCost {get;}
    public HashSet<string> pathfindingTypes {get;}
    
    public TileData(JObject attributes)
    {
        baseFile = (string)attributes.GetValue("baseFile");
        
        name = (string)attributes.GetValue("name");

        description = (string)attributes.GetValue("description");

        textures = new List<Texture2D>();
        if (attributes.ContainsKey("textures")) {
            foreach (string texturePath in attributes.GetValue("textures")) {
                textures.Add(ResourceManager.LoadTexture(texturePath));
            }
        } else {
            string texturePath = (string)attributes.GetValue("texture");
            textures.Add(ResourceManager.LoadTexture(texturePath));
        }
        

        pathfindingTypes = new HashSet<string>();
        foreach (string pathfindingType in attributes.GetValue("pathfindingTypes")) {
            pathfindingTypes.Add(pathfindingType);
        }

        moveCost = (int)attributes.GetValue("moveCost");
    }

    public override string ToString()
    {
        return $"TileData(name = {name}, description = {description}, pathfindingTypes = [{string.Join(", ", pathfindingTypes)}], moveCost = {moveCost})";
    }
}
