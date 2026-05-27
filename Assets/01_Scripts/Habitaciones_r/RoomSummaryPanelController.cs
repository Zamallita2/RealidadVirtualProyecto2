using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomSummaryPanelController : MonoBehaviour
{
    [Header("Textos")]
    public TMP_Text roomTitleText;

    [Header("Dificultad")]
    public Image[] difficultyStars; // 5 imágenes
    public Sprite starFilled;
    public Sprite starEmpty;

    [Header("Enemigos")]
    public Image[] enemyImages; // 5 imágenes
    public Sprite emptyPlusSprite;

    [Header("Editor")]
    public GameObject editorPanel;
    public RoomEditorPanelController editorController;
    public Button editButton;

    private int currentRoomID = 1;

    void Start()
    {
        if (editButton != null)
            editButton.onClick.AddListener(OpenEditor);

        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.OnRoomSelected += OnRoomSelected;
        }

        if (RoomConfigManager.Instance != null)
        {
            RoomConfigManager.Instance.OnRoomConfigChanged += OnRoomConfigChanged;
        }

        Refresh(RoomManager.Instance != null ? RoomManager.Instance.habitacionSeleccionada : 1);
    }

    private void OnDestroy()
    {
        if (RoomManager.Instance != null)
            RoomManager.Instance.OnRoomSelected -= OnRoomSelected;

        if (RoomConfigManager.Instance != null)
            RoomConfigManager.Instance.OnRoomConfigChanged -= OnRoomConfigChanged;
    }

    private void OnRoomSelected(int roomID)
    {
        currentRoomID = roomID;
        Refresh(roomID);
    }

    private void OnRoomConfigChanged(int roomID)
    {
        if (roomID == currentRoomID)
            Refresh(roomID);
    }

    public void Refresh(int roomID)
    {
        currentRoomID = roomID;

        if (roomTitleText != null)
            roomTitleText.text = "SALA " + roomID;

        RoomConfigData config = RoomConfigManager.Instance.GetRoomConfig(roomID);

        UpdateDifficulty(config.difficulty);
        UpdateEnemySlots(config);
    }

    private void UpdateDifficulty(int difficulty)
    {
        for (int i = 0; i < difficultyStars.Length; i++)
        {
            if (difficultyStars[i] == null)
                continue;

            difficultyStars[i].sprite = (i < difficulty) ? starFilled : starEmpty;
        }
    }

    private void UpdateEnemySlots(RoomConfigData config)
    {
        for (int i = 0; i < enemyImages.Length; i++)
        {
            if (enemyImages[i] == null)
                continue;

            string enemyId = config.enemyIds[i];

            if (string.IsNullOrEmpty(enemyId))
            {
                enemyImages[i].sprite = emptyPlusSprite;
                enemyImages[i].color = Color.white;
            }
            else
            {
                EnemyGachaData data = GachaInventoryManager.Instance.GetEnemyData(enemyId);
                if (data != null && data.cardFrontSprite != null)
                {
                    enemyImages[i].sprite = data.cardFrontSprite;
                    enemyImages[i].color = Color.white;
                }
                else
                {
                    enemyImages[i].sprite = emptyPlusSprite;
                    enemyImages[i].color = Color.white;
                }
            }
        }
    }

    private void OpenEditor()
    {
        if (editorPanel != null)
            editorPanel.SetActive(true);

        if (editorController != null)
            editorController.OpenRoom(currentRoomID);
    }
}