    using System.Collections.Generic;
    using JKFrame;
    using Sirenix.OdinInspector;
    using UnityEngine;
[CreateAssetMenu(menuName = "Config/建造配置",fileName = "建造配置")]
    public class BuildConfig:ConfigBase
    {
        [LabelText("类型")] public BuildType buildType;
        [LabelText("建造条件")] public List<BuildConfigCondition> buildList=new List<BuildConfigCondition>();
        [LabelText("建造出来的物品id")] public int configID;
        [LabelText(" 解锁科技条件)")] public List<int> scicenOnCick;

        public bool CheckCondition()
        {
            for (int j = 0; j < buildList.Count; j++)
            {
                int currentOjbectCount = InventoryManager.Instance.GetMainSlotCount(buildList[j].configID);
                if (currentOjbectCount<buildList[j].count)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class BuildConfigCondition
    {
        [LabelText("物品ID"),HorizontalGroup()] public int configID;
        [LabelText("物品数量"),HorizontalGroup()] public int count;
    }
