using UnityEngine;
using UnityEngine.EventSystems;
using Manager;

using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
//TODO CameraWork comes from PunBasics, maybe should be removed/changed later

using System.Collections;


/// <summary>
/// Player manager.
/// Handles pushing and shoving.
/// </summary>
public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
{
    #region IPunObservable implementation


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(IsPushing);
            stream.SendNext(Health);
        }
        else
        {
            // Network player, receive data
            this.IsPushing = (bool)stream.ReceiveNext();
            this.Health = (float)stream.ReceiveNext();
        }
    }


    #endregion


    #region Private Fields

    [Tooltip("The trigger volume from which you can push another player")]
    [SerializeField]
    private GameObject pushVolume;
    
    //True, when the user is pushing
    bool IsPushing;

    [Tooltip("The Following this player")]
    [SerializeField]
    CameraWork _cameraWork;
    #endregion


    #region Public Fields

    [Tooltip("The current Health of our player")]
    public float Health = 1f;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    [Tooltip("The Player's UI GameObject Prefab")]
    [SerializeField]
    public GameObject PlayerUiPrefab;

    #endregion

    #region MonoBehaviour CallBacks

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            PlayerManager.LocalPlayerInstance = this.gameObject;
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);

        if (pushVolume == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> pushVolume Reference.", this);
        }
        else
        {
            pushVolume.SetActive(false);
        }
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {
        CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();


        if (_cameraWork != null)
        {
            if (photonView.IsMine)
            {
                _cameraWork.OnStartFollowing();
            }
        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }

        if (PlayerUiPrefab != null)
        {
            GameObject _uiGo = Instantiate(PlayerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }
        else
        {
            Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
        }

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity on every frame.
    /// </summary>
    void Update()
    {

        if (photonView.IsMine)
        {
            ProcessInputs();
        }

        // trigger pushing active state
        if (pushVolume != null && IsPushing != pushVolume.activeInHierarchy)
        {
            pushVolume.SetActive(IsPushing);
        }

        if (Health <= 0f)
        {
            Manager.GameManager.Instance.LeaveRoom();
        }
    }

    /// <summary>
    /// MonoBehaviour method called when the Collider 'other' enters the trigger.
    /// Affect Health of the Player if the collider is a push
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }
        // We are only interested in pushes
        // we should be using tags but for the sake of distribution, let's simply check by name.
        if (!other.name.Contains("Push"))
        {
            return;
        }
        Health -= 0.1f;
    }
    /// <summary>
    /// MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
    /// We're going to affect health while the beams are touching the player
    /// </summary>
    /// <param name="other">Other.</param>
    void OnTriggerStay(Collider other)
    {
        // we dont' do anything if we are not the local player.
        if (!photonView.IsMine)
        {
            return;
        }
        // We are only interested in Beamers
        // we should be using tags but for the sake of distribution, let's simply check by name.
        if (!other.name.Contains("Push"))
        {
            return;
        }
        // we slowly affect health when beam is constantly hitting us, so player has to move to prevent death.
        Health -= 0.1f * Time.deltaTime;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Processes the inputs. Maintain a flag representing when the user is pressing Fire.
    /// </summary>
    void ProcessInputs()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (!IsPushing)
            {
                IsPushing = true;
            }
        }
        if (Input.GetButtonUp("Fire1"))
        {
            if (IsPushing)
            {
                IsPushing = false;
            }
        }
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
    {
        this.CalledOnLevelWasLoaded(scene.buildIndex);
    }

    void OnLevelWasLoaded(int level)
    {
        this.CalledOnLevelWasLoaded(level);
    }

    void CalledOnLevelWasLoaded(int level)
    {
        // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
        //Dont need to do this since we aren't reloading levels but I'm putting the callback in just in case
        //if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
        //{
        //    transform.position = new Vector3(0f, 5f, 0f);
        //}
        GameObject _uiGo = Instantiate(this.PlayerUiPrefab);
        _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
    }
    #endregion
}