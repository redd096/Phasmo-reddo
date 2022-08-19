using Fusion;
using UnityEngine;
using redd096.GameTopDown2D;

namespace Fusion100Example
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private Ball prefabBall;
        [SerializeField] private PhysxBall prefabPhysxBall;

        [Networked] private TickTimer delay { get; set; }

        MovementComponent movementComponent;

        #region example of OnValueChanged with sync vars

        private SpriteRenderer sprite;
        SpriteRenderer Sprite
        {
            get
            {
                if (sprite == null)
                    sprite = GetComponentInChildren<SpriteRenderer>();
                return sprite;
            }
        }

        //set Networked but add OnChanged function
        [Networked(OnChanged = nameof(OnBallSpawned))]
        public NetworkBool spawned { get; set; }

        //function must be static, and pass a Changed variable. Can call LoadOld to see old values with var.Behaviour, or call LoadNew to re-show new values
        public static void OnBallSpawned(Changed<Player> changed)
        {
            changed.Behaviour.sprite.color = Color.white;
            //changed.LoadOld();
            //changed.Behaviour.somethingPreviousValue
        }

        //run after FixedUpdateNetwork() and it uses Time.deltaTime rather than Runner.DeltaTime because it is running in Unity's render loop
        public override void Render()
        {
            base.Render();
            Sprite.color = Color.Lerp(Sprite.color, Color.blue, Time.deltaTime);
        }

        #endregion

        #region RPC

        private void Update()
        {
            //this is called on every client, so check this has input authority
            if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
            {
                RPC_SendMessage("Hey Mate!");
            }
        }

        private UnityEngine.UI.Text textInScene;

        //rpc called only from who has Input Authority, to everybody
        //InputAuthority is client, StateAuthority is Server, Proxies every other client not self, All everybody
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public void RPC_SendMessage(string message, RpcInfo info = default) //can set RPCInfo = default to get info
        {
            //get text UI
            if (textInScene == null)
                textInScene = FindObjectOfType<UnityEngine.UI.Text>();

            if (info.IsInvokeLocal)
                message = $"You said: {message}\n";
            else
                message = $"Some other player said: {message}\n";
            textInScene.text += message;
        }

        //static rpc ignore source and targets. Can be called from every client to every client. Can call on one specific client, see Targeted RPC
        //It must have NetworkRunner has first parameter
        [Rpc]
        public static void Rpc_MyStaticRpc(NetworkRunner runner, int a) { }

        //can set other optional variables:
        //InvokeLocal (default true): True indicates that the RPC will be invoked on the local client too
        //InvokeResim (default false): True indicates that the RPC will be invoked during re-simulations
        //TickAligned (default true): Set to false if you do not want the receiving end to delay execution of the RPC until at or after the tick in which in was sent
        //Channel (default Reliable): Set to Unreliable if the RPC can be lost in transmission
        //HostMode (default SourceIsServer): Used when set RPCInfo in parameters. When you read info.Source (player ref) if is server return None.
        //To include the Host's local PlayerRef instead, the HostMode attribute has to be set to RpcHostMode.SourceIsHostPlayer
        [Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = true, InvokeResim = true, TickAligned = false, Channel = RpcChannel.Unreliable, HostMode = RpcHostMode.SourceIsHostPlayer)]
        void RpcStartBoost(RpcInfo info = default)
        {
            //info.Tick: at which tick was it sent.
            //info.Source: which player (PlayerRef) sent it. (server is None. In host mode, can set RpcHostMode.SourceIsHostPlayer to receive local host player ref)
            //info.Channel: was it sent as an Unreliable or Reliable RPC.
            //info.IsInvokeLocal: if it is the local player who originally invoked this RPC.
        }


        //can send RPC to a specific client only. To do this, set a PlayerRef parameter with [RpcTarget] attribute
        //Passing PlayerRef.None for the [RpcTarget] parameter will target the server!
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public void Rpc_TargetedInstanceMessage([RpcTarget] PlayerRef player, string message) { }

        //can do this on static RPC too
        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
        public static void Rpc_MyTargetedStaticMessage(NetworkRunner runner, [RpcTarget] PlayerRef player, string message) { }

        #endregion

        private void Awake()
        {
            //get references
            movementComponent = GetComponent<MovementComponent>();
        }

        /// <summary>
        /// Called on every simulation tick
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            //get inputs
            if (GetInput(out NetworkInputData data))
            {
                //use direction to move (direction is normalized to prevent cheating. And speed is in movement component, so use the one on the network)
                data.direction.Normalize();
                movementComponent.MoveInDirection(data.direction);

                //checks button down only with a delay
                if (delay.ExpiredOrNotRunning(Runner))
                {
                    //if pressed button 0, spawn ball rotated in direction
                    if ((data.buttons & NetworkInputData.MOUSEBUTTON0) != 0)
                    {
                        //set delay
                        delay = TickTimer.CreateFromSeconds(Runner, 0.5f);

                        //in movement direction. If still, move to the right or left
                        Vector2 direction = data.direction != Vector2.zero ? data.direction : movementComponent.IsMovingRightDirection;
                        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.AngleAxis(90, Vector3.forward) * direction);    //rotate direction to left, to use right as forward

                        Runner.Spawn(prefabBall,
                        (Vector2)transform.position + direction, rotation,
                        Object.InputAuthority,                              //use Object.InputAuthority to get player ref
                        (runner, o) => { o.GetComponent<Ball>().Init(); }); //initialize the ball before sync it

                        //used for OnValueChanged example
                        spawned = !spawned;
                    }
                    //else if pressed button 1, spawn physx ball rotated in direction
                    if ((data.buttons & NetworkInputData.MOUSEBUTTON1) != 0)
                    {
                        //set delay
                        delay = TickTimer.CreateFromSeconds(Runner, 0.5f);

                        //in movement direction. If still, move to the right or left
                        Vector2 direction = data.direction != Vector2.zero ? data.direction : movementComponent.IsMovingRightDirection;
                        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.AngleAxis(90, Vector3.forward) * direction);    //rotate direction to left, to use right as forward

                        Runner.Spawn(prefabPhysxBall,
                        (Vector2)transform.position + direction, rotation,
                        Object.InputAuthority,                                                  //use Object.InputAuthority to get player ref
                        (runner, o) => { o.GetComponent<PhysxBall>().Init(10 * direction); });  //initialize the ball before sync it, passing push force

                        //used for OnValueChanged example
                        spawned = !spawned;
                    }
                }
            }
        }
    }
}