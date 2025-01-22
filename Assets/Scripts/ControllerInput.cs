using UnityEngine;
using System.IO.Ports;
using System;
using System.Linq;

// Translates inputs from the controller into feedback for actions in the game
public class ControllerInput : MonoBehaviour
{
    public static ControllerInput Instance;
    private SerialPort serial;
    private string arduinoInput;
    private bool usingArduinoInput_X;
    private bool usingArduinoInput_Y;

    private bool button1Trigger = false;
    private bool button2Trigger = false;
    private float axis_X;
    private float axis_Y;
    private float prevAxis_X;
    private float prevAxis_Y;

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

            usingArduinoInput_X = false;
            usingArduinoInput_Y = false;
            axis_X = 0;
            axis_Y = 0;
            prevAxis_X = axis_X;
            prevAxis_Y = axis_Y;

            DontDestroyOnLoad(gameObject);

            #if !UNITY_WEBGL
            // Automatically checks all available ports for a serial to connect to
            // (Except in WebGL builds because it breaks everything)
            OpenSerial();
            #endif
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
        

        // If the game connected to a serial and receives a new line from it,
        // checks if it's a joystick position update and uses the exact value
        // of the joystick positions to update the movement axises.

        if(serial != null && serial.BytesToRead > 0)
        {
            // Reads the received line from the serial
            arduinoInput = serial.ReadLine();

            // Checks if the new line refers to the X axis of the joystick
            if(arduinoInput.Contains("JOYSTICK_X: "))
            {
                string newString = arduinoInput.Remove(0,"JOYSTICK_X: ".Count());
                float value = float.Parse(newString);

                // Placeholder code until joystick specific values are implemented
                if(value != 0)
                {
                    // Controller inputs have priority over keyboard inputs
                    usingArduinoInput_X = true;

                    if(value > 0)
                        axis_X = 1;

                    else if(value < 0)
                        axis_X = -1;
                }
                else
                {
                    // No input received, so the keyboard inputs can now be used
                    usingArduinoInput_X = false;

                    axis_X = 0;
                }
            }

            // Checks if the new line refers to the Y axis of the joystick
            if(arduinoInput.Contains("JOYSTICK_Y: "))
            {
                string newString = arduinoInput.Remove(0,"JOYSTICK_Y: ".Count());
                float value = float.Parse(newString);

                // Placeholder code until joystick specific values are implemented
                if(value != 0)
                {
                    // Controller inputs have priority over keyboard inputs
                    usingArduinoInput_Y = true;

                    if(value > 0)
                        axis_Y = 1;

                    else if(value < 0)
                        axis_Y = -1;
                }
                else
                {
                    // No input received, so the keyboard inputs can now be used
                    usingArduinoInput_Y = false;

                    axis_Y = 0;
                }
            }
        }


        // If serial didn't receive any new inputs (or if a serial wasn't found),
        // updates the movement axises based on the state of the keyboard's keys
        // as long as the joystick isn't currently being moved in the respective axis.

        float checkAxis_X = Input.GetAxisRaw("Strafe");
        float checkAxis_Y = Input.GetAxisRaw("Forward");

        if(!usingArduinoInput_X && checkAxis_X != prevAxis_X)
            axis_X = checkAxis_X;

        if(!usingArduinoInput_Y && checkAxis_Y != prevAxis_Y)
            axis_Y = checkAxis_Y;

        
        // Registers the previous position of the axises to prevent looping the key presses
        // (which would override the controller inputs)

        prevAxis_X = axis_X;
        prevAxis_Y = axis_Y;
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
                Debug.Log($"Connected to Arduino on port \"{s}\".");
                break;
            }

            catch (Exception e)
            {
                Debug.LogWarning($"Unable to open port \"{s}\": {e}");
            }
        }

        // If it was not possible to connect to any port, starts checking for keyboard inputs instead
        if(serial == null || !serial.IsOpen)
        {
            Debug.LogWarning("Unable to connect to a port - Inputs will switch to the PC Input system.");
        }
    }


    // Closes any serials that are currently open and resets the input mode to keyboard
    public void CloseSerial()
    {
        if(serial != null && serial.IsOpen)
        {
            serial.Close();
            Debug.Log($"Disconnected from Arduino.");
        }
    }
}
