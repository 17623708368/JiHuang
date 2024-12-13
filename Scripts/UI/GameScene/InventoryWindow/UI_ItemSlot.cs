using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JKFrame;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// 物品栏中的一个格子
/// </summary>
public class UI_ItemSlot : MonoBehaviour
{
    [SerializeField] Image bgImg;
    [SerializeField] Image iconImg;
    [SerializeField] Text countText;

    public ItemData ItemData { get; private set; }  // 格子里面的数据
    public int Index { get; private set; }          // 第几个格子
    private InventoryWindowBase ownerWindowBase;         // 宿主窗口：物品栏或仓库

    private Transform slotTransform;
    private Transform iconTransform;

    public static UI_ItemSlot CurrentMouseEnterSlot;
    public static UI_ItemSlot WeaponSlot;
    private Func<int, AudioType> useItemAction;

    public bool isCurrent=false;
    private void Start()
    {
        slotTransform = transform;
        iconTransform = iconImg.transform;
        // 鼠标交互事件
        this.OnMouseEnter(MouseEnter);
        this.OnMouseExit(MouseExit);
        this.OnBeginDrag(BeginDrag);
        this.OnDrag(Drag);
        this.OnEndDrag(EndDrag);
    }
    private void OnEnable()
    {
        this.OnUpdate(CheckMouseRightClick);
    }
    private void OnDisable()
    {
        this.RemoveUpdate(CheckMouseRightClick);
    }
 
    /// <summary>
    /// 检测鼠标右键
    /// 是否可以使用物品检测
    /// </summary>
    private void CheckMouseRightClick()
    {
        if (ItemData == null||useItemAction==null) return;
        //判断是否有可输入字符串输入
        if (!string.IsNullOrEmpty(Input.inputString))
        {
            //得到第一个输入的字符串
            char inputChar = Input.inputString[0];
            if (inputChar >= '0' && inputChar <= '9')
            {
                isCurrent = Index==((inputChar-'0')-1);
            }
        }
        // 当 当前格子为选中状态，并且按下了右键，则目的是使用物品
        if ((isMouseStay && Input.GetMouseButtonDown(1))||isCurrent)
        {
            // 根据使用的情况来播放音效
            AudioType reslutAudioType =useItemAction.Invoke(Index);
            ProjectTool.PlayAudio(reslutAudioType);
            isCurrent = false;  
        }
    }

    public void Init(int index,InventoryWindowBase ownerWindowBase,Func<int,AudioType>useItemAction=null)
    { 
        this.Index = index;
        this.ownerWindowBase = ownerWindowBase;
        this.useItemAction = useItemAction;
        bgImg.sprite = ownerWindowBase.bgSprites[0];
    }

    public void InitData(ItemData itemData=null)
    {
        this.ItemData = itemData;
        // 如果数据为空，意味着是个空格子
        if (ItemData == null)
        {
            // 外框设置为白色、隐藏数量、Icon
            bgImg.color = Color.white;
            countText.gameObject.SetActive(false);
            iconImg.sprite = null;
            iconImg.gameObject.SetActive(false);
            return;
        }
        // 有数据的情况
        countText.gameObject.SetActive(true);
        iconImg.gameObject.SetActive(true);
        iconImg.sprite = ItemData.Config.Icon;  // 根据格子里面的数据 的配置来显示图片
        UpdateCountTextView();
    }

    public void UpdateCountTextView()
    {
        // 根据不同的物品类型，显示不同的效果
        switch (ItemData.Config.ItemType)
        {
            case ItemType.Weapon:
                bgImg.color = Color.white;
                countText.text = (ItemData.ItemTypeData as ItemWeaponData).Durability.ToString() + "%";
                break;

            case ItemType.Consumable:
                bgImg.color = new Color(0, 1, 0, 0.5f);
                countText.text = (ItemData.ItemTypeData as ItemConsumableData).Count.ToString();
                break;

            case ItemType.Material:
                bgImg.color = Color.white;
                countText.text = (ItemData.ItemTypeData as ItemMaterialData).Count.ToString();
                break;
        }
    }

    #region 鼠标交互
    private bool isMouseStay = false;
    private void MouseEnter(PointerEventData arg1, object[] arg2)
    {
        GameManager.Instance.SetCursorState(CursorState.Handle);
        bgImg.sprite = ownerWindowBase.bgSprites[1];
        isMouseStay = true;
        CurrentMouseEnterSlot = this;
    }
    private void MouseExit(PointerEventData arg1, object[] arg2)
    {
        GameManager.Instance.SetCursorState(CursorState.Normal);
        bgImg.sprite = ownerWindowBase.bgSprites[0];
        isMouseStay = false;
        CurrentMouseEnterSlot = null;
    }


