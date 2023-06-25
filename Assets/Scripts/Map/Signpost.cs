using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class Signpost : Structure
{
    public string baseFile {get; private set;}
    public string name {get; private set;}
    public string description {get; private set;}
    public Texture2D texture {get; private set;}

    public Signpost(JObject baseAttributes, Position position, string name, string description) : base(position)
    {    
        LoadBaseAttributes(baseAttributes);
        this.name = name;
        this.description = description;
    }

    /// Creates the mapSprite GameObject representing this signpost
    protected override void CreateSprite()
    {
        mapSprite = new GameObject($"Signpost({name} - {position.x},{position.y})");
        mapSprite.transform.position = position;
        SpriteRenderer spriteRenderer = mapSprite.AddComponent<SpriteRenderer>();
    }

    /// Updates the sprite of mapSprite GameObject.
    public override void UpdateSprite()
    {
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), TileMap.tileSize);
        SpriteRenderer spriteRenderer = mapSprite.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 10;
    }

    /// Adds this signpost to the tileMap and creates the signpost sprite
    public override void AddToGame()
    {
        base.AddToGame();

        CreateSprite();
        UpdateSprite();
    }

    /// Destroys this signpost and removes it from the game
    public override void Destroy()
    {
        base.Destroy();

        GameObject.Destroy(mapSprite);
    }

    /// Serializes this signpost into a JObject
    public JObject ToJObject()
    {
        JObject signpostJObject = new JObject();

        if (baseFile != null) {
            signpostJObject.Add("baseFile", baseFile);
        }

        signpostJObject.Add("position", new JArray(position.x, position.y));

        signpostJObject.Add("name", name);
        signpostJObject.Add("description", description);

        signpostJObject.Add("tileTypes", new JArray(tileTypes));

        ResourceManager.Minimize(signpostJObject);

        return signpostJObject;
    }

    /// Creates a new signpost from JObject
    public static Signpost FromJObject(JObject attributes)
    {
        ResourceManager.ExpandWithBaseFile(attributes);

        Position position = new Position((int)attributes.GetValue("position")[0], (int)attributes.GetValue("position")[1]);

        string name = (string)attributes.GetValue("name");
        string description = (string)attributes.GetValue("description");

        Signpost newSignpost = new Signpost(attributes, position, name, description);

        return newSignpost;
    }

    /// Loads the base attributes (only texture, and baseFile)
    private void LoadBaseAttributes(JObject baseAttributes)
    {
        ResourceManager.ExpandWithBaseFile(baseAttributes);
        
        baseFile = null;
        if (baseAttributes.ContainsKey("baseFile")) {
            baseFile = (string)baseAttributes.GetValue("baseFile");
        }

        string texturePath = (string)baseAttributes.GetValue("texture");
        texture = ResourceManager.LoadTexture(texturePath);

        if (baseAttributes.ContainsKey("tileTypes")) {
            foreach (string tileType in baseAttributes.GetValue("tileTypes")) {
                tileTypes.Add(tileType);
            }
        }
    }

    public override string ToString()
    {
        return $"Signpost({name}, {description}) at {position}";
    }
}
