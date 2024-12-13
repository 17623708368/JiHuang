using System.Collections;
using System.Collections.Generic;
using JKFrame;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuildWindow_BuildItem : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private  Text  txticon;
 
    public void UpdateView(int configID,int currentCount,int maxCount)
    {
        ItemConfig itemConfig = ConfigManager.Instance.GetConfig<ItemConfig>(ConfigName.item, configID);
        iconImage.sprite = itemConfig.Icon;
        txticon.color = currentCount >= maxCount ? Color.white : Color.magenta;
        txticon.text = currentCount + "/" + maxCount;
    }

    
}
