using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

public static class ResourceManager
{
    private static GameController gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

    private static Dictionary<string, JObject> loadedResources = new Dictionary<string, JObject>();  // works like - Dictionary<pathToResource, JObjectLoadedResource>()
    private static Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();  // works like - Dictionary<pathToTexture, Texture2DLoadedTexture>()

    /// Loads a json resource from a given path into a loaded resources database, if it wasn't already loaded. Returns JObject of loaded resource
    public static JObject LoadResource(string resourcePath)
    {
        if (!loadedResources.ContainsKey(resourcePath)) {
            string fileContents = File.ReadAllText(resourcePath);
            JObject resource = JObject.Parse(fileContents);

            ExpandWithBaseFile(resource);
            if (!resource.ContainsKey("baseFile")) {
                resource.Add("baseFile", resourcePath);  // todo maybe replace resourcePath with int resourceID or something like that
            }
            

            loadedResources[resourcePath] = resource;
        }

        return loadedResources[resourcePath];  // todo I'm not sure if these JObjects will be modified. If yes - we should return a copy here.        
    }

    /// Loads texture from a given path into a loaded textures database, if it wasn't already loaded. Returns Texture2D
    public static Texture2D LoadTexture(string texturePath)
    {
        if (!loadedTextures.ContainsKey(texturePath)) {
            byte[] binaryImageData = File.ReadAllBytes(texturePath);

            Texture2D texture = new Texture2D(0, 0);  // todo for some reason this works, but I *really* don't like this.
            texture.LoadImage(binaryImageData);
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            loadedTextures[texturePath] = texture;
        }

        return loadedTextures[texturePath];  // todo I'm not sure if these textures will be modified. If yes - we should return a copy here.  
    }

    /// Adds to the resource all fields not present in the resource but present in its base file
    public static void ExpandWithBaseFile(JObject resource)
    {
        if (resource.ContainsKey("baseFile")) {
            string baseFilePath = (string)resource.GetValue("baseFile");
            JObject baseResource = LoadResource(baseFilePath);
            if (baseResource.ContainsKey("baseFile") && baseFilePath != (string)baseResource.GetValue("baseFile")) {
                ExpandWithBaseFile(baseResource);
            }
            foreach (JProperty property in baseResource.Properties()) {
                if (!resource.ContainsKey(property.Name)) {
                    resource.Add(property.Name, property.Value);
                }
            }
        }
    }

    /// Saves the game into a json file named fileName
    public static void SaveGame(string fileName)
    {
        Debug.Log("Saving...");
        JObject save = new JObject();

        // todo also save gameController's stuff (active player, turn number) and TileMap, ofc

        save.Add("players", new JArray(gameController.players.Select(player => player.ToJObject())));
        save.Add("armies", new JArray(gameController.armies.Select(army => army.ToJObject())));
        save.Add("cities", new JArray(gameController.cities.Select(city => city.ToJObject())));
        save.Add("tileMap", gameController.tileMap.ToJObject());

        File.WriteAllText(fileName, save.ToString());
        Debug.Log("Saved!");
    }

    /// Loads the game from a json file named fileName
    public static void LoadGame(string fileName)
    {
        Debug.Log("Loading...");
        string fileContents = File.ReadAllText(fileName);
        JObject loadJObject = JObject.Parse(fileContents);

        gameController.Clear();

        gameController.tileMap.FromJObject((JObject)loadJObject.GetValue("tileMap"));

        foreach (JObject playerJObject in loadJObject.GetValue("players")) {
            gameController.AddPlayer(Player.FromJObject(playerJObject));
        }
        foreach (JObject armyJObject in loadJObject.GetValue("armies")) {
            Army newArmy = Army.FromJObject(armyJObject);
            newArmy.AddToGame();
        }
        foreach (JObject cityJObject in loadJObject.GetValue("cities")) {
            City newCity = City.FromJObject(cityJObject);
            newCity.AddToGame();
        }
        Debug.Log("Loaded!");
    }
}
