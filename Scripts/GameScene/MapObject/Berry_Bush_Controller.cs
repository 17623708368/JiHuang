using System;
using System.Collections;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;

/// <summary>
/// 浆果控制器
/// </summary>
public class Berry_Bush_Controller : Bush_Controller,IBuilding
{
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Material[] materials;  // 0是有果子 1是没有果子
    private BerryTypeData berryTypeData;
    [SerializeField] private int berryGrowDayNum;
    [SerializeField] private new Collider collider;
    #region 预览模式
    public Collider Collider => collider;
    private List<Material> mmaterialsList;
    public List<Material> MaterialsList { get=>mmaterialsList; set=>mmaterialsList=value; }
    public GameObject GameObject => gameObject;
    public float TouchDictance { get; }

    #endregion
    public override void Init(MapChunkController mapChunk, ulong id,bool isFormBuilding)
    {
        base.Init(mapChunk, id,isFormBuilding);
        if (ArchiveManager.Instance.TryGetMapObjectTypeData(id,out IMapObjectTypeData typeData))
        {
         if (typeData is BerryTypeData)
         {
             berryTypeData=  typeData as BerryTypeData;
         }
        }
        else
        {
            berryTypeData=new BerryTypeData();
            ArchiveManager.Instance.AddMapObjectTypeData(id,berryTypeData);
        }
        EventManager.AddEventListener(EventName.OnMorn,OnMorn);
        if (isFormBuilding)
        {
            berryTypeData.lastTicpUPDayNum = TimeManager.Instance.currentDay;
        }
        CheckDayAndSetState();
    }

 
    public override int OnPickUp()
    {
        
        // 修改外表
        meshRenderer.sharedMaterial = materials[1];
        berryTypeData.lastTicpUPDayNum = TimeManager.Instance.currentDay;
        canPickUp = false;
        return pickUpItemConfigID;
    }

    public void CheckDayAndSetState()
    {
        if (berryTypeData.lastTicpUPDayNum==-1)
        {
            meshRenderer.sharedMaterial = materials[0];
            canPickUp = true;
        }
        else if(Mathf.Abs(berryTypeData.lastTicpUPDayNum-TimeManager.Instance.currentDay)>berryGrowDayNum)
        {
            meshRenderer.sharedMaterial = materials[0];
            canPickUp = true;
        }
        else
        {
            meshRenderer.sharedMaterial = materials[1];
            canPickUp = false;
        }
    }
    private void OnMorn()
    {
        if (!canPickUp) CheckDayAndSetState();
        
    }


}
