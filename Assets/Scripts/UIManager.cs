using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private bool isMainMenu = false;
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
    [SerializeField] private TextMeshProUGUI objectivesText;
    [SerializeField] private GameObject arrowsCheckmark;
    [SerializeField] private GameObject glowCheckmark;
    [SerializeField] private GameObject keyboardInputCheckmark;
    [SerializeField] private GameObject controllerInputCheckmark;
    [SerializeField] private GameObject volumeSlider;
    [SerializeField] private GameObject enemySpeedSlider;
    [SerializeField] private GameObject enemiesCheckmark;
    [SerializeField] private GameObject[] directionalArrows;
    [SerializeField] private GameObject[] helpingGlows;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI winScoreText;
    [SerializeField] private float UIButtonMoveCooldown = 0.5f;

    [HideInInspector] public bool isPaused;
    [HideInInspector] public List<GameObject> currentButtons;
    [HideInInspector] public int currentButtonIndex;
    [HideInInspector] public GameObject currentSelectionGlow;

    private EventSystem eventSystem;
    private bool arrowsEnabled;
    private bool glowEnabled;
    private int volumeSliderValue;
    private int enemySpeedSliderValue;
    private bool enemiesEnabled;
    private bool inputMode;
    private EnemyMovement[] enemies;
    private int score;
    private float time;
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
        time = 0f;
        ControllerInput.Instance.Button1Trigger = false;
        ControllerInput.Instance.Button2Trigger = false;
        button1Buffer = true;
        button2Buffer = true;
        UIButtonMoveTimer = 0f;
        scrollUIButtonsHorizontally = false;

        isPaused = pauseMenu.activeSelf;
        CheckTimeScale();

        // Resets all settings before loading them

        arrowsEnabled = false;
        arrowsCheckmark.SetActive(false);

        glowEnabled = false;
        glowCheckmark.SetActive(false);

        volumeSliderValue = 100;
        enemySpeedSliderValue = 100;

        enemiesEnabled = true;
        enemiesCheckmark.SetActive(true);

        enemies = FindObjectsOfType<EnemyMovement>();


        // Loads any saved settings

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

        if (PlayerPrefs.GetInt("Volume", 100) != 100)
        {
            volumeSliderValue = PlayerPrefs.GetInt("Volume");
            volumeSlider.GetComponent<Slider>().value = volumeSliderValue;
            ChangeVolume();
        }

        if (PlayerPrefs.GetInt("EnemySpeed", 100) != 100)
        {
            enemySpeedSliderValue = PlayerPrefs.GetInt("EnemySpeed");
            enemySpeedSlider.GetComponent<Slider>().value = enemySpeedSliderValue;
            ChangeEnemySpeed();
        }

        if (PlayerPrefs.GetInt("EnemyToggle", 1) == 0)
        {
            enemiesEnabled = false;
            enemiesCheckmark.SetActive(false);
        }
        
        // Sets the current buttons depending on whether this UI is the main menu or not
        if(isMainMenu)
            SetCurrentUIButtons(pauseMenuButtons);
        else
            UpdateSettings();
    }

    private void Update()
    {
        // Checks the inputs from the joystick
        CheckControllerBuffer();

        // While the game isn't paused or over and this UI isn't the main menu
        if(!isPaused && !loseScreen.activeSelf && !winScreen.activeSelf && !isMainMenu)
        {
            // Increases the in-game time
            time += Time.deltaTime;

            // Translates the time into Minutes : Seconds : Milliseconds and updates the timer text

            float minutes = Mathf.Floor(time / 60);
            float seconds = Mathf.Floor(time % 60);
            float milliseconds = Mathf.Floor((time % 60 - Mathf.Floor(time % 60)) * 100);

            timerText.text = "Tempo: ";
            
            if (minutes < 10)
                timerText.text += "0";
            timerText.text += $"{minutes}:";

            if (seconds < 10)
                timerText.text += "0";
            timerText.text += $"{seconds}:";

            if (milliseconds < 10)
                timerText.text += "0";
            timerText.text += $"{milliseconds}";
        }
    }

    // Registers any inputs from the joystick and translates it into actions for the UI
    private void CheckControllerBuffer()
    {
        ControllerInput controllerInput = ControllerInput.Instance;
        float axis;

        // Checks if the current UI uses the X axis or the Y axis to cycle through its options
        if(scrollUIButtonsHorizontally)
            axis = -controllerInput.Axis_X;
        else
            axis = controllerInput.Axis_Y;

        
        // If the game is paused or over, or if it's the main menu, and a joystick input is detected
        if((isPaused || isMainMenu || winScreen.activeSelf || loseScreen.activeSelf) && axis != 0)
        {
            // Checks if the player is holding the joystick in a direction and
            // decreases a short timer for a delay between cycling to other options
            if(UIButtonMoveTimer > 0)
            {
                UIButtonMoveTimer -= 0.01f;
            }

            // Otherwise, if the cycling to another option is allowed
            else
            {
                // Resets the timer in case the player still keeps holding the joystick afterwards
                UIButtonMoveTimer = UIButtonMoveCooldown;

                // Checks if the player is moving forward through the UI options and
                // selects the next one
                if(axis < 0)
                {
                    currentButtonIndex += 1;

                    if(currentButtonIndex >= currentButtons.Count)
                        currentButtonIndex = 0;

                    SelectUIElement(currentButtons[currentButtonIndex]);
                }

                // Checks if the player is moving backwards through the UI options and
                // selects the previous one
                else if(axis > 0)
                {
                    currentButtonIndex -= 1;

                    if(currentButtonIndex < 0)
                        currentButtonIndex = currentButtons.Count - 1;

                    SelectUIElement(currentButtons[currentButtonIndex]);
                }
            }
        }

        // If the game is paused or is the main menu, a joystick horizontal input is detected, and the
        // current UI option is a slider
        else if((isPaused || isMainMenu) && controllerInput.Axis_X != 0 &&
            currentButtons[currentButtonIndex].GetComponent<Slider>())
        {
            Slider slider = currentButtons[currentButtonIndex].GetComponent<Slider>();
            float axis_X = controllerInput.Axis_X;

            // If the input is allowed, checks the direction of the axis and
            // either increases or decreases the slider's value accordingly
            if(UIButtonMoveTimer <= 0)
            { 
                if(axis_X > 0)
                    slider.value += 5;

                else
                    slider.value -= 5;

                // Resets the timer for if the player keeps holding the direction
                UIButtonMoveTimer = UIButtonMoveCooldown;
            }

            // If the movement is on cooldown, decreases the timer
            else UIButtonMoveTimer -= Time.deltaTime;
        }

        // When the joystick is released, immediately resets the timer
        else UIButtonMoveTimer = 0f;


        // When the green button is pressed, plays this code only on the first frame of the press
        if (controllerInput.Button1Trigger && !button1Buffer)
        {
            // If it's not the main menu, this button pauses the game or
            // returns to the pause menu if already paused
            if(!isMainMenu)
            {
                TogglePause();
                button1Buffer = true;
            }

            // If it's in the main menu, this button makes the player return to the main screen
            else
            {
                pauseMenu.SetActive(true);
                settingsMenu.SetActive(false);
                creditsMenu.SetActive(false);
                levelSelectMenu.SetActive(false);
                
                SetCurrentUIButtons(pauseMenuButtons);
            }
        }

        // Resets the buffer once the button is released
        else if (!controllerInput.Button1Trigger && button1Buffer)
            button1Buffer = false;


        // When the blue button is pressed and the game isn't paused, over, or isn't the main menu,
        // plays this code only on the first frame of the press
        if(controllerInput.Button2Trigger && !button2Buffer && (isPaused || isMainMenu || winScreen.activeSelf || loseScreen.activeSelf))
        {
            // Checks if any UI option is currently selected and if it's a button
            if(eventSystem.currentSelectedGameObject != null && eventSystem.currentSelectedGameObject.GetComponent<Button>())
            {
                // Presses the button
                eventSystem.currentSelectedGameObject.GetComponent<Button>().onClick.Invoke();
                button2Buffer = true;
            }
        }

        // Resets the buffer once the button is released
        else if (!controllerInput.Button2Trigger && button2Buffer)
            button2Buffer = false;
    }

    // Triggers the win screen and the winning fanfare, and adds score for winning
    public void Win()
    {
        UIButtonMoveTimer = UIButtonMoveCooldown;

        winScreen.SetActive(true);
        SetCurrentUIButtons(winScreenButtons);
        levelAudioManager.PlayLevelWin();
        AddScore(Mathf.Max(10, 100 - (int)time));
    }

    // Triggers the lose screen and the losing fanfare, then resets the score
    public void Lose()
    {
        UIButtonMoveTimer = UIButtonMoveCooldown;

        loseScreen.SetActive(true);
        SetCurrentUIButtons(loseScreenButtons);
        levelAudioManager.PlayLevelLose();
        scoreText.text = "Pontos: 00000";
    }

    // Updates the score when an objective is grabbed and destroys that objective afterwards
    public void GetObjective(GameObject objective)
    {
        levelAudioManager.PlayItemPickup();
        AddScore(Mathf.Max(5, 50 - (int)time));
        
        Destroy(objective);
    }

    // Updates the Objective display on the HUD
    public void UpdateObjectiveText(int gottenObjectives, int totalObjectives)
    {
        objectivesText.text = $"{gottenObjectives} / {totalObjectives}";
    }

    // Pauses the game if unpaused, or unpauses if paused, or
    // returns to the pause screen if it's paused but on another screen
    public void TogglePause()
    {   
        UIButtonMoveTimer = UIButtonMoveCooldown;

        // Checks if the game isn't over
        if(!winScreen.activeSelf && !loseScreen.activeSelf)
        {
            // Disables any other active menus
            settingsMenu.SetActive(false);

            // Toggles the pause menu
            pauseMenu.SetActive(!pauseMenu.activeSelf);
            
            // If the pause menu is active, updates the UI buttons for the controller to cycle through
            if(pauseMenu.activeSelf)
                SetCurrentUIButtons(pauseMenuButtons);

            isPaused = pauseMenu.activeSelf;

            // Updates the time scale of the game
            CheckTimeScale();
        }
    }

    // Stops or Resumes the movements and physics of the game
    // depending on whether the game is paused or not, respectively
    private void CheckTimeScale()
    {
        if (isPaused)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }

    // Enables the settings menu and hides the pause menu, and updates the UI buttons for the controller
    public void ToggleSettingsMenu()
    {
        settingsMenu.SetActive(!settingsMenu.activeSelf);
        pauseMenu.SetActive(!settingsMenu.activeSelf);

        if(settingsMenu.activeSelf)
            SetCurrentUIButtons(settingsMenuButtons);
        else
            SetCurrentUIButtons(pauseMenuButtons);
    }

    // Enables the credits menu and hides the main menu, and updates the UI buttons for the controller
    public void ToggleCreditsMenu()
    {
        creditsMenu.SetActive(!creditsMenu.activeSelf);
        pauseMenu.SetActive(!creditsMenu.activeSelf);
        
        if(creditsMenu.activeSelf)
            SetCurrentUIButtons(creditsMenuButtons);
        else
            SetCurrentUIButtons(pauseMenuButtons);
    }

    // Enables the level select screen and hides the main menu, and updates the UI buttons for the controller
    public void ToggleLevelSelectMenu()
    {
        levelSelectMenu.SetActive(!levelSelectMenu.activeSelf);
        pauseMenu.SetActive(!levelSelectMenu.activeSelf);
        
        if(levelSelectMenu.activeSelf)
            SetCurrentUIButtons(levelSelectMenuButtons, true);
        else
            SetCurrentUIButtons(pauseMenuButtons);
    }

    // Loads the given level
    public void LoadScene(int n)
    {
        SceneManager.LoadScene(n);
    }

    // Toggles the directional guide and saves the setting
    public void ToggleDirectionalArrows()
    {
        arrowsEnabled = !arrowsEnabled;
        arrowsCheckmark.SetActive(arrowsEnabled);

        if (arrowsEnabled)
            PlayerPrefs.SetInt("Arrows", 1);
        else
            PlayerPrefs.SetInt("Arrows", 0);

            
        PlayerPrefs.Save();
        if(!isMainMenu)
            UpdateSettings();
    }

    // Toggles the relevancy glows and saves the setting
    public void ToggleHelpingGlow()
    {
        glowEnabled = !glowEnabled;
        glowCheckmark.SetActive(glowEnabled);

        if(glowEnabled)
            PlayerPrefs.SetInt("Glow", 1);
        else
            PlayerPrefs.SetInt("Glow", 0);

            
        PlayerPrefs.Save();
        if(!isMainMenu)
            UpdateSettings();
    }

    // Changes the volume of the game and saves the setting
    public void ChangeVolume()
    {
        volumeSliderValue = (int) volumeSlider.GetComponent<Slider>().value;
        levelAudioManager.SetVolume(volumeSliderValue);
        MenuAudioManager.SetVolume(volumeSliderValue);
        
        PlayerPrefs.SetInt("Volume", volumeSliderValue);
        PlayerPrefs.Save();
    }

    // Changes the global speed of all enemies and saves the setting
    public void ChangeEnemySpeed()
    {
        enemySpeedSliderValue = (int) enemySpeedSlider.GetComponent<Slider>().value;

        PlayerPrefs.SetInt("EnemySpeed", enemySpeedSliderValue);
        PlayerPrefs.Save();
    }

    // Toggles all active enemies between active or sleeping and saves the setting
    public void ToggleEnemies()
    {
        enemiesEnabled = !enemiesEnabled;
        enemiesCheckmark.SetActive(enemiesEnabled);

        if(enemiesEnabled)
            PlayerPrefs.SetInt("EnemyToggle", 1);
        else
            PlayerPrefs.SetInt("EnemyToggle", 0);

        PlayerPrefs.Save();

        if(!isMainMenu)
            UpdateSettings();
    }

    // Updates all features based on the player's settings
    private void UpdateSettings()
    {
        // Toggles the directional guide
        foreach(GameObject go in directionalArrows)
        {
            if(go)
                go.SetActive(arrowsEnabled);
        }

        // Toggles the relevancy glows
        foreach(GameObject go in helpingGlows)
        {
            if(go)
                go.SetActive(glowEnabled);
        }

        // Toggles all enemies that weren't already asleep between active or sleeping
        if(enemies == null) Start();
        foreach(EnemyMovement e in enemies)
        {
            if(!e.OriginallySleeping)
                e.sleeping = !enemiesEnabled;
        }
    }

    // Updates the current list of UI buttons for the controller to cycle through
    public void SetCurrentUIButtons(List<GameObject> buttons, bool scrollHorizontally = false)
    {
        currentButtons = buttons;
        currentButtonIndex = 0;
        SelectUIElement(currentButtons[0]);
        scrollUIButtonsHorizontally = scrollHorizontally;
    }

    // Adds points to the player's score in a level and fills the missing spaces with 0's
    public void AddScore(int amount)
    {
        string scoreString = "Pontos: ";
        score += amount;


        for(int i = 100; i >= 10; i /= 10)
        {
            if (score < i)
                scoreString += "0";
            else break;
        }

        scoreString += score;

        scoreText.text = scoreString;
        winScoreText.text = scoreString;
    }

    // Selects a given UI element and updates it's visual to indicate it
    public void SelectUIElement(GameObject selected)
    {
        if(currentSelectionGlow != null)
            currentSelectionGlow.SetActive(false);
        
        eventSystem.SetSelectedGameObject(selected);
        
        currentSelectionGlow = selected.transform.Find("ButtonSelected").gameObject;
        currentSelectionGlow.SetActive(true);
    }

    // Closes the game
    public void QuitGame()
    {
        Application.Quit();
    }
}

