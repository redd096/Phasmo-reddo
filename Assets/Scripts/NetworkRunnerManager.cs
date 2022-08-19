using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerManager : MonoBehaviour
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
}
