using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManagement : MonoBehaviour
{
    public GameObject inventoryPanel;
    public TextMeshProUGUI heroName;
    public TextMeshProUGUI itemStatus;
    public TextMeshProUGUI[] itemsText = new TextMeshProUGUI[3];
    public Button actionButton;

    public GameController gameController;
    public MouseSelection mouseSelection;
    public int currentHeroIndex;

    public List<ItemData> items;

    public List<bool> isCarried;

    public TextMeshProUGUI[] heroStats=new TextMeshProUGUI[4];
    public TextMeshProUGUI heroIndexInfo;

    [SerializeField]
    private int scrollCounter; //index for items


    
    public string[] itemy;

    // Start is called before the first frame update
    void Start()
    {
        gameController = FindObjectOfType<GameController>();
        mouseSelection = FindObjectOfType<MouseSelection>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(items);
    }

    public void LoadData(int index)
    {
        int size = 0;
        if (gameController.activePlayer.heroes[index].heroData.items != null)
        {
            size += gameController.activePlayer.heroes[index].heroData.items.Count;
        }

        if (gameController.tileMap.GetTile(gameController.activePlayer.heroes[index].army.position).items != null)
        {
            size += gameController.tileMap.GetTile(gameController.activePlayer.heroes[index].army.position).items.Count;
        }

        itemy = new string[size];
        scrollCounter = 1;
        if (size == 1)
        {
            scrollCounter = 0;
        }

        if (gameController.tileMap.GetTile(gameController.activePlayer.heroes[index].army.position).items != null)
        {
            for (int i = 0; i < gameController.tileMap.GetTile(gameController.activePlayer.heroes[index].army.position).items.Count; i++)
            {
                //Debug.Log("JII: " + i);
                itemy[i+ gameController.activePlayer.heroes[index].heroData.items.Count] = gameController.tileMap.GetTile(gameController.activePlayer.heroes[index].army.position).items[i].itemData.name + " | " + gameController.tileMap.GetTile(gameController.activePlayer.heroes[index].army.position).items[i].itemData.description;
            }
        }

        if (gameController.activePlayer.heroes[index].heroData.items != null)
        {
            for (int i = 0; i < gameController.activePlayer.heroes[index].heroData.items.Count; i++)
            {

                itemy[i] = gameController.activePlayer.heroes[index].heroData.items[i].name + " | " + gameController.activePlayer.heroes[index].heroData.items[i].description;
            }
        }


        for(int i = 0; i < 3; i++)
        {
            itemsText[i].text = "";
        }
        itemStatus.text = "";

        items = new List<ItemData>();
        isCarried = new List<bool>();

        if (gameController.activePlayer.heroes[index].heroData.items != null)
        {
            for (int i = 0; i < gameController.activePlayer.heroes[index].heroData.items.Count; i++)
            {

                items.Add(gameController.activePlayer.heroes[index].heroData.items[i]);
                isCarried.Add(true);
            }
        }

        if (gameController.tileMap.GetTile(gameController.activePlayer.heroes[index].army.position).items != null)
        {
            for (int i = 0; i < gameController.tileMap.GetTile(gameController.activePlayer.heroes[index].army.position).items.Count; i++)
            {
                //Debug.Log("JII: " + i);
                items.Add(gameController.tileMap.GetTile(gameController.activePlayer.heroes[index].army.position).items[i].itemData);
                isCarried.Add(false);
            }
        }

        


        //items =new List<ItemData>(allItems);

        currentHeroIndex = index;
        heroName.text = gameController.activePlayer.heroes[index].name;

        if (items.Count == 1)
        {
            if (items != null)
            {
                itemsText[1].text = items[0].name + " | " + items[0].description;
            }
        }
        else if (items.Count == 2)
        {
            if (items != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (i < 2)
                    {
                        itemsText[i].text = items[i].name + " | " + items[i].description;

                    }
                }
            }
        }
        else if (items.Count > 2)
        {
            if (items != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (i < 3)
                    {
                        itemsText[i].text = items[i].name + " | " + items[i].description;

                    }
                }
            }
        }
        if (items.Count > 1)
        {
            if (isCarried[1])
            {
                itemStatus.text = "Carried";
                itemStatus.color = Color.yellow;
                actionButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Drop It";
            }
            else
            {
                itemStatus.text = "Ground"; 
                itemStatus.color = Color.green;
                actionButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Take It";
            }
        }
        else if(items.Count ==1)
        {
            if (isCarried[0])
            {
                itemStatus.text = "Carried";
                itemStatus.color = Color.yellow;
                actionButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Drop It";
            }
            else
            {
                itemStatus.text = "Ground"; 
                itemStatus.color = Color.green;
                actionButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Take It";
            }
        }
        heroStats[0].text = gameController.activePlayer.heroes[index].battleStats.strength.ToString();
        heroStats[1].text = gameController.activePlayer.heroes[index].heroData.level.ToString();
        heroStats[2].text = gameController.activePlayer.heroes[index].battleStats.command.ToString();
        heroStats[3].text = gameController.activePlayer.heroes[index].heroData.experience.ToString();

        heroIndexInfo.text = currentHeroIndex + 1 + " of " + gameController.activePlayer.heroes.Count;
        if (isCarried.Count == 1)
        {
            if (isCarried[0])
            {
                itemsText[1].color = Color.yellow;
            }
            else
            {
                itemsText[1].color = Color.green;
            }
        }
        else
        {
            if (isCarried.Count > 0)
            {
                if (isCarried[0])
                {
                    itemsText[0].color = Color.yellow;
                }
                else
                {
                    itemsText[0].color = Color.green;
                }
            }

            if (isCarried.Count > 1)
            {
                if (isCarried[1])
                {
                    itemsText[1].color = Color.yellow;
                }
                else
                {
                    itemsText[1].color = Color.green;
                }
            }

            if (isCarried.Count > 2)
            {
                if (isCarried[2])
                {
                    itemsText[2].color = Color.yellow;
                }
                else
                {
                    itemsText[2].color = Color.green;
                }
            }
        }
        
        
    }

    public void ShowPanel()
    {
        if (gameController.activePlayer.heroes.Count > 0)
        {
            int index = 0;
            if (mouseSelection.selectedArmy != null)
            {
                Tile selectedArmyTile = gameController.tileMap.GetTile(mouseSelection.selectedArmy.position);
                for (int i = 0; i < selectedArmyTile.armies.Count; i++)
                {
                    if (selectedArmyTile.armies[i].heroes.Count > 0)
                    {
                        index = gameController.activePlayer.heroes.FindIndex(x => x == selectedArmyTile.armies[i].heroes[0]);
                    }
                }
            }

            LoadData(index);
            inventoryPanel.SetActive(true);
        }
    }

    public void HidePanel()
    {
        inventoryPanel.SetActive(false);
    }

    public void ScrollInventoryUp()
    {
        if (items.Count > 1)
        {
            scrollCounter--;
            if (scrollCounter < 0)
            {
                scrollCounter = items.Count - 1;
            }

            ItemData lastElement= items[items.Count - 1];
            bool boolLastElement= isCarried[items.Count - 1];

            for (int i = items.Count - 1; i > 0; i--)
            {
                items[i] = items[i - 1];
                isCarried[i] = isCarried[i - 1];
            }

            items[0] = lastElement;
            isCarried[0] = boolLastElement;

            if (items != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (i < 3)
                    {
                        itemsText[i].text = items[i].name + " | " + items[i].description;

                    }
                }
            }

            if (isCarried[0])
            {
                itemsText[0].color = Color.yellow;
            }
            else
            {
                itemsText[0].color = Color.green;
            }

            if (isCarried[1])
            {
                itemStatus.text = "Carried";
                itemStatus.color = Color.yellow;
                itemsText[1].color = Color.yellow;
                actionButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Drop It";
            }
            else
            {
                itemStatus.text = "Ground";
                itemStatus.color = Color.green;
                itemsText[1].color = Color.green;
                actionButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Take It";
            }

            if (isCarried.Count > 2)
            {
                if (isCarried[2])
                {
                    itemsText[2].color = Color.yellow;
                }
                else
                {
                    itemsText[2].color = Color.green;
                }
            }
            
        }
    }

    public void ScrollInventoryDown()
    {
        if (items.Count > 1)
        {
            scrollCounter++;
            if(scrollCounter > items.Count-1)
            {
                scrollCounter = 0;
            }
            ItemData firstElement = items[0];
            bool boolFirstElement = isCarried[0];

            for (int i = 0; i < items.Count-1; i++)
            {
                items[i] = items[i + 1];
                isCarried[i] = isCarried[i + 1];
            }

            items[items.Count - 1] = firstElement;
            isCarried[items.Count - 1] = boolFirstElement;

            if (items != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (i < 3)
                    {
                        itemsText[i].text = items[i].name + " | " + items[i].description;

                    }
                }
            }
            if (isCarried[0])
            {
                itemsText[0].color = Color.yellow;
            }
            else
            {
                itemsText[0].color = Color.green;
            }

            if (isCarried[1])
            {
                itemStatus.text = "Carried";
                itemStatus.color = Color.yellow;
                itemsText[1].color = Color.yellow;
                actionButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Drop It";
            }
            else
            {
                itemStatus.text = "Ground";
                itemStatus.color = Color.green;
                itemsText[1].color = Color.green;
                actionButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Take It";
            }

            if (isCarried.Count > 2)
            {
                if (isCarried[2])
                {
                    itemsText[2].color = Color.yellow;
                }
                else
                {
                    itemsText[2].color = Color.green;
                }
            }
                
        }
    }

    public void TakeOrDropItem()
    {
        if (items.Count > 1)
        {
            if (isCarried[1])
            {
                gameController.activePlayer.heroes[currentHeroIndex].heroData.DropItem(items[1], gameController.activePlayer.heroes[currentHeroIndex].army.position);
            }
            else
            {
                gameController.activePlayer.heroes[currentHeroIndex].heroData.PickUpItem(gameController.tileMap.GetTile(gameController.activePlayer.heroes[currentHeroIndex].army.position).items[scrollCounter- gameController.activePlayer.heroes[currentHeroIndex].heroData.items.Count]);
            }
        }
        else if(items.Count==1)
        {
            if (isCarried[0])
            {
                gameController.activePlayer.heroes[currentHeroIndex].heroData.DropItem(items[0], gameController.activePlayer.heroes[currentHeroIndex].army.position);
            }
            else
            {
                gameController.activePlayer.heroes[currentHeroIndex].heroData.PickUpItem(gameController.tileMap.GetTile(gameController.activePlayer.heroes[currentHeroIndex].army.position).items[0]);
            }
        }

        LoadData(currentHeroIndex);
    }

    public void PreviousHero()
    {
        if (currentHeroIndex > 0)
        {
            LoadData(--currentHeroIndex);
            heroIndexInfo.text =currentHeroIndex+1+ " of " + gameController.activePlayer.heroes.Count;
        }
    }
    public void NextHero()
    {
        if (currentHeroIndex + 1 < gameController.activePlayer.heroes.Count)
        {
            LoadData(++currentHeroIndex);
            heroIndexInfo.text = currentHeroIndex + 1 + " of " + gameController.activePlayer.heroes.Count;
        }
        
    }
}
