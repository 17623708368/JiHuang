using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName ="Config/玩家设置",fileName = "玩家设置")]
public class PlayerConfig : ConfigBase
{
    #region 角色配置
    [FoldoutGroup("角色配置"), LabelText("旋转速度")]
    public float RotateSpeed = 10;
    [FoldoutGroup("角色配置"), LabelText("移动速度")]
    public float MoveSpeed  = 4;
    [FoldoutGroup("角色配置"), LabelText("最大生命值")]
    public float MaxHp = 100;
    [FoldoutGroup("角色配置"), LabelText("最大饥饿值")]
    public float MaxHungry = 100;   
    [FoldoutGroup("角色配置"), LabelText("最大饥饿值")]
    public float MaxSpirit = 100;
    [FoldoutGroup("角色配置"), LabelText("饥饿值衰减速度")]
    public float HungryReduceSeed = 0.2f;
    [FoldoutGroup("角色配置"), LabelText("当饥饿值为0的时 生命值的衰减速度")]
    public float HpReduceSpeedOnHungryIsZero = 2;
    [FoldoutGroup("角色配置"), LabelText("当饥饿值为0的时 精神值的衰减速度")]
    public float spiritDampSpeedOnHungryDampIsZero = 1;
    [FoldoutGroup("角色配置"), LabelText("走路音效")]
    public AudioClip[] FootstepAudioClis;
    [FoldoutGroup("角色配置"), LabelText("默认动画状态机")]
    public RuntimeAnimatorController NormalAnimatorController;
    #endregion
    [FoldoutGroup("角色配置"), LabelText("ui精灵")]
    public Dictionary<StringNameType, Sprite[]> mainInfoSprptDic;
    #region 杂项
    [FoldoutGroup("杂项"), LabelText("音效配置")]
    public Dictionary<AudioType, AudioClip> AudioClipDic;
    #endregion
}
