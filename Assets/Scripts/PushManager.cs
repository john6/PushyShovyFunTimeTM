using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.Demo.PunBasics;

public class PushManager : MonoBehaviour
{

    [Tooltip("The trigger volume from which you can push another player")]
    [SerializeField]
    private SphereCollider pushCollider;

    [Tooltip("The trigger volume from which you can push another player")]
    [SerializeField]
    private PlayerManager pManager;

    // Start is called before the first frame update
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

    // Update is called once per frame
    void Update()
    {
        
    }


    //void OnCollisionEnter(Collision collision)
    //{
    //    UnityEngine.Debug.Log("There was a collision");
    //    if ((collision.gameObject.tag == "Player") && pManager.IsPushing)
    //    {
    //        UnityEngine.Debug.Log("I push another player");
    //        pManager.SendPushAction(collision);
    //    }
    //}

    void OnTriggerEnter(Collider other)
    {
        //UnityEngine.Debug.Log("There was a collision");
        if ((other.gameObject.tag == "Player") && pManager.IsPushing)
        {
            UnityEngine.Debug.Log("I push another player");
            pManager.SendPushAction(other);
        }
    }
}
