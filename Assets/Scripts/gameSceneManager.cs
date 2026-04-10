using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class gameSceneManager : MonoBehaviourPunCallbacks
{
    public static gameSceneManager Instance;
    public bool gameStarted = false;

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
        floorGen.currentFloor = 0;
        gameStarted = true;
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4;
        options.IsOpen = true;
        options.IsVisible = false;
        PhotonNetwork.CreateRoom("game_" + Random.Range(1000, 9999), options);
    }

    public override void OnJoinedRoom()
    {
        if (gameStarted)
        {
            PhotonNetwork.LoadLevel("testLobby");
        }
    }

    public void descendToNextFloor()
    {
        floorGen.currentFloor++;
        
        if (floorGen.currentFloor != 1)
        {
        difficultyManager.Instance.advanceDifficulty(floorGen.currentFloor);
        }

        Debug.Log("Loading floor " + floorGen.currentFloor);
        PhotonNetwork.LoadLevel("roomGenTest");
    }

    public void returnHome()
    {
        floorGen.currentFloor = 0;
        bankStash();
        PhotonNetwork.LoadLevel("testLobby");
    }

    private void bankStash()
    {
        if (saveManager.Instance == null) return;
        if (stashManager.Instance == null) return;

        saveData data = saveManager.Instance.getActiveSave();
        if (data == null) return;

        // add junk value to cave
        data.totalLootValue += stashManager.Instance.totalJunkValue;

        // calculate wizard currency from magical value
        int wizardCut = Mathf.RoundToInt(stashManager.Instance.totalMagicalValue * 0.5f);
        data.wizardCurrency += wizardCut;

        // update metrics
        data.totalRuns++;

        // save to disk
        saveManager.Instance.saveSlot(saveManager.Instance.activeSaveSlot);

        // clear run stash
        stashManager.Instance.clearRunStash();

        Debug.Log("Banked stash — Loot: " + data.totalLootValue + " Wizard currency: " + data.wizardCurrency);
    }
}