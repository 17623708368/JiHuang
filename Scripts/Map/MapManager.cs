using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using UnityEngine.AI;

public class MapManager : SingletonMono<MapManager>
{
#region 地图时间逻辑

// 网格碰撞器，用于生成地面的物理碰撞效果
[SerializeField] MeshCollider meshCollider;

// 记录上一次观察者的位置，用于判断是否需要更新地图块
private Vector3 lastViewerPos = Vector3.one * -1;

// 存储所有的地图块（索引 -> 控制器）
private Dictionary<Vector2Int, MapChunkController> mapChunkDic;

// 地图生成器，用于生成地图数据和地图块
private MapGenerator mapGenerator;

// 观察者（通常是玩家角色）
private Transform viewer;

// 地图块刷新时间间隔
private float updateChunkTime = 1f;

// 标志地图块是否可以刷新
private bool canUpdateChunk = true;

// 地图实际大小（世界坐标系中的尺寸）
private float mapSizeOnWorld;

// 单个地图块的实际大小（单位为米）
private float chunkSizeOnWorld;

// 存储上次可见的地图块控制器列表
private List<MapChunkController> lastVisibleChunkList = new List<MapChunkController>();

#endregion

#region 配置
// 地图配置，存储地图生成相关参数
private MapConfig mapConfig;
public MapConfig MapConfig=>mapConfig;
// 不同地形类型的可生成物体配置字典
private Dictionary<MapVertexType, List<int>> spawnMapObjectConfigDic;
private Dictionary<MapVertexType, List<int>> spawnAIConfigDic;

#endregion

#region 存档

// 地图初始化数据（来自存档）
private MapInitData mapInitData;

// 地图运行时数据（来自存档）
private MapData mapData;

#endregion

#region 导航

[SerializeField] private NavMeshSurface navMeshSurface;

public void BakeNavMesh()
{
    navMeshSurface.BuildNavMesh();
}


#endregion
/// <summary>
/// 初始化地图管理器
/// </summary>
public void Init()
{
    StartCoroutine(DoInit());
}

/// <summary>
/// 异步初始化地图数据和地图块
/// </summary>
private IEnumerator DoInit()
{
    // 从存档中加载地图初始化数据和地图运行时数据
    mapInitData = ArchiveManager.Instance.MapInitData;
    mapData = ArchiveManager.Instance.MapData;

    // 加载地图配置
    mapConfig = ConfigManager.Instance.GetConfig<MapConfig>(ConfigName.map);

    // 初始化生成物体的配置字典
    Dictionary<int, ConfigBase> tempDic = ConfigManager.Instance.GetConfigs(ConfigName.mapOjbect);
    spawnMapObjectConfigDic = new Dictionary<MapVertexType, List<int>>();
    spawnMapObjectConfigDic.Add(MapVertexType.Forest, new List<int>());
    spawnMapObjectConfigDic.Add(MapVertexType.Marsh, new List<int>());
    foreach (var item in tempDic)
    {
        MapVertexType mapVertexType = (item.Value as MapObjectConfig).MapVertexType;
        if (mapVertexType == MapVertexType.None) continue;
        spawnMapObjectConfigDic[mapVertexType].Add(item.Key);
    } 
    //得到AI配置
       tempDic = ConfigManager.Instance.GetConfigs(ConfigName.ai);
       //初始化AI字典
       spawnAIConfigDic = new Dictionary<MapVertexType, List<int>>();
       spawnAIConfigDic.Add(MapVertexType.Forest, new List<int>());
       spawnAIConfigDic.Add(MapVertexType.Marsh, new List<int>());
    foreach (var item in tempDic)
    {
        MapVertexType mapVertexType = (item.Value as AIConfig).MapVertexType;
        if (mapVertexType == MapVertexType.None) continue;
        spawnAIConfigDic[mapVertexType].Add(item.Key);
    }

    // 初始化地图生成器
    mapGenerator = new MapGenerator(mapConfig, mapInitData, mapData, spawnMapObjectConfigDic,spawnAIConfigDic);
    mapGenerator.GenerateMapData();

    // 初始化地图块字典
    mapChunkDic = new Dictionary<Vector2Int, MapChunkController>();

    // 计算地图和地图块的实际尺寸
    chunkSizeOnWorld = mapConfig.mapChunkSize * mapConfig.cellSize;
    mapSizeOnWorld = chunkSizeOnWorld * mapInitData.mapSize;

    // 生成地面碰撞网格
    meshCollider.sharedMesh = GenerateGroundMesh(mapSizeOnWorld, mapSizeOnWorld);
   BakeNavMesh();
    // 恢复存档中的地图块
    int mapChunkCount = mapData.MapChunkIndexList.Count;
    if (mapChunkCount > 0)
    {
        // 根据存档加载地图块并设置为不可见
        for (int i = 0; i < mapData.MapChunkIndexList.Count; i++)
        {
            Serialization_Vector2 chunkIndex = mapData.MapChunkIndexList[i];
            MapChunkData chunkData = ArchiveManager.Instance.GetMapChunkData(chunkIndex);
            GenerateMapChunk(chunkIndex.ConverToSVector2Init(), chunkData).gameObject.SetActive(false);
        }

        // 刷新可见地图块
        DoUpdateVisibleChunk();

        // 显示加载进度
        for (int i = 1; i <= mapChunkCount; i++)
        {
            yield return new WaitForSeconds(0.1f);
            GameSceneManager.Instance.UpdateMapProgress(i, mapChunkCount);
        }
    }
    else
    {
        // 新存档的情况，初始化地图
        DoUpdateVisibleChunk();
        for (int i = 1; i <= 10; i++)
        {
            yield return new WaitForSeconds(0.1f);
            GameSceneManager.Instance.UpdateMapProgress(i, 10);
        }
    }

    ShowMapUI();
    CloseMapUI();
    EventManager.AddEventListener(EventName.SaveGame,OnSaveGame);
    
}

/// <summary>
/// 更新观察者的位置（通常是玩家位置）
/// </summary>
/// <param name="player">玩家的 Transform</param>
public void UpdateViewer(Transform player)
{
    this.viewer = player;
}

/// <summary>
/// 每帧更新地图管理器状态
/// </summary>
void Update()
{
    // 如果场景未初始化，直接返回
    if (GameSceneManager.Instance.IsInitialized == false) return;

    // 更新可见的地图块
    UpdateVisibleChunk();

    // 显示或关闭地图UI
    if (Input.GetKeyDown(KeyCode.M))
    {
        if (isShowMaping)
        {
            CloseMapUI();
        }
        else
        {
            ShowMapUI();
        }
        isShowMaping = !isShowMaping;
    }

    // 如果地图UI正在显示，则更新UI
    if (isShowMaping)
    {
        UpdateMapUI();
    }
}


