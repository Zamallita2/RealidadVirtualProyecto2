using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RoomEditorPanelController : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panelRoot;

    [Header("Sala actual")]
    public TMP_Text roomTitleText;

    [Header("Dificultad")]
    public Image[] difficultyStars; // 5 imágenes
    public Sprite starFilled;
    public Sprite starEmpty;

    [Header("Drop")]
    public Slider rewardDropSlider;
    public TMP_Text rewardDropText;

    [Header("Lista izquierda")]
    public Transform unlockedEnemiesContent;
    public GameObject enemyItemPrefab;

    [Header("Slots derecha")]
    public RoomEditorSlotUI[] roomSlots; // 5 slots

    [Header("Botones")]
    public Button saveButton;
    public Button closeButton;

    private int currentRoomID = 1;
    private List<GameObject> spawnedEnemyItems = new List<GameObject>();

    private void Start()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveAndClose);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        if (rewardDropSlider != null)
            rewardDropSlider.onValueChanged.AddListener(OnDropSliderChanged);

        for (int i = 0; i < roomSlots.Length; i++)
        {
            if (roomSlots[i] != null)
                roomSlots[i].Setup(this, i);
        }

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void OpenRoom(int roomID)
    {
        currentRoomID = roomID;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (roomTitleText != null)
            roomTitleText.text = "EDITAR SALA " + roomID + " - Juguetería Maldita";

        RoomConfigData config = RoomConfigManager.Instance.GetRoomConfig(roomID);

        if (rewardDropSlider != null)
            rewardDropSlider.SetValueWithoutNotify(config.rewardDrop);

        UpdateDropText(config.rewardDrop);
        UpdateDifficultyVisual(config.difficulty);
        RefreshSlots();
        RefreshEnemyList();
    }

    public void SetDifficulty(int difficulty)
    {
        RoomConfigManager.Instance.SetDifficulty(currentRoomID, difficulty);
        UpdateDifficultyVisual(difficulty);
    }

    private void OnDropSliderChanged(float value)
    {
        int dropValue = Mathf.RoundToInt(value);
        RoomConfigManager.Instance.SetRewardDrop(currentRoomID, dropValue);
        UpdateDropText(dropValue);
    }

    private void UpdateDropText(int value)
    {
        if (rewardDropText != null)
            rewardDropText.text = value + "%";
    }

    private void UpdateDifficultyVisual(int difficulty)
    {
        for (int i = 0; i < difficultyStars.Length; i++)
        {
            if (difficultyStars[i] == null)
                continue;

            difficultyStars[i].sprite = (i < difficulty) ? starFilled : starEmpty;
        }
    }

    public void RefreshEnemyList()
    {
        ClearEnemyList();

        if (GachaInventoryManager.Instance == null)
            return;

        RoomConfigData config = RoomConfigManager.Instance.GetRoomConfig(currentRoomID);
        bool bossAllowed = RoomConfigManager.Instance.IsBossAllowedInRoom(currentRoomID);

        foreach (EnemyGachaData enemy in GachaInventoryManager.Instance.allEnemies)
        {
            if (enemy == null)
                continue;

            if (!GachaInventoryManager.Instance.HasEnemy(enemy.enemyId))
                continue;

            int copies = GachaInventoryManager.Instance.GetCopies(enemy.enemyId);
            if (copies <= 0)
                continue;

            GameObject obj = Instantiate(enemyItemPrefab, unlockedEnemiesContent);
            spawnedEnemyItems.Add(obj);

            EnemyPaletteItemUI itemUI = obj.GetComponent<EnemyPaletteItemUI>();
            if (itemUI != null)
            {
                bool draggable = true;

                if (enemy.isBoss && !bossAllowed)
                    draggable = false;

                itemUI.Setup(enemy, copies, draggable);
            }
        }
    }

    private void ClearEnemyList()
    {
        for (int i = 0; i < spawnedEnemyItems.Count; i++)
        {
            if (spawnedEnemyItems[i] != null)
                Destroy(spawnedEnemyItems[i]);
        }

        spawnedEnemyItems.Clear();
    }

    public void RefreshSlots()
    {
        RoomConfigData config = RoomConfigManager.Instance.GetRoomConfig(currentRoomID);

        for (int i = 0; i < roomSlots.Length; i++)
        {
            if (roomSlots[i] == null)
                continue;

            string enemyId = config.enemyIds[i];

            if (string.IsNullOrEmpty(enemyId))
            {
                roomSlots[i].SetEmpty();
            }
            else
            {
                EnemyGachaData data = GachaInventoryManager.Instance.GetEnemyData(enemyId);
                if (data != null)
                    roomSlots[i].SetEnemy(data);
                else
                    roomSlots[i].SetEmpty();
            }
        }
    }

    public void TryAssignEnemyToSlot(string enemyId, int slotIndex)
    {
        bool ok = RoomConfigManager.Instance.TryPlaceEnemy(currentRoomID, slotIndex, enemyId);

        if (ok)
        {
            RefreshSlots();
            RefreshEnemyList();
        }
    }

    public void RemoveEnemyFromSlot(int slotIndex)
    {
        bool ok = RoomConfigManager.Instance.RemoveEnemyFromSlot(currentRoomID, slotIndex);

        if (ok)
        {
            RefreshSlots();
            RefreshEnemyList();
        }
    }

    public void SaveAndClose()
    {
        RoomConfigManager.Instance.Save();
        ClosePanel();
    }

    public void ClosePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}