using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class EntityStats
{
    public int maxHealth;
    [SerializeField] protected float Thealth;
    [SerializeField] protected ProjectileEffect Teffect;
    public float health { get { return Thealth; }
        set
        {
            Thealth = value;
            if (Thealth < 0)
                Thealth = -3;

            if (Thealth > maxHealth)
                Thealth = maxHealth;
        }
    }

    public ProjectileEffect effect
    {
        get { return Teffect; }
        set
        {
            Teffect = value;
            stats.projectileStats = BulletFactory.Define<BulletStats, ProjectileEffect>(value);
        }
    }

    public EntityStatsStorage stats;

    public EntityStats(int health, EntityStatsStorage stats)
    {
        this.health = health;
        this.stats = stats;
    }

    public virtual void ReceiveDamage(EntityStats sender, Vector3 hitPos)
    {
        int damageTaken = sender.stats.attack - stats.armor;
        damageTaken = Mathf.Max(damageTaken, 1);
        this.health -= damageTaken;

        if (sender.stats.projectileStats.onHit != null)
        {
            OnHitInfo info = new OnHitInfo(sender, this, hitPos);
            sender.stats.projectileStats.onHit.Invoke(info);
        }
    }
}

[System.Serializable]
public class EnemyStats : EntityStats
{
    public int attackRange;
    [HideInInspector] public int sqrRange;

    public EnemyStats(int health, EntityStatsStorage stats, int attackRange, int speed) : base (health, stats)
    {
        this.attackRange = attackRange;
        sqrRange = attackRange * attackRange;
    }

    public bool isInRange(Vector3 pos, Vector3 target)
    {
        if ((pos - target).sqrMagnitude < sqrRange)
            return true;

        return false;
    }
}

[System.Serializable]
public class PlayerStats : EntityStats
{
    public int magicAttackAdd;
    public float overloadDefence;
    public float regen;
    [SerializeField] private PlayerEntity owner;

    public PlayerStats(int health, EntityStatsStorage stats) : base(health, stats) { }

    public void ReceiveOverload(int amount) => owner.ReceiveOverload(amount);

    public void ReduceOverloadGain(int amount) => owner.ReduceOverloadGain(amount);

    public void MagicState(bool state) => owner.isMagicBlocked = state;

    public void BasicState(bool state) => owner.isBasicBlocked = state;
}

[Serializable]
public class EntityStatsStorage
{
    public int attack;
    public int armor;
    public float reload;
    public int speed;
    public float bulletSpeed;
    public BulletStats projectileStats;

    public EntityStatsStorage(int attack, int armor, float reload, float bulletSpeed, BulletStats stats)
    {
        this.attack = attack;
        this.armor = armor;
        this.reload = reload;
        this.bulletSpeed = bulletSpeed;
        this.projectileStats = stats;
    }
}

public abstract class EntityBehaviour : MonoBehaviour
{
    [SerializeField] protected GameObject basicBullet;
    [SerializeField] protected float bulletOffset = 3f;
    public bool isReloaded = true;

    protected void Shoot(EntityStats stats, GameObject bulletObj = null)
    {
        GameObject bullet = (bulletObj == null) ? basicBullet : bulletObj;
        GameObject projectile = GameObject.Instantiate(bullet, this.transform.position + this.transform.forward * bulletOffset, this.transform.rotation);
        projectile.GetComponent<ProjectileStorage>().AssignBullet(stats);
        StartCoroutine(Reload(stats));
    }

    protected IEnumerator Reload(EntityStats entity)
    {
        isReloaded = false;
        yield return new WaitForSeconds(entity.stats.reload);
        isReloaded = true;
    }

    public abstract void ReceiveDamage(EntityStats sender);
    public abstract void CheckHealth();
}