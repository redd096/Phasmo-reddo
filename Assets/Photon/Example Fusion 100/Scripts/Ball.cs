using Fusion;

namespace Fusion100Example
{
    public class Ball : NetworkBehaviour
    {
        [Networked] private TickTimer life { get; set; }

        public void Init()
        {
            //set timer of 5 seconds
            life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        }

        public override void FixedUpdateNetwork()
        {
            //move by Runner.DeltaTime instead of Time.deltaTime, cause FixedUpdateNetwork is called at every simulation (can be also more times for frame)
            transform.position += 5 * transform.right * Runner.DeltaTime;

            //if timer is finished, destroy ball (use Object, not gameObject)
            if (life.Expired(Runner))
                Runner.Despawn(Object);
        }
    }
}