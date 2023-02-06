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


    // Start is called before the first frame update
    void Start()
    {
        tileMap = GameObject.Find("TileMap").GetComponent<TileMap>();


    }

    // Update is called once per frame
    void Update()
    {
        if (cam != null)
        {
            viewportWorldWidth = cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(cam.transform.position.z))).x - cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(cam.transform.position.z))).x;
            viewportWorldHeight = cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(cam.transform.position.z))).y - cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(cam.transform.position.z))).y;


            viewportWorldPosX = cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Mathf.Abs(cam.transform.position.z))).x;
            viewportWorldPosY = cam.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Mathf.Abs(cam.transform.position.z))).y;
        }
        

        viewportNormalizedPosX = viewportWorldPosX / 80;
        viewportNormalizedPosY = viewportWorldPosY / 50;

        viewportNormalizedWidth = viewportWorldWidth / 80;
        viewportNormalizedHeight = viewportWorldHeight / 50;

        minimapIndicator.GetComponent<RectTransform>().anchoredPosition = new Vector2((viewportNormalizedPosX * 200)-100, (viewportNormalizedPosY * 125)-100+ (200-125)/2);
        minimapIndicator.GetComponent<RectTransform>().sizeDelta= new Vector2(viewportNormalizedWidth*200, viewportNormalizedHeight*125);

 
        /*
        if (Input.GetKey(KeyCode.P))
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
        }*/

        if (tileMap.miniMapTexture != null && isLoaded==false)
        {
            tileMapTexture = tileMap.miniMapTexture;
            height = tileMap.height;
            width = tileMap.width;
            isLoaded = true;
            miniMapImage.GetComponent<RawImage>().texture = tileMapTexture;
            
            if (width >= height)
            {
                ratio = height / width;
                miniMapImage.GetComponent<RectTransform>().sizeDelta = new Vector2(200, ratio * 200);
            }
            else
            {
                ratio = width / height;
                miniMapImage.GetComponent<RectTransform>().sizeDelta = new Vector2(ratio * 200, 200);
            }
            
            
        }
        
    }
}
