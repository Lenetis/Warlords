using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class Road : Structure
{
    public string baseFile {get; private set;}
    public Texture2D texture {get; private set;}
    private List<(Neighbours<string>, string)> relativeTextures;

    public Road(JObject baseAttributes, Position position) : base(position)
    {    
        LoadBaseAttributes(baseAttributes);
    }

    /// Creates the mapSprite GameObject representing this road
    protected override void CreateSprite()
    {
        mapSprite = new GameObject($"Road({position.x},{position.y})");
        mapSprite.transform.position = position;
        SpriteRenderer spriteRenderer = mapSprite.AddComponent<SpriteRenderer>();
    }

    /// Updates the sprite of mapSprite GameObject. (E.g. when a road is added/removed from neighbouring tile  -- TODO not yet implemented)
    public override void UpdateSprite()
    {
        Texture2D newTexture = TextureUtilities.GetRelativeTexture(relativeTextures, gameController.tileMap.GetNeighbouringTiles(position));
        if (newTexture != null) {
            texture = newTexture;
        }

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), TileMap.tileSize);
        SpriteRenderer spriteRenderer = mapSprite.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 10;
    }

    /// Adds this road to the tileMap and creates the road sprite
    public override void AddToGame()
    {
        base.AddToGame();

        CreateSprite();
        UpdateSprite();
    }

    /// Destroys this road and removes it from the game
    public override void Destroy()
    {
        base.Destroy();

        GameObject.Destroy(mapSprite);
    }

    /// Serializes this road into a JObject
    public JObject ToJObject()
    {
        JObject roadJObject = new JObject();

        if (baseFile != null) {
            roadJObject.Add("baseFile", baseFile);
        }

        roadJObject.Add("position", new JArray(position.x, position.y));

        roadJObject.Add("pathfinding", pathfinding.ToJObject());

        roadJObject.Add("tileTypes", new JArray(tileTypes));

        ResourceManager.Minimize(roadJObject);

        return roadJObject;
    }

    /// Creates a new road from JObject
    public static Road FromJObject(JObject attributes)
    {
        ResourceManager.ExpandWithBaseFile(attributes);

        Position position = new Position((int)attributes.GetValue("position")[0], (int)attributes.GetValue("position")[1]);

        Road newRoad = new Road(attributes, position);

        return newRoad;
    }

    /// Loads the base attributes (the ones that are guaranteed to be in every road JObject, i.e. everything except position)
    private void LoadBaseAttributes(JObject baseAttributes)
    {
        ResourceManager.ExpandWithBaseFile(baseAttributes);
        
        baseFile = null;
        if (baseAttributes.ContainsKey("baseFile")) {
            baseFile = (string)baseAttributes.GetValue("baseFile");
        }

        string texturePath = (string)baseAttributes.GetValue("texture");
        texture = ResourceManager.LoadTexture(texturePath);

        relativeTextures = new List<(Neighbours<string>, string)>();
        if (baseAttributes.ContainsKey("relativeTextures")) {
            foreach (JObject relativeTexture in baseAttributes.GetValue("relativeTextures")) {
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

        pathfinding = PathfindingData.FromJObject((JObject)baseAttributes.GetValue("pathfinding"));

        if (baseAttributes.ContainsKey("tileTypes")) {
            foreach (string tileType in baseAttributes.GetValue("tileTypes")) {
                tileTypes.Add(tileType);
            }
        }
    }
}
