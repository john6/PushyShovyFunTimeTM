using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Demo.PunBasics;

public class PushManager : MonoBehaviour
{

    [Tooltip("The trigger volume from which you can push another player")]
    [SerializeField]
    private SphereCollider pushCollider;

    [Tooltip("The player manager script that owns this push volume")]
    [SerializeField]
    private PlayerManager pManager;

    void Start()
    {
        if (pushCollider == null)
        {
            pushCollider = GetComponent<SphereCollider>();
        }

        if (pManager == null)
        {
            pManager = GetComponentInParent<PlayerManager>();
        }
    }

    //void Update()
    //{
    //}

    void OnTriggerEnter(Collider other)
    {
        Push(other);
    }

    void OnTriggerStay(Collider other)
    {
        Push(other);
    }

    void Push(Collider other)
    {
        if ((other.gameObject.tag == "Player") && pManager.IsPushing)
        {
            UnityEngine.Debug.Log("I push another player");
            pManager.PushOtherPlayer(other);
        }
    }


}
