using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class saveManager : MonoBehaviour
{
    public static saveManager Instance;

    public saveData[] saveSlots = new saveData[3];
    public int activeSaveSlot = -1;

    private string savePath => Application.persistentDataPath + "/saves/";

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

    public void loadAllSlots()
    {
        for (int i = 0; i < 3; i++)
        {
            string path = savePath + "slot" + i + ".json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                saveSlots[i] = JsonUtility.FromJson<saveData>(json);
                Debug.Log("Loaded slot " + i);
            }
            else
            {
                saveSlots[i] = null;
            }
        }
    }

    public void saveSlot(int slot)
    {
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        string json = JsonUtility.ToJson(saveSlots[slot], true);
        File.WriteAllText(savePath + "slot" + slot + ".json", json);
        Debug.Log("Saved slot " + slot);
    }

    public void createNewSave(int slot)
    {
        saveSlots[slot] = new saveData();
        saveSlot(slot);
        Debug.Log("Created new save in slot " + slot);
    }

    public void deleteSave(int slot)
    {
        string path = savePath + "slot" + slot + ".json";
        if (File.Exists(path))
            File.Delete(path);
        saveSlots[slot] = null;
        Debug.Log("Deleted slot " + slot);
    }

    public void setActiveSlot(int slot)
    {
        activeSaveSlot = slot;
        Debug.Log("Active save slot: " + slot);
    }

    public saveData getActiveSave()
    {
        if (activeSaveSlot == -1) return null;
        return saveSlots[activeSaveSlot];
    }
}