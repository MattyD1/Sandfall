using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpinBehaviour : MonoBehaviour
{
    private void Awake()
    {
        transform.Rotate(Vector3.up, Random.value * 360);
    }

    void Update()
    {
        
        if (Time.frameCount % 5 == 0)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y,
                transform.position.z + Mathf.Sin(Time.time + transform.position.x + transform.position.y)*0.01f);
        }
    }
}
