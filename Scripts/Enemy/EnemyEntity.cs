using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DeathAnimation { Sinking, Explosion };
public class EnemyEntity : EntityBehaviour
{
    [SerializeField] private EnemyStats entity;
    [SerializeField] private DeathAnimation deathAnim;

    Transform hitPointsBar;
    Rigidbody controller;
    Transform target;

    void Start()
    {
        entity.stats.projectileStats = BulletFactory.Define<BulletStats, ProjectileEffect>(entity.effect);
        target = GlobalLibrary.player.transform;
        controller = this.GetComponent<Rigidbody>();
        entity.sqrRange = entity.attackRange * entity.attackRange;

        CreateHitBar();
    }

    void Update()
    {
        Move();
        CheckHealth();
        Shoot();
    }

    void Move()
    {
        if (entity.stats.speed < 0)
            return;

        float yRot = Quaternion.LookRotation(target.position - this.transform.position, this.transform.up).eulerAngles.y;
        this.transform.rotation = Quaternion.Euler(0, yRot, 0);
        controller.AddForce(this.transform.forward * Time.deltaTime * entity.stats.speed);
    }

    void Shoot()
    {
        if (entity.isInRange(this.transform.position, target.position))
        {
            if (isReloaded)
                Shoot(entity);
        }
    }

    public override void CheckHealth()
    {
        hitPointsBar.transform.localScale = new Vector3(5 * (float)((float)entity.health / (float)entity.maxHealth), 0.5f, 0.5f) / 120f;

        if (entity.health <= 0)
            Death();
    }

    void Death()
    {
        GlobalLibrary.roundManager.UnsubscribeEnemy(this.gameObject);
        this.transform.tag = "Untagged";
        this.enabled = false;
        Destroy(hitPointsBar.gameObject);

        //Choose death animation
        switch(deathAnim)
        {
            case DeathAnimation.Sinking:
                this.GetComponent<BoxCollider>().enabled = false;
                GeneralFunctions.AddTimer(delegate () { Destroy(this.gameObject); }, 5f);
                break;
            case DeathAnimation.Explosion:
                GeneralFunctions.AddExplosionEffect(this.transform.position);
                Destroy(this.gameObject);
                break;
        }
    }

    void CreateHitBar()
    {
        GameObject hitBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hitBar.GetComponent<MeshRenderer>().material.color = Color.red;
        hitBar.transform.parent = this.transform;
        hitBar.transform.position = this.transform.position + new Vector3(0, 3, 0);
        hitPointsBar = hitBar.transform;
    }

    public override void ReceiveDamage(EntityStats sender) => entity.ReceiveDamage(sender, this.transform.position);
}