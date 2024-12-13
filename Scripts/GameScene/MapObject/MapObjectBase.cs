using JKFrame;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

/// <summary>
/// ��ͼ��������
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
/// ��ͼ�������
/// </summary>
public abstract class MapObjectBase : MonoBehaviour
{
    [SerializeField] MapObjectType objectType;
    public MapObjectType ObjectType { get => objectType; }

    // ��������
    [SerializeField] protected float touchDinstance;
    public float TouchDinstance { get => touchDinstance; }
    // �ܷ��ժ
    [SerializeField] protected bool canPickUp;
    public bool CanPickUp { get => canPickUp;}

    [SerializeField] protected int pickUpItemConfigID = -1; // -1��ζ����Ч
    public int PickUpItemConfigID { get => pickUpItemConfigID;}

    protected MapChunkController mapChunk;  // ��ǰ���ڵĵ�ͼ��
    protected ulong id;
    public ulong discardGameObjectID;

    public virtual void Init(MapChunkController mapChunk, ulong id,bool isFormBuilding)
    {
        this.mapChunk = mapChunk;
        this.id = id;
    }

  
    /// <summary>
    /// �ӵ�ͼ���Ƴ�
    /// </summary>
    public virtual void RemoveOnMap()
    {
        mapChunk.RemoveMapObject(id);
    }

    /// <summary>
    /// ����������
    /// </summary>
    public virtual int OnPickUp()
    {
        RemoveOnMap();  // �ӵ�ͼ����ʧ
        return pickUpItemConfigID;
    }

    #region �༭��

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
