using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JKFrame;

/// <summary>
/// UI地图窗口类
/// 显示并管理游戏内的地图界面，包括地图块和地图对象的图标
/// </summary>
[UIElement(true, "UI/UI_MapWindow", 4)]
public class UI_MapWindow : UI_WindowBase
{
    [SerializeField] private RectTransform content; // 包含地图块和图标的容器
    private float contentSize; // 地图内容的总大小

    [SerializeField] private GameObject mapItemPrefab; // 地图块的UI预制体
    [SerializeField] private GameObject mapIconPrefab; // 地图对象图标的UI预制体
    [SerializeField] private RectTransform playerIcon; // 显示玩家位置的图标

    private Dictionary<ulong, Image> mapObjectIconDic = new Dictionary<ulong, Image>(); // 地图对象图标字典
    private Dictionary<Vector2Int, Image> mapChunkIconDic = new Dictionary<Vector2Int, Image>(); // 地图对象图标字典
    private float mapChunkImageSize; // 单个地图块在UI中的尺寸
    private int mapChunkSize; // 一个地图块包含的单位格子数
    private float mapSizeOnWorld; // 3D地图在实际世界中的总尺寸
    private Sprite forestSprite; // 森林块的贴图精灵

    private float minScale; // 最小缩放比例
    private float maxScale = 10f; // 最大缩放比例

    /// <summary>
    /// 初始化地图窗口
    /// </summary>
    public override void Init()
    {
        // 监听滚动视图的滚动事件，用于更新玩家图标的位置
        transform.Find("Scroll View").GetComponent<ScrollRect>().onValueChanged.AddListener(UpdatePlayerIconPos);
    }

    private void Update()
    {
        // 检测鼠标滚轮，调整地图缩放比例
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newScale = Mathf.Clamp(content.localScale.x + scroll, minScale, maxScale);
            content.localScale = new Vector3(newScale, newScale, 1);
        }
    }

    /// <summary>
    /// 初始化地图数据
    /// </summary>
    /// <param name="mapSize">地图的大小（单位为地图块数量）</param>
    /// <param name="mapChunkSize">单个地图块的单位格子数</param>
    /// <param name="mapSizeOnWord">地图在世界中的总尺寸</param>
    /// <param name="forestTexture">森林区域的默认贴图</param>
    public void InitMap(float mapSize, int mapChunkSize, float mapSizeOnWord, Texture2D forestTexture)
    {
        this.mapSizeOnWorld = mapSizeOnWord;
        forestSprite = CreateMapSprite(forestTexture);
        this.mapChunkSize = mapChunkSize;

        // 设置内容的总尺寸
        contentSize = mapSizeOnWord * 10;
        content.sizeDelta = new Vector2(contentSize, contentSize);
        content.localScale = new Vector3(maxScale, maxScale, 1);

        // 计算单个地图块的尺寸
        mapChunkImageSize = contentSize / mapSize;
        minScale = 1050f / contentSize;
    }

    /// <summary>
    /// 根据观察者的位置更新地图中心点
    /// </summary>
    /// <param name="viewerPosition">观察者的世界位置</param>
    public void UpdatePivot(Vector3 viewerPosition)
    {
        float x = viewerPosition.x / mapSizeOnWorld;
        float y = viewerPosition.z / mapSizeOnWorld;
        content.pivot = new Vector2(x, y);
    }

    /// <summary>
    /// 更新玩家图标的位置
    /// </summary>
    /// <param name="value">滚动视图的变化值</param>
    public void UpdatePlayerIconPos(Vector2 value)
    {
        // 根据Content的位置更新玩家图标的位置
        playerIcon.anchoredPosition3D = content.anchoredPosition3D;
    }

    /// <summary>
    /// 添加一个地图块到地图界面
    /// </summary>
    /// <param name="chunkIndex">地图块的索引</param>
    /// <param name="mapObjectDic">地图对象的数据字典</param>
    /// <param name="texture">地图块的贴图（可选）</param>
    public void AddMapChunk(Vector2Int chunkIndex, Serialization_Dic<ulong, MapObjectData> mapObjectDic, Texture2D texture = null)
    {
        // 实例化地图块的UI
        RectTransform mapChunkRect = Instantiate(mapItemPrefab, content).GetComponent<RectTransform>();
        mapChunkRect.anchoredPosition = new Vector2(chunkIndex.x * mapChunkImageSize, chunkIndex.y * mapChunkImageSize);
        mapChunkRect.sizeDelta = new Vector2(mapChunkImageSize, mapChunkImageSize);

        // 设置地图块的贴图
        Image mapChunkImage = mapChunkRect.GetComponent<Image>();
        mapChunkIconDic.Add(chunkIndex,mapChunkImage);
        if (texture == null)
        {
            mapChunkImage.type = Image.Type.Tiled;
            float ratio = forestSprite.texture.width / mapChunkImageSize;
            mapChunkImage.pixelsPerUnitMultiplier = mapChunkSize * ratio;
            mapChunkImage.sprite = forestSprite;
        }
        else
        {
            mapChunkImage.sprite = CreateMapSprite(texture);
        }

        // 添加地图块中的对象图标
        foreach (var item in mapObjectDic.Dictionary.Values)
        {
            AddMapObjectIcon(item);
        }
    }

    /// <summary>
    /// 生成地图块的精灵
    /// </summary>
    private Sprite CreateMapSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// 移除地图对象的图标
    /// </summary>
    /// <param name="mapObjectID">地图对象的唯一ID</param>
    public void RemoveMapObjectIcon(ulong mapObjectID)
    {
        if (mapObjectIconDic.TryGetValue(mapObjectID, out Image iconImg))
        {
            iconImg.JKGameObjectPushPool();
            mapObjectIconDic.Remove(mapObjectID);
        }
    }

    /// <summary>
    /// 添加一个地图对象的图标到地图界面
    /// </summary>
    /// <param name="mapObjectData">地图对象的数据</param>
    public void AddMapObjectIcon(MapObjectData mapObjectData)
    {
        MapObjectConfig config = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.mapOjbect, mapObjectData.ConfigID);
        if (config.MapIconSprite == null) return;

        GameObject go = PoolManager.Instance.GetGameObject(mapIconPrefab, content);
        Image iconImg = go.GetComponent<Image>();
        iconImg.sprite = config.MapIconSprite;
        iconImg.transform.localScale = Vector3.one * config.IconSize;

        float x = mapObjectData.Position.x * 10;
        float y = mapObjectData.Position.z * 10;
        go.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);

        mapObjectIconDic.Add(mapObjectData.ID, iconImg);
    }

    public void RestWindow()
    {
        foreach (var item in mapObjectIconDic.Values)
        {
            item.JKGameObjectPushPool();
        }

        foreach (var item in mapChunkIconDic.Values)
        {
            Destroy(item);
        }
        mapObjectIconDic.Clear();
        mapChunkIconDic.Clear();
    }
}
