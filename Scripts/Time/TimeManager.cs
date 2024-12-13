using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using System;
using Sirenix.OdinInspector;

/// <summary>
/// 时间管理器
/// </summary>
public class TimeManager : LogicManagerBase<TimeManager>
{
    [SerializeField] private Light mainLight;                   // 太阳
    private TimeData timeData;
    private TimeConfig timeConfig;
    [SerializeField,Range(0,30)] public float timeScale = 1;
    private int nextIndex;
    public int currentDay { get=>timeData.DayNum; }
    protected override void RegisterEventListener() { }

    protected override void CancelEventListener() { }

    public void Init()
    {
        timeConfig = ConfigManager.Instance.GetConfig<TimeConfig>(ConfigName.time);
        timeData = ArchiveManager.Instance.TimeData;
        InitState();
        EventManager.AddEventListener(EventName.SaveGame,OnSaveGame);
    }

    private void Update()
    {
        if (GameSceneManager.Instance.IsInitialized == false) return;
        UpdateTime();
    }

    // 首次应用存档数时，需要根据数据做初始的设置
    private void InitState()
    {
        // 检查边界，超过就从0开始
        nextIndex = timeData.StateIndex + 1 >= timeConfig.TimeStateConfigs.Length ? 0 : timeData.StateIndex + 1; 
        // 设置初始的雾设置
        RenderSettings.fog = timeConfig.TimeStateConfigs[timeData.StateIndex].fog;
        // 背景音乐
        if (timeConfig.TimeStateConfigs[timeData.StateIndex].bgAudioClip != null) StartCoroutine(ChangeBGAudio(timeConfig.TimeStateConfigs[timeData.StateIndex].bgAudioClip));
        // 发送是否为太阳的事件
        EventManager.EventTrigger<bool>(EventName.UpdateTimeState, timeData.StateIndex <= 1);
        // 发送当前是第几天的事件
        EventManager.EventTrigger<int>(EventName.UpdateDayNum, timeData.DayNum);
    }
    private void UpdateTime()
    {
        timeData.CalculateTime -= Time.deltaTime * timeScale;
        // 计算并且得到阳光相关的设置
        if (!timeConfig.TimeStateConfigs[timeData.StateIndex].CheckAndCalTime(timeData.CalculateTime, timeConfig.TimeStateConfigs[nextIndex], out Quaternion quaternion, out Color color, out float sunIntensity))
        {
            EnterNextState();
        }
        SetLight(sunIntensity, quaternion, color);
    }
    // 进入新的事件状态
    private void EnterNextState()
    {
        // 切换到下一个状态
        timeData.StateIndex = nextIndex;
        // 检查边界，超过就从0开始
        nextIndex = timeData.StateIndex + 1 >= timeConfig.TimeStateConfigs.Length ? 0 : timeData.StateIndex + 1;
        // 如果现在是早上，也就是currentStateIndex==0，那么意味着，天数加1
        if (timeData.StateIndex == 0)
        {
            timeData.DayNum++;
            // 发送当前是第几天的事件
            EventManager.EventTrigger<int>(EventName.UpdateDayNum, timeData.DayNum);
            // 发送当前时早晨的事件
            EventManager.EventTrigger(EventName.OnMorn);
        }
       
        timeData.CalculateTime = timeConfig.TimeStateConfigs[timeData.StateIndex].durationTime;

        // 雾
        RenderSettings.fog = timeConfig.TimeStateConfigs[timeData.StateIndex].fog;

        // 背景音乐
        if (timeConfig.TimeStateConfigs[timeData.StateIndex].bgAudioClip != null)
            StartCoroutine(ChangeBGAudio(timeConfig.TimeStateConfigs[timeData.StateIndex].bgAudioClip));

        // 发送是否为太阳的事件
        EventManager.EventTrigger<bool>(EventName.UpdateTimeState, timeData.StateIndex <= 1);
    }
    private void SetLight(float intensity,Quaternion rotation,Color color)
    { 
        // 设置环境光的亮度
        RenderSettings.ambientIntensity = intensity;
        mainLight.intensity = intensity;
        mainLight.transform.rotation = rotation;
        mainLight.color = color;
    }

    /// <summary>
    /// 切换背景音乐
    /// </summary>
    private IEnumerator ChangeBGAudio(AudioClip audioClip)
    {
        float old = AudioManager.Instance.BGVolume;
        if (old <= 0) yield break;
        float current = old;
        // 降低音量
        while (current>0)
        {
            yield return null;
            current -= Time.deltaTime / 2;
            AudioManager.Instance.BGVolume = current;
        }
        AudioManager.Instance.PlayBGAudio(audioClip);
        // 提高音量
        while (current < old)
        {
            yield return null;
            current += Time.deltaTime / 2;
            AudioManager.Instance.BGVolume = current;
        }
        AudioManager.Instance.BGVolume = old;
    }

    private void OnSaveGame()
    {
        ArchiveManager.Instance.SaveTimeData();
    }
}
