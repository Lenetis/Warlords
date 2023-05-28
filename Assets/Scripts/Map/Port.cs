using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class Port : Structure
{
    public string baseFile {get; private set;}
    public Texture2D texture {get; private set;}

    public Port(JObject baseAttributes, Position position) : base(position)
    {    
        LoadBaseAttributes(baseAttributes);
    }

    /// Creates the mapSprite GameObject representing this port
    protected override void CreateSprite()
    {
        mapSprite = new GameObject($"Port({position.x},{position.y})");
        mapSprite.transform.position = position;
        SpriteRenderer spriteRenderer = mapSprite.AddComponent<SpriteRenderer>();
    }

    /// Updates the sprite of mapSprite GameObject.
    public override void UpdateSprite()
    {
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 32);
        SpriteRenderer spriteRenderer = mapSprite.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 10;
    }

    /// Adds this port to the tileMap and creates the port sprite
    public override void AddToGame()
    {
        base.AddToGame();

        CreateSprite();
        UpdateSprite();
    }

    /// Destroys this port and removes it from the game
    public override void Destroy()
    {
        base.Destroy();

        GameObject.Destroy(mapSprite);
    }

    /// Serializes this port into a JObject
    public JObject ToJObject()
    {
        JObject portJObject = new JObject();

        if (baseFile != null) {
            portJObject.Add("baseFile", baseFile);
        }

        portJObject.Add("position", new JArray(position.x, position.y));

        portJObject.Add("pathfinding", pathfinding.ToJObject());

        ResourceManager.Minimize(portJObject);

        return portJObject;
    }

    /// Creates a new port from JObject
    public static Port FromJObject(JObject attributes)
    {
        ResourceManager.ExpandWithBaseFile(attributes);

        Position position = new Position((int)attributes.GetValue("position")[0], (int)attributes.GetValue("position")[1]);

        Port newPort = new Port(attributes, position);

        return newPort;
    }

    /// Loads the base attributes (the ones that are guaranteed to be in every port JObject, i.e. everything except position)
    private void LoadBaseAttributes(JObject baseAttributes)
    {
        ResourceManager.ExpandWithBaseFile(baseAttributes);
        
        baseFile = null;
        if (baseAttributes.ContainsKey("baseFile")) {
            baseFile = (string)baseAttributes.GetValue("baseFile");
        }

        string texturePath = (string)baseAttributes.GetValue("texture");
        texture = ResourceManager.LoadTexture(texturePath);

        pathfinding = PathfindingData.FromJObject((JObject)baseAttributes.GetValue("pathfinding"));
    }
}
