using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using JKFrame;
using System;
using Random = UnityEngine.Random;
/// <summary>
/// 地图生成工具
/// </summary>
public class MapGenerator
{
    #region 运行时的变量
    private MapGrid mapGrid;        // 地图逻辑网格、顶点数据
    private Material marshMaterial;
    private Mesh chunkMesh;
    private int forestSpawanMapObjectWeightTotal;
    private int marshSpawanMapOjbectWeightTotal;   
    private int forestSpawanAIWeightTotal;
    private int marshSpawanAIWeightTotal;
    #endregion

    #region 配置
    private Dictionary<MapVertexType, List<int>> spawnMapObjectConfigDic;
    private Dictionary<MapVertexType, List<int>> spawnAIConfigDic;
    private MapConfig mapConfig;
    #endregion

    #region 存档
    private MapInitData mapInitData;
    private MapData mapData;
    #endregion

    public MapGenerator(MapConfig mapConfig,MapInitData mapInitData,MapData mapData,Dictionary<MapVertexType, List<int>> spawnMapObjectConfigDic,Dictionary<MapVertexType, List<int>> spawnAIConfigDic)
    {
        this.mapConfig = mapConfig;
        this.mapInitData = mapInitData;
        this.mapData = mapData;
        this.spawnMapObjectConfigDic = spawnMapObjectConfigDic;
        this.spawnAIConfigDic = spawnAIConfigDic;
    }


