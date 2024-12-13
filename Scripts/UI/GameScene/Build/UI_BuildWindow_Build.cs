using System;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 三级菜单
/// </summary>
public class UI_BuildWindow_Build : MonoBehaviour
{
    [SerializeField] private Button btnBuild;
    [SerializeField] private Transform buildItemPrefab;
    private List<UI_BuildWindow_BuildItem> visibleItemList=new List<UI_BuildWindow_BuildItem>();
    private UI_BuildWindow_SecondaryMenuWindow onwerWindow;
    [SerializeField] private Transform parent;
    [SerializeField] private Text txtDescriptionText;
    private BuildConfig buildConfig;
    public void Init(UI_BuildWindow_SecondaryMenuWindow onwerWindow)
    {
        btnBuild.onClick.AddListener(OnClick);
        this.onwerWindow = onwerWindow;
        Close();
    }
/// <summary>
/// 显示三级窗口
/// </summary>
/// <param name="buildConfig"></param>
    public void Show( BuildConfig buildConfig)
    {
        for (int i = 0; i < visibleItemList.Count; i++)
        {
            visibleItemList[i].JKGameObjectPushPool();
        }
        visibleItemList.Clear();
        gameObject.SetActive(true);
        List<BuildConfigCondition> buildConfigConditionList = buildConfig.buildList;
        for (int i = 0; i < buildConfigConditionList.Count; i++)
        {
           UI_BuildWindow_BuildItem buildItem= PoolManager.Instance.GetGameObject<UI_BuildWindow_BuildItem>(buildItemPrefab.gameObject,parent);
           int configID = buildConfigConditionList[i].configID;
           int currentCount = InventoryManager.Instance.GetMainSlotCount(configID);
           buildItem.UpdateView(configID, currentCount, buildConfigConditionList[i].count);
           visibleItemList.Add(buildItem);
        }

        this.buildConfig = buildConfig;
        btnBuild.interactable = buildConfig.CheckCondition();
        //如果是武器配置就调用item里面的物体配置
        if (this.buildConfig.buildType==BuildType.Weapon)
        {
            txtDescriptionText.text = ConfigManager.Instance.GetConfig<ItemConfig>(ConfigName.item, buildConfig.configID).Description;
        }
        //TODO：有可能是农作物还要加一层判断
        else
        {
            txtDescriptionText.text = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.mapOjbect, buildConfig.configID).Description;

        }
    }
    private void OnClick()
    {
        if (buildConfig.buildType==BuildType.Weapon)
        {
            if (InventoryManager.Instance.AddMainItemAndPlayAuio(buildConfig.configID))
            {
                bool isMeet=true;
                //对物体材料进行减去
                InventoryManager.Instance.UpdateMainItemForBuilds(buildConfig);
                RefreshView();
            }
            else
            {
                UIManager.Instance.AddTips("背包已装满");
            }
        }
        else
        {
            //进入建造模式
            onwerWindow.Close();
            EventManager.EventTrigger<BuildConfig>(EventName.BuildBuilding,buildConfig);
        }
        
    }

    public void RefreshView()
    {
        Show(buildConfig);
        onwerWindow.RefeshUpdateView();
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
