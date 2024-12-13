using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using Sirenix.OdinInspector;
using System;

/// <summary>
/// 时间状态数据
/// </summary>
[Serializable]
public class TimeStateConfig
{
    // 持续时间
    public float durationTime;
    // 阳光强度
    public float sunIntensity;
    // 阳光颜色
    public Color sunColor;
    // 太阳的角度
    [OnValueChanged(nameof(SetRotation))]
    public Vector3 sunRotation;
    [HideInInspector]
    public Quaternion sunQuaternion;

    // 雾 都是从0.1向下衰减，没有考虑多个时间点都有雾的情况，也就是状态过渡
    public bool fog;
    // 背景音乐
    public AudioClip bgAudioClip;

    private void SetRotation()
    {
        sunQuaternion = Quaternion.Euler(sunRotation);
    }

    /// <summary>
    /// 检查并且计算时间
    /// </summary>
    /// <returns>是否还在当前状态</returns>
    public bool CheckAndCalTime(float currTime, TimeStateConfig nextState, out Quaternion rotation, out Color color, out float sunIntensity)
    {
        // 0~1之间
        float ratio = 1f - (currTime / durationTime);
        rotation = Quaternion.Slerp(this.sunQuaternion, nextState.sunQuaternion, ratio);
        color = Color.Lerp(this.sunColor, nextState.sunColor, ratio);
        sunIntensity = UnityEngine.Mathf.Lerp(this.sunIntensity, nextState.sunIntensity, ratio);

        if (fog)
        {
            RenderSettings.fogDensity = 0.1f * (1 - ratio);
        }

        // 如果时间大于0所以还在本状态
        return currTime > 0;
    }
}

[CreateAssetMenu(fileName = "时间配置", menuName = "Config/时间配置")]
public class TimeConfig : ConfigBase
{
    [LabelText("时间状态/阶段数据")]
    // 0意味着新的一天
    // 0-1是白天 其余是晚上
    public TimeStateConfig[] TimeStateConfigs;    // 时间配置 
}
