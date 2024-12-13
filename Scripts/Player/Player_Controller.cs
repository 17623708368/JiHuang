using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using System;
using Unity.VisualScripting;
using StateMachine = JKFrame.StateMachine;

/// <summary>
/// ���״̬
/// </summary>
public enum PlayerState
{ 
    Idle,
    Move,
    Attack,
    Hurt,
    PickUp,
    Dead
}
/// <summary>
/// ��ҿ�����
/// </summary>
public class Player_Controller : SingletonMono<Player_Controller>,IStateMachineOwner
{
    [SerializeField] Player_Model player_Model;
    [SerializeField] Animator animator;
    [SerializeField] private Collider collider;
      public Collider Collider;
    public CharacterController characterController;
    public bool atteckDir;
    private StateMachine stateMachine;
    public Transform playerTransform { get; private set; }

    private PlayerConfig playerConfig;
    public float rotateSpeed { get=> playerConfig.RotateSpeed; } 
    public float moveSpeed { get=> playerConfig.MoveSpeed;  }
    public Vector2 positionXScope { get; private set; }// X�ķ�Χ
    public Vector2 positionZScope { get; private set; }// Z�ķ�Χ
    public bool CanUseItem { get; private set; } = true;    // ��ǰ�Ƿ����ʹ����Ʒ����������Ʒ������
    

    #region �浵��ص�����
    private PlayerTransformData playerTransformData;
    private PlayerMainData playerMainData;
    #endregion

    #region ��ʼ��
    public void Init(float mapSizeOnWorld)
    {
        // ȷ������
        playerConfig = ConfigManager.Instance.GetConfig<PlayerConfig>(ConfigName.player);
        // ȷ���浵
        playerTransformData = ArchiveManager.Instance.PlayerTransformData;
        playerMainData = ArchiveManager.Instance.PlayerMainData;

        player_Model.Init(PlayAudioOnFootstep,OnStartHit,OnStopHit,OnAttackOver,HurtOver,DeadOver);
        playerTransform = transform;

        stateMachine = ResManager.Load<StateMachine>();
        stateMachine.Init(this);
        // ��ʼ״̬Ϊ����
        stateMachine.ChangeState<Player_Idle>((int)PlayerState.Idle);
        InitPositionScope(mapSizeOnWorld);

        // ��ʼ���浵��ص�����
        playerTransform.localPosition = playerTransformData.Position;
        playerTransform.localRotation = Quaternion.Euler(playerTransformData.Rotation);

        // ������ʼ������¼�
        TriggerUpdateHPEvent();
        TriggerUpdateHungryEvent();
        TriggerUpdateSpiritEvent();
        EventManager.AddEventListener(EventName.SaveGame, OnGameSave);
    }



    // ��ʼ�����귶Χ
    private void InitPositionScope(float mapSizeOnWorld)
    {
        positionXScope = new Vector2(1, mapSizeOnWorld - 1);
        positionZScope = new Vector2(1, mapSizeOnWorld - 1);
    }
    #endregion

