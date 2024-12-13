using JKFrame;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

/// <summary>
/// 地图物体类型
/// </summary>
public enum MapObjectType
{ 
    Tree,
    Stone,
    Bush,
    Consumable,
    Material,
    Weapon,
    Build,
}

/// <summary>
/// 地图对象基类
/// </summary>
public abstract class MapObjectBase : MonoBehaviour
{
    [SerializeField] MapObjectType objectType;
    public MapObjectType ObjectType { get => objectType; }

    // 交换距离
    [SerializeField] protected float touchDinstance;
    public float TouchDinstance { get => touchDinstance; }
    // 能否采摘
    [SerializeField] protected bool canPickUp;
    public bool CanPickUp { get => canPickUp;}

    [SerializeField] protected int pickUpItemConfigID = -1; // -1意味着无效
    public int PickUpItemConfigID { get => pickUpItemConfigID;}

    protected MapChunkController mapChunk;  // 当前所在的地图块
    protected ulong id;
    public ulong discardGameObjectID;

    public virtual void Init(MapChunkController mapChunk, ulong id,bool isFormBuilding)
    {
        this.mapChunk = mapChunk;
        this.id = id;
    }

  
    /// <summary>
    /// 从地图上移除
    /// </summary>
    public virtual void RemoveOnMap()
    {
        mapChunk.RemoveMapObject(id);
    }

    /// <summary>
    /// 当被捡起来
    /// </summary>
    public virtual int OnPickUp()
    {
        RemoveOnMap();  // 从地图上消失
        return pickUpItemConfigID;
    }

    #region 编辑器

    #if UNITY_EDITOR
    [Sirenix.OdinInspector.Button]
    public void AddNavMeshObstacle()
    {
       NavMeshObstacle  navMeshObstacle= transform.AddComponent<NavMeshObstacle>();
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (boxCollider!=null)
        {
            navMeshObstacle.shape = NavMeshObstacleShape.Box;
            navMeshObstacle.center = boxCollider.center;
            navMeshObstacle.size = boxCollider.size;
            navMeshObstacle.carving = true;
        }
        else if (capsuleCollider!=null)
        {
            navMeshObstacle.shape = NavMeshObstacleShape.Capsule;
            navMeshObstacle.center = capsuleCollider.center;
            navMeshObstacle.radius = capsuleCollider.radius;
            navMeshObstacle.height = capsuleCollider.height;
            navMeshObstacle.carving = true;
        }
    }
    #endif

    #endregion

}
