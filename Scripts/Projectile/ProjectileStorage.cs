using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Special bullet type that have contol over player code
public class ProjectileStorage : MonoBehaviour
{
    [SerializeField] private bool isSeeking;
    [SerializeField] private bool isAntiEnemy;
    [SerializeField] private bool isAntiPlayer;
    [SerializeField] private GameObject onHitEffect;
    Transform target;

    EntityStats sender;
    float bulletSpeed;
    CharacterController controller;

    public void AssignBullet(EntityStats sender)
    {
        this.sender = sender;
        this.bulletSpeed = sender.stats.bulletSpeed;
        controller = this.GetComponent<CharacterController>();
        this.target = GlobalLibrary.player.transform;

        StartCoroutine(Dissapear());
    }

    private void Update()
    {
        if(isSeeking)
        {
            float yRot = Quaternion.LookRotation(target.position - this.transform.position).eulerAngles.y;
            this.transform.rotation = Quaternion.Euler(0, yRot, 0);
        }

        controller.Move(this.transform.forward * bulletSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision other)
    {
        bool isDesignedTarget = (isAntiPlayer && other.gameObject.tag == "Player") || (isAntiEnemy && other.gameObject.tag == "Enemy");
        if (isDesignedTarget)
        {
            other.gameObject.GetComponent<EntityBehaviour>().ReceiveDamage(sender);
            OnHit();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Obstacle")
        {
            OnHit();
        }
    }

    private void OnHit()
    {
        if(onHitEffect != null)
        {
            GameObject effect = GameObject.Instantiate(onHitEffect, this.transform.position, this.transform.rotation);
            GeneralFunctions.AddTimer(delegate () { Destroy(effect); }, 3f);
        }
        Destroy(this.gameObject);
    }

    IEnumerator Dissapear()
    {
        yield return new WaitForSeconds(5f);
        Destroy(this.gameObject);
    }
}