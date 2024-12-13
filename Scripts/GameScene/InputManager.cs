using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using UnityEngine.EventSystems;
/// <summary>
/// 输入管理器
/// </summary>
public class InputManager : SingletonMono<InputManager>
{
    private List<RaycastResult> raycastResultList = new List<RaycastResult>();
    [SerializeField] LayerMask bigMapObjectLayer;       // 大型地图对象层
    [SerializeField] LayerMask mouseMapObjectLayer;     // 鼠标交互地图对象层
    [SerializeField] LayerMask groundLayer;             // 地面层
    [SerializeField] LayerMask buildLayer;             // 建筑层
 
    private bool wantCheck = false;                     // 需要检测
   
    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        SetCheckState(true);
    }
    private void Update()
    {
        if (GameSceneManager.Instance.IsInitialized == false) return;
        CheckSelectMapObject();
    }
    public void SetCheckState(bool wantCheck)
    {
        this.wantCheck = wantCheck;
    }

    /// <summary>
    /// 检查选中地图物体
    /// </summary>
    private void CheckSelectMapObject()
    {
        if (wantCheck == false) return;
        bool mouseButtonDown = Input.GetMouseButtonDown(0);
        bool mouseButton = Input.GetMouseButton(0);

        if (mouseButtonDown || mouseButton)
        {
            // 如果检测到UI则无视
            if (CheckMouseOnUI()) return;
            // 射线检测地图上的3D物体
            Ray ray = Camera_Controller.Instance.Camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray,out RaycastHit hitInfo,100,mouseMapObjectLayer))
            {
                // 发给Player_Controller去处理
                Player_Controller.Instance.OnSelectMapObject(hitInfo, mouseButtonDown);
            }

            if (mouseButtonDown&&Physics.Raycast(ray,out hitInfo,100,buildLayer))
            {
                BuildingBase buildingBase = hitInfo.collider.GetComponent<BuildingBase>();
                if (buildingBase.TouchDinstance > 0)
                {
                    if (Vector3.Distance(Player_Controller.Instance.transform.position,buildingBase.transform.position)<buildingBase.TouchDinstance)
                    {
                        buildingBase.OnSelect();
                    }
                }
            }
        }
    }

    /// <summary>
    /// 检查鼠标是否在UI上
    /// </summary>
    /// <returns></returns>
    public bool CheckMouseOnUI()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        // 射线去检测有没有除了Mask以外的任何UI物体
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);
        for (int i = 0; i < raycastResultList.Count; i++)
        {
            RaycastResult raycastResult = raycastResultList[i];
            // 是UI，同时还不是Mask作用的物体
            if (raycastResult.gameObject.TryGetComponent<RectTransform>(out var _temp1) && raycastResult.gameObject.name != "Mask")
            {
                raycastResultList.Clear();
                return true;
            }
        }
        raycastResultList.Clear();
        return false;
    }


    /// <summary>
    /// 检测鼠标是否在较大的地图对象上
    /// </summary>
    public bool CheckMouseOnBigMapObject()
    {
        return Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), 1000, bigMapObjectLayer);
    }

    /// <summary>
    /// 获取鼠标在地面上的世界坐标
    /// </summary>
    /// <returns></returns>
    public bool GetMouseWorldPositionOnGround(Vector3 mousePos,out Vector3 worldPos)
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(mousePos),out RaycastHit hitInfo,1000, groundLayer))
        {
            worldPos = hitInfo.point;
            return true;
        }
        worldPos = Vector3.zero;
        return false;
    }

    public bool CheckMouseOnBuildingObject( int itemID )
    {
        if (  Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out RaycastHit hitInfo ,1000, buildLayer))
        {
        BuildingBase    buildingBase = hitInfo.collider.GetComponent<BuildingBase>();
        return buildingBase.OnSlotEndDragSelect(itemID);
        }

        return false;
    }
}
