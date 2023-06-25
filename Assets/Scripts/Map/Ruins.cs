using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

public class Ruins : Structure, IExplorable
{
    public string baseFile {get; private set;}
    public Texture2D texture {get; private set;}

    public bool explored {get; private set;}

    public Ruins(JObject baseAttributes, Position position) : base(position)
    {
        LoadBaseAttributes(baseAttributes);
    }

    /// Creates the mapSprite GameObject representing this ruins
    protected override void CreateSprite()
    {
        mapSprite = new GameObject($"Ruins({position.x},{position.y})");
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

    /// Adds this ruins to the tileMap and creates the ruins sprite
    public override void AddToGame()
    {
        base.AddToGame();

        CreateSprite();
        UpdateSprite();
    }

    /// Destroys this ruins and removes it from the game
    public override void Destroy()
    {
        base.Destroy();

        GameObject.Destroy(mapSprite);
    }

    /// Explores this ruins by the explorer unit
    public void Explore(Unit explorer)
    {
        if (explored) {
            return;
        }

        explorer.heroData.experience += Constants.ruinsExploreExperience;

        RuinsInfoData eventData;
        eventData.explorationInfo = new List<string>();

        int unitsCount = Random.Range(1, 4 + 1);
        if (Random.Range(0f, 1f) < Constants.ruinsUnitJoinChance && gameController.tileMap.GetTile(position).unitCount + unitsCount <= Constants.maxUnitsPerTile) {
            List<Unit> ruinsUnitList = new List<Unit>();
            int allyUnitIndex = Random.Range(0, Constants.ruinsUnits.Length);
            for(int i = 0; i < unitsCount; i += 1) {
                Unit ally = Unit.FromJObject(ResourceManager.LoadResource(Constants.ruinsUnits[allyUnitIndex]));
                ally.economy.upkeep = 0;
                ruinsUnitList.Add(ally);
            }

            Army ruinsArmy = new Army(ruinsUnitList, position, explorer.army.owner);
            ruinsArmy.AddToGame();
            eventData.explorationInfo.Add($"{unitsCount} {(unitsCount == 1 ? "unit" : "units")} of {ruinsUnitList[0].name} offer to join {explorer.name}!");
            Debug.Log($"{unitsCount} {(unitsCount == 1 ? "unit" : "units")} of {ruinsUnitList[0].name} offer to join {explorer.name}!");
            explored = true;
        }
        else {
            string enemy = Constants.ruinsEnemies[Random.Range(0, Constants.ruinsEnemies.Length)];
            eventData.explorationInfo.Add($"{explorer.name} encounters {enemy}...");
            Debug.Log($"{explorer.name} encounters {enemy}...");
            if (Random.Range(0f, 1f) < Constants.ruinsBattleVictoryChance) {
                eventData.explorationInfo.Add("And is victorious!");
                Debug.Log("And is victorious!");
                if (gameController.ruinsItems.Count > 0 && Random.Range(0f, 1f) < Constants.ruinsItemFindChance) {
                    ItemData ruinsItemData = gameController.ruinsItems[Random.Range(0, gameController.ruinsItems.Count)];
                    gameController.ruinsItems.Remove(ruinsItemData);

                    Item ruinsItem = new Item(ruinsItemData, position);
                    ruinsItem.AddToGame();
                    eventData.explorationInfo.Add($"{explorer.name} has found the {ruinsItemData.name}!");
                    Debug.Log($"{explorer.name} has found the {ruinsItemData.name}!");
                }
                else {
                    int ruinsGold = Random.Range(Constants.ruinsMinGold, Constants.ruinsMaxGold);

                    explorer.army.owner.gold += ruinsGold;
                    eventData.explorationInfo.Add($"{explorer.name} has found {ruinsGold} gold!");
                    Debug.Log($"{explorer.name} has found {ruinsGold} gold!");
                }
                explored = true;
            }
            else {
                eventData.explorationInfo.Add("And is slain by it!");
                Debug.Log("And is slain by it!");
                explorer.Destroy();
            }
        }
        EventManager.OnRuinsInfo(this, eventData);
        EventManager.OnRuinsExplored(this);
    }

    /// Serializes this ruins into a JObject
    public JObject ToJObject()
    {
        JObject ruinsJObject = new JObject();

        if (baseFile != null) {
            ruinsJObject.Add("baseFile", baseFile);
        }

        ruinsJObject.Add("position", new JArray(position.x, position.y));

        ruinsJObject.Add("pathfinding", pathfinding.ToJObject());

        ruinsJObject.Add("tileTypes", new JArray(tileTypes));

        ResourceManager.Minimize(ruinsJObject);

        return ruinsJObject;
    }

    /// Creates a new ruins from JObject
    public static Ruins FromJObject(JObject attributes)
    {
        ResourceManager.ExpandWithBaseFile(attributes);

        Position position = new Position((int)attributes.GetValue("position")[0], (int)attributes.GetValue("position")[1]);

        Ruins newRuins = new Ruins(attributes, position);

        return newRuins;
    }

    /// Loads the base attributes (the ones that are guaranteed to be in every ruins JObject, i.e. everything except position)
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

        if (baseAttributes.ContainsKey("explored")) {
            explored = (bool)baseAttributes.GetValue("explored");
        } else {
            explored = false;
        }

        if (baseAttributes.ContainsKey("tileTypes")) {
            foreach (string tileType in baseAttributes.GetValue("tileTypes")) {
                tileTypes.Add(tileType);
            }
        }
    }
}
