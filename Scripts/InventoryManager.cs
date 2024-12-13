using System;
using System.Collections;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;

public class InventoryManager : SingletonMono<InventoryManager>
{
  private UI_MainInventoryWindow mainInventoryWindow;

  public void Init()
  {
    mainInventoryWindow = UIManager.Instance.Show<UI_MainInventoryWindow>();
    mainInventoryWindow.InitData();
    EventManager.AddEventListener(EventName.SaveGame,OnSaveGame);
  }

  #region 主背包
  /// <summary>
  ///得到物品数量 
  /// </summary>
  public int GetMainSlotCount(int configID)
  {
      return mainInventoryWindow.GetSlotCount(configID);
  }

  /// <summary>
  /// 添加物品并播放相应的音效
  /// </summary>
  /// <param name="itemConfigID">物品的配置ID</param>
  /// <returns>添加成功返回 true，失败返回 false</returns>
  public bool AddMainItemAndPlayAuio(int itemConfigID )
  {
      return mainInventoryWindow.AddItemAndPlayAuio(itemConfigID );
  }

  /// <summary>
  /// 对材料进行相减
  /// </summary>
  /// <param name="buildConfig"></param>
  public void UpdateMainItemForBuilds(BuildConfig buildConfig)
  {
      mainInventoryWindow.UpdateItemForBuilds(buildConfig);
  }

  /// <summary>
  /// 添加物品到背包
  /// </summary>
  /// <param name="itemConfigID">物品的配置ID</param>
  /// <returns>添加成功返回 true，失败返回 false</returns>
  public bool AddMainItem(int itemConfigID,ulong discardGameObjectID)
  {
      return mainInventoryWindow.AddItem(itemConfigID,discardGameObjectID);
  }

  #endregion

  #region 储物箱

  public void OpenStorageBoxInventoryUI(StorageBox_Controller storageBoxController,InventoryData inventoryData,Vector2Int size)
  {
          ProjectTool.PlayAudio(AudioType.Bag);
          UIManager.Instance.Close<UI_StorageBoxInventoryWindow>();
          UIManager.Instance.Show<UI_StorageBoxInventoryWindow>().Init(storageBoxController,inventoryData,size);
  }
  

  #endregion
  private void OnSaveGame()
  {
      ArchiveManager.Instance.SaveInventoryData();
  }
}
