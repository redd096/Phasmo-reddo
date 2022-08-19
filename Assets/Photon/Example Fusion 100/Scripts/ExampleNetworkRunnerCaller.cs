using Fusion.Sockets;
using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion100Example
{
    public class ExampleNetworkRunnerCaller : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkPrefabRef playerPrefab = default;

        private Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
        private bool mouseButton0;
        private bool mouseButton1;

        private void Update()
        {
            //To ensure that quick taps are not missed, the mouse button is sampled in Update() and reset once it has been recorded in the input structure in OnInput()
            mouseButton0 = mouseButton0 || Input.GetMouseButton(0);
            mouseButton1 = mouseButton1 || Input.GetMouseButton(1);
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                // Create a unique position for the player
                Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.DefaultPlayers) * 3, 1, 0);
                //instantiate player prefab on the network, at position and rotation. And set PlayerRef as authority (the player allowed to provide input for the avatar)
                NetworkObject networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
                // Keep track of the player avatars so we can remove it when they disconnect (authority and instance of the player prefab)
                spawnedCharacters.Add(player, networkPlayerObject);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            // Find and remove the players avatar
            if (spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                //destroy instance and remove from dictionary
                runner.Despawn(networkObject);
                spawnedCharacters.Remove(player);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            //client collect inputs
            ExampleNetworkInputData data = new ExampleNetworkInputData();

            if (Input.GetKey(KeyCode.W))
                data.direction += Vector2.up;

            if (Input.GetKey(KeyCode.S))
                data.direction += Vector2.down;

            if (Input.GetKey(KeyCode.A))
                data.direction += Vector2.left;

            if (Input.GetKey(KeyCode.D))
                data.direction += Vector2.right;

            //checks for the primary mouse button and set the first bit of the buttons field if it is down. Be sure to reset variable
            if (mouseButton0)
                data.buttons |= ExampleNetworkInputData.MOUSEBUTTON0;
            mouseButton0 = false;

            if (mouseButton1)
                data.buttons |= ExampleNetworkInputData.MOUSEBUTTON1;
            mouseButton1 = false;

            //set input to network
            input.Set(data);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}