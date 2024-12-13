using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using JKFrame;
using UnityEngine;
using UnityEngine.UI;
[UIElement(true,"UI/UI_StorageBoxInventoryWindow",1)]
public class UI_StorageBoxInventoryWindow : InventoryWindowBase
{
    [SerializeField] private Button btnClose;
    [SerializeField] private Transform parent;
    private StorageBox_Controller storageBoxController;
    private bool isOpen=false;
    public override void Init()
    {
        base.Init();
        btnClose.onClick.AddListener(ButtonClose);
        slots = new List<UI_ItemSlot>(15);
    }

    public void Init( StorageBox_Controller storageBoxController,InventoryData inventoryData,Vector2Int size)
    {
        this.inventoryData = inventoryData;
        this.storageBoxController = storageBoxController;
        SetWindowSize(size);
        for (int i = 0; i < inventoryData.ItemDatas.Length; i++)
        {
            UI_ItemSlot slot = ResManager.Load<UI_ItemSlot>("UI/UI_ItemSlot",parent);
            slot.transform.SetParent(parent,false);
            slot.Init(i,this);
            slot.InitData(inventoryData.ItemDatas[i]);
            slots.Add(slot);
        }
        isOpen = true;
    }
    private void SetWindowSize(Vector2Int size)
    {
        RectTransform rectTransform=transform as RectTransform;
        rectTransform.sizeDelta = new Vector2(30 + size.x * 75, 65 + size.y * 75);
    }

    private void Update()
    {
        if (Player_Controller.Instance!=null&& Vector3.Distance( Player_Controller.Instance.transform.position,storageBoxController.transform.position)>storageBoxController.TouchDinstance)
        {
            ButtonClose();
        }
    }

    public void ButtonClose()
    {
        ProjectTool.PlayAudio(AudioType.Bag);
        Close();
    }
    public override  void OnClose()
    {
        if (inventoryData!=null)
        {
            for (int i = 0; i < inventoryData.ItemDatas.Length; i++)
            {
                slots[i].JKGameObjectPushPool();
            }
            inventoryData = null;
            slots.Clear();
            isOpen = false;
        }
    }
}