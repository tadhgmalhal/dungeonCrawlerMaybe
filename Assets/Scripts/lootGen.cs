using UnityEngine;
using System.Collections.Generic;

public class lootGen : MonoBehaviour
{
    [SerializeField] private GameObject[] lootPrefabs;
    [SerializeField] private int lootPerRoom = 3;
    [SerializeField] private float lootSpawnHeight = 0.5f;

    public void spawnLoot(List<Room> placedRooms)
    {
        foreach (Room room in placedRooms)
        {
            for (int i = 0; i < lootPerRoom; i++)
            {
                Vector3 randomOffset = new Vector3(
                    Random.Range(-3f, 3f),
                    lootSpawnHeight,
                    Random.Range(-3f, 3f)
                );
                Vector3 spawnPos = room.transform.position + randomOffset;
                GameObject lootPrefab = lootPrefabs[Random.Range(0, lootPrefabs.Length)];
                Instantiate(lootPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}