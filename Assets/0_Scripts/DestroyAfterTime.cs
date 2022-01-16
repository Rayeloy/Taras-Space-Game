using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float maxLifeTime;
    float currentLifeTime = 0;

    private void Start()
    {
        currentLifeTime = 0;
    }
    // Update is called once per frame
    void Update()
    {
        if(currentLifeTime>= maxLifeTime)
        {
            Destroy(gameObject);
        }
        else
        {
            currentLifeTime += Time.deltaTime;
        }
    }
}
