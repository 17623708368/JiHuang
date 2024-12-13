using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 表示一个地图网格，用于存储网格的顶点和单元格数据
/// </summary>
public class MapGrid
{
    // 顶点字典，存储所有顶点信息（键为顶点坐标，值为顶点对象）
    public Dictionary<Vector2Int, MapVertex> vertexDic = new Dictionary<Vector2Int, MapVertex>();
    
    // 单元格字典，存储所有单元格信息（键为单元格坐标，值为单元格对象）
    public Dictionary<Vector2Int, MapCell> cellDic = new Dictionary<Vector2Int, MapCell>();

    /// <summary>
    /// 构造函数，用于初始化地图网格
    /// </summary>
    /// <param name="mapHeight">地图高度（单位为网格数量）</param>
    /// <param name="mapWdith">地图宽度（单位为网格数量）</param>
    /// <param name="cellSize">单元格大小（单位为实际距离）</param>
    public MapGrid(int mapHeight, int mapWdith, float cellSize)
    {
        MapHeight = mapHeight; // 设置地图高度
        MapWidth = mapWdith;   // 设置地图宽度
        CellSize = cellSize;   // 设置单元格大小

        // 创建所有的顶点和单元格
        for (int x = 1; x < mapWdith; x++)
        {
            for (int z = 1; z < mapHeight; z++)
            {
                AddVertex(x, z); // 添加顶点
                AddCell(x, z);   // 添加单元格
            }
        }

        // 补充边界上的单元格，确保地图边缘完整
        for (int x = 1; x <= mapWdith; x++)
        {
            AddCell(x, mapHeight); // 添加地图顶部的单元格
        }
        for (int z = 1; z < mapWdith; z++)
        {
            AddCell(mapWdith, z); // 添加地图右侧的单元格
        }
    }

    // 地图的高度（只读属性）
    public int MapHeight { get; private set; }
    
    // 地图的宽度（只读属性）
    public int MapWidth { get; private set; }
    
    // 单元格的大小（只读属性）
    public float CellSize { get; private set; }

    #region 顶点操作

    /// <summary>
    /// 添加一个顶点到顶点字典
    /// </summary>
    /// <param name="x">顶点的 X 坐标（网格索引）</param>
    /// <param name="y">顶点的 Y 坐标（网格索引）</param>
    private void AddVertex(int x, int y)
    {
        vertexDic.Add(
            new Vector2Int(x, y), // 键为顶点坐标
            new MapVertex()       // 值为 MapVertex 对象
            {
                Position = new Vector3(x * CellSize, 0, y * CellSize) // 设置顶点的世界坐标
            });
    }

    /// <summary>
    /// 根据索引获取顶点对象，如果不存在则返回 Null
    /// </summary>
    /// <param name="index">顶点的索引（Vector2Int 格式）</param>
    /// <returns>对应的 MapVertex 对象或 Null</returns>
    public MapVertex GetVertex(Vector2Int index)
    {
        MapVertex vertex = null;
        vertexDic.TryGetValue(index, out vertex); // 尝试从字典中获取顶点
        return vertex; // 返回顶点对象
    }

    /// <summary>
    /// 根据 X 和 Y 索引获取顶点对象
    /// </summary>
    /// <param name="x">顶点的 X 坐标（网格索引）</param>
    /// <param name="y">顶点的 Y 坐标（网格索引）</param>
    /// <returns>对应的 MapVertex 对象或 Null</returns>
    public MapVertex GetVertex(int x, int y)
    {
        return GetVertex(new Vector2Int(x, y)); // 转换为 Vector2Int 调用方法
    }

