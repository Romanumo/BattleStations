using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{
    Transform player;
    void Start()
    {
        player = GlobalLibrary.player.transform;
    }

    void Update()
    {
        this.transform.position = new Vector3(0, 23f, 0) + player.transform.position;
    }
}
