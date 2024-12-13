using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using UnityEngine.AI;
/// <summary>
/// 地图区块控制器
/// 管理区块的对象实例化、状态切换、数据管理等
/// </summary>
public class MapChunkController : MonoBehaviour
{
    // 区块索引
    public Vector2Int ChunkIndex { get; private set; }

    // 区块中心位置
    public Vector3 CentrePosition { get; private set; }

    // 是否整个区块为森林
    public bool IsAllForest { get; private set; }

    // 区块数据，包含此区块的所有信息
    public MapChunkData MapChunkData { get; private set; }

    // 需要销毁的地图对象数据字典
    private Dictionary<ulong, MapObjectData> wantDestoryMapObjectDic;

    // 地图对象的实例（GameObject）字典
    private Dictionary<ulong, MapObjectBase> mapObjectDic;
    //AI对象字典
    private Dictionary<ulong, AIBase> AIObjectDic;

    // 是否已经初始化
    public bool IsInitialized { get; private set; } = false;

    // 当前区块是否处于激活状态
    private bool isActive = false;

    /// <summary>
    /// 初始化地图区块
    /// </summary>
    public void Init(Vector2Int chunkIndex, Vector3 centrePosition, bool isAllForest, MapChunkData mapChunkData)
    {
        ChunkIndex = chunkIndex;
        CentrePosition = centrePosition;
        MapChunkData = mapChunkData;

        // 初始化地图对象字典和销毁对象字典
        mapObjectDic = new Dictionary<ulong, MapObjectBase>(MapChunkData.MapObjectDic.Dictionary.Count);
        AIObjectDic = new Dictionary<ulong, AIBase>(MapChunkData.AIObjectDic.Dictionary.Count);
        wantDestoryMapObjectDic = new Dictionary<ulong, MapObjectData>();

        // 设置森林标记
        IsAllForest = isAllForest;

        // 设置初始化状态
        IsInitialized = true;

        // 添加需要销毁的地图对象到销毁字典
        foreach (var item in MapChunkData.MapObjectDic.Dictionary.Values)
        {
            if (item.DestoryDays > 0) wantDestoryMapObjectDic.Add(item.ID, item);
        }

        // 注册晨间事件处理
        EventManager.AddEventListener(EventName.OnMorn, OnMorn);
        EventManager.AddEventListener(EventName.SaveGame, OnSaveGame);
    }

    /// <summary>
    /// 设置区块的激活状态
    /// </summary>
    public void SetActive(bool active)
    {
        if (isActive != active)
        {
            isActive = active;
            gameObject.SetActive(isActive);

            // 激活状态：实例化区块内的所有地图对象
            if (isActive)
            {
                //添加地图对象
                foreach (var item in MapChunkData.MapObjectDic.Dictionary.Values)
                {
                    InstantiateMapObject(item, false);
                }

                //添加ai对象
                foreach (var item in MapChunkData.AIObjectDic.Dictionary.Values)
                {
                    InstantiateAIObject(item);
                }
            }
            // 非激活状态：将区块内的所有对象返回对象池
            else
            {
                foreach (var item in mapObjectDic)
                {
                    item.Value.JKGameObjectPushPool();
                }

                foreach (var item in  AIObjectDic)
                {
                    item.Value.Destroy();
                }
                //隐藏ai对象
                mapObjectDic.Clear();
                AIObjectDic.Clear();
            }
        }
    }

    #region 地图对象
    /// <summary>
    /// 实例化地图对象
    /// </summary>
    private void InstantiateMapObject(MapObjectData mapObjectData, bool isFormBuilding)
    {
        // 获取地图对象的配置
        MapObjectConfig config =
            ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.mapOjbect, mapObjectData.ConfigID);

