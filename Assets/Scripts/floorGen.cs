using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class floorGen : MonoBehaviourPunCallbacks
{
    [Header("Room Settings")]
    [SerializeField] private GameObject[] roomPrefabs;
    [SerializeField] private int roomCount = 10;
    [SerializeField] private GameObject wallCapPrefab;
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private GameObject playerPrefab;

    private List<Room> placedRooms = new List<Room>();
    private List<roomConnection> openConnectors = new List<roomConnection>();
    private List<roomConnection> allConnectors = new List<roomConnection>();

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("test", new Photon.Realtime.RoomOptions { MaxPlayers = 4 }, null);
    }

    public override void OnJoinedRoom()
    {
        generate();
    }

    void generate()
    {
        GameObject startRoom = Instantiate(roomPrefabs[Random.Range(0, roomPrefabs.Length)], Vector3.zero, Quaternion.identity);
        Room startRoomComponent = startRoom.GetComponent<Room>();
        placedRooms.Add(startRoomComponent);

        foreach (roomConnection connector in startRoomComponent.connectors)
        {
            openConnectors.Add(connector);
            allConnectors.Add(connector);
        }

        int attempt = 0;

        while (placedRooms.Count < roomCount && openConnectors.Count > 0 && attempt < 100)
        {
            attempt++;

            roomConnection currentConnector = openConnectors[0];
            openConnectors.RemoveAt(0);

            if (currentConnector.isConnected) continue;

            GameObject newRoomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
            GameObject newRoomObject = Instantiate(newRoomPrefab, Vector3.zero, Quaternion.identity);
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
        }

        Vector3 portalPos = placedRooms[0].transform.position;
        Instantiate(portalPrefab, portalPos, Quaternion.identity);

        int middleIndex = placedRooms.Count / 2;

        Vector3 spawnPos = placedRooms[0].transform.position + Vector3.up * 1f;
        PhotonNetwork.Instantiate("Player", spawnPos, Quaternion.identity);

        Debug.Log("Open connectors remaining: " + openConnectors.Count);

        sealOpenConnectors();
        Debug.Log("Generated " + placedRooms.Count + " rooms.");
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
}