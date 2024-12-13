using System;
using System.Collections;
using System.Collections.Generic;
using JKFrame;
using Unity.VisualScripting;
using UnityEngine;

public class Campfire_Controller :BuildingBase
{
    [SerializeField] private new Light light;
    [SerializeField] private GameObject tinyFire;
    [SerializeField] private AudioSource audioSource;
    private CampfireConfig campfireConfig;
    private CampfireData campfireData;
    public override void Init(MapChunkController mapChunk, ulong id, bool isFormBuilding)
    {
        base.Init(mapChunk, id, isFormBuilding);
        campfireConfig = ConfigManager.Instance.GetConfig<CampfireConfig>(ConfigName.campfire, 0);
        if (isFormBuilding)
        {
            campfireData = new CampfireData()
            {
                currentFuel = campfireConfig.maxFuel
            };
            ArchiveManager.Instance.AddMapObjectTypeData(id,campfireData);
        }
        else
        {
            campfireData = ArchiveManager.Instance.GetMapObjectTypeData(id)as CampfireData;
        }
        SetLitght(campfireData.currentFuel);
        MonoManager.Instance.AddUpdateListener(OnUpdate);
        EventManager.AddEventListener(EventName.SaveGame,OnSaveGame);
    } 


    private void OnUpdate()
    {
        if (!GameSceneManager.Instance.IsInitialized)return;
        if ( campfireData. currentFuel<=0)return;
        //TODO:后面根据timemanage来更改
        campfireData.   currentFuel = Mathf.Clamp( campfireData. currentFuel - TimeManager.Instance.timeScale*Time.deltaTime * campfireConfig.fireSeed, 0, campfireConfig.maxFuel);
        SetLitght( campfireData. currentFuel);

    }

    public override void Onpreview()
    {
        SetLitght(0);
    }

    public override bool OnSlotEndDragSelect(int itemID)
    {
        if (campfireConfig.addFuelDic.TryGetValue(itemID,out float fuleValue) )
        {
            campfireData.currentFuel += fuleValue;
            SetLitght(campfireData.currentFuel);
            return true;
        }

        if (campfireData.currentFuel>0)
        {
            if (campfireConfig.rawItemToCookedItemDic.TryGetValue(itemID,out int cookedItmeID))
            {
                InventoryManager.Instance.AddMainItemAndPlayAuio(cookedItmeID);
                return true;
            }
        }
      

        return false;
    }

    private void SetLitght(float fuelValue)
    {
        light.gameObject.SetActive(fuelValue!=0);
        tinyFire.SetActive(fuelValue!=0);
        audioSource.enabled = fuelValue != 0;
        if (fuelValue!=0)
        {
            float ratioValue = fuelValue / campfireConfig.maxFuel;
            light.intensity = Mathf.Lerp(0, campfireConfig.maxIntenity, ratioValue);
            light.range = Mathf.Lerp(0, campfireConfig.maxRange, ratioValue);
            audioSource.volume = ratioValue;
        }
    }
    private void OnSaveGame()
    {
        MonoManager.Instance.RemoveUpdateListener(OnUpdate);
    }

}
