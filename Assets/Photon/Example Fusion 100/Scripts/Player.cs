using Fusion;
using UnityEngine;
using redd096.GameTopDown2D;

namespace Fusion100Example
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private Ball prefabBall;

        [Networked] private TickTimer delay { get; set; }

        MovementComponent movementComponent;

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
                    //if pressed button, spawn ball rotated in direction
                    if ((data.buttons & NetworkInputData.MOUSEBUTTON1) != 0)
                    {
                        //set delay
                        delay = TickTimer.CreateFromSeconds(Runner, 0.5f);

                        //in movement direction. If still, move to the right or left
                        Vector2 direction = movementComponent.MoveDirectionInput != Vector2.zero ? movementComponent.MoveDirectionInput : movementComponent.IsMovingRightDirection;
                        Quaternion rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.AngleAxis(90, Vector3.forward) * direction);    //rotate direction to left, to use right as forward

                        Runner.Spawn(prefabBall,
                        (Vector2)transform.position + direction, rotation,
                        Object.InputAuthority,                              //use Object.InputAuthority to get player ref
                        (runner, o) => { o.GetComponent<Ball>().Init(); }); //initialize the ball before sync it
                    }
                }
            }
        }
    }
}