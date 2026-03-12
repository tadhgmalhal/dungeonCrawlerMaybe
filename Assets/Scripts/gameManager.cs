using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class gameManager : MonoBehaviourPunCallbacks
{
    public static gameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
    }

    public void createRoom(string roomCode)
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4;
        options.IsOpen = true;
        options.IsVisible = false;
        PhotonNetwork.CreateRoom(roomCode, options);
    }

    public void joinRoom(string roomCode)
    {
        PhotonNetwork.JoinRoom(roomCode);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room successfully");
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join room: " + message);
    }
}   

