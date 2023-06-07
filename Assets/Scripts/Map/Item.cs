using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class Item : MapObject
{
    public string baseFile {get; private set;}
    public ItemData itemData {get; private set;}

    public Item(JObject baseAttributes, Position position) : base(position)
    {
        LoadBaseAttributes(baseAttributes);
    }

    /// Returns true (MapItems can always be added)  //todo - what about water or mountains?
    public override bool CanAddToGame()
    {
        return true;
    }

    /// Adds this Item to the game and tilemap
    public override void AddToGame()
    {
        base.AddToGame();

        gameController.tileMap.GetTile(position).AddItem(this);

        CreateSprite();
        UpdateSprite();

        EventManager.OnItemCreated(this);
    }

    /// Creates the mapSprite GameObject representing this item
    protected override void CreateSprite()
    {
        mapSprite = new GameObject($"Item({position.x},{position.y})");
        mapSprite.transform.position = position;
        SpriteRenderer spriteRenderer = mapSprite.AddComponent<SpriteRenderer>();
    }

    /// Updates the sprite of mapSprite GameObject.
    public override void UpdateSprite()
    {
        Sprite sprite = Sprite.Create(itemData.texture, new Rect(0, 0, itemData.texture.width, itemData.texture.height), new Vector2(0, 0), TileMap.tileSize);
        SpriteRenderer spriteRenderer = mapSprite.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 15;
    }

    /// Destroys this Item, removing it from the game and tilemap
    public override void Destroy()
    {
        gameController.tileMap.GetTile(position).RemoveItem(this);

        GameObject.Destroy(mapSprite);

        EventManager.OnItemDestroyed(this);
    }

    /// Serializes this item into a JObject
    public JObject ToJObject()
    {
        JObject mapItemJObject = new JObject();

        if (baseFile != null) {
            mapItemJObject.Add("baseFile", baseFile);
        }

        mapItemJObject.Add("position", new JArray(position.x, position.y));

        mapItemJObject.Merge(itemData.ToJObject());

        ResourceManager.Minimize(mapItemJObject);

        return mapItemJObject;
    }

    /// Creates a new item from JObject
    public static Item FromJObject(JObject attributes)
    {
        ResourceManager.ExpandWithBaseFile(attributes);

        Position position = new Position((int)attributes.GetValue("position")[0], (int)attributes.GetValue("position")[1]);

        Item newMapItem = new Item(attributes, position);

        return newMapItem;
    }

    /// Loads the base attributes (the ones that are guaranteed to be in every item JObject, i.e. everything except position)
    private void LoadBaseAttributes(JObject baseAttributes)
    {
        ResourceManager.ExpandWithBaseFile(baseAttributes);
        
        baseFile = null;
        if (baseAttributes.ContainsKey("baseFile")) {
            baseFile = (string)baseAttributes.GetValue("baseFile");
        }

        itemData = ItemData.FromJObject(baseAttributes);
    }

    public override string ToString()
    {
        return $"{itemData} at {position}";
    }
}
