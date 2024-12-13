using System.Collections;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;
[UIElement(false,"UI/UI_MainInventoryWindow",1)]
public class UI_MainInventoryWindow : InventoryWindowBase
{
    // 武器槽，用于装备武器的专属槽位
    [SerializeField] UI_ItemSlot weaponSlot;
    // 背包背景图数组，用于 UI 显示
    private InventoryMainData inventoryMainData;
    public override  void Init()
    {
        // 注册事件监听，当武器攻击成功时执行逻辑
        EventManager.AddEventListener(EventName.PlayerWeaponAttackSucceed, OnPlayerWeaponAttackSucceed);
        // 调用基类的初始化方法
        base.Init();
    }
    public  void InitData()
    {
        // 使用存档数据初始化背包中的物品
        InitInventorData();
        InitSlotData();
        // 根据存档数据初始化玩家当前的武器
        Player_Controller.Instance.ChangeWeapon(inventoryMainData.WeaponSlotItemData);
    }
    private   void InitInventorData()
    {
        inventoryData = ArchiveManager.Instance.InventoryMainData;
        inventoryMainData=inventoryData as InventoryMainData;
    }
    protected override void InitSlotData()
    {
        // 初始化普通物品槽位的数据
        for (int i = 0; i < inventoryData.ItemDatas.Length; i++)
        {
            slots[i].Init(i, this,UseItem); // 初始化槽位
            slots[i].InitData(inventoryData.ItemDatas[i]); // 初始化槽位中的物品数据
        }
        // 初始化武器槽
        weaponSlot.Init(slots.Count, this,UseItem); // 将武器槽的位置设为普通槽位数组的最后一位
        UI_ItemSlot.WeaponSlot = weaponSlot; // 设置全局的武器槽引用
        // 初始化武器槽的数据
        weaponSlot.InitData(inventoryMainData.WeaponSlotItemData);
    }

    public override bool AddItem(int itemConfigID,ulong lastWeaponID=0)
    {
        // 根据配置ID获取物品配置
        ItemConfig itemConfig = ConfigManager.Instance.GetConfig<ItemConfig>(ConfigName.item, itemConfigID);

        // 根据物品类型进行不同的处理
        switch (itemConfig.ItemType)
        {
            case ItemType.Weapon:
                ItemData itemData=null;
                if (lastWeaponID!=0)
                {
                   itemData = inventoryData.DiscardWeaponDic.Dictionary[lastWeaponID];
                }
                // 如果武器槽为空，直接放入武器槽
                if (weaponSlot.ItemData == null)
                {
                    if (itemData==null)
                    {
                        itemData=ItemData.CreateItemData(itemConfigID);
                    }
                    weaponSlot.InitData(itemData);
                    inventoryMainData.SetWeaponItem(itemData);
                    Player_Controller.Instance.ChangeWeapon(itemData);
                    return true;
                }
                // 如果武器槽已满，尝试添加到空槽位
                return CheckAndAddItemForEmptySlot(itemConfigID,itemData);
        }
        return base.AddItem(itemConfigID,lastWeaponID);
        
    }

    protected override void RemoveItem(int index)
    {
        // 判断是否是武器格子
        if (index == inventoryMainData.ItemDatas.Length)
        {
            inventoryMainData.RemoveWeaponItem();
            weaponSlot.InitData(null);
        }
        else
        {
            base.RemoveItem(index);
        }
    }

    public override void DiscardItem(int index)
    {
        //  如果格子是武器格子
        if (index == slots.Count)
        {
            ulong ID = (weaponSlot.ItemData.ItemTypeData as ItemWeaponData).ID;
            inventoryData.DiscardWeaponDic.Dictionary[ID] = weaponSlot.ItemData;
            RemoveItem(index);
            Player_Controller.Instance.ChangeWeapon(null);
        }
        else
        {
            base.DiscardItem(index);
        }
    }

