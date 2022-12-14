using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSelection : MonoBehaviour
{
    public GameObject tmpMarker;
    private TileMap tileMap;
    void Start()
    {
        tileMap = FindObjectOfType<TileMap>();
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo)) {
            Debug.Log(tileMap.GetTileAtPosition(hitInfo.point));
            tmpMarker.transform.position = new Vector2((int)hitInfo.point.x + 0.5f, (int)hitInfo.point.y + 0.5f);
        }
    }
}
