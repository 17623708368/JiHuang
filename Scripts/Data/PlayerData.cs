using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 玩家位置和旋转的存档数据
/// 用于保存和恢复玩家的位置信息和旋转信息
/// </summary>
[Serializable]
public class PlayerTransformData
{
    // 私有字段：用于序列化保存玩家位置
    private Serialization_Vector3 position;

    /// <summary>
    /// 玩家位置（世界坐标）
    /// 通过序列化和反序列化机制支持存档系统
    /// </summary>
    public Vector3 Position
    {
        // 将序列化的 Vector3 数据转换为 Unity 的 Vector3 类型
        get => position.ConverToVector3();
        // 将 Unity 的 Vector3 类型转换为可序列化的 Vector3 数据
        set => position = value.ConverToSVector3();
    }

    // 私有字段：用于序列化保存玩家旋转信息
    private Serialization_Vector3 rotation;

    /// <summary>
    /// 玩家旋转（欧拉角形式）
    /// 通过序列化和反序列化机制支持存档系统
    /// </summary>
    public Vector3 Rotation
    {
        // 将序列化的 Vector3 数据转换为 Unity 的 Vector3 类型
        get => rotation.ConverToVector3();
        // 将 Unity 的 Vector3 类型转换为可序列化的 Vector3 数据
        set => rotation = value.ConverToSVector3();
    }
}

/// <summary>
/// 玩家主要状态数据
/// 用于存储和管理玩家的生命值、饥饿值和精神值
/// </summary>
[Serializable]
public class PlayerMainData
{
    /// <summary>
    /// 玩家当前的生命值
    /// </summary>
    public float Hp;

    /// <summary>
    /// 玩家当前的饥饿值
    /// </summary>
    public float Hungry;

    /// <summary>
    /// 玩家当前的精神值
    /// </summary>
    public float Spirit;
}