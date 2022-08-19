using Fusion;
using UnityEngine;

namespace Fusion100Example
{
    public class PhysxBall : NetworkBehaviour
    {
        //timer
        [Networked] private TickTimer life { get; set; }

        public void Init(Vector2 forward)
        {
            //set timer of 5 seconds
            life = TickTimer.CreateFromSeconds(Runner, 5.0f);

            //set speed
            GetComponent<Rigidbody2D>().velocity = forward;
        }

        public override void FixedUpdateNetwork()
        {
            //check when timer finish
            if (life.Expired(Runner))
                Runner.Despawn(Object);
        }
    }
}