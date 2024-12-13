using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;

/// <summary>
/// 存档管理器，用于管理游戏中的存档数据
/// </summary>
public class ArchiveManager : Singleton<ArchiveManager>
{
    /// <summary>
    /// 构造函数，初始化存档管理器，并加载存档数据
    /// </summary>
    public ArchiveManager()
    {
        // 加载存档数据
        LoadSaveData();
    }

    // 玩家位置数据
    public PlayerTransformData PlayerTransformData { get; private set; }

    // 玩家主要数据，如等级、经验等
    public PlayerMainData PlayerMainData { get; private set; }

    // 地图初始化数据
    public MapInitData MapInitData { get; private set; }

    // 地图当前状态数据
    public MapData MapData { get; private set; }
    public Serialization_Dic<ulong, IMapObjectTypeData> ImapObjectTypeDataDic{ get; private set; } 

    // 玩家背包数据
    public InventoryMainData InventoryMainData { get; private set; }

    // 游戏时间数据
    public TimeData TimeData { get; private set; }
    
    //科技数据
    public ScienceMachineData ScienceMachineData { get; private set; }

    /// <summary>
    /// 是否有存档
    /// </summary>
    public bool HaveArchive { get; private set; }


    /// <summary>
    /// 加载存档数据，用于初始化存档系统的主要数据
    /// </summary>
    public void LoadSaveData()
    {
        // 获取存档项，这里假设只有一个存档（编号为0）
        SaveItem saveItem = SaveManager.GetSaveItem(0);

        // 判断是否存在存档
        HaveArchive = saveItem != null;
    }



