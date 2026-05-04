using UnityEngine;
using System.Collections.Generic;

public class enemySpawnManager : MonoBehaviour
{
    public static enemySpawnManager Instance;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject spiderPrefab;

    [Header("Gem Modifiers")]
    public bool abundanceGem = false;
    public bool scarcityGem = false;
    public bool hellGem = false;
    public bool depthGem = false;

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

    int calculateEnemyCount(int floor, int difficulty)
    {
        float t = Mathf.Clamp01((float)(difficulty - 1) / 19f);
        float divisor = Mathf.Lerp(20f, 10f, t);
        int rooms = calculateRoomCount(floor, difficulty);
        int flat = Mathf.FloorToInt(rooms / divisor);

        int floorMin = 5 * floor;
        int rawMin = Mathf.Max(flat - 5, floorMin);
        int rawMax = flat + 5;

        int count = Random.Range(rawMin, rawMax + 1);

        if (abundanceGem)
        {
            count += Mathf.Min(difficulty, 20);
        }

        if (scarcityGem)
        {
            count -= Mathf.Min(difficulty, 20);
        }

        return Mathf.Max(count, 1);
    }

    int calculateRoomCount(int floor, int difficulty)
    {
        float floorT = (float)(floor - 1) / 9f;
        float diffT = (float)(difficulty - 1) / 19f;
        int rooms = 100 + Mathf.FloorToInt((floorT * 0.4f + diffT * 0.6f) * 900f);
        return Mathf.Clamp(rooms, 100, 1000);
    }

    int getVirtualDifficulty(int floor, int realDifficulty)
    {
        if (depthGem && floor > 10)
        {
            return realDifficulty + (floor - 10) * 2;
        }
        return realDifficulty;
    }

    GameObject getRandomEnemy(int difficulty)
    {
        // placeholder — only spider exists
        // weight table will expand as enemies are added
        return spiderPrefab;
    }

    public void spawnEnemiesForFloor(List<Room> placedRooms, int floor)
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

        if (hellGem)
        {
            difficulty = 20;
        }

        int virtualDifficulty = getVirtualDifficulty(floor, difficulty);
        int enemyCount = calculateEnemyCount(floor, virtualDifficulty);

        Debug.Log("Spawning " + enemyCount + " enemies on floor " + floor + " (difficulty " + difficulty + ", virtual " + virtualDifficulty + ")");

        List<Room> spawnableRooms = new List<Room>();
        for (int i = 1; i < placedRooms.Count; i++)
        {
            spawnableRooms.Add(placedRooms[i]);
        }

        for (int i = 0; i < enemyCount; i++)
        {
            if (spawnableRooms.Count == 0) break;

            Room room = spawnableRooms[Random.Range(0, spawnableRooms.Count)];
            Vector3 spawnPos = room.transform.position + Vector3.up * 1f;
            GameObject enemyPrefab = getRandomEnemy(virtualDifficulty);

            if (enemyPrefab != null)
            {
                Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}