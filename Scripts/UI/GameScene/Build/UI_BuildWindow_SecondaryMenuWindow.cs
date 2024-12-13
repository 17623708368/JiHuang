using System.Collections;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 二级主菜单
/// </summary>
public class UI_BuildWindow_SecondaryMenuWindow : MonoBehaviour
{
    private UI_BuildWindow_SecondaryMenuItem currentSecondaryMenuItem;
    private Dictionary<BuildType, List<BuildConfig>> allBuildConfigDic;
    private List<UI_BuildWindow_SecondaryMenuItem> secondaryMenuItemVisibleList;
    [SerializeField] private GameObject itemPrefab;

    [SerializeField] private Transform itemParent;

    //得到满足条件列表
    private List<BuildConfig> meetTheConditionList;

    //不满足条件的列表
    private List<BuildConfig> faileToMeetTheCondidtionList;

    //一级菜单父物体
    [SerializeField] private UI_BuildWindow_Build uiBuildWindowBuild;
    private BuildType currentBuildType;
    private BuildConfig currentBuildConfig;

    public void Init()
    {
        //对buildConfig进行按类型分类
        allBuildConfigDic = new Dictionary<BuildType, List<BuildConfig>>(3);
        allBuildConfigDic.Add(BuildType.Building, new List<BuildConfig>(10));
        allBuildConfigDic.Add(BuildType.Weapon, new List<BuildConfig>(10));
        allBuildConfigDic.Add(BuildType.Sow, new List<BuildConfig>(10));
        Dictionary<int, ConfigBase> buildConfigBases = ConfigManager.Instance.GetConfigs(ConfigName.build);
//对建造栏进行分类
        foreach (ConfigBase item in buildConfigBases.Values)
        {
            BuildConfig buildConfig = (BuildConfig)item;
            allBuildConfigDic[buildConfig.buildType].Add(buildConfig);
        }
        secondaryMenuItemVisibleList = new List<UI_BuildWindow_SecondaryMenuItem>(10);
        uiBuildWindowBuild.Init(this);
        Close();
    }

    /// <summary>
    /// 显示列表内容
    /// </summary>
    public void Show(BuildType buildType)
    {
        currentBuildType = buildType;
        for (int i = 0; i < secondaryMenuItemVisibleList.Count; i++)
        {
            secondaryMenuItemVisibleList[i].JKGameObjectPushPool();
        }

        secondaryMenuItemVisibleList.Clear();
        gameObject.SetActive(true);
        meetTheConditionList = new List<BuildConfig>(10);
        faileToMeetTheCondidtionList = new List<BuildConfig>(10);
        List<BuildConfig> buildConfigList = allBuildConfigDic[buildType];

        for (int i = 0; i < buildConfigList.Count; i++)
        {
            bool isOnChick = true;
            if (buildConfigList[i].scicenOnCick != null)
            {
                for (int j = 0; j < buildConfigList[i].scicenOnCick.Count; j++)
                {
                    if (!ScienceMachineManager.Instance.CheckScience(buildConfigList[i].scicenOnCick[j]))
                    {
                        isOnChick = false;
                        continue;
                    }

                    List<BuildingBase> allScience =
                        ScienceMachineManager.Instance.GetScienceObjectList(buildConfigList[i].scicenOnCick[j]);
                    if (allScience!=null)
                    {
                        for (int k = 0; k < allScience.Count; k++)
                        {
                            if (Vector3.Distance(Player_Controller.Instance.transform.position,allScience[k].transform.position)<allScience[k].TouchDinstance)
                            {
                                isOnChick = true;
                               break;
                            }
                            else
                            {
                                isOnChick = false;
                            }
                          
                        }     
                        
                    }

                   
                }
            }

            if (isOnChick)
            {
                if (buildConfigList[i].CheckCondition())
                {
                    meetTheConditionList.Add(buildConfigList[i]);
                }
                else
                    faileToMeetTheCondidtionList.Add(buildConfigList[i]);
            }

        }

        //显示成功列表
        for (int i = 0; i < meetTheConditionList.Count; i++)
        {
            secondaryMenuItemVisibleList.Add(AddSecondaryMenuItem(meetTheConditionList[i], true));
        }

        //显示失败列表
        for (int i = 0; i < faileToMeetTheCondidtionList.Count; i++)
        {
            secondaryMenuItemVisibleList.Add(AddSecondaryMenuItem(faileToMeetTheCondidtionList[i], false));
        }
        uiBuildWindowBuild.Close();
    }

    /// <summary>
    /// 添加二级菜单列表
    /// </summary>
    public UI_BuildWindow_SecondaryMenuItem AddSecondaryMenuItem(BuildConfig buildConfig, bool isMeet)
    {
        UI_BuildWindow_SecondaryMenuItem uiBuildWindowSecondaryMenuItem =
            PoolManager.Instance.GetGameObject<UI_BuildWindow_SecondaryMenuItem>(itemPrefab, itemParent);
        uiBuildWindowSecondaryMenuItem.Init(this, buildConfig);
        return uiBuildWindowSecondaryMenuItem;
    }

    public void RefeshUpdateView()
    {
        Show(currentBuildType);
        for (int i = 0; i < secondaryMenuItemVisibleList.Count; i++)
        {
            if (secondaryMenuItemVisibleList[i].buildConfig == currentSecondaryMenuItem.buildConfig)
            {
                SelecteSecondaryMenuItem(secondaryMenuItemVisibleList[i]);
            }
        }
    }

 
    /// <summary>
    ///选择的二级按钮
    /// </summary>
    public void SelecteSecondaryMenuItem(UI_BuildWindow_SecondaryMenuItem selectSecondaryMenuItem)
    {
        if (currentSecondaryMenuItem != null)
        {
            currentSecondaryMenuItem.UnSelect();
        }

        currentSecondaryMenuItem = selectSecondaryMenuItem;
        selectSecondaryMenuItem.Select();
        //开启三级窗口
        uiBuildWindowBuild.Show(selectSecondaryMenuItem.buildConfig);
    
    }

    public void Close()
    {
   
        uiBuildWindowBuild.Close();
        gameObject.SetActive(false);
    }
}