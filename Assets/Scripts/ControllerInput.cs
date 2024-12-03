using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;
using System.Linq;
using TMPro;

public class ControllerInput : MonoBehaviour
{
    public static ControllerInput Instance;
    private SerialPort serial;
    private UIManager uiManager;
    private string arduinoInput;
    private bool usingArduino;
    public bool UsingArduino {get => usingArduino;}
    private int portsAvailable = 0;

    private bool button1Trigger = false;
    private bool button2Trigger = false;
    private float axis_X = 0f;
    private float axis_Y = 0f;

    public bool Button1Trigger {get => button1Trigger;}
    public bool Button2Trigger {get => button2Trigger;}
    public float Axis_X {get => axis_X;}
    public float Axis_Y {get => axis_Y;}

    private void Start()
    {
        UpdateUIManager();

        if(Instance != null)
        {
            Destroy(gameObject);
        }

        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            portsAvailable = SerialPort.GetPortNames().Count();
            OpenSerial();
        }
    }

    private void Update()
    {
        if(usingArduino)
        {
            if(serial != null && serial.BytesToRead > 0)
            {
                arduinoInput = serial.ReadLine();

                if(arduinoInput.Contains("BUTTON1_DOWN"))
                    button1Trigger = true;

                if(arduinoInput.Contains("BUTTON1_UP"))
                    button1Trigger = false;

                if(arduinoInput.Contains("BUTTON2_DOWN"))
                    button2Trigger = true;

                if(arduinoInput.Contains("BUTTON2_UP"))
                    button2Trigger = false;

                if(arduinoInput.Contains("JOYSTICK_X: "))
                {
                    string newString = arduinoInput.Remove(0,"JOYSTICK_X: ".Count());
                    float value = float.Parse(newString);

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


                    if(value > 0)
                        axis_Y = 1;
                    else if(value < 0)
                        axis_Y = -1;
                    else
                        axis_Y = 0;
                }
            }
        }

        else
        {
            if(Input.GetKeyDown(KeyCode.Escape))
                button1Trigger = true;
                
            if(Input.GetKeyUp(KeyCode.Escape))
                button1Trigger = false;

            if(Input.GetKeyDown(KeyCode.Q))
                button2Trigger = true;
                
            if(Input.GetKeyUp(KeyCode.Q))
                button2Trigger = false;

            axis_X = Input.GetAxisRaw("Strafe");
            axis_Y = Input.GetAxisRaw("Forward");
        }
    }

    public void OpenSerial()
    {
        CloseSerial();

        foreach(string s in SerialPort.GetPortNames())
        {
            try
            {
                serial = new SerialPort(s, 9600);
                serial.Open();
                usingArduino = true;
                uiManager.ToggleInputCheckmarks(true);
                Debug.Log($"Connected to Arduino on port \"{s}\".");
                break;
            }

            catch (Exception e)
            {
                Debug.LogWarning($"Unable to open port \"{s}\": {e}");
            }
        }

        if(!usingArduino)
        {
            uiManager.ToggleInputCheckmarks(false);
            Debug.LogWarning("Unable to connect to a port - Inputs will switch to the PC Input system.");
        }
    }

    public void CloseSerial()
    {
        uiManager.ToggleInputCheckmarks(false);
        
        if(usingArduino)
        {
            serial.Close();
            usingArduino = false;
            Debug.Log($"Disconnected from Arduino.");
        }
    }

    private void UpdateUIManager()
    {
        uiManager = FindAnyObjectByType<UIManager>();
    }
}
