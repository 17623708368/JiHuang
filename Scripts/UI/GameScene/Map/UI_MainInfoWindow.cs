using JKFrame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[UIElement(false, "UI/UI_MainInfoWindow", 1)]
public class UI_MainInfoWindow : UI_WindowBase
{
    [SerializeField] private Image imgHungry;
    [SerializeField] private Image imgBgHungry;
    [SerializeField] private Image imgHp;
    [SerializeField] private Image imgBgHp;
    [SerializeField] private Image imgSpirit;
    [SerializeField] private Image imgBgSpirit;
    [SerializeField] private Text txtdayNum;
    [SerializeField] private Image imgTimeState;
    private PlayerConfig playerConfig;

    protected override void RegisterEventListener()
    {
        base.RegisterEventListener();
        EventManager.AddEventListener<bool>(EventName.UpdateTimeState, UpdateTimeState);
        EventManager.AddEventListener<int>(EventName.UpdateDayNum, UpdateDayNum);
        EventManager.AddEventListener<float>(EventName.UpdatePlayerHP, UpdatePlayerHp);
        EventManager.AddEventListener<float>(EventName.UpdatePlayerHungry, UpdatePlayerHungry);
        EventManager.AddEventListener<float>(EventName.UpdatePlayerSpirit, UpdatePlayerSpirit);
    }


    protected override void CancelEventListener()
    {
        base.CancelEventListener();
        EventManager.RemoveEventListener<bool>(EventName.UpdateTimeState, UpdateTimeState);
        EventManager.RemoveEventListener<int>(EventName.UpdateDayNum, UpdateDayNum);
        EventManager.RemoveEventListener<float>(EventName.UpdatePlayerHP, UpdatePlayerHp);
        EventManager.RemoveEventListener<float>(EventName.UpdatePlayerHungry, UpdatePlayerHungry);
        EventManager.RemoveEventListener<float>(EventName.UpdatePlayerSpirit, UpdatePlayerSpirit);
    }

    private void UpdateTimeState(bool isSun)
    {
        imgTimeState.sprite = playerConfig.mainInfoSprptDic[StringNameType.TimeState][isSun == true ? 0 : 1];
    }

    private void UpdateDayNum(int num)
    {
        txtdayNum.text = "Day" + num.ToString();
    }

    public override void Init()
    {
        base.Init();
        playerConfig = ConfigManager.Instance.GetConfig<PlayerConfig>(ConfigName.player);
    }

    private void UpdatePlayerHp(float hp)
    {
        float radio = hp / playerConfig.MaxHp;
        imgHp.fillAmount = radio;
        int index = hp > 0
            ? (int)((1 - radio) * playerConfig.mainInfoSprptDic[StringNameType.Hp].Length)
            : playerConfig.mainInfoSprptDic[StringNameType.Hp].Length - 1;
        imgBgHp.sprite = playerConfig.mainInfoSprptDic[StringNameType.Hp][index];
    }

    private void UpdatePlayerHungry(float hungry)
    {
        float radio = hungry / playerConfig.MaxHungry;
        imgHungry.fillAmount = radio;
        int index = hungry > 0
            ? (int)((1 - radio) * playerConfig.mainInfoSprptDic[StringNameType.Hungry].Length)
            : playerConfig.mainInfoSprptDic[StringNameType.Hungry].Length - 1;
        imgBgHungry.sprite = playerConfig.mainInfoSprptDic[StringNameType.Hungry][index];
    }

    private void UpdatePlayerSpirit(float spirit)
    {
        float radio = spirit / playerConfig.MaxSpirit;
        imgSpirit.fillAmount = radio;
        int index = spirit > 0
            ? (int)((1 - radio) * playerConfig.mainInfoSprptDic[StringNameType.Spirit].Length)
            : playerConfig.mainInfoSprptDic[StringNameType.Spirit].Length - 1;
        imgBgSpirit.sprite = playerConfig.mainInfoSprptDic[StringNameType.Spirit][index];
        // Debug.Log((int)(1 - radio) * playerConfig.mainInfoSprptDic[StringName.Spirit].Length);
    }
}

public enum StringNameType
{
    Hungry,
    TimeState,
    Hp,
    Spirit,
}