 /// <summary>
/// 生成地面的 Mesh
/// </summary>
/// <param name="height">地面的高度</param>
/// <param name="width">地面的宽度</param>
/// <returns>生成的地面 Mesh</returns>
private Mesh GenerateGroundMesh(float height, float width)
{
    Mesh mesh = new Mesh();

    // 设置 Mesh 的顶点位置
    mesh.vertices = new Vector3[]
    {
        new Vector3(0, 0, 0),
        new Vector3(0, 0, height),
        new Vector3(width, 0, height),
        new Vector3(width, 0, 0),
    };

    // 设置 Mesh 的三角形索引，定义两个三角形组成的平面
    mesh.triangles = new int[]
    {
        0, 1, 2,
        0, 2, 3
    };

    // 设置 UV 坐标，用于贴图
    mesh.uv = new Vector2[]
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 1),
        new Vector2(1, 0),
    };

    return mesh;
}

#region 地图块管理

/// <summary>
/// 更新可见的地图块
/// 根据观察者的位置刷新哪些地图块是可见的
/// </summary>
private void UpdateVisibleChunk()
{
    // 如果观察者没有移动，则无需刷新
    if (viewer.position == lastViewerPos) return;

    // 更新上一次观察者位置
    lastViewerPos = viewer.position;

    // 如果地图UI正在显示，更新地图UI的中心点
    if (isShowMaping)
    {
        mapUI.UpdatePivot(viewer.position);
    }

    // 如果地图块当前无法更新，则直接返回
    if (!canUpdateChunk) return;

    // 执行实际的地图块刷新逻辑
    DoUpdateVisibleChunk();
}

