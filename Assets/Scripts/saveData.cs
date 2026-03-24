using System;

[Serializable]
public class saveData
{
    // metrics
    public int totalRuns = 0;
    public int totalWipes = 0;
    public float averageFloorReached = 0f;
    public int[] deathsPerDifficulty = new int[20];

    // cave state
    public int totalLootValue = 0;

    // wizard
    public int wizardCurrency = 0;

    // constructor sets death defaults to 1
    public saveData()
    {
        for (int i = 0; i < deathsPerDifficulty.Length; i++)
            deathsPerDifficulty[i] = 1;
    }
}