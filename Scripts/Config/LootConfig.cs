using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using Sirenix.OdinInspector;

/// <summary>
/// µôÂäÅäÖÃ
/// </summary>
[CreateAssetMenu(fileName ="µôÂäÅäÖÃ",menuName ="Config/µôÂäÅäÖÃ")]
public class LootConfig : ConfigBase
{
    [LabelText("µôÂäÅäÖÃÁĞ±í")] public List<LootConfigModel> Configs;
}

public class LootConfigModel
{
    [LabelText("µôÂäÎïÌåID")] public int LootObjectConfigID;
    [LabelText("µôÂä¸ÅÂÊ%")] public int Probability;
}
