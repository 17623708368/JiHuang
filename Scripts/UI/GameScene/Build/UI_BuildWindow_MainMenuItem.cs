using UnityEngine;
using UnityEngine.UI;

public class UI_BuildWindow_MainMenuItem : MonoBehaviour
{
    [SerializeField] private Image bgImg;
    public BuildType buildType;
    private UI_BuildWindow ownerWindow;
    [SerializeField] private Button btnMainMenuItem;
    [SerializeField] private Sprite[] bgSprites;

    public void Init(BuildType itemType, UI_BuildWindow ownerWindow)
    {
        this.buildType = itemType;
        this.ownerWindow = ownerWindow;
        btnMainMenuItem.onClick.AddListener(SelectMainMenuItem);
        OnUnSelect();
        UITool.BindMouseEffect(this);
    }

    private void SelectMainMenuItem()
    {
        ownerWindow.SelectMainMenuItem(this);
    }

    public void OnSelect()
    {
        bgImg.sprite = bgSprites[1];
    }

    public void OnUnSelect()
    {
        bgImg.sprite = bgSprites[0];
    }
}