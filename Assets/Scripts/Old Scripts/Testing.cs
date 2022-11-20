using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class Testing : MonoBehaviour
{
    private SimulationGrid _grid;
    private int _width = 50;
    private int _height = 70;
    private void Start()
    {
        _grid = new SimulationGrid(_width, _height, 3f, new Vector3(-30,-20));
        // StartCoroutine(UpdateBlocks());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _grid.SetValue(UtilsClass.GetMouseWorldPosition(), 10);
        }
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log(_grid.GetValue(UtilsClass.GetMouseWorldPosition()));
        }

        if (Time.frameCount % 2 == 0)
        {
            for (int y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x)
                {
                    // yield return new WaitForSeconds(0.0001f);
                    _grid.UpdateValues(x, y);
                }
            }
        }

        
    }

    // IEnumerator UpdateBlocks()
    // {
    //     while (true)
    //     {
    //         for (int y = 0; y < _height; ++y)
    //         {
    //             for (int x = 0; x < _width; ++x)
    //             {
    //                 yield return new 
    //                 _grid.UpdateValues(x, y);
    //             }
    //         }
    //     }
    // }
}
