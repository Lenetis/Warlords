using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using Newtonsoft.Json.Linq;

public class ResourceManager
{
    private Dictionary<string, JObject> loadedResources;  // works like - Dictionary<pathToResource, JObjectLoadedResource>()
    private Dictionary<string, Texture2D> loadedTextures;  // works like - Dictionary<pathToTexture, Texture2DLoadedTexture>()

    public ResourceManager()
    {
        loadedResources = new Dictionary<string, JObject>();
        loadedTextures = new Dictionary<string, Texture2D>();
    }

    /// Loads a json resource from a given path into a loaded resources database, if it wasn't already loaded. Returns JObject of loaded resource
    public JObject LoadResource(string resourcePath)
    {
        if (!loadedResources.ContainsKey(resourcePath)) {
            string fileContents = File.ReadAllText(resourcePath);
            JObject jObject = JObject.Parse(fileContents);
            loadedResources[resourcePath] = jObject;
        }

        return loadedResources[resourcePath];  // todo I'm not sure if these JObjects will be modified. If yes - we should return a copy here.        
    }

    /// Loads texture from a given path into a loaded textures database, if it wasn't already loaded. Returns Texture2D
    public Texture2D LoadTexture(string texturePath)
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
}
