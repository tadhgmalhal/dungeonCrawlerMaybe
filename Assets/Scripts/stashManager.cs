using UnityEngine;

public class stashManager : MonoBehaviour
{
    public static stashManager Instance;

    public int totalJunkValue = 0;
    public int totalMagicalValue = 0;

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

    public void addLoot(int junk, int magical)
    {
        totalJunkValue += junk;
        totalMagicalValue += magical;
        Debug.Log("Stash — Junk: " + totalJunkValue + " Magical: " + totalMagicalValue);
    }

    public void clearRunStash()
    {
        totalJunkValue = 0;
        totalMagicalValue = 0;
    }

    public void applyDeathPenalty()
    {
        totalJunkValue = Mathf.RoundToInt(totalJunkValue * 0.25f);
        totalMagicalValue = Mathf.RoundToInt(totalMagicalValue * 0.25f);
        Debug.Log("Death penalty applied. Remaining stash — Junk: " + totalJunkValue);
    }
}