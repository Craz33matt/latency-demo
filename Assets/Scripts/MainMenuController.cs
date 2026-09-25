using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu UI")]
    public GameObject mainMenuPanel;
    public GameObject gameplayUI;
    public Button startButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Difficulty UI")]
    public TextMeshProUGUI difficultyText;
    public Button decreaseDifficultyButton;
    public Button increaseDifficultyButton;
    public float difficultyStepMs = 100f;
    public float minDelayMs = 0f;
    public float maxDelayMs = 1000f;

    private Player player;

    private void Awake()
    {
        player = FindObjectOfType<Player>(true);

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartButtonPressed);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitButtonPressed);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OnSettingsButtonPressed);
        }

        if (decreaseDifficultyButton != null)
        {
            decreaseDifficultyButton.onClick.RemoveAllListeners();
            decreaseDifficultyButton.onClick.AddListener(DecreaseDifficulty);
        }

        if (increaseDifficultyButton != null)
        {
            increaseDifficultyButton.onClick.RemoveAllListeners();
            increaseDifficultyButton.onClick.AddListener(IncreaseDifficulty);
        }

        UpdateDifficultyText();
    }

    private void Start()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (gameplayUI != null)
            gameplayUI.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMusic();

        UpdateDifficultyText();
    }

    private void IncreaseDifficulty()
    {
        ChangeDifficulty(difficultyStepMs);
        ClearSelectedButton();
    }

    private void DecreaseDifficulty()
    {
        ChangeDifficulty(-difficultyStepMs);
        ClearSelectedButton();
    }

    private void ChangeDifficulty(float amount)
    {
        if (player == null)
            player = FindObjectOfType<Player>(true);

        if (player == null)
            return;

        player.inputDelayMs = Mathf.Clamp(player.inputDelayMs + amount, minDelayMs, maxDelayMs);
        UpdateDifficultyText();
    }

    private void UpdateDifficultyText()
    {
        if (difficultyText == null)
            return;

        if (player == null)
            player = FindObjectOfType<Player>(true);

        if (player == null)
        {
            difficultyText.text = "DIFFICULTY ?";
            return;
        }

        int difficulty = Mathf.RoundToInt(player.inputDelayMs / difficultyStepMs);
        difficultyText.text = $"DIFFICULTY {difficulty}";
    }

    private void ClearSelectedButton()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void OnStartButtonPressed()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameMusic();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (gameplayUI != null)
            gameplayUI.SetActive(true);
    }

    public void OnSettingsButtonPressed()
    {
        ClearSelectedButton();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowSettingsMenu();
        }
    }

    public void OnQuitButtonPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
