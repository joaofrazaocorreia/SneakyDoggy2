using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float idleTime = 5f;
    public float aggroTime = 2f;
    public float aggroDistance = 4f;
    public Transform[] movementTargets;
    public PlayerMovement player;
    public GameObject cameraRig;

    private Rigidbody rb;
    private UIManager UIManager;
    private Vector3 movement;
    private float aggroTimer;
    private float idleTimer;
    private Transform currentTarget;
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        UIManager = player.uiManager;
        movement = new Vector3();
        aggroTimer = 0f;
        idleTimer = 0f;
        currentTarget = movementTargets[Random.Range(0, movementTargets.Length)];
        moveSpeed *= 3f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.gameStopped && !UIManager.isPaused && !rb.isKinematic)
        {
            Vector3 toTarget = currentTarget.position - transform.position;

            if ((player.transform.position - transform.position).magnitude <= aggroDistance && !player.playingDead)
            {
                aggroTimer = aggroTime;
                idleTimer = 0f;
                currentTarget = player.transform;
                toTarget = currentTarget.position - transform.position;
            }

            if (aggroTimer > 0 && toTarget.magnitude > 2f && (!player.playingDead || toTarget.magnitude > aggroDistance))
            {
                movement = toTarget;
                movement.Normalize();
                rb.velocity = movement * moveSpeed;

                if (movement != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(movement);
                    transform.GetChild(0).rotation = rotation;
                }
            }

            else if (aggroTimer > 0 && player.playingDead && toTarget.magnitude <= aggroDistance)
            {
                rb.velocity = Vector3.zero;
                aggroTimer -= Time.deltaTime;

                if (aggroTimer <= 0)
                {
                    Transform prevTarget = currentTarget;

                    while (currentTarget == prevTarget)
                        currentTarget = movementTargets[Random.Range(0, movementTargets.Length)];
                }

            }

            else if (aggroTimer <= 0 && toTarget.magnitude > 2f && idleTimer <= 0)
            {
                movement = toTarget;
                movement.Normalize();
                rb.velocity = movement * moveSpeed;

                if (movement != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(movement);
                    transform.GetChild(0).rotation = rotation;
                }
            }

            else if (aggroTimer <= 0 && toTarget.magnitude <= 2f && idleTimer <= 0)
            {
                idleTimer = idleTime;
            }

            else
            {
                rb.velocity = Vector3.zero;
                idleTimer -= Time.deltaTime;

                if (idleTimer <= 0)
                {
                    Transform prevTarget = currentTarget;

                    while (currentTarget == prevTarget)
                        currentTarget = movementTargets[Random.Range(0, movementTargets.Length)];
                }
            }
        }

        else if(!rb.isKinematic)
            rb.velocity = Vector3.zero;
    }

    public void ToggleSpectateCamera()
    {
        cameraRig.SetActive(!cameraRig.activeSelf);
    }
}
