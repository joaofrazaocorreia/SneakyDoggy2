using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static bool active = false;
    private void Awake()
    {
        if (!active)
        {
            DontDestroyOnLoad(gameObject);
            active = true;
        }
        
        else
            gameObject.SetActive(false);
    }
}
