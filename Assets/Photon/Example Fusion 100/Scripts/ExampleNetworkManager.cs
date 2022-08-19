using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fusion100Example
{
    public class ExampleNetworkManager : MonoBehaviour
    {
        public static NetworkRunner Runner;

        async void StartGame(GameMode mode)
        {
            // Create the Fusion runner and let it know that we will be providing user input
            Runner = GetComponent<NetworkRunner>();
            Runner.ProvideInput = true;

            // Start or join (depends on gamemode) a session with a specific name
            await Runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,                                            //game mode
                SessionName = "TestRoom",                                   //room name
                Scene = SceneManager.GetActiveScene().buildIndex,           //relevant only for host, clients will be forced to use the scene specified by the host
                SceneManager = GetComponent<NetworkSceneManagerDefault>()   //handles instantiation of NetworkObjects that are placed directly in the scene
            });
        }
        private void OnGUI()
        {
            //In hosted mode, one network peer is both server and client and creates the network session,
            //while the remaining peers are just clients joining the existing session
            if (Runner == null)
            {
                if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
                {
                    StartGame(GameMode.Host);
                }
                if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
                {
                    StartGame(GameMode.Client);
                }
            }
        }
    }
}