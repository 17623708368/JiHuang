using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using UnityEngine.Serialization;

/// <summary>
/// 游戏场景管理器
/// </summary>
public class GameSceneManager : LogicManagerBase<GameSceneManager>
{
    private bool isGameOver=false;
    public bool IsGameOver => isGameOver;
    #region 测试逻辑
    public bool IsTest = true;
    public bool isCreateNewArchive;
    #endregion
    protected override void CancelEventListener() { }
    protected override void RegisterEventListener() { }
    
    private void Start()
    {
        #region 测试逻辑
        if (IsTest)
        {
            if (isCreateNewArchive)
            {
                ArchiveManager.Instance.CreateNewArchive(10, 1, 1, 0.75f);
            }
            else
            {
                ArchiveManager.Instance.LoadCurrentArchive();
            }
        }
        #endregion
        UIManager.Instance.CloseAll();
        StartGame();
    }

    private void StartGame()
    {
        // 如果运行到这里，那么一定所有的存档都准备好了
        IsInitialized = false;
        // 加载进度条
        loadingWindow = UIManager.Instance.Show<UI_GameLoadingWindow>();
        loadingWindow.UpdateProgress(0);

        // 确定地图初始化配置数据
        MapConfig mapConfig = ConfigManager.Instance.GetConfig<MapConfig>(ConfigName.map);
        float mapSizeOnWolrd = ArchiveManager.Instance.MapInitData.mapSize * mapConfig.mapChunkSize * mapConfig.cellSize;
        // 初始化角色、相机
        Player_Controller.Instance.Init(mapSizeOnWolrd);
        Camera_Controller.Instance.Init(mapSizeOnWolrd);
        // 显示主信息面板:
        // 依赖于TimeManager的信息发送
        // 依赖于Player_Controller的信息发送
        InventoryManager.Instance.Init();
        // 初始化时间
        TimeManager.Instance.Init();

        // 初始化地图
        MapManager.Instance.UpdateViewer(Player_Controller.Instance.transform);
        MapManager.Instance.Init();
        //初始化地图
        UIManager.Instance.Show<UI_MainInfoWindow>();

        // 初始化背包
        UIManager.Instance.Show<InventoryWindowBase>();


        // 初始化输入管理器
        InputManager.Instance.Init();
        
        //开启建造栏
        BuildManager.Instance.Init();
        
        //科技树初始化
        ScienceMachineManager.Instance.Init();
    }

    #region 加载进度
    private UI_GameLoadingWindow loadingWindow;
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// 更新进度
    /// </summary>
    public void UpdateMapProgress(int current, int max)
    {
        float currentProgress = (100/max)*current;
        if (current == max)
        {
            loadingWindow.UpdateProgress(100);
            IsInitialized = true;
            loadingWindow.Close();
            loadingWindow = null;

        }
        else
        {
            loadingWindow.UpdateProgress(currentProgress);
        }
    }
    #endregion

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.Show<UI_PauseWindow>();
            Time.timeScale = 0;
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        ArchiveManager.Instance.CleanArchive();
        EnterMenuScene();
    }

    public void CloseAndSave()
    {
            EventManager.EventTrigger(EventName.SaveGame);
            EnterMenuScene();
    }

    public void EnterMenuScene()
    {
        Time.timeScale = 1;
        MapManager.Instance.OnCloseGameScene();
        MonoManager.Instance.StopAllCoroutines();
        UIManager.Instance.CloseAll();
        EventManager.Clear();
        GameManager.Instance.EnterMenu();
    }

    public void UnPauseGame()
    {
        Time.timeScale = 1;
        UIManager.Instance.Close<UI_PauseWindow>();

    }
    private void OnApplicationQuit()
    {
        if (!isGameOver)
        {
            EventManager.EventTrigger(EventName.SaveGame);
        }
    }
}
