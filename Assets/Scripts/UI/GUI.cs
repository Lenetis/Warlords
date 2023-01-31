using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GUI : MonoBehaviour
{
    public TileMap tileMap;
    public Texture2D tileMapTexture;
    public GameObject miniMapImage;
    public bool isLoaded=false;
    public float width;
    public float height;
    public float ratio;



    // Start is called before the first frame update
    void Start()
    {
        tileMap = GameObject.Find("TileMap").GetComponent<TileMap>();


    }

    // Update is called once per frame
    void Update()
    {
        if (tileMap.miniMapTexture != null && isLoaded==false)
        {
            tileMapTexture = tileMap.miniMapTexture;
            height = tileMap.height;
            width = tileMap.width;
            isLoaded = true;
            miniMapImage.GetComponent<RawImage>().texture = tileMapTexture;
            ratio = height / width;
            miniMapImage.GetComponent<RectTransform>().sizeDelta = new Vector2(200, ratio * 200);
        }
        
    }
}
