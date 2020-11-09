using UnityEngine;
using UnityEngine.EventSystems;
//using Manager;

using Photon.Pun;

using System.Collections;
using System.Diagnostics;

/// <summary>
/// Player manager.
/// Handles pushing and shoving.
/// </summary>

namespace Photon.Pun.Demo.PunBasics
{
    public class PlayerManager : MonoBehaviourPunCallbacks
    {
        #region Private Fields

        [Tooltip("The trigger volume from which you can push another player")]
        [SerializeField]
        private GameObject pushVolume;
        //True, when the user is pushing
        bool IsPushing;
        #endregion

        #region Public Fields

        [Tooltip("The current Health of our player")]
        public float Health = 1f;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            if (pushVolume == null)
            {
                UnityEngine.Debug.LogError("<Color=Red><a>Missing</a></Color> pushVolume Reference.", this);
            }
            else
            {
                pushVolume.SetActive(false);
            }

            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.IsMine)
            {
                PlayerManager.LocalPlayerInstance = this.gameObject;
            }
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(this.gameObject);
        }

        void Start()
        {
            CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();

            if (_cameraWork != null)
            {
                if (photonView.IsMine)
                {
                    _cameraWork.OnStartFollowing();
                }
                else
                {
                    UnityEngine.Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
                }
            }
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        void Update()
        {

            ProcessInputs();

            // trigger pushing active state
            if (pushVolume != null && IsPushing != pushVolume.activeInHierarchy)
            {
                pushVolume.SetActive(IsPushing);
            }

            if (Health <= 0f)
            {
                GameManager.Instance.LeaveRoom();
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

        #region Custom

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

        #endregion
    }
}