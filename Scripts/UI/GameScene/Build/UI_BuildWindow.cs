using System;
using JKFrame;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum BuildType
{
    Weapon,
    Building,
    Sow,
}
[UIElement(true,"UI/UI_BuildWindow",1)]
public class UI_BuildWindow : UI_WindowBase
{
    private UI_BuildWindow_MainMenuItem currentMainMenuItem;
  [SerializeField]  private UI_BuildWindow_MainMenuItem[] allUIBuildWindowMainMenuItems;
  private bool isTouch;
  [FormerlySerializedAs("uiSecondaryMenu")] [SerializeField]
  private UI_BuildWindow_SecondaryMenuWindow uiBuildWindowSecondaryMenu;
    public override void Init()
    {
        for (int i = 0; i < allUIBuildWindowMainMenuItems.Length; i++)
        {
            allUIBuildWindowMainMenuItems[i].Init((BuildType)i,this);
        }
        uiBuildWindowSecondaryMenu.Init();
        MonoManager.Instance.AddUpdateListener(OnUpdate);
    }

    private void OnUpdate()
    {
        if (isTouch&&
            !RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform ,Input.mousePosition))
        {
            CloseMenu();
            isTouch = false;
            currentMainMenuItem.OnUnSelect();
            currentMainMenuItem = null;
        }
    }

    private void OnDestroy()
    {
        MonoManager.Instance.RemoveUpdateListener(OnUpdate);
    }

    private void CloseMenu()
    {
        uiBuildWindowSecondaryMenu.Close();
    }

    public void SelectMainMenuItem(UI_BuildWindow_MainMenuItem selectMainMenuItem)
    {
        if (currentMainMenuItem!=null)  currentMainMenuItem.OnUnSelect();
        currentMainMenuItem = selectMainMenuItem;
        currentMainMenuItem.OnSelect();
        //开启二级窗口
        uiBuildWindowSecondaryMenu.Show(selectMainMenuItem.buildType);
        //更改开启检测鼠标位置关闭窗口
        isTouch = true;
    }
 
}
