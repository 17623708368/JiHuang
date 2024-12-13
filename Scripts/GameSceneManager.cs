using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using UnityEngine.Serialization;

/// <summary>
/// ��Ϸ����������
/// </summary>
public class GameSceneManager : LogicManagerBase<GameSceneManager>
{
    private bool isGameOver=false;
    public bool IsGameOver => isGameOver;
    #region �����߼�
    public bool IsTest = true;
    public bool isCreateNewArchive;
    #endregion
    protected override void CancelEventListener() { }
    protected override void RegisterEventListener() { }
    
    private void Start()
    {
        #region �����߼�
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
        // ������е������ôһ�����еĴ浵��׼������
        IsInitialized = false;
        // ���ؽ�����
        loadingWindow = UIManager.Instance.Show<UI_GameLoadingWindow>();
        loadingWindow.UpdateProgress(0);

        // ȷ����ͼ��ʼ����������
        MapConfig mapConfig = ConfigManager.Instance.GetConfig<MapConfig>(ConfigName.map);
        float mapSizeOnWolrd = ArchiveManager.Instance.MapInitData.mapSize * mapConfig.mapChunkSize * mapConfig.cellSize;
        // ��ʼ����ɫ�����
        Player_Controller.Instance.Init(mapSizeOnWolrd);
        Camera_Controller.Instance.Init(mapSizeOnWolrd);
        // ��ʾ����Ϣ���:
        // ������TimeManager����Ϣ����
        // ������Player_Controller����Ϣ����
        InventoryManager.Instance.Init();
        // ��ʼ��ʱ��
        TimeManager.Instance.Init();

        // ��ʼ����ͼ
        MapManager.Instance.UpdateViewer(Player_Controller.Instance.transform);
        MapManager.Instance.Init();
        //��ʼ����ͼ
        UIManager.Instance.Show<UI_MainInfoWindow>();

        // ��ʼ������
        UIManager.Instance.Show<InventoryWindowBase>();


        // ��ʼ�����������
        InputManager.Instance.Init();
        
        //����������
        BuildManager.Instance.Init();
        
        //�Ƽ�����ʼ��
        ScienceMachineManager.Instance.Init();
    }

    #region ���ؽ���
    private UI_GameLoadingWindow loadingWindow;
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// ���½���
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