    /// <summary>
    /// 生成地图数据，主要是所有地图块都通用的数据
    /// </summary>
    public void GenerateMapData()
    {
        // 生成噪声图
        // 应用地图种子
        Random.InitState(mapInitData.mapSeed);
        float[,] noiseMap = GenerateNoiseMap(mapInitData.mapSize * mapConfig.mapChunkSize, mapInitData.mapSize * mapConfig.mapChunkSize, mapConfig.noiseLacunarity);
        // 生成网格数据
        mapGrid = new MapGrid(mapInitData.mapSize * mapConfig.mapChunkSize, mapInitData.mapSize * mapConfig.mapChunkSize, mapConfig.cellSize);
        // 确定网格 格子的贴图索引
        mapGrid.CalculateMapVertexType(noiseMap, mapInitData.marshLimit);
        // 初始化默认材质的尺寸
        mapConfig.mapMaterial.mainTexture = mapConfig.forestTexutre;
        mapConfig.mapMaterial.SetTextureScale("_MainTex", new Vector2(mapConfig.cellSize * mapConfig.mapChunkSize, mapConfig.cellSize * mapConfig.mapChunkSize));
        // 实例化一个沼泽材质
        marshMaterial = new Material(mapConfig.mapMaterial);
        marshMaterial.SetTextureScale("_MainTex", Vector2.one);

        chunkMesh = GenerateMapMesh(mapConfig.mapChunkSize, mapConfig.mapChunkSize, mapConfig.cellSize);
        // 使用种子来进行随机生成
        Random.InitState(mapInitData.spawnSeed);

        List<int> temps = spawnMapObjectConfigDic[MapVertexType.Forest];
        for (int i = 0; i < temps.Count; i++) forestSpawanMapObjectWeightTotal += ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.mapOjbect, temps[i]).Probability;
        temps = spawnMapObjectConfigDic[MapVertexType.Marsh];
        for (int i = 0; i < temps.Count; i++) marshSpawanMapOjbectWeightTotal += ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.mapOjbect, temps[i]).Probability;
        temps = spawnAIConfigDic[MapVertexType.Forest];
        for (int i = 0; i < temps.Count; i++)
            forestSpawanAIWeightTotal += ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.ai, temps[i]).Probability;
        temps = spawnAIConfigDic[MapVertexType.Marsh];
        for (int i = 0; i < temps.Count; i++) 
            marshSpawanAIWeightTotal += ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.ai, temps[i]).Probability;
    }

    #region 地图块
    /// <summary>
    /// 生成地图块
    /// </summary>
    public MapChunkController GenerateMapChunk(Vector2Int chunkIndex, Transform parent, MapChunkData mapChunkData, Action callBackForMapTexture)
    {
        // 生成地图块物体
        GameObject mapChunkObj = new GameObject("Chunk_" + chunkIndex.ToString());
        MapChunkController mapChunk = mapChunkObj.AddComponent<MapChunkController>();
        // 生成Mesh
        mapChunkObj.AddComponent<MeshFilter>().mesh = chunkMesh;

        bool allForest;
        // 生成地图块的贴图
        Texture2D mapTexture;
        this.StartCoroutine
        (
            GenerateMapTexture(chunkIndex, (tex, isAllForest) => {
                allForest = isAllForest;
                 // 如果完全是森林，没必要在实例化一个材质球
                if (isAllForest)
                {
                    mapChunkObj.AddComponent<MeshRenderer>().sharedMaterial = mapConfig.mapMaterial;
                }
                else
                {
                    mapTexture = tex;
                    Material material = new Material(marshMaterial);
                    material.mainTexture = tex;
                    mapChunkObj.AddComponent<MeshRenderer>().material = material;

                }
                callBackForMapTexture?.Invoke();

                 // 确定坐标
                Vector3 position = new Vector3(chunkIndex.x * mapConfig.mapChunkSize * mapConfig.cellSize, 0, chunkIndex.y * mapConfig.mapChunkSize * mapConfig.cellSize);
                mapChunk.transform.position = position;
                mapChunkObj.transform.SetParent(parent);

                 // 如果没有指定地图快数据，说明是新建的，需要生成默认数据
                if (mapChunkData == null)
                {
                     // 生成场景物体数据
                    mapChunkData  = GenerateMaoChunkData(chunkIndex);
                     // 生成后进行持久化保存
                     ArchiveManager.Instance.AddAndSaveMapChunkData(chunkIndex, mapChunkData);
                }
                else
                {
                    //恢复存档信息
                    RecoverMapChunkData(chunkIndex,mapChunkData);
                }

                mapChunk.Init(chunkIndex, position + new Vector3((mapConfig.mapChunkSize * mapConfig.cellSize) / 2, 0, (mapConfig.mapChunkSize * mapConfig.cellSize) / 2), allForest, mapChunkData);
            }));
        

        return mapChunk;
    }

    /// <summary>
    /// 生成地形Mesh
    /// </summary>
    private Mesh GenerateMapMesh(int height, int wdith, float cellSize)
    {
        Mesh mesh = new Mesh();
        // 确定顶点在哪里
        mesh.vertices = new Vector3[]
        {
            new Vector3(0,0,0),
            new Vector3(0,0,height*cellSize),
            new Vector3(wdith*cellSize,0,height*cellSize),
            new Vector3(wdith*cellSize,0,0),
        };
        // 确定哪些点形成三角形
        mesh.triangles = new int[]
        {
            0,1,2,
            0,2,3
        };
        mesh.uv = new Vector2[]
        {
            new Vector3(0,0),
            new Vector3(0,1),
            new Vector3(1,1),
            new Vector3(1,0),
        };
        // 计算法线
        mesh.RecalculateNormals();
        return mesh;
    }

    /// <summary>
    /// 生成噪声图
    /// </summary>
    private float[,] GenerateNoiseMap(int width, int height, float lacunarity)
    {
        lacunarity += 0.1f;
        // 这里的噪声图是为了顶点服务的
        float[,] noiseMap = new float[width, height];
        float offsetX = Random.Range(-10000f, 10000f);
        float offsetY = Random.Range(-10000f, 10000f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                noiseMap[x, y] = Mathf.PerlinNoise(x * lacunarity + offsetX, y * lacunarity + offsetY);
            }
        }
        return noiseMap;
    }

    /// <summary>
    /// 分帧 生成地图贴图
    /// 如果这个地图块完全是森林，直接返回森林贴图
    /// </summary>
    private IEnumerator GenerateMapTexture(Vector2Int chunkIndex, System.Action<Texture2D, bool> callBack)
    {
        // 当前地块的偏移量 找到这个地图块具体的每一个格子
        int cellOffsetX = chunkIndex.x * mapConfig.mapChunkSize + 1;
        int cellOffsetY = chunkIndex.y * mapConfig.mapChunkSize + 1;

        // 是不是一张完整的森林地图块
        bool isAllForest = true;
        // 检查是否只有森林类型的格子
        for (int y = 0; y < mapConfig.mapChunkSize; y++)
        {
            if (isAllForest == false) break;
            for (int x = 0; x < mapConfig.mapChunkSize; x++)
            {
                MapCell cell = mapGrid.GetCell(x + cellOffsetX, y + cellOffsetY);
                if (cell != null && cell.TextureIndex != 0)
                {
                    isAllForest = false;
                    break;
                }
            }
        }

        Texture2D mapTexture = null;
        // 有沼泽的情况
        if (!isAllForest)
        {
            // 贴图都是矩形
            int textureCellSize = mapConfig.forestTexutre.width;
            // 整个地图块的宽高,正方形
            int textureSize = mapConfig.mapChunkSize * textureCellSize;
            mapTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);

            // 遍历每一个格子
            for (int y = 0; y < mapConfig.mapChunkSize; y++)
            {
                // 一帧只执行一列 只绘制一列的像素
                yield return null;
                // 像素偏移量
                int pixelOffsetY = y * textureCellSize;
                for (int x = 0; x < mapConfig.mapChunkSize; x++)
                {

                    int pixelOffsetX = x * textureCellSize;
                    int textureIndex = mapGrid.GetCell(x + cellOffsetX, y + cellOffsetY).TextureIndex - 1;
                    // 绘制每一个格子内的像素
                    // 访问每一个像素点
                    for (int y1 = 0; y1 < textureCellSize; y1++)
                    {
                        for (int x1 = 0; x1 < textureCellSize; x1++)
                        {

                            // 设置某个像素点的颜色
                            // 确定是森林还是沼泽
                            // 这个地方是森林 ||
                            // 这个地方是沼泽但是是透明的，这种情况需要绘制groundTexture同位置的像素颜色
                            if (textureIndex < 0)
                            {
                                Color color = mapConfig.forestTexutre.GetPixel(x1, y1);
                                mapTexture.SetPixel(x1 + pixelOffsetX, y1 + pixelOffsetY, color);
                            }
                            else
                            {
                                // 是沼泽贴图的颜色
                                Color color = mapConfig.marshTextures[textureIndex].GetPixel(x1, y1);
                                if (color.a < 1f)
                                {
                                    mapTexture.SetPixel(x1 + pixelOffsetX, y1 + pixelOffsetY, mapConfig.forestTexutre.GetPixel(x1, y1));
                                }
                                else
                                {
                                    mapTexture.SetPixel(x1 + pixelOffsetX, y1 + pixelOffsetY, color);
                                }
                            }

                        }
                    }
                }
            }
            mapTexture.filterMode = FilterMode.Point;
            mapTexture.wrapMode = TextureWrapMode.Clamp;
            mapTexture.Apply();
        }
        callBack?.Invoke(mapTexture, isAllForest);
    }


    /// <summary>
    /// 生成一个地图对象的数据
    /// </summary>
    public MapObjectData GenerateMapObjectData(int mapObjectConfigID, Vector3 pos)
    {
        MapObjectData mapObjectData = null;
        MapObjectConfig mapObjectConfig = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.mapOjbect, mapObjectConfigID);
        if (mapObjectConfig.IsEmpty == false)
        {
            mapObjectData = GenerateMapObjectData(mapObjectConfigID,pos, mapObjectConfig.DestoryDays);
        }
        return mapObjectData;
    }

    /// <summary>
    /// 通过权重获取一个地图对象的配置ID
    /// </summary>
    /// <returns></returns>
    private int GetMapObjectConfigIDForWeight(MapVertexType mapVertexType)
    {
        // 根据概率配置随机
        List<int> configIDs = spawnMapObjectConfigDic[mapVertexType];
        // 确定权重的总和
        int weightTotal = mapVertexType == MapVertexType.Forest ? forestSpawanMapObjectWeightTotal : marshSpawanMapOjbectWeightTotal;
        int randValue = Random.Range(1, weightTotal + 1); // 实际命中数字是从1~weightTotal
        float temp = 0;
        for (int i = 0; i < configIDs.Count; i++)
        {
            temp += ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.mapOjbect, configIDs[i]).Probability;
            if (randValue < temp)
            {
                // 命中
                return configIDs[i];
            }
        }
        return 0;
    }

    private int GetAIConfigIDForWeight(MapVertexType mapVertexType)
    {
        List<int> configIDs = spawnAIConfigDic[mapVertexType];
        int weightToal = mapVertexType is MapVertexType.Forest ? forestSpawanAIWeightTotal : marshSpawanAIWeightTotal;
        int randValue = Random.Range(1, weightToal + 1);
        int temp = 0;
        for (int i = 0; i < configIDs.Count; i++)
        {
            temp += ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.ai, configIDs[i]).Probability;
            if (randValue<temp)
            {
                return configIDs[i];
            }
        }

        return 0;
    }
    /// <summary>
    /// 生成地图对象数据，为了地图块初始化准备的
    /// </summary>
    private MapChunkData GenerateMaoChunkData(Vector2Int chunkIndex)
    {
        MapChunkData mapChunkData = new MapChunkData();
        mapChunkData.MapObjectDic = new Serialization_Dic<ulong, MapObjectData>();
        mapChunkData.AIObjectDic = new Serialization_Dic<ulong, MapObjectData>();
        mapChunkData.forestLsit = new List<MapVertex>();
        mapChunkData.MarshLsit = new List<MapVertex>();
        int offsetX = chunkIndex.x * mapConfig.mapChunkSize;
        int offsetY = chunkIndex.y * mapConfig.mapChunkSize;
        // 遍历地图顶点
        for (int x = 1; x < mapConfig.mapChunkSize; x++)
        {
            for (int y = 1; y < mapConfig.mapChunkSize; y++)
            {
                MapVertex mapVertex = mapGrid.GetVertex(x + offsetX, y + offsetY);
                if (mapVertex.VertexType is MapVertexType.Forest)
                {
                    mapChunkData.forestLsit.Add(mapVertex );
                }
                else if (mapVertex.VertexType is MapVertexType.Marsh)
                {
                    mapChunkData.MarshLsit.Add(mapVertex);
                }
                // 根据权重获取一个地图对象的配置ID
                int configID = GetMapObjectConfigIDForWeight(mapVertex.VertexType);
                // 确定到底生成什么地图物体
                MapObjectConfig objectConfig = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.mapOjbect, configID);
                if (objectConfig.IsEmpty == false)
                {
                    Vector3 position = mapVertex.Position + new Vector3(Random.Range(-mapConfig.cellSize / 2, mapConfig.cellSize / 2), 0, Random.Range(-mapConfig.cellSize / 2, mapConfig.cellSize / 2));
                    mapVertex.MapObjectID = mapData.CurrentID;
                    mapChunkData.MapObjectDic.Dictionary.Add(mapData.CurrentID, GenerateMapObjectData(configID, position, objectConfig.DestoryDays));
                }
            }
        }

        List<MapObjectData> aiDataList = GenerateAIObjectDataList(mapChunkData);
        for (int i = 0; i < aiDataList.Count; i++)
        {
            mapChunkData.AIObjectDic.Dictionary.Add(aiDataList[i].ID, aiDataList[i]);
        }
         
        return mapChunkData;
    }

    public void RecoverMapChunkData(Vector2Int chunkIndex,MapChunkData mapChunkData ) 
    {
        mapChunkData.forestLsit = new List<MapVertex>();
        mapChunkData.MarshLsit = new List<MapVertex>();
        int offsetX = chunkIndex.x * mapConfig.mapChunkSize;
        int offsetY = chunkIndex.y * mapConfig.mapChunkSize;
        // 遍历地图顶点
        for (int x = 1; x < mapConfig.mapChunkSize; x++)
        {
            for (int y = 1; y < mapConfig.mapChunkSize; y++)
            {
                MapVertex mapVertex = mapGrid.GetVertex(x + offsetX, y + offsetY);
                if (mapVertex.VertexType is MapVertexType.Forest)
                {
                    mapChunkData.forestLsit.Add(mapVertex);
                }
                else if (mapVertex.VertexType is MapVertexType.Marsh)
                {
                    mapChunkData.MarshLsit.Add(mapVertex);
                }
            }
        }
    }

    // 用来避免每次都返回一个新的list对象
    List<MapObjectData> mapOjbectDataList = new List<MapObjectData>();
    /// <summary>
    /// 为地图块刷新，生成地图对象列表
    /// 可能为null
    /// 你给一个地图块索引，返回一个今天这个地图块多出来的物体数据
    /// </summary>
    public List<MapObjectData> GenerateMapObjectDataListOnMapChunkRefresh(Vector2Int chunkIndex)
    {
        // 清理数据
        mapOjbectDataList.Clear();
        int offsetX = chunkIndex.x * mapConfig.mapChunkSize;
        int offsetY = chunkIndex.y * mapConfig.mapChunkSize;

        // 遍历地图顶点
        for (int x = 1; x < mapConfig.mapChunkSize; x++)
        {
            for (int y = 1; y < mapConfig.mapChunkSize; y++)
            {
                // 如果概率没命中，这一个顶点不刷新
                if (Random.Range(0, mapConfig.RefreshProbability) != 0) continue;
                MapVertex mapVertex = mapGrid.GetVertex(x + offsetX, y + offsetY);

                // 不为空则不能生成
                if (mapVertex.MapObjectID !=0) continue;

                // 根据权重获取一个地图对象的配置ID
                int configID = GetMapObjectConfigIDForWeight(mapVertex.VertexType);
                // 确定到底生成什么地图物体
                MapObjectConfig objectConfig = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.mapOjbect, configID);
                if (objectConfig.IsEmpty == false)
                {
                    Vector3 position = mapVertex.Position + new Vector3(Random.Range(-mapConfig.cellSize / 2, mapConfig.cellSize / 2), 0, Random.Range(-mapConfig.cellSize / 2, mapConfig.cellSize / 2));
                    mapOjbectDataList.Add(GenerateMapObjectData(configID, position, objectConfig.DestoryDays));
                    mapVertex.MapObjectID = mapData.CurrentID;
                }
            }
        }
        return mapOjbectDataList;
    }

    /// <summary>
    /// 为地图块刷新，生成地图对象列表
    /// </summary>h
    public List<MapObjectData> GenerateAIObjectDataList(MapChunkData mapChunkData)
    {    // 清理数据
        mapOjbectDataList.Clear();
        //生成AI数据相关
        //地图块顶点数量大于配置才开始生成
        int maxCount = mapConfig.maxAIOnChunk - mapChunkData.AIObjectDic.Dictionary.Count;
        if (mapChunkData.forestLsit.Count>mapConfig.GenerateAiMinVertexCountOnChunk)
        {
            for (int i = 0; i < maxCount; i++)
            {
                int configAiID = GetAIConfigIDForWeight(MapVertexType.Forest);
                AIConfig aiConfig = ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.ai, configAiID);
                if (!aiConfig.IsEmpty)
                {
                    mapOjbectDataList.Add(  GenerateMapObjectData(configAiID, Vector3.zero, -1));
                    maxCount--;
                }
            }
        }
        else if (mapChunkData.MarshLsit.Count>mapConfig.GenerateAiMinVertexCountOnChunk)
        {
            for (int i = 0; i < maxCount; i++)
            {
                int configAiID = GetAIConfigIDForWeight(MapVertexType.Marsh);
                AIConfig aiConfig = ConfigManager.Instance.GetConfig<AIConfig>(ConfigName.ai, configAiID);
                if (!aiConfig.IsEmpty)
                {
                    mapOjbectDataList.Add(  GenerateMapObjectData(configAiID, Vector3.zero, -1));
                }
            }
        }
        return mapOjbectDataList;

    }
    /// <summary>
    /// 生成一个地图对象数据
    /// </summary>
    private MapObjectData GenerateMapObjectData(int mapObjectConfigID,Vector3 position,int destoryDays )
    {
        MapObjectData mapObjectData = PoolManager.Instance.GetObject<MapObjectData>();
        mapObjectData.ConfigID = mapObjectConfigID;
        mapObjectData.ID = mapData.CurrentID;
        mapData.CurrentID += 1;
        mapObjectData.Position = position;
        mapObjectData.DestoryDays = destoryDays;
        return mapObjectData;
    }

    #endregion



}


