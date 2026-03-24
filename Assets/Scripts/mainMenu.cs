using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class mainMenu : MonoBehaviourPunCallbacks
{
    public TMP_Text statusText;
    public Button hostButton;
    public Button joinButton;
    public Button confirmJoinButton;
    public TMP_InputField roomCodeInput;

    private void Start()
    {
        hostButton.interactable = false;
        joinButton.interactable = false;
        statusText.text = "Connecting...";
        PhotonNetwork.ConnectUsingSettings();

        hostButton.onClick.AddListener(hostRoom);
        joinButton.onClick.AddListener(toggleJoinField);
        confirmJoinButton.onClick.AddListener(joinRoom);
    }

    public override void OnConnectedToMaster()
    {
        statusText.text = "Connected";
        hostButton.interactable = true;
        joinButton.interactable = true;
    }

    private void toggleJoinField()
    {
        bool active = !roomCodeInput.gameObject.activeSelf;
        roomCodeInput.gameObject.SetActive(active);
        confirmJoinButton.gameObject.SetActive(active);
    }

    private void hostRoom()
    {
        string code = Random.Range(1000, 9999).ToString();
        statusText.text = "Room Code: " + code;
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4;
        options.IsOpen = true;
        options.IsVisible = false;
        PhotonNetwork.CreateRoom(code, options);
    }

    private void joinRoom()
    {
        if (roomCodeInput.text.Length < 1) return;
        statusText.text = "Joining...";
        PhotonNetwork.JoinRoom(roomCodeInput.text.ToUpper());
    }

    public override void OnJoinedRoom()
    {
        statusText.text = "Joined!";
        PhotonNetwork.LoadLevel("testLobby");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText.text = "Failed to create: " + message;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = "Failed to join: " + message;
    }
}