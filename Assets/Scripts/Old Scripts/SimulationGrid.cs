using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class SimulationGrid : MonoBehaviour
{
    private int _width;
    private int _height;
    private float _cellSize;
    private Vector3 _originPosition;
    private int[,] _gridArray;
    private TextMesh[,] _debugTextArray;

    public SimulationGrid(int width, int height, float cellSize, Vector3 originPosition)
    {
        _width = width;
        _height = height;
        _cellSize = cellSize;
        _originPosition = originPosition;
        _gridArray = new int[width, height];
        _debugTextArray = new TextMesh[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // if (y > 28)
                //     _gridArray[x, y] = 1;
                _debugTextArray[x,y]= UtilsClass.CreateWorldText(_gridArray[x, y].ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f, 20, Color.white,
                    TextAnchor.MiddleCenter);
                Debug.DrawLine(GetWorldPosition(x,y), GetWorldPosition(x,y + 1), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(x,y), GetWorldPosition(x+1,y), Color.white, 100f);
            }
        }
        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * _cellSize + _originPosition;
    }

    private void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - _originPosition).x / _cellSize);
        y = Mathf.FloorToInt((worldPosition - _originPosition).y / _cellSize);
    }


    public void SetValue(int x, int y, int value)
    {
        if (x >= 0 && y >= 0 && x < _width && y < _height)
        {
            _gridArray[x, y] = value;
            _debugTextArray[x,y].text = _gridArray[x, y].ToString();
        }
    }

    public void SetValue(Vector3 worldPosition, int value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetValue(x,y,value);
    }

    public int GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < _width && y < _height)
        {
            return _gridArray[x, y];
        }

        return 0;
    }
    
    public int GetValue(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetValue(x, y);
    }
    
    public void UpdateValues(int x, int y)
    {
        var currentCell = _gridArray[x, y];
        if (currentCell == 0)
            return;

        CheckEmpty(x, y);
    }


    public void SwapCells(int x, int y, int x1, int y1)
    {
        var temp = _gridArray[x, y];
        _gridArray[x, y] = _gridArray[x1, y1];
        _gridArray[x1, y1] = temp;
        
        Debug.Log(x + " "+ y);

        _debugTextArray[x, y].text = _gridArray[x, y].ToString();
        _debugTextArray[x1, y1].text = _gridArray[x1, y1].ToString();
    }

    private void CheckEmpty(int x, int y)
    {
        Debug.Log("Test");
        if (y - 1 >= 0)
        {
            Debug.Log("Test");
            SwapCells(x, y, x, y-1);
        }
        else if(x - 1 >= 0)
            SwapCells(x, y, x-1, y);
        else 
            SwapCells(x, y, x+1, y);
    }
}