 /// <summary>
/// 创建新的存档
/// 初始化方式：根据传入参数设置地图、玩家状态和物品等数据
/// </summary>
/// <param name="mapSize">地图大小</param>
/// <param name="mapSeed">地图生成种子</param>
/// <param name="spawnSeed">出生点种子</param>
/// <param name="marshLimit">沼泽限制参数</param>
public void CreateNewArchive(int mapSize, int mapSeed, int spawnSeed, float marshLimit)
{
    // 清除当前存档数据
    SaveManager.Clear();

    // 创建新的存档项
    // 1. 在 SaveManager 中创建一个新的 SaveItem
    SaveManager.CreateSaveItem();
    HaveArchive = true;

    // 2. 初始化各类数据对象并保存
    // 地图初始化数据
    MapInitData = new MapInitData()
    {
        mapSize = mapSize,
        mapSeed = mapSeed,
        spawnSeed = spawnSeed,
        marshLimit = marshLimit
    };
    SaveManager.SaveObject(MapInitData);

    // 获取地图配置，用于计算世界中的地图大小
    MapConfig mapConfig = ConfigManager.Instance.GetConfig<MapConfig>(ConfigName.map);
    float mapSizeOnWorld = mapSize * mapConfig.mapChunkSize * mapConfig.cellSize;

    // 设置玩家初始位置和旋转角度
    PlayerTransformData = new PlayerTransformData()
    {
        Position = new Vector3(mapSizeOnWorld / 2, 0, mapSizeOnWorld / 2), // 设置玩家出生在地图中心
        Rotation = Vector3.zero // 初始无旋转
    };
    SavePlayerTransformData();

    // 获取玩家配置，初始化玩家状态（如生命值和饥饿值）
    PlayerConfig playerConfig = ConfigManager.Instance.GetConfig<PlayerConfig>(ConfigName.player);
    PlayerMainData = new PlayerMainData()
    {
        Hp = playerConfig.MaxHp, // 设置初始生命值
        Hungry = playerConfig.MaxHungry // 设置初始饥饿值
    };
    SavePlayerMainData();

    // 初始化地图状态数据
    MapData = new MapData();
    ImapObjectTypeDataDic = new Serialization_Dic<ulong, IMapObjectTypeData>();
    SaveMapData();

    // 初始化背包数据，并设置背包容量为14
    InventoryMainData = new InventoryMainData(14);

    #region 初始化背包物品
    // 初始化物品数据，预设若干物品
    InventoryMainData.ItemDatas[0] = ItemData.CreateItemData(0); // 物品ID为0
    (InventoryMainData.ItemDatas[0].ItemTypeData as ItemMaterialData).Count = 5; // 设置数量为5

    InventoryMainData.ItemDatas[1] = ItemData.CreateItemData(1); // 物品ID为1

    InventoryMainData.ItemDatas[2] = ItemData.CreateItemData(2); // 物品ID为2
    (InventoryMainData.ItemDatas[2].ItemTypeData as ItemWeaponData).Durability = 60; // 设置耐久度为60

    InventoryMainData.ItemDatas[3] = ItemData.CreateItemData(3); // 物品ID为3
    (InventoryMainData.ItemDatas[3].ItemTypeData as ItemConsumableData).Count = 10; // 设置数量为10

    InventoryMainData.ItemDatas[4] = ItemData.CreateItemData(4); // 物品ID为4
    InventoryMainData.ItemDatas[5] = ItemData.CreateItemData(5); // 物品ID为5
    #endregion
    SaveInventoryData();

    // 初始化时间数据
    TimeConfig timeConfig = ConfigManager.Instance.GetConfig<TimeConfig>(ConfigName.time);
    TimeData = new TimeData()
    {
        StateIndex = 0, // 初始状态索引（例如白天/夜晚）
        DayNum = 0, // 游戏天数初始为0
        CalculateTime = timeConfig.TimeStateConfigs[0].durationTime // 设置初始时间状态的持续时间
    };
    SaveTimeData();
    //初始化科技数据
    ScienceMachineData = new ScienceMachineData();
}

/// <summary>
/// 加载当前存档 - 用于恢复游戏状态
/// </summary>
public void LoadCurrentArchive()
{
    // 从存档管理器中加载地图初始化数据
    MapInitData = SaveManager.LoadObject<MapInitData>();

    // 从存档管理器中加载玩家位置和旋转数据
    PlayerTransformData = SaveManager.LoadObject<PlayerTransformData>();

    //得到浆果数据
    ImapObjectTypeDataDic = SaveManager.LoadObject<Serialization_Dic<ulong, IMapObjectTypeData>>();
  
    // 从存档管理器中加载地图当前状态数据
    MapData = SaveManager.LoadObject<MapData>();

    // 从存档管理器中加载玩家背包数据
    InventoryMainData = SaveManager.LoadObject<InventoryMainData>();

    // 从存档管理器中加载游戏时间数据
    TimeData = SaveManager.LoadObject<TimeData>();

    // 从存档管理器中加载玩家主要数据（如生命值、饥饿值）
    PlayerMainData = SaveManager.LoadObject<PlayerMainData>();
     //读取科技数据
     ScienceMachineData = SaveManager.LoadObject<ScienceMachineData>();

}

public void CleanArchive()
{
    SaveManager.Clear();
    LoadSaveData();
}
/// <summary>
/// 保存玩家位置数据到存档系统
/// </summary>
public void SavePlayerTransformData()
{
    // 将玩家位置和旋转数据保存到存档管理器
    SaveManager.SaveObject(PlayerTransformData);
}

/// <summary>
/// 保存玩家主要数据到存档系统
/// </summary>
public void SavePlayerMainData()
{
    // 将玩家的主要状态（如生命值、饥饿值）保存到存档管理器
    SaveManager.SaveObject(PlayerMainData);
}

/// <summary>
/// 保存地图数据到存档系统
/// </summary>
public void SaveMapData()
{
    SavMapObjectTypeData();
    // 将地图的当前状态数据保存到存档管理器
    SaveManager.SaveObject(MapData);
}

/// <summary>
/// 添加并保存新的地图区块数据
/// </summary>
/// <param name="chunkIndex">区块索引</param>
/// <param name="mapChunkData">区块数据对象</param>
public void AddAndSaveMapChunkData(Vector2Int chunkIndex, MapChunkData mapChunkData)
{
    // 将 Vector2Int 类型的区块索引转换为可序列化的格式
    Serialization_Vector2 index = chunkIndex.ConverToSVector2();

    // 将区块索引添加到地图数据的索引列表中
    MapData.MapChunkIndexList.Add(index);

    // 保存更新后的地图数据
    SaveMapData();

    // 保存该区块的数据，文件名为 "Map_<区块索引>"
    SaveManager.SaveObject(mapChunkData, "Map_" + index.ToString());
}


/// <summary>
/// 保存一个地图区块数据到存档系统
/// </summary>
/// <param name="chunkIndex">区块索引 (Vector2Int 类型)</param>
/// <param name="mapChunkData">区块数据对象</param>
public void SaveMapChunkData(Vector2Int chunkIndex, MapChunkData mapChunkData)
{
    // 将 Vector2Int 类型的区块索引转换为可序列化的格式
    Serialization_Vector2 index = chunkIndex.ConverToSVector2();

    // 保存该区块数据到存档管理器，文件名格式为 "Map_<区块索引>"
    SaveManager.SaveObject(mapChunkData, "Map_" + index.ToString());
}

/// <summary>
/// 获取一个地图区块的存档数据
/// </summary>
/// <param name="chunkIndex">区块索引 (Serialization_Vector2 类型)</param>
/// <returns>返回指定索引的地图区块数据</returns>
public MapChunkData GetMapChunkData(Serialization_Vector2 chunkIndex)
{
    // 从存档管理器中加载指定区块的数据，文件名格式为 "Map_<区块索引>"
    return SaveManager.LoadObject<MapChunkData>("Map_" + chunkIndex.ToString());
}

/// <summary>
/// 保存玩家背包数据到存档系统
/// </summary>
public void SaveInventoryData()
{
    // 将玩家的背包数据（物品信息）保存到存档管理器
    SaveManager.SaveObject(InventoryMainData);
}

/// <summary>
/// 保存游戏时间数据到存档系统
/// </summary>
public void SaveTimeData()
{
    // 将当前的游戏时间数据保存到存档管理器
    SaveManager.SaveObject(TimeData);
}

public IMapObjectTypeData GetMapObjectTypeData(ulong ID)
{
    return ImapObjectTypeDataDic.Dictionary[ID];
}
/// <summary>
/// 判断是否存在这个键
/// </summary>
public bool TryGetMapObjectTypeData(ulong ID, out IMapObjectTypeData mapObjectTypeData)
{
    return ImapObjectTypeDataDic.Dictionary.TryGetValue(ID,out   mapObjectTypeData);
}
/// <summary>
/// 添加地图对象类型数据
/// </summary>
public void AddMapObjectTypeData(ulong ID, IMapObjectTypeData mapObjectTypeData)
{
    ImapObjectTypeDataDic.Dictionary.Add(ID,mapObjectTypeData);
}
/// <summary>
/// 保存类型对象地图数据
/// </summary>
public void SavMapObjectTypeData()
{
    SaveManager.SaveObject(ImapObjectTypeDataDic);
}

public void SaveScienceMachineData()
{
    SaveManager.SaveObject(ScienceMachineData);
}
}

