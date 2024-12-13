using System;
using System.Collections;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuildWindow_SecondaryMenuItem : MonoBehaviour
{
    public BuildConfig buildConfig;
    [SerializeField] private Button btnSecondaryMenuItem;
    [SerializeField] private Image bgImage;
    [SerializeField] private Image iconImag;
    [SerializeField] private Sprite[] bgSprite;
    private UI_BuildWindow_SecondaryMenuWindow onwerWindow;

    public void Start()
    {
         UITool.BindMouseEffect(this);
         btnSecondaryMenuItem.onClick.AddListener(OnClick);
    }

    public void Init(UI_BuildWindow_SecondaryMenuWindow onwerWindow,BuildConfig buildConfig )
    {
        this.onwerWindow = onwerWindow;
        this.buildConfig = buildConfig;
        if (buildConfig.buildType==BuildType.Weapon)
        {
            iconImag.sprite = ConfigManager.Instance.GetConfig<ItemConfig>(ConfigName.item, buildConfig.configID).Icon;
        }
        else
        {
            iconImag.sprite  = ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.mapOjbect, buildConfig.configID).MapIconSprite;
        }
      
       iconImag.color =!buildConfig.CheckCondition()? Color.black:Color.white;
        UnSelect();
    }
    private void OnClick()
    {
        onwerWindow.SelecteSecondaryMenuItem(this);
    }

    public void Select()
    {
        bgImage.sprite = bgSprite[1];
    }

    public void UnSelect()
    {
        bgImage.sprite = bgSprite[0];

    }
}
