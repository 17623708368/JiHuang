using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class InventoryData
{
    // ��������װ����Ʒ
    public ItemData[] ItemDatas { get; private set; }
    public Serialization_Dic<ulong, ItemData> DiscardWeaponDic = new Serialization_Dic<ulong, ItemData>();
    public InventoryData(int itemCount)
    {
        ItemDatas = new ItemData[itemCount];
    }
    public void RemoveItem(int index)
    {
        ItemDatas[index] = null;
    }
    public void SetItem(int index,ItemData itemData)
    {
        ItemDatas[index] = itemData;
    }

}
/// <summary>
/// ��Ʒ������
/// </summary>
[Serializable]
public class InventoryMainData:InventoryData
{
    // ��������װ����Ʒ
    public InventoryMainData(int itemCount) : base(itemCount){}
    public ItemData WeaponSlotItemData { get; private set; }
    public void RemoveWeaponItem()
    {
        WeaponSlotItemData = null;
    }
    public void SetWeaponItem(ItemData itemData)
    {
        WeaponSlotItemData = itemData;
    }
}
