using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class gameSceneManager : MonoBehaviourPunCallbacks
{
    public static gameSceneManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void startGame()
    {
        floorGen.currentFloor = 1;
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4;
        options.IsOpen = true;
        options.IsVisible = false;
        PhotonNetwork.CreateRoom("game_" + Random.Range(1000, 9999), options);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("roomGenTest");
    }

    public void descendToNextFloor()
    {
        floorGen.currentFloor++;
        Debug.Log("Loading floor " + floorGen.currentFloor);
        PhotonNetwork.LoadLevel("roomGenTest");
    }

    public void returnHome()
    {
        floorGen.currentFloor = 0;
        PhotonNetwork.LoadLevel("testLobby");
    }
}