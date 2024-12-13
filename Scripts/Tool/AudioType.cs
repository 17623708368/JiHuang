using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 音效类型
/// </summary>
public enum AudioType
{ 
    [LabelText("玩家不能使用")] PlayerConnot,
    [LabelText("武器拿起")] TakeUpWeapon,
    [LabelText("武器拿下")] TakeDownWeapon,
    [LabelText("消耗品成功")] ConsumablesOK,
    [LabelText("消耗品失败")] ConsumablesFail,
    [LabelText("背包")] Bag,
    [LabelText("通用失败")] Fail,
}
