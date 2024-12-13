using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Serialization;

/// <summary>
/// 地图初始化数据
/// </summary>
[Serializable]
public class MapInitData
{
    public int mapSize;
    public int mapSeed;
    public int spawnSeed;
    public float marshLimit;
}

/// <summary>
/// 地图数据
/// </summary>
[Serializable]
public class MapData
{
    // 当前地图对象ID取值
    public ulong CurrentID = 1;
    // 当前玩家去过的所有地图块（已经生成过的地图块）
    public List<Serialization_Vector2> MapChunkIndexList = new List<Serialization_Vector2>();
}

/// <summary>
/// 地图块数据
/// </summary>
[Serializable]
public class MapChunkData
{
    public Serialization_Dic<ulong, MapObjectData> MapObjectDic ;
    public Serialization_Dic<ulong, MapObjectData> AIObjectDic ; 
   [NonSerialized] public List<MapVertex> forestLsit;
    [NonSerialized]public List<MapVertex> MarshLsit;
}

/// <summary>
/// 地图块上一个对象的数据
/// </summary>
[Serializable]
public class MapObjectData
{
    // 唯一的身份标识
    public ulong ID;

    public ulong discardGameObjectID;
    // 当前到底是什么配置
    public int ConfigID;
    // 剩余的腐烂天数，-1代表无效
    public int DestoryDays;
    // 坐标
    private Serialization_Vector3 position;
    public Vector3 Position
    {
        get => position.ConverToVector3();
        set => position = value.ConverToSVector3();
    }
}