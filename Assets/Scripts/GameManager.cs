using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set;} //only this case can manage instance but anything can access this
    
    public float initialGameSpeed = 5f; //base speed
    public float gameSpeedIncrease = 0.1f; //increase of speed
    public float maxGameSpeed = 20f; //speed cap
    public float gameSpeed {get; private set;} //actual gamespeed
    public float delayForMaxScoreMultiplier = 1000f;
    public float maxScoreMultiplier = 5f;

    private Player player;
    private Spawner spawner;

    public TextMeshProUGUI gameOverText;
    public Button retyButton;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hiScoreText;

    [Header("Menu")]
    public GameObject mainMenuPanel;
    public GameObject gameplayUI;
    public GameObject settingsPanel;
    public Button returnToMenuButton;
    public bool startWithMenu = true;

    private float score; // usually integers for this but use float because it is based on time

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(gameObject); //only one instance
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying && settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
#endif

    private void Start()
    {
        player = FindObjectOfType<Player>();
        spawner = FindObjectOfType<Spawner>();

        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.RemoveAllListeners();
            returnToMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        if (startWithMenu && mainMenuPanel != null)
        {
            ShowMainMenu();
        }
        else
        {
            NewGame();
        }
    }

    public void NewGame() // public so we can call the function through the button object
    {
        ClearSelectedButton();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameMusic();

        Obstacle[] obstacles = FindObjectsOfType<Obstacle>();

        // remove old obstacles when starting a new game
        foreach (var obstacle in obstacles)
        {
           Destroy(obstacle.gameObject); // need to remove the entire thing ".gameObject"
        }

        gameSpeed = initialGameSpeed;
        score = 0f; // need this or else score continues from last run
        enabled = true;

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (gameplayUI != null)
        {
            gameplayUI.SetActive(true);
        }

        player.gameObject.SetActive(true);
        player.ResetState();
        spawner.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(false);
        retyButton.gameObject.SetActive(false);

        if (returnToMenuButton != null)
            returnToMenuButton.gameObject.SetActive(false);

        UpdateHiscore();
    }

    public void StartGame()
    {
        NewGame();
    }

    public void ShowSettingsMenu()
    {
        ClearSelectedButton();

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (gameplayUI != null)
        {
            gameplayUI.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void HideSettingsMenu()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        ClearSelectedButton();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMusic();

        Obstacle[] obstacles = FindObjectsOfType<Obstacle>();

        foreach (var obstacle in obstacles)
        {
            Destroy(obstacle.gameObject);
        }

        gameSpeed = 0;
        score = 0f;
        enabled = false;

        if (player != null)
            player.gameObject.SetActive(false);

        if (spawner != null)
            spawner.gameObject.SetActive(false);

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);

        if (retyButton != null)
            retyButton.gameObject.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (gameplayUI != null)
            gameplayUI.SetActive(false);

        if (returnToMenuButton != null)
            returnToMenuButton.gameObject.SetActive(false);

        UpdateHiscore();
    }

    public void ReturnToMainMenu()
    {
        ShowMainMenu();
    }

    public void GameOver() // set as public since we need to call from Player.CS
    {
        ClearSelectedButton();

        gameSpeed = 0;
        enabled = false;

        player.gameObject.SetActive(false);
        spawner.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(true);
        retyButton.gameObject.SetActive(true);

        if (returnToMenuButton != null)
            returnToMenuButton.gameObject.SetActive(true);

        UpdateHiscore();
    }

    private void Update()
    {
        gameSpeed += gameSpeedIncrease * Time.deltaTime;
        gameSpeed = Mathf.Min(gameSpeed, maxGameSpeed);

        // score for current run
        score += gameSpeed * Time.deltaTime * GetDelayScoreMultiplier(); // as game gets faster you get more score
        scoreText.text = Mathf.FloorToInt(score).ToString("D5"); // rounds down for every case
    }

    private float GetDelayScoreMultiplier()
    {
        if (player == null)
        {
            return 1f;
        }

        float delayPercent = Mathf.InverseLerp(0f, delayForMaxScoreMultiplier, player.inputDelayMs);
        return Mathf.Lerp(1f, maxScoreMultiplier, delayPercent);
    }

    private void ClearSelectedButton()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // hiscore (update and save)
    private void UpdateHiscore()
    {
        float hiscore = PlayerPrefs.GetFloat("hiscore",  0);

        if (score > hiscore)
        {
            hiscore = score;
            PlayerPrefs.SetFloat("hiscore", hiscore);
        }

        hiScoreText.text = Mathf.FloorToInt(hiscore).ToString("D5");
    }
}  

