using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using CodeMonkey.Utils;

public struct Cell
{
    public int CellId;
    public int Density;
    // Gravity is y velocity and spread factor is x
    public Vector2Int Movement;
    public bool IsFalling;
    public float InertiaResistance;

    public bool Fluid;
    public bool AntiGravity;
    
    // Custom Cell Options
    

    public Cell(int cellId, int density)
    {
        CellId = cellId;
        Density = density;
        Movement = new Vector2Int(0, 0);
        IsFalling = false;
        InertiaResistance = 0.0f;
        Fluid = false;
        AntiGravity = false;
    }
    public Cell(int cellId, int density, Vector2Int movement, bool falling, float resistance)
    {
        CellId = cellId;
        Density = density;
        Movement = movement;
        IsFalling = falling;
        InertiaResistance = resistance;
        Fluid = false;
        AntiGravity = false;
    }

    public Cell(int cellId, int density, Vector2Int movement, bool falling, float resistance, bool fluid, bool antiGravity)
    {
        CellId = cellId;
        Density = density;
        Movement = movement;
        IsFalling = falling;
        InertiaResistance = resistance;
        AntiGravity = antiGravity;
        Fluid = fluid;
    }
}

public class Simulation : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;
    [SerializeField] private int chunkSize;
    
    // Custom Cell Options
    [SerializeField] private int density;
    [SerializeField] private Vector2Int movement;
    [SerializeField] private float resistance;
    [SerializeField] private bool fluid;
    [SerializeField] private bool antiGravity;
    [SerializeField] private Color customColor;
    

    // Cell Variables
    private const int Gravity = 1;

    // Colors
    private static readonly Color EmptyColor = Color.HSVToRGB(0.0f,0.0f,0.27f);
    private static readonly Color DebugColor = Color.cyan;
    
    // Cells
    private static readonly Cell EmptyCell = new(0, 0);
    private static readonly Cell SandCell = new(1, 10, new Vector2Int(1, Gravity), true, 0.1f);
    private static readonly Cell StoneCell = new(2, 100);
    private static readonly Cell WaterCell = new(3, 5, new Vector2Int(2, Gravity), false, 0.0f);
    private static readonly Cell DirtCell = new(4, 10, new Vector2Int(1, Gravity), true, 0.5f);
    private static readonly Cell CoalCell = new(5, 10, new Vector2Int(1, Gravity), true, 0.75f);

    private List<GameObject> _objects;
    private Cell[] _cellData;
    private Vector3 _originPosition;

    private void Awake()
    {
        CreateGrid(chunkSize, new Vector3(0,0));
    }

    private void Update()
    {
        var mousePosition = UtilsClass.GetMouseWorldPosition();
        
        if (Input.GetMouseButton(1))
        {
            SetValue(mousePosition, 0);
        }

        if(Input.GetKey(KeyCode.S))
            SetValue(mousePosition, 1);
        if(Input.GetKey(KeyCode.Space))
            SetValue(mousePosition, 2);
        if(Input.GetKey(KeyCode.W))
            SetValue(mousePosition, 3);
        if(Input.GetKey(KeyCode.D))
            SetValue(mousePosition, 4);
        if(Input.GetKey(KeyCode.C))
            SetValue(mousePosition, 5);
        if (Input.GetKey(KeyCode.U))
            SetValue(mousePosition, 100);

        if(Time.frameCount % 1 == 0)
            UpdateGrid();
    }

    private void UpdateGrid()
    {
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                var i = x * chunkSize + y;
                
                if(_cellData[i].CellId == 1)
                    UpdateSand(x, y);
                if(_cellData[i].CellId == 3)
                    UpdateWater(x, y);
                if(_cellData[i].CellId == 4)
                    UpdateDirt(x, y);
                if(_cellData[i].CellId == 5)
                    UpdateCoal(x, y);
                if(_cellData[i].CellId == 5)
                    UpdateCoal(x, y);
                if (_cellData[i].CellId == 100)
                    UpdateCustom(x, y);
            }
        }

        // for (int y = 0; y < chunkSize; y++)
        // {
        //     for (int x = chunkSize - 1; x >= 0; x--)
        //     {
        //         var i = x * chunkSize + y;
                
        //         if (_cellData[i].CellId == 100 && _cellData[i].AntiGravity)
        //             UpdateCustom(x, y);
        //     }
        // }
    }

    private void UpdateSand(int x, int y)
    {
        var currentLocation = new Vector2Int(x, y);
        var moveDown = SwapWith(x, y, 0, -1, SandCell);
        var currentCell = _cellData[x * chunkSize + y];
        var releaseChance = Random.value;
        var isReleased = releaseChance > currentCell.InertiaResistance;

        if (moveDown != currentLocation)
        {
            _cellData[x * chunkSize + y].IsFalling = true;
            // if (isReleased) UpdateFreeFalling(x, y);
            Swap(x,y,moveDown.x, moveDown.y);
            return;
        }

        if (currentCell.IsFalling && isReleased)
        {
            var moveRightDown = SwapWith(x, y, 1, -1, SandCell);

            if (moveRightDown != currentLocation)
            {
                UpdateFreeFalling(x, y);
                Swap(x,y,moveRightDown.x, moveRightDown.y);
                return;
            }
        
            var moveLeftDown = SwapWith(x, y, -1, -1, SandCell);
            if (moveLeftDown != currentLocation)
            {
                UpdateFreeFalling(x, y);
                Swap(x,y,moveLeftDown.x, moveLeftDown.y);
                return;
            }
        }
        
        _cellData[x * chunkSize + y].IsFalling = false;
    }
    
    private void UpdateDirt(int x, int y)
    {
        var currentLocation = new Vector2Int(x, y);
        var moveDown = SwapWith(x, y, 0, -1, DirtCell);
        var currentCell = _cellData[x * chunkSize + y];
        var releaseChance = Random.value;
        var isReleased = releaseChance > currentCell.InertiaResistance;
        
        if (moveDown != currentLocation)
        {
            _cellData[x * chunkSize + y].IsFalling = true;
            if (isReleased) UpdateFreeFalling(x, y);
            Swap(x,y,moveDown.x, moveDown.y);
            return;
        }

        if (currentCell.IsFalling && isReleased)
        {
            var moveRightDown = SwapWith(x, y, 1, -1, DirtCell);

            if (moveRightDown != currentLocation)
            {
                UpdateFreeFalling(x, y);
                Swap(x,y,moveRightDown.x, moveRightDown.y);
                return;
            }
        
            var moveLeftDown = SwapWith(x, y, -1, -1, DirtCell);
            if (moveLeftDown != currentLocation)
            {
                UpdateFreeFalling(x, y);
                Swap(x,y,moveLeftDown.x, moveLeftDown.y);
                return;
            }
        }
        
        _cellData[x * chunkSize + y].IsFalling = false;
    }
    
    private void UpdateCoal(int x, int y)
    {
        var currentLocation = new Vector2Int(x, y);
        var moveDown = SwapWith(x, y, 0, -1, CoalCell);
        var currentCell = _cellData[x * chunkSize + y];
        var releaseChance = Random.value;
        var isReleased = releaseChance > currentCell.InertiaResistance;

        if (moveDown != currentLocation)
        {
            _cellData[x * chunkSize + y].IsFalling = true;
            if (isReleased) UpdateFreeFalling(x, y);
            Swap(x,y,moveDown.x, moveDown.y);
            return;
        }

        if (currentCell.IsFalling && isReleased)
        {
            var moveRightDown = SwapWith(x, y, 1, -1, CoalCell);

            if (moveRightDown != currentLocation)
            {
                UpdateFreeFalling(x, y);
                Swap(x,y,moveRightDown.x, moveRightDown.y);
                return;
            }
        
            var moveLeftDown = SwapWith(x, y, -1, -1, CoalCell);
            if (moveLeftDown != currentLocation)
            {
                UpdateFreeFalling(x, y);
                Swap(x,y,moveLeftDown.x, moveLeftDown.y);
                return;
            }
        }
        
        _cellData[x * chunkSize + y].IsFalling = false;
    }
    
    private void UpdateWater(int x, int y)
    {
        var currentLocation = new Vector2Int(x, y);
        var moveDown = SwapWith(x, y, 0, -1, WaterCell);
        
        if (moveDown != currentLocation)
        {
            Swap(x,y,moveDown.x, moveDown.y);
            return;
        }
        var moveRightDown = SwapWith(x, y, 1, -1, WaterCell);

        if (moveRightDown != currentLocation)
        {
            Swap(x,y,moveRightDown.x, moveRightDown.y);
            return;
        }
        
        var moveLeftDown = SwapWith(x, y, -1, -1, WaterCell);
        if (moveLeftDown != currentLocation)
        {
            Swap(x,y,moveLeftDown.x, moveLeftDown.y);
            return;
        }

        
        var moveLeft = SwapWith(x, y, -1, 0, WaterCell);
        var moveRight = SwapWith(x, y, 1, 0, WaterCell);

        var availableDirs = currentLocation != moveLeft && currentLocation != moveRight;
        var dirChange = Random.value;
        if (availableDirs)
        {
            if (dirChange > 0.5f)
            {
                Swap(x, y, moveRight.x, moveRight.y);
            }
            else
            { 
                Swap(x,y, moveLeft.x, moveLeft.y);
            }
        }
        else
        {
            if(moveRight != currentLocation)
                Swap(x,y, moveRight.x, moveRight.y);
            else
                Swap(x,y, moveLeft.x, moveLeft.y);
        }


    }

    private void UpdateCustom(int x, int y)
    {
        var currentLocation = new Vector2Int(x, y);
        var currentCell = _cellData[x * chunkSize + y];
        var up = currentCell.AntiGravity ? 1 : -1;

        var moveDown = SwapWith(x, y, 0, up, currentCell);
        var releaseChance = Random.value;
        var isReleased = releaseChance > currentCell.InertiaResistance;
        
        if (moveDown != currentLocation)
        {
            _cellData[x * chunkSize + y].IsFalling = true;
            if (isReleased) UpdateFreeFalling(x, y);
            Swap(x,y,moveDown.x, moveDown.y);
            return;
        }

        if (currentCell.IsFalling && isReleased || currentCell.Fluid)
        {
            var moveRightDown = SwapWith(x, y, 1, up, currentCell);

            if (moveRightDown != currentLocation)
            {
                UpdateFreeFalling(x, y);
                Swap(x,y,moveRightDown.x, moveRightDown.y);
                return;
            }
        
            var moveLeftDown = SwapWith(x, y, -1, up, currentCell);
            if (moveLeftDown != currentLocation)
            {
                UpdateFreeFalling(x, y);
                Swap(x,y,moveLeftDown.x, moveLeftDown.y);
                return;
            }

            if(currentCell.Fluid){
                var moveLeft = SwapWith(x, y, -1, 0, currentCell);
                var moveRight = SwapWith(x, y, 1, 0, currentCell);

                var availableDirs = currentLocation != moveLeft && currentLocation != moveRight;
                var dirChange = Random.value;
                if (availableDirs)
                {
                    if (dirChange > 0.5f)
                    {
                        Swap(x, y, moveRight.x, moveRight.y);
                    }
                    else
                    { 
                        Swap(x,y, moveLeft.x, moveLeft.y);
                    }
                }
                else
                {
                    if(moveRight != currentLocation)
                        Swap(x,y, moveRight.x, moveRight.y);
                    else
                        Swap(x,y, moveLeft.x, moveLeft.y);
                }
            }
        }
        
        _cellData[x * chunkSize + y].IsFalling = false;
    }
    
    private bool CanSwap(int x, int y, Cell cell)
    {
        if (y < 0 || x < 0 || x >= chunkSize || y >= chunkSize)
            return false;

        var density = cell.Density;
        return _cellData[x * chunkSize + y].Density < density;
    }

    private Vector2Int SwapWith(int x, int y, int xDir, int yDir, Cell cell)
    {
        var finalX = x + xDir * cell.Movement.x;
        var finalY = y + yDir * cell.Movement.y;
        var previousX = x;
        var previousY = y;
        for (var i = 1; i <= cell.Movement.x + 1; i++)
        {
            for (var j = 1; j <= cell.Movement.y + 1; j++)
            {
                var currentX = x + xDir * i;
                var currentY = y + yDir * j;
                if (!CanSwap(currentX, currentY, cell))
                    return new Vector2Int(previousX, previousY);
                previousX = currentX;
                previousY = currentY;
            }
        }

        return new Vector2Int(finalX, finalY);
    }

    private void Swap(int xInit, int yInit, int xFinal, int yFinal)
    {
        var iInit = xInit * chunkSize + yInit;
        var iFinal = xFinal * chunkSize + yFinal;

        // Swap Data
        (_cellData[iInit], _cellData[iFinal]) = (_cellData[iFinal], _cellData[iInit]);

        (_objects[iInit].transform.position, _objects[iFinal].transform.position) = (_objects[iFinal].transform.position, _objects[iInit].transform.position);

        (_objects[iInit], _objects[iFinal]) = (_objects[iFinal], _objects[iInit]);
        
        // Cell tempData = _cellData[final_i];
        // _cellData[final_i] = _cellData[init_i];
        // _cellData[init_i] = tempData;
        //
        // Vector3 tempPosition = _objects[init_i].transform.position;
        // _objects[init_i].transform.position = _objects[final_i].transform.position;
        // _objects[final_i].transform.position = tempPosition;
        //
        // GameObject tempCell = _objects[init_i];
        // _objects[init_i] = _objects[final_i];
        // _objects[final_i] = tempCell;
    }

    private void UpdateFreeFalling(int x, int y)
    {
        for (var i = x-1; i <= x+1; i++)
        {
            for (var j = y-1; j <= y+1; j++)
            {
                if (j < 0 || i < 0 || j >= chunkSize || i >= chunkSize) continue;
                
                var index = i * chunkSize + j;
                if (_cellData[index].CellId == 0) continue;
                
                
                _cellData[index].IsFalling = true;
            }
        }
    }

    
    /* Creates Grid */
    private void CreateGrid(int size, Vector3 originPosition)
    {
        _objects = new List<GameObject>();
        _cellData = new Cell[chunkSize * chunkSize];
        _originPosition = originPosition;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                CreateCell(0, x, y, EmptyColor);
            }
        }
    }

    private void CreateCell(int id, int x, int y, Color color)
    {
        GameObject cell = new GameObject("Cell: " + x + ", " + y, typeof(MeshFilter), typeof(MeshRenderer));
        cell.GetComponent<MeshFilter>().mesh = mesh;
        cell.GetComponent<MeshRenderer>().material = new Material(material);
        cell.GetComponent<MeshRenderer>().material.color = color;
        //cell.AddComponent<SpinBehaviour>();
        cell.transform.position = new Vector3(x + _originPosition.x, y + _originPosition.y, Random.Range(-0.1f, 0.1f));
        cell.transform.SetParent(transform, false);
        _objects.Add(cell);
        
        _cellData[x * chunkSize + y] = EmptyCell;
    }

    
    /* Getter and Setter Methods */
    private void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - _originPosition).x);
        y = Mathf.FloorToInt((worldPosition - _originPosition).y);
    }

    private void GetValue(Vector3 position)
    {
        GetXY(position, out var x, out var y);
        GetPosition(x, y);
    }

    private void GetPosition(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < chunkSize && y < chunkSize)
        {
            Debug.Log(_cellData[x * chunkSize + y].CellId);        
        }
    }
    
    private void SetValue(Vector3 position, int id)
    {
        GetXY(position, out var x, out var y);
        SetValue(x, y, id);
    }

    private void SetValue(int x, int y, int id)
    {
        if (x < 0 || y < 0 || x >= chunkSize || y >= chunkSize) return;
        var i = x * chunkSize + y;
        if (_cellData[i].CellId != 0 && id != 0) return;
        switch (id)
        {
            case 0:
            {
                _cellData[i] = EmptyCell;
                _objects[i].GetComponent<MeshRenderer>().material.color = EmptyColor;
                break;
            }
            case 1:
            {
                var sandCell = Color.HSVToRGB(.125f, 0.351f, Random.Range(0.8f, 0.7f));
                _cellData[i] = SandCell;
                _objects[i].GetComponent<MeshRenderer>().material.color = sandCell;
                break;
            }
            case 2:
            {
                var sandCell = Color.HSVToRGB(.125f, 0.05f, Random.Range(0.5f, 0.6f));
                _cellData[i] = StoneCell;
                _objects[i].GetComponent<MeshRenderer>().material.color = sandCell;
                break;
            }
            case 3:
            {
                var waterCell = Color.HSVToRGB(.55f, 0.59f, Random.Range(0.64f, 0.73f));
                _cellData[i] = WaterCell;
                _objects[i].GetComponent<MeshRenderer>().material.color = waterCell;
                break;
            }
            case 4:
            {
                var dirtCell = Color.HSVToRGB(.07f, 0.26f, Random.Range(0.20f, 0.45f));
                _cellData[i] = DirtCell;
                _objects[i].GetComponent<MeshRenderer>().material.color = dirtCell;
                break;
            }
            case 5:
            {
                var coalCell = Color.HSVToRGB(.5f, 0.09f, Random.Range(0.0f, 0.15f));
                _cellData[i] = CoalCell;
                _objects[i].GetComponent<MeshRenderer>().material.color = coalCell;
                break;
            }
            case 100:
            {
                _cellData[i] = new Cell(100, density, movement, true, resistance, fluid, antiGravity);
                _objects[i].GetComponent<MeshRenderer>().material.color = customColor;
                break;
            }
        }
    }
   
}
