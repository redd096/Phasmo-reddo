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