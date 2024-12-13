using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "��ͼ��������", menuName = "Config/��ͼ��������")]
public class MapObjectConfig : ConfigBase
{
    [LabelText("�յ� ��������Ʒ")]
    public bool IsEmpty = false;
    [LabelText("���ڵĵ�ͼ��������")]
    public MapVertexType MapVertexType;
    [LabelText("���ɵ�Ԥ����")]
    public GameObject Prefab;
    [LabelText("��UI��ͼ�ϵ�Icon")]
    public Sprite MapIconSprite;
    [LabelText("UI��ͼIcon�ߴ�")]
    public float IconSize = 1;
    [LabelText("��������,-1����������")]
    public int DestoryDays = -1;
    [LabelText("���ɸ��� Ȩ������")]
    public int Probability;
    [LabelText("����"),MultiLineProperty] public string Description;

}