        // 从对象池获取对象并设置初始状态
        MapObjectBase mapObj = PoolManager.Instance.GetGameObject(config.Prefab, transform)
            .GetComponent<MapObjectBase>();
        mapObj.transform.position = mapObjectData.Position;
        mapObj.Init(this, mapObjectData.ID, isFormBuilding);
        mapObj.discardGameObjectID = mapObjectData.discardGameObjectID;
        // 添加到对象字典
        mapObjectDic.Add(mapObjectData.ID, mapObj);
    }

    /// <summary>
    /// 移除一个地图对象
    /// </summary>
    public void RemoveMapObject(ulong mapObjectID)
    {
        // 从数据字典中移除对象数据
        MapChunkData.MapObjectDic.Dictionary.Remove(mapObjectID, out MapObjectData mapObjectData);
        mapObjectData.JKObjectPushPool();

        // 如果对象已实例化，移除显示的对象
        if (mapObjectDic.TryGetValue(mapObjectID, out MapObjectBase mapObjectBase))
        {
            mapObjectBase.JKGameObjectPushPool();
            mapObjectDic.Remove(mapObjectID);
        }

        // 通知地图管理器更新UI或地图状态
        MapManager.Instance.RemoveMapObject(mapObjectID);
    }

    /// <summary>
    /// 添加一个地图对象
    /// 1. 添加到区块数据
    /// 2. 如果激活，则实例化显示
    /// </summary>
    public void AddMapObject(MapObjectData mapObjectData, bool isFormBuilding)
    {
        // 添加到数据字典
        MapChunkData.MapObjectDic.Dictionary.Add(mapObjectData.ID, mapObjectData);

        // 如果对象有销毁时间，加入销毁队列
        if (mapObjectData.DestoryDays > 0) wantDestoryMapObjectDic.Add(mapObjectData.ID, mapObjectData);

        // 如果区块激活，直接实例化对象
        if (isActive)
        {
            InstantiateMapObject(mapObjectData, isFormBuilding);
        }
    }

    #endregion

    #region AI对象
    /// <summary>
    /// 添加一个地图对象
    /// 1. 添加到区块数据
    /// 2. 如果激活，则实例化显示
    /// </summary>
    public void AddAIObject(MapObjectData aiObjectData )
    {
        // 添加到数据字典
        MapChunkData.AIObjectDic.Dictionary.Add(aiObjectData.ID, aiObjectData);
        // 如果区块激活，直接实例化对象
        if (isActive)
        {
            InstantiateAIObject(aiObjectData);
        }
    }
    /// <summary>
    /// 实例化地图对象
    /// </summary>
    private void InstantiateAIObject(MapObjectData mapObjectData)
    {
        // 获取地图对象的配置
        AIConfig aiConfig = ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.ai, mapObjectData.ConfigID);

        // 从对象池获取对象并设置初始状态
        AIBase mapAIObj = PoolManager.Instance.GetGameObject(aiConfig.Prefab, transform)
            .GetComponent<AIBase>();
        if(mapObjectData.Position==Vector3.zero)
        {
            mapObjectData.Position = GetAIObjectRandomPoint(aiConfig.MapVertexType);
        }
    
        mapAIObj.Init(this, mapObjectData);
        // 添加到对象字典
        AIObjectDic.Add(mapObjectData.ID, mapAIObj);
    }

    public Vector3 GetAIObjectRandomPoint(MapVertexType vertexType)
    {
        List<MapVertex> vertexList =
            vertexType == MapVertexType.Forest ? MapChunkData.forestLsit.Count<MapManager.Instance.MapConfig.GenerateAiMinVertexCountOnChunk ?MapChunkData.MarshLsit:MapChunkData.forestLsit : 
                MapChunkData.MarshLsit.Count<MapManager.Instance.MapConfig.GenerateAiMinVertexCountOnChunk ? MapChunkData.forestLsit: MapChunkData.MarshLsit;
        
        int index = Random.Range(0, vertexList.Count);
        if (NavMesh.SamplePosition(vertexList[index].Position,out NavMeshHit hitInfo,1,NavMesh.AllAreas))
        {
            return hitInfo.position;
        }

        return GetAIObjectRandomPoint(vertexType);
    }

    /// <summary>
    /// 删除一个AI
    /// </summary>
    public void RemoveAIObjectOnTransfer(ulong mapObjectID)
    {
        MapChunkData.AIObjectDic.Dictionary.Remove(mapObjectID);
        AIObjectDic.Remove(mapObjectID);
    }

    public void RemoveAIObject(ulong aiObjectID)
    { 
       
        MapChunkData.AIObjectDic.Dictionary.Remove(aiObjectID,out MapObjectData aiData);
        aiData.Position = Vector3.zero;
        aiData.JKObjectPushPool();
        if (AIObjectDic.Remove(aiObjectID,out AIBase aiObject))
        {
            aiObject.Destroy();
        }
    }
    /// <summary>
    /// 添加一个AI
    /// </summary>
    public void AddAIObjectOnTransfer(MapObjectData aiObjectData, AIBase aiObject)
    {
        MapChunkData.AIObjectDic.Dictionary.Add(aiObjectData.ID,aiObjectData);
        AIObjectDic.Add(aiObjectData.ID,aiObject);
        aiObject.transform.SetParent(transform);
        aiObject.InitOnTransfer(this);
        
    }
 
    #endregion

 

    // 静态列表，用于批量移除对象
    private static List<ulong> doDestoryMapObjectList = new List<ulong>(20);

    /// <summary>
    /// 在清晨刷新时更新区块的对象状态
    /// </summary>
    private void OnMorn()
    {
        // 检查需要销毁的对象
        foreach (var item in wantDestoryMapObjectDic.Values)
        {
            item.DestoryDays -= 1;
            if (item.DestoryDays == 0)
            {
                doDestoryMapObjectList.Add(item.ID);
            }
        }

        // 执行销毁操作
        for (int i = 0; i < doDestoryMapObjectList.Count; i++)
        {
            RemoveMapObject(doDestoryMapObjectList[i]);
        }

        doDestoryMapObjectList.Clear();

        // 刷新区块并添加新的地图对象
        List<MapObjectData> mapObjectDatas = MapManager.Instance.SpawnMapObjectDataOnMapChunkRefresh(ChunkIndex);
        for (int i = 0; i < mapObjectDatas.Count; i++)
        {
            AddMapObject(mapObjectDatas[i], false);
        }
        //刷新ai
        if (TimeManager.Instance.currentDay%3==0)
        {
            mapObjectDatas = MapManager.Instance.SpawnAIObjectDataOnMapChunkRefresh(MapChunkData);
            for (int i = 0; i < mapObjectDatas.Count; i++)
            {
                AddAIObject(mapObjectDatas[i]);
            }
        }
      
    }

    /// <summary>
    /// 在销毁区块时保存区块数据
    /// </summary>
    private void OnSaveGame()
    {
        ArchiveManager.Instance.SaveMapChunkData(ChunkIndex, MapChunkData);
    }

    public void OnCloseGameScene()
    {
        SetActive(false);
    }
}