using Fusion;
using UnityEngine;

namespace Fusion100Example
{
    //client will provide an input structure that the host will then interpret in order to update the network state
    //client may apply the input locally to provide instant feedback to the user, but this is just a local prediction which may be overruled by the host.
    public struct NetworkInputData : INetworkInput
    {
        public const byte MOUSEBUTTON1 = 0x01;

        public byte buttons;
        public Vector2 direction;
    }
}