    /// <summary>
    /// 根据世界坐标获取顶点对象
    /// </summary>
    /// <param name="position">世界空间中的位置</param>
    /// <returns>对应的 MapVertex 对象</returns>
    public MapVertex GetVertexByWorldPosition(Vector3 position)
    {
        // 将世界坐标转换为网格坐标索引
        int x = Mathf.Clamp(Mathf.RoundToInt(position.x / CellSize), 1, MapWidth); // 限制在有效范围内
        int y = Mathf.Clamp(Mathf.RoundToInt(position.z / CellSize), 1, MapHeight); // 限制在有效范围内
        return GetVertex(x, y); // 返回对应索引的顶点
    }
    /// <summary>
    /// 通过索引设置顶点类型
    /// </summary>
    private void SetVertexType(Vector2Int vertexIndex, MapVertexType mapVertexType)
    {
        MapVertex vertex = GetVertex(vertexIndex);
        if (vertex.VertexType != mapVertexType)
        {
            vertex.VertexType = mapVertexType;
            if (vertex.VertexType == MapVertexType.Marsh)
            { 
                MapCell tempCell = GetLeftBottomMapCell(vertexIndex);
                if (tempCell != null) tempCell.TextureIndex += 1;

                tempCell = GetRightBottomMapCell(vertexIndex);
                if (tempCell != null) tempCell.TextureIndex += 2;

                 tempCell = GetLeftTopMapCell(vertexIndex);
                if (tempCell != null) tempCell.TextureIndex += 4;

                tempCell = GetRightTopMapCell(vertexIndex);
                if (tempCell != null) tempCell.TextureIndex += 8;
            }
        }
    }

    /// <summary>
    /// 设置顶点类型
    /// </summary>
    private void SetVertexType(int x,int y,MapVertexType mapVertexType)
    {
        SetVertexType(new Vector2Int(x, y), mapVertexType);
    }
    #endregion

    #region 格子
    private void AddCell(int x, int y)
    {
        float offset = CellSize / 2;
        cellDic.Add(new Vector2Int(x, y), 
            new MapCell()
            {
                Position = new Vector3(x * CellSize - offset, 0, y * CellSize - offset)
            }
        );
    }

    /// <summary>
    /// 得到格子
    /// </summary>
    public MapCell GetCell(Vector2Int index)
    {
        MapCell cell = null;
        cellDic.TryGetValue(index, out cell);
        return cell;
    }

    public MapCell GetCell(int x,int y)
    {
        return GetCell(new Vector2Int(x,y));
    }

    /// <summary>
    /// 得到左下角的格子
    /// </summary>
    public MapCell GetLeftBottomMapCell(Vector2Int vertexIndex)
    {
        return GetCell(vertexIndex);
    }

    /// <summary>
    ///得到右下角的格子
    /// </summary>
    public MapCell GetRightBottomMapCell(Vector2Int vertexIndex)
    {
        return GetCell(vertexIndex.x+1,vertexIndex.y);
    }

    /// <summary>
    /// 得到左上角的格子
    /// </summary>
    public MapCell GetLeftTopMapCell(Vector2Int vertexIndex)
    {
        return GetCell(vertexIndex.x, vertexIndex.y+1);
    }

    /// <summary>
    /// ��ȡ���ϽǸ���
    /// </summary>
    public MapCell GetRightTopMapCell(Vector2Int vertexIndex)
    {
        return GetCell(vertexIndex.x+1, vertexIndex.y + 1);
    }
    #endregion

    /// <summary>
    /// 技术按顶点类型
    /// </summary>
    public void CalculateMapVertexType(float[,] noiseMap,float limit)
    { 
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        for (int x = 1; x < width; x++)
        {
            for (int z = 1; z < height; z++)
            {
                 
                if (noiseMap[x,z] >=limit)
                {
                    SetVertexType(x, z, MapVertexType.Marsh);
                }
                else
                {
                    SetVertexType(x, z, MapVertexType.Forest);
                }
            }
        }
    }
}

/// <summary>
/// 地图顶点类型枚举
/// </summary>
public enum MapVertexType
{ 
    None,
    Forest, //森林
    Marsh,  //沼泽
}

/// <summary>
/// 地图顶点类，表示网格的顶点
/// </summary>
public class MapVertex
{
    public Vector3 Position;          // 顶点的世界位置
    public MapVertexType VertexType; // 顶点的类型（森林或沼泽）
    public ulong MapObjectID;         // 顶点上绑定的地图对象 ID（0 表示没有对象）
}

/// <summary>
/// 地图单元格类，表示网格的单元格
/// </summary>
public class MapCell
{
    public Vector3 Position;   // 单元格的中心位置
    public int TextureIndex;  // 单元格的纹理索引（用于渲染）
}
