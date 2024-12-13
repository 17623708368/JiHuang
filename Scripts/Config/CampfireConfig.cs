using System.Collections.Generic;
using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/篝火配置", fileName = "篝火配置")]
public class CampfireConfig : ConfigBase
{
    [LabelText("最大燃料")] public int  maxFuel;
    [LabelText("最小燃料")] public int mainFuel;
    [LabelText("最大范围")] public int maxRange;
    [LabelText("最大强度")] public int maxIntenity;
    [LabelText(("火焰燃烧速度"))] public int fireSeed;
    [LabelText("燃料添加对应配置")] public Dictionary<int, float> addFuelDic;
    [LabelText("物品烤熟对应的itemID")] public Dictionary<int, int> rawItemToCookedItemDic;
}