/// <summary>
/// 实际执行更新可见地图块的逻辑
/// </summary>
private void DoUpdateVisibleChunk()
{
    // 获取当前观察者所在的地图块索引
    Vector2Int currChunkIndex = GetMapChunkIndexByWorldPosition(viewer.position);

    // 隐藏超出可见范围的地图块
    for (int i = lastVisibleChunkList.Count - 1; i >= 0; i--)
    {
        Vector2Int chunkIndex = lastVisibleChunkList[i].ChunkIndex;
        if (Mathf.Abs(chunkIndex.x - currChunkIndex.x) > mapConfig.viewDinstance
            || Mathf.Abs(chunkIndex.y - currChunkIndex.y) > mapConfig.viewDinstance)
        {
            // 如果地图块超出可见范围，则将其设置为非激活状态
            lastVisibleChunkList[i].SetActive(false);
            lastVisibleChunkList.RemoveAt(i);
        }
    }

    // 遍历视野范围内的地图块，显示它们或生成新的地图块
    int startX = currChunkIndex.x - mapConfig.viewDinstance;
    int startY = currChunkIndex.y - mapConfig.viewDinstance;

    for (int x = 0; x < 2 * mapConfig.viewDinstance + 1; x++)
    {
        for (int y = 0; y < 2 * mapConfig.viewDinstance + 1; y++)
        {
            Vector2Int chunkIndex = new Vector2Int(startX + x, startY + y);

            // 如果地图块已经存在于字典中
            if (mapChunkDic.TryGetValue(chunkIndex, out MapChunkController chunk))
            {
                // 如果该地图块未在可见列表中，且已初始化，则将其激活
                if (!lastVisibleChunkList.Contains(chunk) && chunk.IsInitialized)
                {
                    lastVisibleChunkList.Add(chunk);
                    chunk.SetActive(true);
                }
            }
            // 如果地图块不存在，则生成新的地图块
            else
            {
                chunk = GenerateMapChunk(chunkIndex, null);
            }
        }
    }

    // 设置地图块更新标志为 false，防止频繁刷新
    canUpdateChunk = false;
    Invoke(nameof(RestCanUpdateChunkFlag), updateChunkTime);
}

/// <summary>
/// 根据世界坐标获取地图块的索引
/// </summary>
/// <param name="worldPostion">世界坐标</param>
/// <returns>地图块的索引</returns>
private Vector2Int GetMapChunkIndexByWorldPosition(Vector3 worldPostion)
{
    int x = Mathf.Clamp(Mathf.FloorToInt(worldPostion.x / chunkSizeOnWorld), 1, mapInitData.mapSize);
    int y = Mathf.Clamp(Mathf.FloorToInt(worldPostion.z / chunkSizeOnWorld), 1, mapInitData.mapSize);
    return new Vector2Int(x, y);
}
/// <summary>
/// 根据世界坐标获取地图块的索引
/// </summary>
/// <param name="worldPostion">世界坐标</param>
/// <returns>地图块的索引</returns>
public MapChunkController GetMapChunkByWorldPosition(Vector3 worldPostion)
{
    return mapChunkDic[GetMapChunkIndexByWorldPosition(worldPostion)];
}
/// <summary>
/// 生成地图块
/// </summary>
/// <param name="index">地图块的索引</param>
/// <param name="mapChunkData">地图块的数据（可选）</param>
/// <returns>生成的地图块控制器</returns>
private MapChunkController GenerateMapChunk(Vector2Int index, MapChunkData mapChunkData = null)
{
    // 检查索引是否合法
    if (index.x > mapInitData.mapSize - 1 || index.y > mapInitData.mapSize - 1) return null;
    if (index.x < 0 || index.y < 0) return null;

    // 调用地图生成器生成地图块
    MapChunkController chunk = mapGenerator.GenerateMapChunk(index, transform, mapChunkData, () => mapUIUpdateChunkIndexList.Add(index));

    // 将生成的地图块添加到字典中
    mapChunkDic.Add(index, chunk);
    return chunk;
}

/// <summary>
/// 恢复地图块更新的标志
/// </summary>
private void RestCanUpdateChunkFlag()
{
    canUpdateChunk = true;
}

#endregion


   /// <summary>
/// 为地图块刷新时生成地图对象数据列表
/// 如果生成列表为 null，则不进行任何操作。
/// 若返回非空列表，会根据生成规则为指定地图块生成新的地图对象数据。
/// </summary>
/// <param name="chunkIndex">地图块的索引</param>
/// <returns>生成的地图对象数据列表</returns>
public List<MapObjectData> SpawnMapObjectDataOnMapChunkRefresh(Vector2Int chunkIndex)
{
    return mapGenerator.GenerateMapObjectDataListOnMapChunkRefresh(chunkIndex);
}
public List<MapObjectData> SpawnAIObjectDataOnMapChunkRefresh(MapChunkData mapChunkData)
{
    return mapGenerator.GenerateAIObjectDataList(mapChunkData);
}

