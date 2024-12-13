using System;
using System.Collections;
using System.Collections.Generic;
using JKFrame;
using Unity.VisualScripting;
using UnityEngine;

public class BuildManager : SingletonMono<BuildManager>
{
    private Dictionary<string, IBuilding> buildPreView = new Dictionary<string, IBuilding>();
    [SerializeField]private float virtualCellSize;
    [SerializeField] private LayerMask buildLayerMask;
/// <summary>
/// 管理器初始化进行添加事件 和开启建造面板
/// </summary>
    public void Init()
    {
 
        UIManager.Instance.Show<UI_BuildWindow>();
        EventManager.AddEventListener<BuildConfig>(EventName.BuildBuilding,BuildGameObject);
    }

 

 
/// <summary>
/// 建造世界物体
/// </summary>
    private void BuildGameObject(BuildConfig buildConfig)
    {
        StartCoroutine(DoBuildGameObject(buildConfig));
    }

    IEnumerator DoBuildGameObject(BuildConfig buildConfig)
    {
        //禁止物体交互
        InputManager.Instance.SetCheckState(false);
        UIManager.Instance.SetRaycaster(false);
        //得到要建造的预制体预制体
        GameObject prefabe =
            ConfigManager.Instance.GetConfig<MapObjectConfig>(ConfigName.mapOjbect, buildConfig.configID).Prefab;
        //判断显示字典中有没有如果没有就开始实例化
        if (!buildPreView.TryGetValue(prefabe.name,out IBuilding preViewBuilding))
        {
            //实例化并得到脚本
            preViewBuilding =  Instantiate(prefabe).GetComponent<IBuilding>();
            //脚本初始化
            preViewBuilding.InitOnPreview();
            buildPreView.Add(prefabe.name,preViewBuilding);
        }
        else
        {
            preViewBuilding.GameObject.SetActive(true);
        }
        

        while (true) // 无限循环，实时更新物体位置
        {
            if (Input.GetMouseButtonDown(1))
            {
                preViewBuilding.GameObject.SetActive(false);
                UIManager.Instance.SetRaycaster(true);
                yield break; 
            }
            // 通过传入鼠标的屏幕坐标，获取其对应的世界坐标，并存储到 mouseWorldPos 中
            if (InputManager.Instance.GetMouseWorldPositionOnGround(Input.mousePosition, out Vector3 mouseWorldPos))
            {
                // 初始化一个新的变量，用于存储对齐到虚拟网格的坐标
                Vector3 VirtualCellMouse = mouseWorldPos;
        
                // 计算并对齐 X 轴坐标：
                // 将 X 坐标除以网格大小 virtualCellSize，得到相对于网格的比例值
                // 使用 Mathf.RoundToInt 对比例值四舍五入到最近的整数
                // 再乘以网格大小，得到对齐后的 X 轴坐标
                VirtualCellMouse.x = Mathf.RoundToInt(mouseWorldPos.x / virtualCellSize) * virtualCellSize;

                // 计算并对齐 Y 轴坐标（同理，Y 轴按网格对齐）
                VirtualCellMouse.y = Mathf.RoundToInt(mouseWorldPos.y / virtualCellSize) * virtualCellSize;

                // 更新建筑基座的世界坐标，将其移动到对齐后的网格点位置
                preViewBuilding.GameObject.transform.position = VirtualCellMouse;
            }
            //检测碰撞
            bool isCollider=true;
            if (preViewBuilding.Collider is BoxCollider)
            {
                BoxCollider buildCollider = (BoxCollider)preViewBuilding.Collider;
                isCollider = Physics.CheckBox(
                    buildCollider.transform.position + buildCollider.center,
                    buildCollider.size / 2,
                    buildCollider.transform.rotation,
                    buildLayerMask
                );
            }
            else if (preViewBuilding.Collider is CapsuleCollider )
            {
                CapsuleCollider buildCollider = (CapsuleCollider)preViewBuilding.Collider;

                // 计算 CapsuleCollider 的两个端点位置
                Vector3 startPoint = buildCollider.transform.position +
                                     buildCollider.center +
                                     buildCollider.transform.up * (buildCollider.height / 2 - buildCollider.radius);
                Vector3 endPoint = buildCollider.transform.position +
                                   buildCollider.center -
                                   buildCollider.transform.up * (buildCollider.height / 2 - buildCollider.radius);

                isCollider = Physics.CheckCapsule(
                    startPoint,
                    endPoint,
                    buildCollider.radius,
                    buildLayerMask
                );
            }
            else if (preViewBuilding.Collider is SphereCollider  buildCollider)
            {
                isCollider = Physics.CheckSphere(
                    buildCollider.transform.position + buildCollider.center,
                    buildCollider.radius,
                    buildLayerMask
                );
            }
         preViewBuilding.SetMaterialsColor(isCollider);
           //再次点击就建造物体减去材料
           //并把交互设置为true
           //点击右键取消建造模式
            //点击建造
           if (Input.GetMouseButtonDown(0)&&!isCollider)
           {
               //对跟随目标进行隐藏
               preViewBuilding.GameObject.SetActive(false);
               //设置ui检测
               UIManager.Instance.SetRaycaster(true);
               //设置鼠标交互
               InputManager.Instance.SetCheckState(true);
               //生成物体
               MapManager.Instance.SpawnMapObject(buildConfig.configID,preViewBuilding.GameObject.transform.position,true);
               //减去相应的材料
               InventoryManager.Instance.UpdateMainItemForBuilds(buildConfig);
               yield break; 
           }
            // 挂起当前协程，等待下一帧再执行，避免死循环并实现实时更新
            yield return null;
        }
    }
 
}
