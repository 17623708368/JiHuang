using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using Sirenix.OdinInspector;

/// <summary>
/// ��������
/// </summary>
[CreateAssetMenu(fileName ="��������",menuName ="Config/��������")]
public class LootConfig : ConfigBase
{
    [LabelText("���������б�")] public List<LootConfigModel> Configs;
}

public class LootConfigModel
{
    [LabelText("��������ID")] public int LootObjectConfigID;
    [LabelText("�������%")] public int Probability;
}
