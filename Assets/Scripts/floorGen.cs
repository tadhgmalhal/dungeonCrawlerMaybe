using UnityEngine;
using System.Collections.Generic;

public class floorGen : MonoBehaviour
{
    [Header("Room Settings")]
    public GameObject[] roomPrefabs;
    public int roomCount = 10;

    private List<Room> placedRooms = new List<Room>();
    private List<roomConnection> openConnectors = new List<roomConnection>();

    void Start()
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

            // wait for physics to update
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
                }
            }
        }

        Debug.Log("Generated " + placedRooms.Count + " rooms.");
    }

    void alignRooms(roomConnection current, roomConnection incoming)
    {
        // rotate so incoming connector faces opposite to current connector
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

        // snap positions together
        Vector3 offset = current.transform.position - incoming.transform.position;
        incoming.transform.parent.position += offset;
    }

    bool checkOverlap(GameObject room)
    {
        return false;
    }
}