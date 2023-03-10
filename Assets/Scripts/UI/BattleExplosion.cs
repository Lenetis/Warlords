using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleExplosion : MonoBehaviour
{
    public float duration=0.5f;
    public float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            timer = 0;
            gameObject.SetActive(false);
        }
    }
}
