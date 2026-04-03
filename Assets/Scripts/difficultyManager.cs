using UnityEngine;

public class difficultyManager : MonoBehaviour
{
    public static difficultyManager Instance;

    public int currentDifficulty = 1;
    public int startingDifficulty = 1;
    public int portalModifier = 0;

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

    public void calculateStartingDifficulty()
    {
        saveData data = saveManager.Instance.getActiveSave();
        if (data == null)
        {
            startingDifficulty = 1;
            currentDifficulty = 1;
            return;
        }

        float average = 0f;
        for (int i = 0; i < 5; i++)
        {
            Debug.Log("Difficulty " + (i + 1) + " deaths: " + data.deathsPerDifficulty[i]);
            average += data.deathsPerDifficulty[i];
        }
        average /= 5f;
        Debug.Log("Average deaths: " + average);

        int spike = -1;
        for (int i = 0; i < 5; i++)
        {
            if (data.deathsPerDifficulty[i] > average)
            {
                spike = i + 1;
                break;
            }
        }

        Debug.Log("Spike at: " + spike);
        Debug.Log("Starting difficulty: " + startingDifficulty);
        Debug.Log("Portal modifier: " + portalModifier);
        Debug.Log("Current difficulty: " + currentDifficulty);
    }

    public void advanceDifficulty(int floorNumber)
    {
        // linear probability scaling
        // floor 1 = 20% chance +2, floor 10 = 80% chance +2
        float chanceOfTwo = Mathf.Lerp(0.2f, 0.8f, (floorNumber - 1) / 9f);
        int increase = Random.value < chanceOfTwo ? 2 : 1;
        currentDifficulty = Mathf.Clamp(currentDifficulty + increase, 1, 20);

        Debug.Log("Floor " + floorNumber + " difficulty: " + currentDifficulty);
    }

    public void applyHellGem()
    {
        currentDifficulty = 20;
        Debug.Log("Hell Gem active — difficulty 20");
    }

    public void reset()
    {
        currentDifficulty = 1;
        startingDifficulty = 1;
        portalModifier = 0;
    }
}