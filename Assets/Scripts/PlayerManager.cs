using UnityEngine;
using UnityEngine.EventSystems;
//using Manager;

using Photon.Pun;

using System.Collections;
using System.Diagnostics;
using System.Threading;

/// <summary>
/// Player manager.
/// Handles pushing and shoving.
/// </summary>

namespace Photon.Pun.Demo.PunBasics
{
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Private Fields

        //[Tooltip("The trigger volume from which you can push another player")]
        //[SerializeField]
        //private GameObject pushVolume;

        [Tooltip("The RigidBody attached to this GameObject")]
        [SerializeField]
        public Rigidbody rb;
        #endregion

        #region Public Fields

        [Tooltip("The current Health of our player")]
        public float Health = 1f;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        [Range(0.0f, 25.0f)]
        public float maxSpeed;

        [Range(0.0f, 5000.0f)]
        public float Speed;

        [Range(0.0f, 500.0f)]
        public float jumpSpeed;

        [Tooltip("true during the frame that a character inputs the push command")]
        public bool IsPushing;

        [Tooltip("true during the frame that a character inputs the push command")]
        public bool IsGrabbing;

        private bool isJumping = false;

        private float distToGround;

        private Vector2 playerInput;


        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            //if (pushVolume == null)
            //{
            //    UnityEngine.Debug.LogError("<Color=Red><a>Missing</a></Color> pushVolume Reference.", this);
            //}
            //else
            //{
            //    pushVolume.SetActive(false);
            //}

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

            distToGround = GetComponent<Collider>().bounds.extents.y;
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        void Update()
        {
            // trigger pushing active state
            //if (pushVolume != null && IsPushing != pushVolume.activeInHierarchy)
            //{
            //    pushVolume.SetActive(IsPushing);
            //}

            ProcessInputs();


        }

        void FixedUpdate()
        {
            if (photonView.IsMine && IsGrounded())
            {
                Move(playerInput);
            }
        }

        //public void SendPushAction(Collision collision)
        //{
        //    UnityEngine.Debug.Log("I am sending the push RPC");
        //    Vector3 direction = collision.gameObject.transform.position - transform.position;
        //    Vector3 impulse = direction * 500.0f;
        //    PhotonView otherView = collision.gameObject.GetComponent<PhotonView>();
        //    Photon.Realtime.Player otherplayer = otherView.Owner;
        //    //photonView.RPC("RpcWithObjectArray", RpcTarget.All, impulse, otherplayer);
        //}

        public void SendPushAction(Collider other)
        {
            if (photonView.IsMine)
            {
                Vector3 direction = transform.position - other.gameObject.transform.position;
                Vector3 impulse = direction * 50000.0f;
                PhotonView otherView = other.gameObject.GetComponent<PhotonView>();
                int otherplayerNum = otherView.ControllerActorNr;
                UnityEngine.Debug.Log("I am sending the push RPC to player " + otherplayerNum);
                //int myPlayerNum = gameObject.GetComponent<PhotonView>().OwnerActorNr;
                gameObject.GetComponent<PhotonView>().RPC("GetPushedRPC", RpcTarget.AllViaServer, impulse, otherplayerNum);
            }
        }

        [PunRPC]
        public void GetPushedRPC(Vector3 pushImpulse, int playerNum)
        {
            UnityEngine.Debug.Log("I have received the push RPC, intended for player " + playerNum);
            int myPlayerNum = gameObject.GetComponent<PhotonView>().OwnerActorNr;
            //This seems to never run on the player that is the owner of the whatever client it is running on.
            if (myPlayerNum == playerNum)
            {
                UnityEngine.Debug.Log("I will now push myself because I have received the push RPC");
                rb.AddForce(pushImpulse);
            }

        }

        #endregion

        #region Custom

        /// <summary>
        /// Processes the inputs. Maintain a flag representing when the user is pressing Fire.
        /// </summary>
        void ProcessInputs()
        {
            playerInput.x = 0f;
            playerInput.y = 0f;
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");
            bool jump = Input.GetButtonDown("Jump");
            bool unjump = Input.GetButtonUp("Jump");
            bool push = Input.GetButtonDown("Fire1");
            bool unPush = Input.GetButtonUp("Fire1");
            bool grab = Input.GetButtonDown("Fire2");

            if (push)
            {
                IsPushing = true;
            }
            else if (unPush)
            {
                IsPushing = false;
            }
            if (push)
            {
                IsPushing = push;
            }
            if (jump)
            {
                isJumping = true;
            }
            if (unjump)
            {
                isJumping = false;
            }

            if (isJumping && IsGrounded())
            {
                rb.AddForce(new Vector3(0.0f, jumpSpeed, 0.0f), ForceMode.Impulse);
            }
        }

        #endregion

        #region MigratedFromOtherTODO

        private bool IsGrounded(){
            return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
        }

    private void Move(Vector2 stickMove)
        {
            //UnityEngine.Debug.Log(Time.deltaTime);
            Vector3 velocity = new Vector3(stickMove.x, 0f, stickMove.y) * Speed * Time.deltaTime;
            Vector3 displacement = velocity;
            rb.AddForce(displacement);
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                //stream.SendNext(this.IsFiring);
                stream.SendNext(this.Health);
            }
            else
            {
                // Network player, receive data
                //this.IsFiring = (bool)stream.ReceiveNext();
                this.Health = (float)stream.ReceiveNext();
            }
        }

        #endregion

    }
}