    #region ����/���ߺ���
    private void PlayAudioOnFootstep(int index)
    {
        AudioManager.Instance.PlayOnShot(playerConfig.FootstepAudioClis[index], playerTransform.position, 0.5f);
    }
    /// <summary>
    /// �޸�״̬
    /// </summary>
    public void ChangeState(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Idle:
                CanUseItem = true;
                stateMachine.ChangeState<Player_Idle>((int)playerState);
                break;
            case PlayerState.Move:
                CanUseItem = true;
                stateMachine.ChangeState<Player_Move>((int)playerState);
                break;
            case PlayerState.Attack:
                CanUseItem = false;
                 stateMachine.ChangeState<Player_Attack>((int)playerState);
                break;
            case PlayerState.PickUp:
                CanUseItem = false;
                stateMachine.ChangeState<PlayerState_PickUP>((int)playerState);
                break;
            case PlayerState.Hurt:
                CanUseItem = false;
                stateMachine.ChangeState<Player_Hurt>((int)playerState);
                break;
            case PlayerState.Dead:
                CanUseItem = false;
                stateMachine.ChangeState<Player_Dead>((int)playerState);
                break;
        }
    }

    /// <summary>
    /// ���Ŷ���
    /// </summary>
    public void PlayAnimation(string animationName, float fixedTime = 0.25f)
    {
        animator.CrossFadeInFixedTime(animationName, fixedTime);
    }

    #endregion

    public bool isSpirtOnZero=false;
    #region ������ֵ
    private void CalulateHungryOnUpdate()
    {
        // �����Ҽ���ֵ������
        if (playerMainData.Hungry > 0)
        {
            playerMainData.Hungry -= Time.deltaTime * playerConfig.HungryReduceSeed;
            if (playerMainData.Hungry <= 0) playerMainData.Hungry = 0;
            TriggerUpdateHungryEvent();
        }
        else
        {
            //��Ҿ���ֵ
            if (playerMainData.Spirit > 0)
            {
                isSpirtOnZero = false;
                playerMainData.Spirit -= Time.deltaTime * playerConfig.spiritDampSpeedOnHungryDampIsZero;
                if (playerMainData.Spirit <= 0) playerMainData.Spirit = 0;
                TriggerUpdateSpiritEvent();
            }
            else
            {
                isSpirtOnZero = true;
                //TODO��������ζ�
                //ͨ������ʵ��Ч��
                TriggerUpdateSpiritEvent();
            }

            if (playerMainData.Hp > 0)
            {
                playerMainData.Hp -= Time.deltaTime * (isSpirtOnZero == true
                    ? playerConfig.HpReduceSpeedOnHungryIsZero * 2
                    : playerConfig.HpReduceSpeedOnHungryIsZero);
                if (playerMainData.Hp <= 0) playerMainData.Hp = 0;
                TriggerUpdateHPEvent();
            }
            else
            {
                 ChangeState(PlayerState.Dead);
                TriggerUpdateHPEvent();
            }
        }
    }
    private void TriggerUpdateHPEvent()
    {
        EventManager.EventTrigger(EventName.UpdatePlayerHP, playerMainData.Hp);
    }
    private void TriggerUpdateHungryEvent()
    {
        EventManager.EventTrigger(EventName.UpdatePlayerHungry, playerMainData.Hungry);

    }

    private void TriggerUpdateSpiritEvent()
    {
        EventManager.EventTrigger(EventName.UpdatePlayerSpirit, playerMainData.Spirit);
    }
    /// <summary>
    /// �ָ�����ֵ
    /// </summary>
    public void RecoverHP(float value)
    {
        playerMainData.Hp = Mathf.Clamp(playerMainData.Hp + value, 0, playerConfig.MaxHp);
        TriggerUpdateHPEvent();
    }

    /// <summary>
    /// �ָ�����ֵ
    /// </summary>
    public void RecoverHungry(float value)
    {
        playerMainData.Hungry = Mathf.Clamp(playerMainData.Hungry + value, 0, playerConfig.MaxHungry);
        TriggerUpdateHungryEvent();
    } 
    public void RecoverSripte(float value)
    {
        playerMainData.Spirit = Mathf.Clamp(playerMainData.Spirit+ value, 0, playerConfig.MaxSpirit);
        TriggerUpdateSpiritEvent();
    }
    #endregion

    #region �������
    private ItemData currentWeaponItemData;
    private GameObject currentWeaponGameObject;
    private bool isCurrentWeaponTorch=false;

    /// <summary>
    /// �޸�����
    /// </summary>
    public void ChangeWeapon(ItemData newWeapon)
    {
        // ѹ��û�л�����
        if (currentWeaponItemData == newWeapon)
        {
            currentWeaponItemData = newWeapon;
            return;
        }

        // ��������������ݣ��Ѿ�����ģ�ͻ��յ������
        if (currentWeaponItemData!=null)
        {
            // ���������ʱ���ǻ���GameObject.name�ģ����Բ���ͬ��
            currentWeaponGameObject.JKGameObjectPushPool(); 
        }

        // �������������Null
        if (newWeapon!=null)
        {
            ItemWeaponInfo newWeaponInfo = newWeapon.Config.ItemTypeInfo as ItemWeaponInfo;
            currentWeaponGameObject = PoolManager.Instance.GetGameObject(newWeaponInfo.PrefabOnPlayer,player_Model.WeaponRoot);
            currentWeaponGameObject.transform.localPosition = newWeaponInfo.PositionOnPlayer;
            currentWeaponGameObject.transform.localRotation = Quaternion.Euler(newWeaponInfo.RotationOnPlayer);
            animator.runtimeAnimatorController = newWeaponInfo.AnimatorController;
        }
        // ��������Null����ζ�ſ���
        else
        {
            animator.runtimeAnimatorController = playerConfig.NormalAnimatorController;
        }
        // ���ڶ������߼�״̬������ ��������¼���һ�ζ�������������󣨱������ƶ��У�ͻȻ�л�AnimatorController�᲻������·������
        stateMachine.ChangeState<Player_Idle>((int)PlayerState.Idle, true);
        currentWeaponItemData = newWeapon;
        if (currentWeaponItemData!=null&&(currentWeaponItemData.Config.ItemTypeInfo as ItemWeaponInfo).WeaponType == WeaponType.Torch)
        {
            isCurrentWeaponTorch = true;
            timeScale = TimeManager.Instance.timeScale;
        }
    }

    #endregion

    #region ս������ľ��ժ
    private bool canAttack = true;
    public Quaternion attackDir { get; private set; }
    private List<MapObjectBase> lastAttackedMaoObjectList = new List<MapObjectBase>();

    // ��󹥻��ĵ�ͼ����
    private MapObjectBase lastHitMapObject;
    /// <summary>
    /// ��ѡ���ͼ����ʱ
    /// </summary>
    public void OnSelectMapObject(RaycastHit hitInfo,bool isMouseButtonDown)
    {
        if (hitInfo.collider.TryGetComponent<MapObjectBase>(out MapObjectBase mapObject))
        {
            float dis = Vector3.Distance(playerTransform.position, mapObject.transform.position);
            // ˵�����ڽ�����Χ��
            if (dis > mapObject.TouchDinstance)
            {
                if (isMouseButtonDown)
                {
                    UIManager.Instance.AddTips("���һ��Ŷ��");
                    ProjectTool.PlayAudio(AudioType.Fail);
                }
                return;
            }
            // �ж�ʰȡ
            if (mapObject.CanPickUp)
            {
                if (!isMouseButtonDown) return;
                lastHitMapObject = null;
                // ��ȡ�񵽵���Ʒ
                int itemConfigID = mapObject.PickUpItemConfigID;
                if (itemConfigID != -1)
                {
                    // �����������������ӳɹ��������ٵ�ͼ����
                    if (InventoryManager.Instance.AddMainItem(itemConfigID,mapObject.discardGameObjectID))
                    {
                        mapObject.OnPickUp();
                        // ���ż��������Ķ��� ����û���л�״̬����Ȼ��Idle״̬
                        ChangeState(PlayerState.PickUp);
                        ProjectTool.PlayAudio(AudioType.Bag);
                    }
                    else
                    {
                        if (isMouseButtonDown)
                        {
                            UIManager.Instance.AddTips("�����Ѿ����ˣ�");
                            ProjectTool.PlayAudio(AudioType.Fail);
                        }
                    }
                }
                return;
            }

            
            if (!canAttack) return;
            // ���ڽ����Ķ�������һ�������¶���ֻ������굥��ʱ������������ظ���һ��������в������������ְ���״̬���н���
            if (lastHitMapObject != mapObject && !isMouseButtonDown) return;
            lastHitMapObject = mapObject;
            // �жϹ���
            // �������ѡ�еĵ�ͼ���������Լ���ǰ��ɫ���������ж���ʲô
            switch (mapObject.ObjectType)
            {
                case MapObjectType.Tree:
                    if (!CheckHitMapObject(mapObject, WeaponType.Axe) && isMouseButtonDown)
                    {
                        UIManager.Instance.AddTips("����Ҫװ����ͷ��");
                        ProjectTool.PlayAudio(AudioType.Fail);
                    }
                    break;
                case MapObjectType.Stone:
                    if (!CheckHitMapObject(mapObject, WeaponType.PickAxe) && isMouseButtonDown)
                    {
                        UIManager.Instance.AddTips("����Ҫװ���䣡");
                        ProjectTool.PlayAudio(AudioType.Fail);
                    }
                    break;
                case MapObjectType.Bush:
                    if (!CheckHitMapObject(mapObject, WeaponType.Sickle) && isMouseButtonDown)
                    {
                        UIManager.Instance.AddTips("����Ҫװ��������");
                        ProjectTool.PlayAudio(AudioType.Fail);
                    }
                    break;
            }
            return;
        }
        if (currentWeaponItemData!=null&&hitInfo.collider.TryGetComponent<AIBase>(out AIBase aiObject))
        {
            float dis = Vector3.Distance(playerTransform.position, aiObject.transform.position);
            if (dis<aiObject.Radius+ (currentWeaponItemData.Config.ItemTypeInfo as ItemWeaponInfo).AttackRadius)
            {
                // ��ֹ�����ֽ��й���
                canAttack = false;
                // ���㷽��
                attackDir = Quaternion.LookRotation(aiObject.transform.position - transform.position);
                // �л�״̬
                ChangeState(PlayerState.Attack);
            
             
            }
        }
        
    }

    // �������ͼ����
    public bool CheckHitMapObject(MapObjectBase mapObject, WeaponType weaponType)
    {
        // ������������������Ҫ��
        if (currentWeaponItemData != null
          && (currentWeaponItemData.Config.ItemTypeInfo as ItemWeaponInfo).WeaponType == weaponType)
        {
            // ��ֹ�����ֽ��й���
            canAttack = false;
            // ���㷽��
            attackDir = Quaternion.LookRotation(mapObject.transform.position - transform.position);
            // �л�״̬
            ChangeState(PlayerState.Attack);

            return true;
        }
        return false;
    }

    // �����������˺����
    private void OnStartHit()
    {
        attackSucceedCount = 0;
        currentWeaponGameObject.transform.OnTriggerEnter(OnWeaponTriggerEnter);
    }

    // ������ֹͣ�˺����
    private void OnStopHit()
    {
        currentWeaponGameObject.transform.RemoveTriggerEnter(OnWeaponTriggerEnter);
        lastAttackedMaoObjectList.Clear();
    }

    // ��������״̬�Ľ���
    private void OnAttackOver()
    {
        // �ɹ����й����Σ������ļ����;ö�
        for (int i = 0; i < attackSucceedCount; i++)
        {
            // ����������
            EventManager.EventTrigger(EventName.PlayerWeaponAttackSucceed);
        }

        canAttack = true;
        // �л�״̬������
        ChangeState(PlayerState.Idle);

    }
    // �����ɹ�������
    private int attackSucceedCount;
    // ����������������Ϸ����ʱ
    private void OnWeaponTriggerEnter(Collider other, object[] arg2)
    {
        // �Է����ǵ�ͼ�����������
        if (other.TryGetComponent<HitMapObjectBase>(out HitMapObjectBase mapObject))
        {
            // �Ѿ��������ģ���ֹ�����˺�
            if (lastAttackedMaoObjectList.Contains(mapObject)) return;
            lastAttackedMaoObjectList.Add(mapObject);
            // ���Է���ʲô���� �Լ� �Լ�������ʲô����
            switch (mapObject.ObjectType)
            {
                case MapObjectType.Tree:
                    CheckMapObjectHurt(mapObject, WeaponType.Axe);
                    break;
                case MapObjectType.Stone:
                    CheckMapObjectHurt(mapObject, WeaponType.PickAxe);
                    break;
                case MapObjectType.Bush:
                    CheckMapObjectHurt(mapObject, WeaponType.Sickle);
                    break;
            }
        }
        else if (other.TryGetComponent<AIBase>(out AIBase aiObject))
        {
            ItemWeaponInfo itemWeaponInfo = currentWeaponItemData.Config.ItemTypeInfo as ItemWeaponInfo;
             attackSucceedCount++;
            aiObject.Hurt(itemWeaponInfo.AttackValue);
            if ((currentWeaponItemData.Config.ItemTypeInfo as ItemWeaponInfo).effect!=null)
            {
                GameObject effect = PoolManager.Instance.GetGameObject((currentWeaponItemData.Config.ItemTypeInfo as ItemWeaponInfo).effect);
                effect.transform.position = other.ClosestPoint(currentWeaponGameObject.transform.position);
            }
          
        }
    }

    /// <summary>
    /// ����ͼ�����ܷ�����
    /// </summary>
    private void CheckMapObjectHurt(HitMapObjectBase hitMapObject, WeaponType weaponType)
    {
        ItemWeaponInfo itemWeaponInfo = currentWeaponItemData.Config.ItemTypeInfo as ItemWeaponInfo;
        if (itemWeaponInfo.WeaponType == weaponType)
        {
            // ��������
            hitMapObject.Hurt(itemWeaponInfo.AttackValue);
            attackSucceedCount += 1;
        }
    }


    #endregion

    private float timer;
    private float maxTime=3;
    private float timeScale;
    private void Update()
    {
        if (GameSceneManager.Instance.IsInitialized == false) return;
        CalulateHungryOnUpdate();
        LightTorch();
    }
