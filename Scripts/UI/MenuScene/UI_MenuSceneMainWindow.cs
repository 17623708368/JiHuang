using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JKFrame;
using System;

[UIElement(false, "UI/UI_MenuSceneMainWindow",1)]
public class UI_MenuSceneMainWindow : UI_WindowBase
{
    [SerializeField] Button new_Button;
    [SerializeField] Button continue_Button;
    [SerializeField] Button quit_Button;

    public override void Init()
    {
        new_Button.onClick.AddListener(NewGame);
        continue_Button.onClick.AddListener(ContinueGame);
        quit_Button.onClick.AddListener(Quit);

        new_Button.BindMouseEffect();
        continue_Button.BindMouseEffect();
        quit_Button.BindMouseEffect();
    }
    public override void OnClose()
    {
        base.OnClose();
        new_Button.RemoveMouseEffect();
        continue_Button.RemoveMouseEffect();
        quit_Button.RemoveMouseEffect();
    }
    public override void OnShow()
    {
        base.OnShow();
        // 当前是否要显示 继续游戏 按钮
        if (ArchiveManager.Instance.HaveArchive== false)
        {
            continue_Button.gameObject.SetActive(false);
        }
    }
    private void NewGame()
    {
        // 打开新游戏窗口
        UIManager.Instance.Show<UI_NewGameWindow>();
        Close();
    }
    private void ContinueGame()
    {
        // 基于之前的存档进行游戏
        GameManager.Instance.UseCurrentArchive_EnterGame();
        Close();
    }
    private void Quit()
    {
        Application.Quit();
    }



}
