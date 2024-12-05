using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private bool isMainMenu = false;
    [SerializeField] private bool useTimeLimit = false;
    [SerializeField] private LevelAudioManager levelAudioManager;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject creditsMenu;
    [SerializeField] private GameObject levelSelectMenu;
    [SerializeField] private List<GameObject> winScreenButtons;
    [SerializeField] private List<GameObject> loseScreenButtons;
    [SerializeField] private List<GameObject> pauseMenuButtons;
    [SerializeField] private List<GameObject> settingsMenuButtons;
    [SerializeField] private List<GameObject> creditsMenuButtons;
    [SerializeField] private List<GameObject> levelSelectMenuButtons;
    [SerializeField] private GameObject objectiveIndicator;
    [SerializeField] private GameObject arrowsCheckmark;
    [SerializeField] private GameObject glowCheckmark;
    [SerializeField] private GameObject keyboardInputCheckmark;
    [SerializeField] private GameObject controllerInputCheckmark;
    [SerializeField] private GameObject enemySpeedSlider;
    [SerializeField] private GameObject enemiesCheckmark;
    [SerializeField] private GameObject[] directionalArrows;
    [SerializeField] private GameObject[] helpingGlows;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI winScoreText;
    [SerializeField] private float timeLimit = 90f;
    [SerializeField] private float UIButtonMoveCooldown = 0.5f;

    [HideInInspector] public bool isPaused;
    [HideInInspector] public List<GameObject> currentButtons;
    [HideInInspector] public int currentButtonIndex;

    private EventSystem eventSystem;
    private bool arrowsEnabled;
    private bool glowEnabled;
    private int enemySpeedSliderValue;
    private bool enemiesEnabled;
    private bool inputMode;
    private EnemyMovement[] enemies;
    private int score;
    private bool button1Buffer;
    private bool button2Buffer;
    public bool Button1Buffer {get=> button1Buffer;}
    public bool Button2Buffer {get=> button2Buffer;}
    private float UIButtonMoveTimer;
    private bool scrollUIButtonsHorizontally;

    private void Start()
    {
        eventSystem = FindObjectOfType<EventSystem>();
        score = 0;
        button1Buffer = true;
        button2Buffer = true;
        UIButtonMoveTimer = 0f;
        scrollUIButtonsHorizontally = false;

        isPaused = pauseMenu.activeSelf;
        CheckTimeScale();

        arrowsEnabled = false;
        arrowsCheckmark.SetActive(false);

        glowEnabled = false;
        glowCheckmark.SetActive(false);

        enemySpeedSliderValue = 100;

        enemiesEnabled = true;
        enemiesCheckmark.SetActive(true);

        enemies = FindObjectsOfType<EnemyMovement>();



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

        if (PlayerPrefs.GetInt("EnemySpeed", 100) != 100)
        {
            enemySpeedSliderValue = PlayerPrefs.GetInt("EnemySpeed");
            enemySpeedSlider.GetComponent<Slider>().value = enemySpeedSliderValue;
        }

        if (PlayerPrefs.GetInt("EnemyToggle", 1) == 0)
        {
            enemiesEnabled = false;
            enemiesCheckmark.SetActive(false);
        }

        if (PlayerPrefs.GetInt("InputMode", 0) == 1)
        {
            inputMode = true;
            ToggleInputCheckmarks(true);
        }
        
        UpdateSettings();

        if(isMainMenu)
            SetCurrentUIButtons(pauseMenuButtons);
    }

    private void Update()
    {
        CheckControllerBuffer();

        if(!isPaused && !loseScreen.activeSelf && !winScreen.activeSelf && useTimeLimit && !isMainMenu)
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

    private void CheckControllerBuffer()
    {
        ControllerInput controllerInput = ControllerInput.Instance;
        float axis;

        if(scrollUIButtonsHorizontally)
            axis = -controllerInput.Axis_X;
        else
            axis = controllerInput.Axis_Y;

        
        if(axis != 0 && (isPaused || isMainMenu))
        {
            if(UIButtonMoveTimer > 0)
            {
                UIButtonMoveTimer -= 0.01f;
            }

            else
            {
                UIButtonMoveTimer = UIButtonMoveCooldown;

                if(axis < 0)
                {
                    currentButtonIndex += 1;

                    if(currentButtonIndex >= currentButtons.Count)
                        currentButtonIndex = 0;

                    eventSystem.SetSelectedGameObject(currentButtons[currentButtonIndex]);
                }

                else if(axis > 0)
                {
                    currentButtonIndex -= 1;

                    if(currentButtonIndex < 0)
                        currentButtonIndex = currentButtons.Count - 1;

                    eventSystem.SetSelectedGameObject(currentButtons[currentButtonIndex]);
                }
            }
        }

        else if(controllerInput.Axis_X != 0 && currentButtons[currentButtonIndex].GetComponent<Slider>() && (isPaused || isMainMenu))
        {
            Slider slider = currentButtons[currentButtonIndex].GetComponent<Slider>();
            float axis_X = controllerInput.Axis_X;

            if(UIButtonMoveTimer <= 0)
            { 
                if(axis_X > 0)
                    slider.value += 5;

                else
                    slider.value -= 5;

                UIButtonMoveTimer = UIButtonMoveCooldown;
            }

            else UIButtonMoveTimer -= Time.deltaTime;
        }

        else UIButtonMoveTimer = 0f;


        if (controllerInput.Button1Trigger && !button1Buffer && !isMainMenu)
        {
            TogglePause();
            button1Buffer = true;
        }

        else if (!controllerInput.Button1Trigger && button1Buffer)
            button1Buffer = false;


        if(controllerInput.Button2Trigger && !button2Buffer && (isPaused || isMainMenu || winScreen.activeSelf || loseScreen.activeSelf))
        {
            if(eventSystem.currentSelectedGameObject != null && eventSystem.currentSelectedGameObject.GetComponent<Button>())
            eventSystem.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
            button2Buffer = true;
        }

        else if (!controllerInput.Button2Trigger && button2Buffer)
            button2Buffer = false;
    }

    public void Win()
    {
        winScreen.SetActive(true);
        SetCurrentUIButtons(winScreenButtons);
        levelAudioManager.PlayLevelWin();
        AddScore(1000 + (int)Mathf.Floor(timeLimit/60 * 100));
    }

    public void Lose()
    {
        loseScreen.SetActive(true);
        SetCurrentUIButtons(loseScreenButtons);
        levelAudioManager.PlayLevelLose();
        scoreText.text = "Pontos: 00000";
    }

    public void GetObjective(GameObject objective)
    {
        objectiveIndicator.GetComponent<Image>().sprite = objective.GetComponentInChildren<SpriteRenderer>().sprite;
        objectiveIndicator.GetComponent<Image>().color = Color.white;
        objectiveIndicator.SetActive(true);
        levelAudioManager.PlayItemPickup();
        AddScore(500 + (int)Mathf.Floor(timeLimit/60 * 25));
        
        Destroy(objective);
    }

    public void TogglePause()
    {   
        UIButtonMoveTimer = UIButtonMoveCooldown;


        if(!winScreen.activeSelf && !loseScreen.activeSelf)
        {
            settingsMenu.SetActive(false);

            pauseMenu.SetActive(!pauseMenu.activeSelf);
            
            if(pauseMenu.activeSelf)
                SetCurrentUIButtons(pauseMenuButtons);

            isPaused = pauseMenu.activeSelf;

            CheckTimeScale();
        }
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

        if(settingsMenu.activeSelf)
            SetCurrentUIButtons(settingsMenuButtons);
        else
            SetCurrentUIButtons(pauseMenuButtons);
    }

    public void ToggleCreditsMenu()
    {
        creditsMenu.SetActive(!creditsMenu.activeSelf);
        pauseMenu.SetActive(!creditsMenu.activeSelf);
        
        if(creditsMenu.activeSelf)
            SetCurrentUIButtons(creditsMenuButtons);
        else
            SetCurrentUIButtons(pauseMenuButtons);
    }

    public void ToggleLevelSelectMenu()
    {
        levelSelectMenu.SetActive(!levelSelectMenu.activeSelf);
        pauseMenu.SetActive(!levelSelectMenu.activeSelf);
        
        if(levelSelectMenu.activeSelf)
            SetCurrentUIButtons(levelSelectMenuButtons, true);
        else
            SetCurrentUIButtons(pauseMenuButtons);
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

    /// <summary>
    /// (The arduino will be updated in the future
    /// to send its own key inputs so this is temporary)
    /// </summary>
    /// <param name="toggle">True = Controller inputs; False = Keyboard inputs.</param>
    public void ToggleInputMode(bool toggle)
    {
        if(toggle)
        {
            ControllerInput.Instance.OpenSerial();
        }

        if(!toggle)
        {
            ControllerInput.Instance.CloseSerial();
        }
    }

    /// <summary>
    /// Same as ToggleInputMode() but only updates the checkmarks in settings.
    /// </summary>
    /// <param name="toggle">True = Controller inputs; False = Keyboard inputs.</param>
    public void ToggleInputCheckmarks(bool toggle)
    {
        inputMode = toggle;

        if(inputMode)
            PlayerPrefs.SetInt("InputMode", 1);
        else
            PlayerPrefs.SetInt("InputMode", 0);

        PlayerPrefs.Save();
        UpdateSettings();

        
        if(toggle)
        {
            controllerInputCheckmark.SetActive(true);
            keyboardInputCheckmark.SetActive(false);
        }

        if(!toggle)
        {
            controllerInputCheckmark.SetActive(false);
            keyboardInputCheckmark.SetActive(true);
        }
    }

    public void ChangeEnemySpeed()
    {
        enemySpeedSliderValue = (int) enemySpeedSlider.GetComponent<Slider>().value;
        PlayerPrefs.SetInt("EnemySpeed", enemySpeedSliderValue);


        PlayerPrefs.Save();
        UpdateSettings();
    }

    public void ToggleEnemies()
    {
        enemiesEnabled = !enemiesEnabled;
        enemiesCheckmark.SetActive(enemiesEnabled);

        if(enemiesEnabled)
            PlayerPrefs.SetInt("EnemyToggle", 1);
        else
            PlayerPrefs.SetInt("EnemyToggle", 0);

        PlayerPrefs.Save();
        UpdateSettings();
    }

    private void UpdateSettings()
    {
        foreach(GameObject go in directionalArrows)
        {
            if(go)
                go.SetActive(arrowsEnabled);
        }

        foreach(GameObject go in helpingGlows)
        {
            if(go)
                go.SetActive(glowEnabled);
        }

        foreach(EnemyMovement e in enemies)
        {
            if(e && !e.OriginallySleeping)
                e.sleeping = !enemiesEnabled;
        }
    }

    public void SetCurrentUIButtons(List<GameObject> buttons, bool scrollHorizontally = false)
    {
        currentButtons = buttons;
        currentButtonIndex = 0;
        eventSystem.SetSelectedGameObject(currentButtons[0]);
        scrollUIButtonsHorizontally = scrollHorizontally;
    }

    public void AddScore(int amount)
    {
        string scoreString = "Pontos: ";
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

