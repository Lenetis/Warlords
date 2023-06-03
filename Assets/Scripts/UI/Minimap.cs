using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Minimap : MonoBehaviour
{
    public TileMap tileMap;
    public Texture2D tileMapTexture;
    public GameObject miniMapImage;
    public GameObject cityMiniMapImage;
    public bool isTileMapLoaded = false;
    public bool areCitiesLoaded = false;
    public float width;
    public float height;
    public float ratio;

    //minimap viewport
    public GameObject cam;
    public float viewportWorldWidth;
    public float viewportWorldHeight;

    public float viewportWorldPosX;
    public float viewportWorldPosY;

    public GameObject prefab;

    public float viewportNormalizedPosX;
    public float viewportNormalizedPosY;

    public float viewportNormalizedWidth;
    public float viewportNormalizedHeight;

    public GameObject minimapIndicator;

    //Teleport\\
    public bool isOverMinimap;
    //public GameObject minimapArea;
    public GameObject gui;
    public CameraController cameraController;

    //resizable frame
    public GameObject[] borders=new GameObject[4];

    private UIController uiController;
    private GameController gameController;

    //public Image cityIcon;

    //public Image[] cityIcons;
    //public Image[] cityIconsInMenu;

    public GameObject cityIcon;
    public Sprite razedCityIcon;

    public GameObject[] cityIcons;
    public GameObject[] cityIconsInMenu;

    public GameObject cityIconTransparent;

    public GameObject[] cityIconsTransparent;

    public GameObject[] cityIconsInMenuButtons;

    public Sprite cityIconInMenuImage;
    public Sprite cityIconInMenuSelectedImage;

    public City selectedCity;

    // Start is called before the first frame update
    void Start()
    {
        tileMap = GameObject.Find("TileMap").GetComponent<TileMap>();
        cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();
        uiController = GameObject.Find("UIController").GetComponent<UIController>();
        gameController= GameObject.Find("GameController").GetComponent<GameController>();
        EventManager.CityCreatedEvent += DrawCities;
        EventManager.TurnEvent += DrawCities;
    }

    void Awake()
    {
        EventManager.CityCapturedEvent += DrawCities;
        EventManager.CityRazedEvent += DrawCities;
        EventManager.CityDestroyedEvent += DrawCities;
        
    }

    void OnDestroy()
    {
        EventManager.CityCapturedEvent -= DrawCities;
        EventManager.CityRazedEvent -= DrawCities;
        EventManager.CityCreatedEvent -= DrawCities;
        EventManager.CityDestroyedEvent -= DrawCities;
        EventManager.TurnEvent -= DrawCities;
    }

    // Update is called once per frame
    void Update()
    {
        //Teleport\\

        //Debug.Log(isOverMinimap);

        float minimapAreaWidth = (miniMapImage.GetComponent<RectTransform>().anchorMax.x - miniMapImage.GetComponent<RectTransform>().anchorMin.x) * Screen.width + miniMapImage.GetComponent<RectTransform>().sizeDelta.x * gui.GetComponent<Canvas>().scaleFactor;
        float minimapAreaHeight = (miniMapImage.GetComponent<RectTransform>().anchorMax.y - miniMapImage.GetComponent<RectTransform>().anchorMin.y) * Screen.height + miniMapImage.GetComponent<RectTransform>().sizeDelta.y * gui.GetComponent<Canvas>().scaleFactor;
        float minimapAreaPosX = miniMapImage.GetComponent<RectTransform>().position.x;
        float minimapAreaPosY = miniMapImage.GetComponent<RectTransform>().position.y;
        float minimapAreaOriginX = minimapAreaPosX - (minimapAreaWidth / 2);
        float minimapAreaOriginY = minimapAreaPosY - (minimapAreaHeight / 2);
        float minimapAreaEndX = minimapAreaPosX + (minimapAreaWidth / 2);
        float minimapAreaEndY = minimapAreaPosY + (minimapAreaHeight / 2);


        if (Input.mousePosition.x >= minimapAreaOriginX && Input.mousePosition.x <= minimapAreaEndX)
        {
            if (Input.mousePosition.y >= minimapAreaOriginY && Input.mousePosition.y <= minimapAreaEndY)
            {
                isOverMinimap = true;
                uiController.isOverMinimap = true;
            }
            else
            {
                isOverMinimap = false;
                uiController.isOverMinimap = false;
            }
        }
        else
        {
            isOverMinimap = false;
            uiController.isOverMinimap = false;
        }

        if (Input.GetButton("Select") && isOverMinimap && uiController.controllsAvailable())
        {
            float mouseMinimapX = (Input.mousePosition.x - minimapAreaOriginX) / minimapAreaWidth;
            float mouseMinimapY = (Input.mousePosition.y - minimapAreaOriginY) / minimapAreaHeight;
            //cam.transform.parent.gameObject.transform.position = new Vector3(mouseMinimapX * width, mouseMinimapY * height, cam.transform.position.z);
            cameraController.CheckNSetPosition(new Vector3(mouseMinimapX * width, mouseMinimapY * height, cam.transform.parent.gameObject.transform.position.z));
            //Debug.Log("X: "+mouseMinimapX);
            //Debug.Log("Y: " + mouseMinimapY);
        }

        //Limits\\

        if (cam != null)
        {
            viewportWorldWidth = cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(cam.transform.position.z))).x - cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(cam.transform.position.z))).x;
            viewportWorldHeight = cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(cam.transform.position.z))).y - cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(cam.transform.position.z))).y;


            viewportWorldPosX = cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Mathf.Abs(cam.transform.position.z))).x;
            viewportWorldPosY = cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Mathf.Abs(cam.transform.position.z))).y;
        }
        
        viewportNormalizedPosX = viewportWorldPosX / width;
        viewportNormalizedPosY = viewportWorldPosY / height;

        viewportNormalizedWidth = viewportWorldWidth / width;
        viewportNormalizedHeight = viewportWorldHeight / height;

        minimapIndicator.GetComponent<RectTransform>().anchoredPosition = new Vector2((viewportNormalizedPosX * miniMapImage.GetComponent<RectTransform>().sizeDelta.x) -100+ ((float)(200 - miniMapImage.GetComponent<RectTransform>().sizeDelta.x)) / 2, (viewportNormalizedPosY * miniMapImage.GetComponent<RectTransform>().sizeDelta.y) -100+ ((float)(200- miniMapImage.GetComponent<RectTransform>().sizeDelta.y))/2);
        minimapIndicator.GetComponent<RectTransform>().sizeDelta= new Vector2(viewportNormalizedWidth* miniMapImage.GetComponent<RectTransform>().sizeDelta.x, viewportNormalizedHeight* miniMapImage.GetComponent<RectTransform>().sizeDelta.y);
        borders[0].GetComponent<RectTransform>().anchoredPosition = new Vector2(1f,0);
        borders[1].GetComponent<RectTransform>().anchoredPosition = new Vector2(-1f,0);
        borders[2].GetComponent<RectTransform>().anchoredPosition = new Vector2(0,-1f);
        borders[3].GetComponent<RectTransform>().anchoredPosition = new Vector2(0,1f);

        /*
        if (Input.GetKeyDown(KeyCode.P))
        {
            Color customColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1.0f);
            GameObject cube1= Instantiate(prefab, cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(cam.transform.position.z))), Quaternion.identity);
            cube1.GetComponent<Renderer>().material.SetColor("_Color", customColor);
            GameObject cube2 = Instantiate(prefab, cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0, 1, Mathf.Abs(cam.transform.position.z))), Quaternion.identity);
            cube2.GetComponent<Renderer>().material.SetColor("_Color", customColor);
            GameObject cube3 = Instantiate(prefab, cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1, 0, Mathf.Abs(cam.transform.position.z))), Quaternion.identity);
            cube3.GetComponent<Renderer>().material.SetColor("_Color", customColor);
            GameObject cube4 = Instantiate(prefab, cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(cam.transform.position.z))), Quaternion.identity);
            cube4.GetComponent<Renderer>().material.SetColor("_Color", customColor);
            GameObject cubeCenter = Instantiate(prefab, cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Mathf.Abs(cam.transform.position.z))), Quaternion.identity);
        }*/

        if (tileMap.miniMapTexture != null && isTileMapLoaded==false)
        {
            tileMapTexture = tileMap.miniMapTexture;
            height = tileMap.height;
            width = tileMap.width;
            isTileMapLoaded = true;
            cameraController.mapWidth = (int)width;
            cameraController.mapHeight = (int)height;

            miniMapImage.GetComponent<RawImage>().texture = tileMapTexture;
            cityMiniMapImage.GetComponent<RawImage>().texture = tileMapTexture;

            if (width >= height)
            {
                ratio = height / width;
                miniMapImage.GetComponent<RectTransform>().sizeDelta = new Vector2(200, ratio * 200);
                cityMiniMapImage.GetComponent<RectTransform>().sizeDelta = new Vector2(200, ratio * 200);
            }
            else
            {
                ratio = width / height;
                miniMapImage.GetComponent<RectTransform>().sizeDelta = new Vector2(ratio * 200, 200);
                cityMiniMapImage.GetComponent<RectTransform>().sizeDelta = new Vector2(ratio * 200, 200);
            }
        }
        if (tileMap.miniMapTexture != null && gameController.cities!=null && areCitiesLoaded == false)
        {
            DrawCities(this, System.EventArgs.Empty);
        }
    }

    public void DrawCities(object sender, System.EventArgs args)
    {

        for (int i = 0; i < cityIcons.Length; i++)
        {
            if (cityIcons[i] != null)
            {
                Destroy(cityIcons[i].gameObject);
            }
        }
        
        for (int i = 0; i < cityIconsInMenu.Length; i++)
        {
            if (cityIconsInMenu[i] != null)
            {
                Destroy(cityIconsInMenu[i].gameObject);
            }
        }

        for (int i = 0; i < cityIconsTransparent.Length; i++)
        {
            if (cityIconsTransparent[i] != null)
            {
                Destroy(cityIconsTransparent[i].gameObject);
            }
        }

        cityIcons = new GameObject[gameController.cities.Count];
        cityIconsInMenu = new GameObject[gameController.cities.Count];
        cityIconsTransparent = new GameObject[gameController.cities.Count];

        for (int i=0; i<gameController.cities.Count; i++)
        {
            float normX = ((float)gameController.cities[i].position.x+1) / (float)tileMap.width;
            float normY = ((float)gameController.cities[i].position.y+1) / (float)tileMap.height;

            if (!gameController.cities[i].razed)
            {
                cityIcons[i] = Instantiate(cityIcon, miniMapImage.transform.GetChild(0));
                cityIcons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(-miniMapImage.GetComponent<RectTransform>().sizeDelta.x/2 + miniMapImage.GetComponent<RectTransform>().sizeDelta.x * normX, -miniMapImage.GetComponent<RectTransform>().sizeDelta.y / 2 + miniMapImage.GetComponent<RectTransform>().sizeDelta.y * normY);
                cityIcons[i].GetComponent<Image>().color = gameController.cities[i].owner.color;

                cityIconsTransparent[i] = Instantiate(cityIconTransparent, miniMapImage.transform.GetChild(2));
                cityIconsTransparent[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(-miniMapImage.GetComponent<RectTransform>().sizeDelta.x / 2 + miniMapImage.GetComponent<RectTransform>().sizeDelta.x * normX, -miniMapImage.GetComponent<RectTransform>().sizeDelta.y / 2 + miniMapImage.GetComponent<RectTransform>().sizeDelta.y * normY);
                cityIconsTransparent[i].GetComponent<ButtonRightClick>().buttonName = gameController.cities[i].name;
                cityIconsTransparent[i].GetComponent<ButtonRightClick>().buttonDescription = gameController.cities[i].income+"gp";

                if (gameController.cities[i].owner == gameController.activePlayer)
                {
                    cityIconsInMenu[i] = Instantiate(cityIcon, cityMiniMapImage.transform.GetChild(0));
                    cityIconsInMenu[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(-cityMiniMapImage.GetComponent<RectTransform>().sizeDelta.x / 2 + cityMiniMapImage.GetComponent<RectTransform>().sizeDelta.x * normX, -cityMiniMapImage.GetComponent<RectTransform>().sizeDelta.y / 2 + cityMiniMapImage.GetComponent<RectTransform>().sizeDelta.y * normY);
                    cityIconsInMenu[i].GetComponent<Image>().color = Color.white;
                    
                    if (selectedCity != null)
                    {
                        if (gameController.cities.IndexOf(selectedCity) == i)
                        {
                            cityIconsInMenu[i].GetComponent<Image>().sprite = cityIconInMenuSelectedImage;
                        }
                        else
                        {
                            cityIconsInMenu[i].GetComponent<Image>().sprite = cityIconInMenuImage;
                        }
                    }
                    else
                    {
                        cityIconsInMenu[i].GetComponent<Image>().sprite = cityIconInMenuImage;
                    }
                    cityIconsInMenu[i].name = i.ToString();
                    cityIconsInMenu[i].transform.GetChild(0).gameObject.SetActive(true);
                }
            }
            else
            {
                cityIcons[i] = Instantiate(cityIcon, miniMapImage.transform.GetChild(0));
                cityIcons[i].transform.localScale = new Vector3(0.75f,0.75f,1f);
                cityIcons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(-miniMapImage.GetComponent<RectTransform>().sizeDelta.x / 2 + miniMapImage.GetComponent<RectTransform>().sizeDelta.x * normX, -miniMapImage.GetComponent<RectTransform>().sizeDelta.y / 2 + miniMapImage.GetComponent<RectTransform>().sizeDelta.y * normY);
                cityIcons[i].GetComponent<Image>().color = Color.white;
                cityIcons[i].GetComponent<Image>().sprite = razedCityIcon;

                cityIconsTransparent[i] = Instantiate(cityIconTransparent, miniMapImage.transform.GetChild(2));
                cityIconsTransparent[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(-miniMapImage.GetComponent<RectTransform>().sizeDelta.x / 2 + miniMapImage.GetComponent<RectTransform>().sizeDelta.x * normX, -miniMapImage.GetComponent<RectTransform>().sizeDelta.y / 2 + miniMapImage.GetComponent<RectTransform>().sizeDelta.y * normY);
                cityIconsTransparent[i].GetComponent<ButtonRightClick>().buttonName = gameController.cities[i].name;
                cityIconsTransparent[i].GetComponent<ButtonRightClick>().buttonDescription ="Razed!";
            }
        }
        areCitiesLoaded = true;
    }
}
