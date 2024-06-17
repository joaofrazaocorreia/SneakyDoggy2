using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public LevelAudioManager levelAudioManager;
    public GameObject winScreen;
    public GameObject loseScreen;
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject objectiveIndicator;
    public GameObject arrowsCheckmark;
    public GameObject glowCheckmark;
    public GameObject smoothTurnCheckmark;
    public GameObject[] directionalArrows;
    public GameObject[] helpingGlows;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI winScoreText;
    [SerializeField] private float timeLimit = 90f;

    [HideInInspector] public bool isPaused;
    [HideInInspector] public bool smoothTurnEnabled;
    private bool arrowsEnabled;
    private bool glowEnabled;
    private int score;

    private void Start()
    {
        score = 0;

        isPaused = pauseMenu.activeSelf;
        CheckTimeScale();

        arrowsEnabled = false;
        arrowsCheckmark.SetActive(false);

        glowEnabled = false;
        glowCheckmark.SetActive(false);

        smoothTurnEnabled = true;
        smoothTurnCheckmark.SetActive(true);



        if (PlayerPrefs.GetInt("Arrows", 0) == 1)
        {
            arrowsEnabled = true;
            arrowsCheckmark.SetActive(true);
        }

        if (PlayerPrefs.GetInt("Glow", 0) == 1)
        {
            glowEnabled = true;
            glowCheckmark.SetActive(true);
        }

        if (PlayerPrefs.GetInt("SmoothTurn", 1) == 1)
        {
            smoothTurnEnabled = true;
            smoothTurnCheckmark.SetActive(true);
        }
        
        UpdateSettings();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if(!isPaused && !loseScreen.activeSelf && !winScreen.activeSelf)
        {
            timeLimit -= Time.deltaTime;

            float minutes = Mathf.Floor(timeLimit / 60);
            float seconds = Mathf.Floor(timeLimit % 60);
            float milliseconds = Mathf.Floor((timeLimit % 60 - Mathf.Floor(timeLimit % 60)) * 100);

            timerText.text = "Time left: ";
            
            if (minutes < 10)
                timerText.text += "0";
            timerText.text += $"{minutes}:";

            if (seconds < 10)
                timerText.text += "0";
            timerText.text += $"{seconds}:";

            if (milliseconds < 10)
                timerText.text += "0";
            timerText.text += $"{milliseconds}";


            if (timeLimit >= 11)
                timerText.color = Color.white;
            else
                timerText.color = Color.red;

            if (timeLimit <= 0f)
            {
                Lose();
                timerText.text = $"Time left: 00:00:00";
                levelAudioManager.PlayTimesUp();
            }
        }
    }

    public void Win()
    {
        winScreen.SetActive(true);
        levelAudioManager.PlayLevelWin();
        AddScore(1000 + (int)Mathf.Floor(timeLimit/60 * 100));
    }

    public void Lose()
    {
        loseScreen.SetActive(true);
        levelAudioManager.PlayLevelLose();
        AddScore(-250);
    }

    public void GetObjective(GameObject objective)
    {
        objectiveIndicator.GetComponent<Image>().sprite = objective.GetComponentInChildren<SpriteRenderer>().sprite;
        objectiveIndicator.SetActive(true);
        levelAudioManager.PlayItemPickup();
        AddScore(500 + (int)Mathf.Floor(timeLimit/60 * 25));
        
        Destroy(objective);
    }

    public void TogglePause()
    {
        settingsMenu.SetActive(false);

        pauseMenu.SetActive(!pauseMenu.activeSelf);
        isPaused = pauseMenu.activeSelf;

        CheckTimeScale();
    }

    private void CheckTimeScale()
    {
        if (isPaused)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }

    public void ToggleSettingsMenu()
    {
        settingsMenu.SetActive(!settingsMenu.activeSelf);
        pauseMenu.SetActive(!settingsMenu.activeSelf);
    }

    public void LoadScene(int n)
    {
        SceneManager.LoadScene(n);
    }

    public void ToggleDirectionalArrows()
    {
        arrowsEnabled = !arrowsEnabled;
        arrowsCheckmark.SetActive(arrowsEnabled);

        if (arrowsEnabled)
            PlayerPrefs.SetInt("Arrows", 1);
        else
            PlayerPrefs.SetInt("Arrows", 0);

            
        PlayerPrefs.Save();
        UpdateSettings();
    }

    public void ToggleHelpingGlow()
    {
        glowEnabled = !glowEnabled;
        glowCheckmark.SetActive(glowEnabled);

        if(glowEnabled)
            PlayerPrefs.SetInt("Glow", 1);
        else
            PlayerPrefs.SetInt("Glow", 0);

            
        PlayerPrefs.Save();
        UpdateSettings();
    }

    public void ToggleSmoothTurning()
    {
        smoothTurnEnabled = !smoothTurnEnabled;
        smoothTurnCheckmark.SetActive(smoothTurnEnabled);

        if(smoothTurnEnabled)
            PlayerPrefs.SetInt("SmoothTurn", 1);
        else
            PlayerPrefs.SetInt("SmoothTurn", 0);

            
        PlayerPrefs.Save();
        UpdateSettings();
    }

    private void UpdateSettings()
    {
        foreach(GameObject go in directionalArrows)
        {
            go.SetActive(arrowsEnabled);
        }

        foreach(GameObject go in helpingGlows)
        {
            go.SetActive(glowEnabled);
        }
    }

    public void AddScore(int amount)
    {
        string scoreString = "Score: ";
        score += amount;


        for(int i = 10000; i >= 10; i /= 10)
        {
            if (score < i)
                scoreString += "0";
            else break;
        }

        scoreString += score;

        scoreText.text = scoreString;
        winScoreText.text = scoreString;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

