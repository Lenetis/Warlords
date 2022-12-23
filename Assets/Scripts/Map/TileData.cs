using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

[System.Serializable]
public class TileData
{
    public string name;
    public string description;
    public string texture;  // todo overwrite the deserialization somehow to load the texture directly, without Initialize()
    public string[] pathfindingTypes;
    public float moveCost;

    public Texture2D image;

    public override string ToString()
    {
        return $"TileData(name = {name}, description = {description}, pathfindingTypes = [{string.Join(", ", pathfindingTypes)}], moveCost = {moveCost})";
    }

    public void Initialize()
    {
        byte[] binaryImageData = File.ReadAllBytes(texture);
        image = new Texture2D(0,0);  // todo for some reason this works, but I *really* don't like this.
        image.LoadImage(binaryImageData);
        image.Apply();
    }
}
