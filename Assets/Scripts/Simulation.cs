using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private Material material;
    [SerializeField] private Mesh mesh;
    [SerializeField, Range(10, 300)] private int height = 20;
    [SerializeField, Range(10, 300)] private int width = 30;

    // Cells
    private Cell[,] _cells;
    private static readonly Color CellColorSand = new Color(255, 255, 255, 255);
    private static readonly Color CellColorEmpty = new Color(27, 27, 27, 255);
    private Cell _cellEmpty = new Cell(0, 0, Vector2.zero, CellColorEmpty, false);
    private Cell _cellSand = new Cell(1, 0, Vector2.zero, CellColorSand, false);
    
    // Mesh
    private Vector3[] _vertices;
    private Mesh _mesh;

    private double textureWidth;
    private double textureHeight;
    private Texture2D texture;
    
    // Shader
    private ComputeBuffer _positionsBuffer;
    
    private struct Color
    {
        private int _r;
        private int _g;
        private int _b;
        private int _alpha;

        public Color(int r, int g, int b, int alpha)
        {
            _r = r;
            _g = g;
            _b = b;
            _alpha = alpha;
        }
    }

    private struct Cell
    {
        public int _cellId;
        private float _lifeTime;
        private Vector2 _velocity;
        private Color _color;
        private bool _updated;

        public Cell(int cellId, float lifeTime, Vector2 velocity, Color color, bool updated)
        {
            _cellId = cellId;
            _lifeTime = lifeTime;
            _velocity = velocity;
            _color = color;
            _updated = updated;
        }
    }

    private void OnEnable()
    {
        _positionsBuffer = new ComputeBuffer(width * height, sizeof(int));
    }

    private void OnDisable()
    {
        _positionsBuffer.Release();
        _positionsBuffer = null;
    }

    private void Awake()
    {
         _cells = new Cell[width, height];
         
         for (int x = 0; x < width; x++)
         {
             for (int y = 0; y < height; y++)
             {
                 var bounds = (x % 2 == 0) && (y % 2 != 0) ;
                 var cell = _cells[x,y] = bounds ? _cellSand : _cellEmpty;
             }
         }
         
         textureWidth = 1920.0 / width;
         textureHeight = 1920.0 / height;
         texture = new Texture2D((int)textureWidth*width, (int)textureHeight*height);
         GetComponentInChildren<MeshRenderer>().material.mainTexture = texture;
         DrawCells();
    }

    private void Update()
    {
        for (int y = height - 1; y > 0; --y)
        {
            for (int x = 0; x < width; ++x)
            {
                var currentCell = _cells[x, y];
                if(currentCell._cellId == 0)
                    continue;
                else
                {
                    var bottom = CheckBottom(x, y);
                    if(bottom)
                        continue;
                    
                    SwapCells(x,y, x, y-1);
                }
            }
        }
        DrawCells();
    }

    void SwapCells(int x, int y, int x1, int y1)
    {
        var temp = _cells[x, y];
        _cells[x, y] = _cells[x1, y1];
        _cells[x1, y1] = temp;
    }

    bool CheckBottom(int x, int y)
    {
        if (y - 1 < 0)
            return true;

        var bottomCell = _cells[x, y - 1];
        return (bottomCell._cellId != 0);
    }

    private void DrawCells()
    {
        // GetComponentInChildren<Transform>().localScale = new Vector3(width, height, 1);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                var x1 = (int) (x / textureWidth);
                var y1 = (int)(y / textureHeight);
                var color = (_cells[x1, y1]._cellId == 1U) ?  UnityEngine.Color.yellow : UnityEngine.Color.gray;
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
    }
    
}