#region 地图UI管理
// 是否初始化了地图UI
private bool mapUIInitialized = false;

// 是否正在显示地图
private bool isShowMaping = false;

// 需要更新的地图块索引列表
private List<Vector2Int> mapUIUpdateChunkIndexList = new List<Vector2Int>();

// 地图UI窗口实例
private UI_MapWindow mapUI;

/// <summary>
/// 显示地图UI
/// </summary>
private void ShowMapUI()
{
    // 显示地图窗口
    mapUI = UIManager.Instance.Show<UI_MapWindow>();

    // 初始化地图UI，仅初始化一次
    if (!mapUIInitialized)
    {
        mapUI.InitMap(mapInitData.mapSize, mapConfig.mapChunkSize, mapSizeOnWorld, mapConfig.forestTexutre);
        mapUIInitialized = true;
    }

    // 更新地图UI显示
    UpdateMapUI();
}

/// <summary>
/// 更新地图UI，加载需要更新的地图块
/// </summary>
private void UpdateMapUI()
{
    for (int i = 0; i < mapUIUpdateChunkIndexList.Count; i++)
    {
        Vector2Int chunkIndex = mapUIUpdateChunkIndexList[i];
        Texture2D texture = null;

        // 获取地图块控制器
        MapChunkController mapchunk = mapChunkDic[chunkIndex];

        // 如果该块不是全森林块，获取其纹理
        if (mapchunk.IsAllForest == false)
        {
            texture = (Texture2D)mapchunk.GetComponent<MeshRenderer>().material.mainTexture;
        }

        // 更新地图块信息到地图UI
        mapUI.AddMapChunk(chunkIndex, mapchunk.MapChunkData.MapObjectDic, texture);
    }

    // 清空更新列表
    mapUIUpdateChunkIndexList.Clear();

    // 根据观察者位置调整地图UI的 Content 位置
    mapUI.UpdatePivot(viewer.position);
}

/// <summary>
/// 关闭地图UI
/// </summary>
private void CloseMapUI()
{
    UIManager.Instance.Close<UI_MapWindow>();
}

#endregion

#region 地图对象管理

/// <summary>
/// 移除地图对象
/// </summary>
/// <param name="id">地图对象的唯一标识</param>
public void RemoveMapObject(ulong id)
{
    // 如果地图UI已存在，从UI中移除对应的图标
    if (mapUI != null)
    {
        mapUI.RemoveMapObjectIcon(id);
    }
}

/// <summary>
/// 在指定位置生成地图对象
/// </summary>
/// <param name="mapObjectConfigID">地图对象的配置ID</param>
/// <param name="pos">生成的位置</param>
public void SpawnMapObject(int mapObjectConfigID, Vector3 pos,bool isFormBuilding,ulong discardID=0)
{
    // 根据世界坐标获取地图块索引
    Vector2Int chunkIndex = GetMapChunkIndexByWorldPosition(pos);

    // 在指定的地图块中生成对象
    SpawnMapObject(mapChunkDic[chunkIndex], mapObjectConfigID, pos,isFormBuilding,discardID);
}

/// <summary>
/// 在指定的地图块中生成地图对象
/// </summary>
/// <param name="mapChunkController">地图块控制器</param>
/// <param name="mapObjectConfigID">地图对象的配置ID</param>
/// <param name="pos">生成的位置</param>
public void SpawnMapObject(MapChunkController mapChunkController, int mapObjectConfigID, Vector3 pos,bool isFormBuilding,ulong discardID=0)
{
    // 使用生成器生成地图对象数据
    MapObjectData mapObjectData = mapGenerator.GenerateMapObjectData(mapObjectConfigID, pos);
    if (mapObjectData == null) return; // 如果生成失败，直接返回
    mapObjectData.discardGameObjectID = discardID;
    // 将对象添加到地图块中
    mapChunkController.AddMapObject(mapObjectData,isFormBuilding);

    // 如果地图UI存在，为其添加对应的图标
    if (mapUI != null)
    {
        mapUI.AddMapObjectIcon(mapObjectData);
    }
}

#endregion
    private void OnSaveGame()
    {
        ArchiveManager.Instance.SaveMapData();
    }

    public void OnCloseGameScene()
    {
         mapUI.RestWindow();
        foreach (var item in mapChunkDic.Values)
        {
            item.OnCloseGameScene();
        }
    }
}
