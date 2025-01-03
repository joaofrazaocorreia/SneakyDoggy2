using UnityEngine;
using System.IO.Ports;
using System;
using System.Linq;

// Translates inputs from the controller into feedback for actions in the game
public class ControllerInput : MonoBehaviour
{
    public static ControllerInput Instance;
    private SerialPort serial;
    private UIManager uiManager;
    private string arduinoInput;
    private bool usingArduinoSerial;
    public bool UsingArduino {get => usingArduinoSerial;}

    private bool button1Trigger = false;
    private bool button2Trigger = false;
    private float axis_X = 0f;
    private float axis_Y = 0f;

    public bool Button1Trigger {get => button1Trigger; set => button1Trigger = value;}
    public bool Button2Trigger {get => button2Trigger; set => button2Trigger = value;}
    public float Axis_X {get => axis_X;}
    public float Axis_Y {get => axis_Y;}

    private void Awake()
    {
        // As a static class, only one instance of this script can exist in the game at the same time
        if(Instance != null)
        {
            Destroy(gameObject);
        }

        // If this is the first instance, it remains loaded in the scene permanently
        // (and any others after it get destroyed)
        else
        {
            Instance = this;
            uiManager = FindAnyObjectByType<UIManager>();
            DontDestroyOnLoad(gameObject);


            // Automatically checks all available ports for a serial to connect to
            OpenSerial();
        }
    }

    private void Update()
    {
        // Checks for keyboard inputs of the two buttons

        if(Input.GetKeyDown(KeyCode.Escape))
            button1Trigger = true;
            
        if(Input.GetKeyUp(KeyCode.Escape))
            button1Trigger = false;

        if(Input.GetKeyDown(KeyCode.Q))
            button2Trigger = true;
            
        if(Input.GetKeyUp(KeyCode.Q))
            button2Trigger = false;
        

        // If serial reading is enabled, the code reads the exact value of the joystick positions
        if(usingArduinoSerial && serial != null && serial.BytesToRead > 0)
        {
            arduinoInput = serial.ReadLine();

            if(arduinoInput.Contains("JOYSTICK_X: "))
            {
                string newString = arduinoInput.Remove(0,"JOYSTICK_X: ".Count());
                float value = float.Parse(newString);

                // Temporary code until joystick specific values are implemented
                if(value > 0)
                    axis_X = 1;
                else if(value < 0)
                    axis_X = -1;
                else
                    axis_X = 0;
            }

            if(arduinoInput.Contains("JOYSTICK_Y: "))
            {
                string newString = arduinoInput.Remove(0,"JOYSTICK_Y: ".Count());
                float value = float.Parse(newString);

                // Temporary code until joystick specific values are implemented
                if(value > 0)
                    axis_Y = 1;
                else if(value < 0)
                    axis_Y = -1;
                else
                    axis_Y = 0;
            }
        }

        // If serial reading is disabled, checks for keyboard inputs (like the buttons)
        else if (!usingArduinoSerial)
        {
            axis_X = Input.GetAxisRaw("Strafe");
            axis_Y = Input.GetAxisRaw("Forward");
        }
    }

    // Connects to an available port and reads its serial
    public void OpenSerial()
    {
        // Must close any serials that are currently open
        CloseSerial();

        // Searches through every available port for a serial to open and read,
        // and connects to the first available one
        foreach(string s in SerialPort.GetPortNames())
        {
            try
            {
                serial = new SerialPort(s, 9600);
                serial.Open();
                usingArduinoSerial = true;
                Debug.Log($"Connected to Arduino on port \"{s}\".");
                break;
            }

            catch (Exception e)
            {
                Debug.LogWarning($"Unable to open port \"{s}\": {e}");
            }
        }

        // If it was not possible to connect to any port, starts checking for keyboard inputs instead
        if(!usingArduinoSerial)
        {
            Debug.LogWarning("Unable to connect to a port - Inputs will switch to the PC Input system.");
        }
    }

    // Closes any serials that are currently open and resets the input mode to keyboard
    public void CloseSerial()
    {
        if(usingArduinoSerial)
        {
            serial.Close();
            usingArduinoSerial = false;
            Debug.Log($"Disconnected from Arduino.");
        }
    }
}
