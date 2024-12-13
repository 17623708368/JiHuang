using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using System;

/// <summary>
/// 物品的动态数据
/// 用于存储每种物品的具体状态，例如数量、耐久度等
/// </summary>
[Serializable]
public class ItemData
{
    // 配置ID，用于标识物品的静态配置
    public int ConfigID;
    // 物品类型数据的接口，用于支持不同类型物品的动态数据
    public IItemTypeData ItemTypeData;
    public static ulong currentID=1;
    /// <summary>
    /// 获取物品的静态配置
    /// 通过 ConfigID 从配置管理器中加载物品的静态数据
    /// </summary>
    public ItemConfig Config
    {
        get => ConfigManager.Instance.GetConfig<ItemConfig>(ConfigName.item, ConfigID);
    }

    /// <summary>
    /// 创建一个新的物品数据实例
    /// 根据 ConfigID 和物品类型自动初始化物品的动态数据
    /// </summary>
    /// <param name="configID">物品的配置ID</param>
    /// <returns>返回初始化后的物品数据</returns>
    public static ItemData CreateItemData(int configID)
    {
        // 创建新的物品数据对象
        ItemData itemData = new ItemData();
        itemData.ConfigID = configID;
        
        // 根据物品类型初始化动态数据
        switch (itemData.Config.ItemType)
        {
            case ItemType.Weapon:
                // 武器类型物品，初始化耐久度为 100
                itemData.ItemTypeData = new ItemWeaponData()
                {
                    Durability = 100,
                    ID=currentID++
                };
                break;
            case ItemType.Consumable:
                // 消耗品类型物品，初始化数量为 1
                itemData.ItemTypeData = new ItemConsumableData()
                {
                    Count = 1
                };
                break;
            case ItemType.Material:
                // 材料类型物品，初始化数量为 1
                itemData.ItemTypeData = new ItemMaterialData()
                {
                    Count = 1
                };
                break;
        }
        return itemData;
    }
}

/// <summary>
/// 物品类型动态数据的接口
/// 不同物品类型的数据需要实现该接口
/// </summary>
public interface IItemTypeData { }

/// <summary>
/// 武器类型物品的数据
/// 包括耐久度属性
/// </summary>
[Serializable]
public class ItemWeaponData : IItemTypeData
{
    public ulong ID;
    // 耐久度，默认值为 100
    public float Durability = 100;
}

/// <summary>
/// 堆叠物品的基础类
/// 包含堆叠物品的数量属性
/// </summary>
[Serializable]
public abstract class PileItemTypeDataBase
{
    // 堆叠物品的数量
    public int Count;
}

/// <summary>
/// 消耗品类型物品的数据
/// 继承堆叠物品基础类，同时实现 IItemTypeData 接口
/// </summary>
[Serializable]
public class ItemConsumableData : PileItemTypeDataBase, IItemTypeData
{
}

/// <summary>
/// 材料类型物品的数据
/// 继承堆叠物品基础类，同时实现 IItemTypeData 接口
/// </summary>
[Serializable]
public class ItemMaterialData : PileItemTypeDataBase, IItemTypeData
{
}
