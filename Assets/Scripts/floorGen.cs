using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;
using Unity.AI.Navigation;

public class floorGen : MonoBehaviourPunCallbacks
{
    [Header("Room Settings")]
    [SerializeField] private GameObject[] roomPrefabs;
    [SerializeField] private GameObject wallCapPrefab;
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private lootGen lootGenerator;
    [SerializeField] private GameObject descendChutePrefab;
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private Transform dungeonRoot;

    public static int currentFloor = 1;
    public List<Room> placedRooms = new List<Room>();

    private List<roomConnection> openConnectors = new List<roomConnection>();
    private List<roomConnection> allConnectors = new List<roomConnection>();
    private int roomCount;

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            generate();
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("test", new Photon.Realtime.RoomOptions { MaxPlayers = 4 }, null);
    }

    public override void OnJoinedRoom()
    {
        generate();
    }

    int calculateRoomCount(int floor, int difficulty)
    {
        float floorT = (float)(floor - 1) / 9f;
        float diffT = (float)(difficulty - 1) / 19f;
        int rooms = 100 + Mathf.FloorToInt((floorT * 0.4f + diffT * 0.6f) * 900f);
        return Mathf.Clamp(rooms, 100, 1000);
    }

    void generate()
    {
        int difficulty;
        if (difficultyManager.Instance != null)
        {
            difficulty = difficultyManager.Instance.currentDifficulty;
        }
        else
        {
            difficulty = 1;
        }

        roomCount = calculateRoomCount(currentFloor, difficulty);
        Debug.Log("Generating floor " + currentFloor + " with difficulty " + difficulty + " and " + roomCount + " rooms.");

        GameObject startRoom = Instantiate(roomPrefabs[Random.Range(0, roomPrefabs.Length)], Vector3.zero, Quaternion.identity);
        startRoom.transform.parent = dungeonRoot;
        Room startRoomComponent = startRoom.GetComponent<Room>();
        placedRooms.Add(startRoomComponent);

        foreach (roomConnection connector in startRoomComponent.connectors)
        {
            openConnectors.Add(connector);
            allConnectors.Add(connector);
        }

        while (placedRooms.Count < roomCount && openConnectors.Count > 0)
        {
            roomConnection currentConnector = openConnectors[0];
            openConnectors.RemoveAt(0);

            if (currentConnector.isConnected) continue;

            int attempt = 0;
            bool placed = false;

            while (!placed && attempt < 10)
            {
                attempt++;

                GameObject newRoomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
                GameObject newRoomObject = Instantiate(newRoomPrefab, Vector3.zero, Quaternion.identity);
                newRoomObject.transform.parent = dungeonRoot;
                Room newRoom = newRoomObject.GetComponent<Room>();
                roomConnection newConnector = newRoom.connectors[Random.Range(0, newRoom.connectors.Length)];

                alignRooms(currentConnector, newConnector);
                Physics.SyncTransforms();

                if (checkOverlap(newRoomObject))
                {
                    Destroy(newRoomObject);
                    continue;
                }

                placedRooms.Add(newRoom);
                currentConnector.isConnected = true;
                newConnector.isConnected = true;

                foreach (roomConnection connector in newRoom.connectors)
                {
                    if (!connector.isConnected)
                    {
                        openConnectors.Add(connector);
                        allConnectors.Add(connector);
                    }
                }

                placed = true;
            }

            if (!placed)
            {
                allConnectors.Add(currentConnector);
            }
        }

        Vector3 portalPos = placedRooms[0].transform.position;
        Instantiate(portalPrefab, portalPos, Quaternion.identity);

        int middleIndex = placedRooms.Count / 2;
        if (middleIndex < placedRooms.Count)
        {
            Vector3 chutePos = placedRooms[middleIndex].transform.position + Vector3.up * 0.5f;
            Instantiate(descendChutePrefab, chutePos, Quaternion.identity);
        }

        Vector3 spawnPos = placedRooms[0].transform.position + Vector3.up * 1f;
        PhotonNetwork.Instantiate("Player", spawnPos, Quaternion.identity);

        Debug.Log("Open connectors remaining: " + openConnectors.Count);

        sealOpenConnectors();
        Debug.Log("Generated " + placedRooms.Count + " rooms.");

        lootGenerator.spawnLoot(placedRooms);

        StartCoroutine(buildNavMeshNextFrame());
    }

    void alignRooms(roomConnection current, roomConnection incoming)
    {
        float angle = Vector3.SignedAngle(
            incoming.transform.forward,
            -current.transform.forward,
            Vector3.up
        );

        incoming.transform.parent.RotateAround(
            incoming.transform.position,
            Vector3.up,
            angle
        );

        Vector3 offset = current.transform.position - incoming.transform.position;
        incoming.transform.parent.position += offset;
    }

    bool checkOverlap(GameObject room)
    {
        Bounds newBounds = getRoomBounds(room);

        foreach (Room placedRoom in placedRooms)
        {
            Bounds placedBounds = getRoomBounds(placedRoom.gameObject);
            placedBounds.Expand(-0.5f);

            if (newBounds.Intersects(placedBounds))
                return true;
        }
        return false;
    }

    Bounds getRoomBounds(GameObject room)
    {
        Renderer[] renderers = room.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }
        return bounds;
    }

    void sealOpenConnectors()
    {
        int sealedCount = 0;
        foreach (roomConnection connector in allConnectors)
        {
            if (connector == null) continue;
            if (connector.isConnected) continue;

            bool hasNeighbor = false;
            foreach (roomConnection other in allConnectors)
            {
                if (other == null) continue;
                if (other == connector) continue;
                if (!other.isConnected) continue;

                float dist = Vector3.Distance(connector.transform.position, other.transform.position);
                if (dist < 0.5f)
                {
                    hasNeighbor = true;
                    break;
                }
            }

            if (hasNeighbor) continue;

            GameObject wall = Instantiate(
                wallCapPrefab,
                connector.transform.position,
                connector.transform.rotation
            );
            wall.transform.rotation = Quaternion.LookRotation(-connector.transform.forward);
            sealedCount++;
        }
        Debug.Log("Sealed " + sealedCount + " connectors");
    }

    IEnumerator buildNavMeshNextFrame()
    {
        yield return new WaitForEndOfFrame();
        navMeshSurface.BuildNavMesh();
        StartCoroutine(spawnEnemies());
    }

    IEnumerator spawnEnemies()
    {
        yield return null;
        enemySpawnManager.Instance.spawnEnemiesForFloor(placedRooms, currentFloor);
    }
}