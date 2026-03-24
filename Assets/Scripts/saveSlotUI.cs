using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class saveSlotUI : MonoBehaviour
{
    public Button slot0Button;
    public Button slot1Button;
    public Button slot2Button;

    public TMP_Text slot0Text;
    public TMP_Text slot1Text;
    public TMP_Text slot2Text;

    public Button deleteButton;
    public Button backButton;
    public GameObject mainPanel;

    // confirm panel
    public GameObject confirmPanel;
    public Button confirmYesButton;
    public Button confirmNoButton;

    private int selectedSlot = -1;
    private int slotToDelete = -1;
    private bool isHosting = false;
    private bool deleteMode = false;

    private void Start()
    {
        slot0Button.onClick.AddListener(() => onSlotClicked(0));
        slot1Button.onClick.AddListener(() => onSlotClicked(1));
        slot2Button.onClick.AddListener(() => onSlotClicked(2));
        deleteButton.onClick.AddListener(enterDeleteMode);
        backButton.onClick.AddListener(goBack);
        confirmYesButton.onClick.AddListener(confirmDelete);
        confirmNoButton.onClick.AddListener(cancelDelete);

        confirmPanel.SetActive(false);
    }

    public void open(bool hosting)
    {
        isHosting = hosting;
        deleteMode = false;
        gameObject.SetActive(true);
        refreshSlots();
    }

    private void refreshSlots()
    {
        saveManager.Instance.loadAllSlots();
        slot0Text.text = getSlotText(0);
        slot1Text.text = getSlotText(1);
        slot2Text.text = getSlotText(2);
        selectedSlot = -1;
        deleteMode = false;
    }

    private string getSlotText(int slot)
    {
        saveData data = saveManager.Instance.saveSlots[slot];
        if (data == null)
            return "Empty";
        return "Loot: " + data.totalLootValue;
    }

    private void onSlotClicked(int slot)
    {
        if (deleteMode)
        {
            slotToDelete = slot;
            confirmPanel.SetActive(true);
        }
        else
        {
            selectedSlot = slot;

            if (saveManager.Instance.saveSlots[slot] == null)
            {
                saveManager.Instance.createNewSave(slot);
                refreshSlots();
            }

            saveManager.Instance.setActiveSlot(slot);

            if (isHosting)
                gameSceneManager.Instance.startGame();
        }
    }

    private void enterDeleteMode()
    {
        deleteMode = true;
        deleteButton.interactable = false;
        Debug.Log("Select a slot to delete");
    }

    private void confirmDelete()
    {
        if (slotToDelete == -1) return;
        saveManager.Instance.deleteSave(slotToDelete);
        slotToDelete = -1;
        deleteMode = false;
        deleteButton.interactable = true;
        confirmPanel.SetActive(false);
        refreshSlots();
    }

    private void cancelDelete()
    {
        slotToDelete = -1;
        deleteMode = false;
        deleteButton.interactable = true;
        confirmPanel.SetActive(false);
    }

    private void goBack()
    {
        mainPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}