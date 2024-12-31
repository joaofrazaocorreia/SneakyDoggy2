using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFaceCamera : MonoBehaviour
{
    private Transform playerCamera;


    private void Start()
    {
        playerCamera = GameObject.Find("Main Camera").transform;
    }


    private void Update()
    {
        // Makes the attached UI always face the Main Camera of the level
        
        Vector3 toTarget = playerCamera.position - transform.position;
        Vector3 rotation = Quaternion.LookRotation(toTarget).eulerAngles;

        transform.rotation = Quaternion.Euler(rotation);
    }
}
