using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JKFrame;
using System;using Unity.VisualScripting;

public struct MapDatad
{
    
}
public abstract class InventoryWindowBase : UI_WindowBase
{
// 背包数据，用于存储当前背包中的物品信息
    protected InventoryData inventoryData;

// 普通物品槽位数组
    [SerializeField] protected  List<UI_ItemSlot> slots;
    public Sprite[] bgSprites;
    /// <summary>
    /// 根据存档数据初始化背包中的所有槽位
    /// </summary>
    /// <param name="mainData">背包存档数据</param>
    protected virtual void InitSlotData( )
    {
        // 初始化普通物品槽位的数据
        for (int i = 0; i < inventoryData.ItemDatas.Length; i++)
        {
            slots[i].Init(i, this); // 初始化槽位
            slots[i].InitData(inventoryData.ItemDatas[i]); // 初始化槽位中的物品数据
        }
    }

/// <summary>
/// 添加物品并播放相应的音效
/// </summary>
/// <param name="itemConfigID">物品的配置ID</param>
/// <returns>添加成功返回 true，失败返回 false</returns>
public bool AddItemAndPlayAuio(int itemConfigID,ulong lastWeaponID=0)
{
    // 调用 AddItem 尝试添加物品
    bool res = AddItem(itemConfigID,lastWeaponID);

    // 根据添加是否成功播放对应的音效
    if (res)
    {
        ProjectTool.PlayAudio(AudioType.Bag); // 成功音效
    }
    else
    {
        ProjectTool.PlayAudio(AudioType.Fail); // 失败音效
    }

    return res;
}

/// <summary>
/// 添加物品到背包
/// </summary>
/// <param name="itemConfigID">物品的配置ID</param>
/// <returns>添加成功返回 true，失败返回 false</returns>
public virtual bool AddItem(int itemConfigID,ulong lastWeaponID)
{
    // 根据配置ID获取物品配置
    ItemConfig itemConfig = ConfigManager.Instance.GetConfig<ItemConfig>(ConfigName.item, itemConfigID);

    // 根据物品类型进行不同的处理
    switch (itemConfig.ItemType)
    {
        case ItemType.Consumable:
        case ItemType.Material:
            // 尝试堆叠物品到已有槽位
            if (CheckAndPileItemForSlot(itemConfigID))
            {
                return true;
            }
            // 如果无法堆叠，尝试添加到空槽位
            return CheckAndAddItemForEmptySlot(itemConfigID);
    }

    // 如果物品类型不支持，返回 false
    return false;
}

/// <summary>
/// 检查并尝试将物品添加到空槽位
/// </summary>
/// <param name="itemConfigID">物品的配置ID</param>
/// <returns>添加成功返回 true，失败返回 false</returns>
protected bool CheckAndAddItemForEmptySlot(int itemConfigID,ItemData itemWeaponData=null)
{
    // 获取第一个空槽位的索引
    int index = GetEmptySlot();
    
    // 如果存在空槽，将物品添加到该槽位
    if (index >= 0)
    {
        if (itemWeaponData!=null)
        {
            SetItem(index,itemWeaponData);
            return true;
        }
         ItemData   itemData = ItemData.CreateItemData(itemConfigID);
         SetItem(index,itemData);
        return true;
    }

    // 没有空槽，返回 false
    return false;
}

/// <summary>
/// 获取第一个空槽位的索引
/// </summary>
/// <returns>如果存在空槽，返回槽位索引；如果没有空槽，返回 -1</returns>
protected int GetEmptySlot()
{
    // 遍历所有槽位，找到第一个没有物品的槽位
    for (int i = 0; i < slots.Count; i++)
    {
        if (slots[i].ItemData == null)
        {
            return i; // 返回空槽位的索引
        }
    }

    // 没有空槽，返回 -1
    return -1;
}


    /// <summary>
    /// 检查并尝试将物品堆叠到已有的槽位
    /// </summary>
    /// <param name="itemConfigID">物品的配置ID</param>
    /// <returns>如果成功堆叠，返回 true；否则返回 false</returns>
    protected bool CheckAndPileItemForSlot(int itemConfigID)
    {
        // 遍历所有背包槽位
        for (int i = 0; i < slots.Count; i++)
        {
            // 检查槽位是否：
            // 1. 有物品
            // 2. 槽位中的物品与待添加的物品是同一种（根据 ConfigID 比较）
            if (slots[i].ItemData != null && slots[i].ItemData.ConfigID == itemConfigID)
            {
                // 获取槽位中物品的动态数据（堆叠数量等信息）
                PileItemTypeDataBase data = slots[i].ItemData.ItemTypeData as PileItemTypeDataBase;

                // 获取物品配置中的堆叠信息（最大堆叠数量等）
                PileItemTypeInfoBase info = slots[i].ItemData.Config.ItemTypeInfo as PileItemTypeInfoBase;

                // 如果槽位中的物品未达到最大堆叠数量
                if (data.Count < info.MaxCount)
                {
                    // 增加物品数量
                    data.Count += 1;

                    // 更新 UI 显示（物品数量文本）
                    slots[i].UpdateCountTextView();

                    // 返回堆叠成功
                    return true;
                }
            }
        }

        // 如果所有槽位都无法堆叠，返回 false
        return false;
    }


    /// <summary>
    ///  移除格子
    /// </summary>
    /// <param name="index"></param>
    protected   virtual void RemoveItem(int index)
    {
            inventoryData.RemoveItem(index);
            slots[index].InitData(null);
          
    }

    /// <summary>
    /// 丢弃格子物品
    /// </summary>
    public virtual void DiscardItem(int index)
    {
        ItemData itemData = slots[index].ItemData;
        switch (itemData.Config.ItemType)
        {
            case ItemType.Weapon:
                inventoryData.DiscardWeaponDic.Dictionary[(itemData.ItemTypeData as ItemWeaponData).ID] =
                    slots[index].ItemData;
                RemoveItem(index);
                break;
            default:
                // 是否可以堆叠
                PileItemTypeDataBase typeData = itemData.ItemTypeData as PileItemTypeDataBase;
                typeData.Count -= 1;
                if (typeData.Count == 0)
                {
                    RemoveItem(index);
                }
                else
                { 
                    slots[index].UpdateCountTextView();
                }
                break;
        }

    }


    /// <summary>
    /// 设置格子
    /// </summary>
    public virtual void SetItem(int index,ItemData itemData)
    {
        //普通格子
        
            inventoryData.SetItem(index, itemData);
            slots[index].InitData(itemData);

    }
/// <summary>
///得到物品数量 
/// </summary>
    public int GetSlotCount(int configID)
    {
        int count=0;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].ItemData == null) continue;
            if (slots[i].ItemData.ConfigID!=configID) continue;
            if (slots[i].ItemData.Config.ItemTypeInfo is PileItemTypeInfoBase)
            {
                count += ((PileItemTypeDataBase)(slots[i].ItemData.ItemTypeData)).Count;
            }
            else
            {
                count++;
            }
            
        }
        return count;
    }


}
