using System;
using UnityEngine;

[Serializable]
public class StorageBoxInventoryData:IMapObjectTypeData
{
    public InventoryData inventoryData;
    public InventoryData InventoryData => inventoryData;
    public StorageBoxInventoryData(int count)
    {
        inventoryData = new InventoryData(count);
    }

}