/// <summary>
/// ʹ�û��
/// </summary>
    private void LightTorch()
    {
        if (isCurrentWeaponTorch)
        {
            if (currentWeaponItemData == null||(currentWeaponItemData.Config.ItemTypeInfo as ItemWeaponInfo).WeaponType != WeaponType.Torch)
            {
                isCurrentWeaponTorch = false;
                return;
            }

            timer += Time.deltaTime * TimeManager.Instance.timeScale;
            if (timer>maxTime)
            {
                timer = 0;
                EventManager.EventTrigger(EventName.PlayerWeaponAttackSucceed);
            }
        }
    }
    private void OnGameSave()
    {
        // �Ѵ浵����ʵ��д�����
        playerTransformData.Position = playerTransform.localPosition;
        playerTransformData.Rotation = playerTransform.localRotation.eulerAngles;
        ArchiveManager.Instance.SavePlayerTransformData();
        ArchiveManager.Instance.SavePlayerMainData();
    }

    public void Hurt(float value)
    {
        if (playerMainData.Hp <= 0) return;
        playerMainData.Hp -= value;
        ChangeState(playerMainData.Hp<=0? PlayerState.Dead:PlayerState.Hurt);
     
    }
    private void HurtOver()
    {
        TriggerUpdateHPEvent();
        ChangeState(PlayerState.Idle);
    }
    private void DeadOver()
    {
        GameSceneManager.Instance.GameOver();
    }

    public void OnDestroy()
    {
        if (stateMachine!=null)
        {
            stateMachine.Destory();
            stateMachine = null;
        }
    }
}