    public override void SetItem(int index, ItemData itemData)
    {
        if (index == inventoryMainData.ItemDatas.Length)
        {
            inventoryMainData.SetWeaponItem(itemData);
            weaponSlot.InitData(itemData);
            // 切换玩家显示
            Player_Controller.Instance.ChangeWeapon(itemData);
        }
        else
        {
            base.SetItem(index, itemData);
        }
    }

public AudioType UseItem(int index)
{
    // 检查玩家是否可以使用物品
    if (Player_Controller.Instance.CanUseItem == false) 
        return AudioType.PlayerConnot; // 如果玩家无法使用物品，返回对应音效类型

    // 检查是否选择的是武器格子
    if (index == slots.Count)
    {
        int emptySlotIndex = GetEmptySlot(); // 获取第一个空的背包格子索引
        if (emptySlotIndex > 0)
        {
            // 如果存在空格子，将武器放入空格子
            UI_ItemSlot.SwapSlotItem(weaponSlot, slots[emptySlotIndex]);
            return AudioType.TakeDownWeapon; // 返回取下武器的音效
        }
        else
        {
            // 如果没有空格子，返回失败音效
            return AudioType.Fail;
        }
    }

    // 获取当前槽位的物品数据
    ItemData itemData = slots[index].ItemData;

    // 根据物品类型执行不同操作
    switch (itemData.Config.ItemType)
    {
        case ItemType.Weapon:
            // 如果是武器，将武器从背包中装备到武器槽
            UI_ItemSlot.SwapSlotItem(slots[index], weaponSlot);
            return AudioType.TakeUpWeapon; // 返回装备武器的音效

        case ItemType.Consumable:
            // 如果是消耗品，获取其对应的恢复效果
            ItemCosumableInfo info = itemData.Config.ItemTypeInfo as ItemCosumableInfo;

            // 恢复玩家的生命值、饥饿值或精神值（根据物品属性）
            if (info.RecoverHP != 0) 
                Player_Controller.Instance.RecoverHP(info.RecoverHP);
            if (info.RecoverHungry != 0) 
                Player_Controller.Instance.RecoverHungry(info.RecoverHungry);
            if (info.RecoverSprite != 0) 
                Player_Controller.Instance.RecoverSripte(info.RecoverSprite);

            // 更新消耗品的数量
            PileItemTypeDataBase typeData = itemData.ItemTypeData as PileItemTypeDataBase;
            typeData.Count -= 1; // 减少物品数量

            // 如果物品数量为0，将其从背包中移除
            if (typeData.Count == 0)
            {
                RemoveItem(index);
            }
            else
            {
                // 否则更新显示的物品数量
                slots[index].UpdateCountTextView();
            }
            return AudioType.ConsumablesOK; // 返回消耗品使用成功的音效

        default:
            // 如果物品类型未知，返回失败音效
            return AudioType.Fail;
    }
}

/// <summary>
/// 玩家攻击成功时的逻辑处理
/// </summary>
private void OnPlayerWeaponAttackSucceed()
{
    // 检查武器槽是否有武器数据
    if (inventoryMainData.WeaponSlotItemData == null) 
        return;

    // 获取武器的详细数据和配置信息
    ItemWeaponData weaponData = inventoryMainData.WeaponSlotItemData.ItemTypeData as ItemWeaponData;
    ItemWeaponInfo weaponInfo = inventoryMainData.WeaponSlotItemData.Config.ItemTypeInfo as ItemWeaponInfo;

    // 减少武器的耐久度（根据攻击的消耗值）
    weaponData.Durability -= weaponInfo.AttackDurabilityCost;

    // 如果耐久度小于等于0，移除武器
    if (weaponData.Durability <= 0)
    {
        // 移除武器数据
        inventoryMainData.RemoveWeaponItem();

        // 清空武器槽的数据
        weaponSlot.InitData(null);

        // 通知玩家控制器，当前武器已被移除
        Player_Controller.Instance.ChangeWeapon(null);
    }
    else
    {
        // 如果武器仍有耐久度，更新武器槽的UI显示
        weaponSlot.UpdateCountTextView();
    }
}

    /// <summary>
    /// 对材料进行相减
    /// </summary>
    /// <param name="buildConfig"></param>
    public void UpdateItemForBuilds(BuildConfig buildConfig)
    {
        for (int i = 0; i < buildConfig.buildList.Count; i++)
        {
            UpdateItemForBuild(buildConfig.buildList[i]);
        }
    }

    public void UpdateItemForBuild(BuildConfigCondition buildConfigCondition)
    {
        int count = buildConfigCondition.count;
        for (int i = 0; i < inventoryMainData.ItemDatas.Length; i++)
        {
            if (inventoryMainData.ItemDatas[i]!=null&&inventoryMainData.ItemDatas[i].ConfigID==buildConfigCondition.configID)
            {
                if (inventoryMainData.ItemDatas[i].ItemTypeData is PileItemTypeDataBase)
                {
                    PileItemTypeDataBase pileItemTypeDataBase =
                        inventoryMainData.ItemDatas[i].ItemTypeData as PileItemTypeDataBase;
                    int num = pileItemTypeDataBase.Count - count;
                    if (num>0)
                    {
                        pileItemTypeDataBase.Count -= count;
                        slots[i].UpdateCountTextView();
                        return;
                    }
                    else //差值小于等于零
                    {
                        count -= pileItemTypeDataBase.Count;
                        RemoveItem(i);
                        if (count==0)
                        {
                            return;
                        }

                    }
                }
                else
                {
                    count -= 1;
                    RemoveItem(i);
                    if (count==0)return;
                }
            }
        }
    }
}