    private void BeginDrag(PointerEventData arg1, object[] arg2)
    {
        if (ItemData == null) return;
        iconTransform.SetParent(UIManager.Instance.DragLayer);
    }
    private void Drag(PointerEventData eventData, object[] arg2)
    {
        if (ItemData == null) return;
        GameManager.Instance.SetCursorState(CursorState.Handle);
        iconTransform.position = eventData.position;

    }
    private void EndDrag(PointerEventData eventData, object[] arg2)
    {
        if (ItemData == null) return;
        // 虽然松手了，但是鼠标还在某个格子里面
        if (CurrentMouseEnterSlot == null)
        {
            GameManager.Instance.SetCursorState(CursorState.Normal);
        }

        if (InputManager.Instance.CheckMouseOnBuildingObject(ItemData.ConfigID))
        {
            ResetIcon();
            ownerWindowBase.DiscardItem(Index);
            return;
        }
        ResetIcon();
        // 如果还是自己 不执行任何操作
        if (CurrentMouseEnterSlot == this) return;

        // 物品丢弃
        if (CurrentMouseEnterSlot == null)
        {
            // 射线去检测有没有除了Mask以外的任何UI物体
            if (InputManager.Instance.CheckMouseOnUI()) return;
            // 射线去检测防止玩家讲物品扔到大型物体身上
            if (InputManager.Instance.CheckMouseOnBigMapObject()) return;
            if (InputManager.Instance.GetMouseWorldPositionOnGround(eventData.position,out Vector3 worldPos))
            {
                // 在地面生成物品
                worldPos.y = 1;
                MapManager.Instance.SpawnMapObject(ItemData.Config.MapObjectConfigID, worldPos,false,(ItemData.ItemTypeData is  ItemWeaponData) ?(ItemData.ItemTypeData as  ItemWeaponData).ID:0 );
                ProjectTool.PlayAudio(AudioType.Bag);
                // 自身执行丢弃逻辑
                ownerWindowBase.DiscardItem(Index);
            }
            return;
        }

        // 当前操作的是武器槽
        if (this == WeaponSlot)
        {
            // 目标格子是空的，卸下武器的操作
            if (CurrentMouseEnterSlot.ItemData == null)
            {
                ProjectTool.PlayAudio(AudioType.TakeDownWeapon);
                SwapSlotItem(this, CurrentMouseEnterSlot);
            }
            // 目标格子是武器
            else if (CurrentMouseEnterSlot.ItemData.Config.ItemType == ItemType.Weapon)
            {
                ProjectTool.PlayAudio(AudioType.TakeUpWeapon);
                SwapSlotItem(this, CurrentMouseEnterSlot);
            }
            // 目标格子不是武器
            else
            {
                ProjectTool.PlayAudio(AudioType.Fail);
                UIManager.Instance.AddTips("必须得是装备才行!");
            }
        }

        // 当前是普通格子
        else
        {
            // 普通格子里面的东西拽入武器槽
            if (CurrentMouseEnterSlot == WeaponSlot)
            {
                // 当前格子不是武器，所以不能拽进去
                if (ItemData.Config.ItemType != ItemType.Weapon)
                {
                    ProjectTool.PlayAudio(AudioType.Fail);
                    UIManager.Instance.AddTips("必须得是装备才行!");
                }
                // 当前格子是武器，可以拽入
                else
                {
                    ProjectTool.PlayAudio(AudioType.TakeUpWeapon);
                    Debug.Log("可以装备物品:" + ItemData.Config.Name);
                    SwapSlotItem(this, CurrentMouseEnterSlot);
                }
            }
            // 普通格子和普通格子交换
            else
            {
                // 格子物品交换
                SwapSlotItem(this, CurrentMouseEnterSlot);
                ProjectTool.PlayAudio(AudioType.Bag);
            }
        }
    }

    private void ResetIcon()
    {
        // 当前拖拽中的Icon归位
        iconTransform.SetParent(slotTransform);
        iconTransform.localPosition = Vector3.zero;
        iconTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// 交换格子中的物品数据
    /// </summary>
    public static void SwapSlotItem(UI_ItemSlot slot1,UI_ItemSlot slot2)
    {
        ItemData itemData1 = slot1.ItemData;
        ItemData itemData2 = slot2.ItemData;
        // 窗口、存档的数据交换
        // 有可能是跨窗口交换，比如快捷栏跟仓库交换
        slot1.ownerWindowBase.SetItem(slot1.Index, itemData2);
        slot2.ownerWindowBase.SetItem(slot2.Index, itemData1);
    }

    #endregion
}
