using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageBox_Controller : BuildingBase
{
    private StorageBoxInventoryData storageData;
    [SerializeField] private Vector2Int withSize;
    public override void Init(MapChunkController mapChunk, ulong id, bool isFormBuilding)
    {
        base.Init(mapChunk, id, isFormBuilding);
        if (isFormBuilding)
        {
            storageData = new StorageBoxInventoryData(withSize.x*withSize.y);
            ArchiveManager.Instance.AddMapObjectTypeData(id,storageData);
        }
        else
        {
            storageData = ArchiveManager.Instance.GetMapObjectTypeData(id) as StorageBoxInventoryData;
        }
    }

    public override void OnSelect()
    {
        InventoryManager.Instance.OpenStorageBoxInventoryUI(this,storageData.InventoryData,withSize);
    }

    public override void Onpreview()
    {
        
    }
}
