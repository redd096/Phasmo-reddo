//matchmaking: https://doc.photonengine.com/en-us/fusion/current/manual/matchmaking

using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerManager : MonoBehaviour
{
    public static NetworkRunner Runner;
    const string textToRandomize = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

    async void StartGame(GameMode mode)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        Runner = GetComponent<NetworkRunner>();
        Runner.ProvideInput = true;

        string randomName = string.Empty;
        for (int i = 0; i < 6; i++) randomName += textToRandomize[Random.Range(0, textToRandomize.Length)];

        // Start or join (depends on gamemode) a session with a specific name
        await Runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,                                            //game mode
            SessionName = randomName,                                   //room name
            Scene = SceneManager.GetActiveScene().buildIndex,           //relevant only for host, clients will be forced to use the scene specified by the host
            SceneManager = GetComponent<NetworkSceneManagerDefault>(),  //handles instantiation of NetworkObjects that are placed directly in the scene
            PlayerCount = 4                                             //number of players
        });
    }
}
