using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;

/// <summary>
/// 可受击地图对象基类
/// 提供被攻击时的逻辑处理，例如受伤和死亡
/// </summary>
public abstract class HitMapObjectBase : MapObjectBase
{
    [SerializeField] Animator animator; // 动画控制器，用于播放受击和死亡动画
    [SerializeField] AudioClip[] hurtAudioClips; // 受伤时播放的音效列表
    [SerializeField] float maxHp; // 最大生命值
    [SerializeField] int lootConfigID = -1; // 掉落配置的ID（-1表示无掉落）
    private float hp; // 当前生命值

    /// <summary>
    /// 初始化地图对象
    /// </summary>
    /// <param name="mapChunk">地图区块控制器</param>
    /// <param name="id">地图对象的唯一标识</param>
    public override void Init(MapChunkController mapChunk, ulong id,bool isFormBuilding)
    {
        base.Init(mapChunk, id,isFormBuilding);
        hp = maxHp; // 初始化生命值为最大生命值
    }

    /// <summary>
    /// 受伤处理
    /// </summary>
    /// <param name="damage">受到的伤害值</param>
    public void Hurt(float damage)
    {
        // 减少当前生命值
        hp -= damage;

        // 如果生命值降至0或以下，执行死亡逻辑
        if (hp <= 0)
        {
            Dead();
        }
        else
        {
            // 播放受伤动画
            animator.SetTrigger("Hurt");
        }

        // 播放受伤音效（随机选择一个音效）
        AudioManager.Instance.PlayOnShot(hurtAudioClips[Random.Range(0, hurtAudioClips.Length)], transform.position);
    }

    /// <summary>
    /// 死亡处理
    /// </summary>
    private void Dead()
    {
        // 从地图上移除该对象
        RemoveOnMap();

        // 如果没有掉落配置（lootConfigID为-1），直接返回
        if (lootConfigID == -1) return;

        // 获取掉落配置
        LootConfig lootConfig = ConfigManager.Instance.GetConfig<LootConfig>(ConfigName.loot, lootConfigID);
        if (lootConfig == null) return;

        // 根据掉落配置生成掉落物
        for (int i = 0; i < lootConfig.Configs.Count; i++)
        {
            // 按概率生成掉落物
            int randomValue = Random.Range(1, 101); // 生成1到100之间的随机数
            if (randomValue < lootConfig.Configs[i].Probability)
            {
                // 在对象上方生成掉落物
                Vector3 pos = transform.position + new Vector3(0, 1f, 0);
                MapManager.Instance.SpawnMapObject(mapChunk, lootConfig.Configs[i].LootObjectConfigID, pos,false);
            }
        }
    }
}
