using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionProjectile : MonoBehaviour
{
    EntityStats sender;
    Vector3 position;

    public void SetExplosion(EntityStats stats, Vector3 pos, float radius)
    {
        this.transform.localScale = new Vector3(radius, radius, radius);
        sender = stats;
        position = pos;
        StartCoroutine(SetPosition());
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy" || other.tag == "Player")
        {
            other.gameObject.GetComponent<EntityBehaviour>().ReceiveDamage(sender);
        }
    }

    IEnumerator SetPosition()
    {
        yield return new WaitForSeconds(0.3f);
        this.transform.position = position;
        yield return new WaitForSeconds(0.1f);
        Destroy(this);
    }
}
