using System;
using System.Collections.Generic;
using JKFrame;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 怪物基类
/// </summary>
public abstract class AIBase : SerializedMonoBehaviour, IStateMachineOwner
{
    [SerializeField] private Animator animator;
    [SerializeField] NavMeshAgent navMeshAgent;
    [SerializeField] Collider inputCheckColler;
     public Collider InputCheckColler=>inputCheckColler;
    [SerializeField] private MapVertexType canMoveVertexType;
    [SerializeField] private float radius;
    [SerializeField] private Dictionary<AIState, AudioClip > audioClipDic = new Dictionary<AIState, AudioClip >();
    protected float hp;
    [SerializeField]  protected  float maxHp=5;
    [SerializeField] private float aiDistance;
      public  float AIDistance=>aiDistance;


    public int lootID=-1;
    public float Radius => radius;
    [SerializeField] private float attackValue = 10;
    public float AttackValue => attackValue;
    public Collider weapon;
    public NavMeshAgent NavMeshAgent => navMeshAgent;
    protected MapChunkController mapChunk; // 当前所在的地图块
    public MapChunkController MapChunk => mapChunk;
    protected StateMachine stateMachine;
    protected AIState currentAIState;
    protected MapObjectData aiData;
    public MapObjectData AIData=>aiData;

    private Dictionary<string, Action> animationActionsDic = new Dictionary<string, Action>();

    [SerializeField] protected float attackDistance;
    public float AttackDistance => attackDistance;
    protected StateMachine StateMachine
    {
        get
        {
            if (stateMachine == null)
            {
                stateMachine = PoolManager.Instance.GetObject<StateMachine>();
                stateMachine.Init(this);
            }

            return stateMachine;
        }
    }

    public virtual void Init(MapChunkController mapChunk, MapObjectData aiData)
    {
        this.mapChunk = mapChunk;
        this.aiData = aiData;
        transform.position = this.aiData.Position;
        hp=maxHp;
        ChangeState(AIState.Idle);
        EventManager.AddEventListener(EventName.SaveGame,OnSaveGame);
    }

    public virtual void InitOnTransfer(MapChunkController mapChunk)
    {
        this.mapChunk = mapChunk;
    }
/// <summary>
/// 更改怪物状态
/// </summary>
/// <param name="state"></param>
    public virtual void ChangeState(AIState state)
    {
        switch (state)
        {
            case AIState.Idle:
                StateMachine.ChangeState<AI_IdelState>((int)state);
                break;
            case AIState.Attack:
                StateMachine.ChangeState<AI_AttackState>((int)state);
                break;
            case AIState.Patrol:
                StateMachine.ChangeState<AI_PatrolState>((int)state);
                break;
            case AIState.Hurt:
                StateMachine.ChangeState<AI_HurtState>((int)state);
                break;
            case AIState.Dead:
                StateMachine.ChangeState<AI_DeadState>((int)state);
                break;
            case AIState.Pursue :
                StateMachine.ChangeState<AI_PursueState>((int)state);
                break;
        }

        currentAIState = state;
    }

    public void PlayAnimation(string animationName, float fixedTime = 0.25f)
    {
        animator.CrossFadeInFixedTime(animationName, fixedTime);
    }

    public Vector3 GetAIRandomPoint()
    {
        return mapChunk.GetAIObjectRandomPoint(canMoveVertexType);
    }

    public void SaveAIData()
    {
        aiData.Position = transform.position;
    }

    public void PlayAudio(AIState aiState, float volumeScale)
    {
        if (audioClipDic.TryGetValue(aiState,out AudioClip audioClip))
        {
            AudioManager.Instance.PlayOnShot(audioClip,this,volumeScale);
        }
    }

    public virtual void Hurt(float damg)
    {
        if (hp < 0) return;
        hp -= damg;
        if (hp>0)
        {
            ChangeState(AIState.Hurt);
        }
        else
        {
            //死亡
            ChangeState(AIState.Dead);
        }
    }

    public void AnimationAcitonEvent(string name)
    {
        if (animationActionsDic.TryGetValue(name,out Action action))
        {
            action?.Invoke();
        }
    }

    public void AddAnimationEvent(string eventName, Action eventAciton)
    {
        animationActionsDic[eventName] = eventAciton;
    }

    public void RemoveAnimationEvent(string evnetName)
    {
        animationActionsDic.Remove(evnetName);
    }


    public void AnimationAcitonClear()
    {
        animationActionsDic.Clear();
    }
    public void Destroy()
    {
        this.JKGameObjectPushPool();
        currentAIState = AIState.None;
        stateMachine.Stop();
    }

    public void OnSaveGame()
    {
        SaveAIData();
    }

    public void Dead()
    {
        MapChunk.RemoveAIObject(AIData.ID);
      
        if (lootID==-1)return;
        LootConfig lootConfig = ConfigManager.Instance.GetConfig<LootConfig>(ConfigName.loot, lootID);
        if (lootConfig==null)return;
        for (int i = 0; i < lootConfig.Configs.Count; i++)
        {
            int randomValue = UnityEngine.Random.Range(1, 101);
            if (randomValue<lootConfig.Configs[i].Probability)
            {
                Vector3 position = transform.position + Vector3.up * 0.1f;
                MapManager.Instance.SpawnMapObject(lootConfig.Configs[i].LootObjectConfigID,position,false);
            }
        }
    }
}