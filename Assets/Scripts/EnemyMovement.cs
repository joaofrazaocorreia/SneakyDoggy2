using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    private NavMeshAgent navMeshAgent;
    private UIManager UIManager;
    private float aggroTimer;
    private float idleTimer;
    private Transform currentTarget;
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        UIManager = player.uiManager;
        aggroTimer = 0f;
        idleTimer = 0f;
        currentTarget = movementTargets[Random.Range(0, movementTargets.Length)];
        moveSpeed *= 3f;

        navMeshAgent.SetDestination(currentTarget.position);
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

                navMeshAgent.SetDestination(currentTarget.position);
            }

            if (aggroTimer > 0 && toTarget.magnitude > 2f && (!player.playingDead || toTarget.magnitude > aggroDistance))
            {
                Vector3 motion = toTarget;
                motion.Normalize();
                navMeshAgent.speed = moveSpeed;

                if (motion != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(motion);
                    transform.GetChild(0).rotation = rotation;
                }
            }

            else if (aggroTimer > 0 && player.playingDead && toTarget.magnitude <= aggroDistance)
            {
                navMeshAgent.speed = 0f;
                aggroTimer -= Time.deltaTime;

                if (aggroTimer <= 0)
                {
                    Transform prevTarget = currentTarget;

                    while (currentTarget == prevTarget)
                        currentTarget = movementTargets[Random.Range(0, movementTargets.Length)];

                    navMeshAgent.SetDestination(currentTarget.position);
                }

            }

            else if (aggroTimer <= 0 && toTarget.magnitude > 2f && idleTimer <= 0)
            {
                Vector3 motion = toTarget;
                motion.Normalize();
                navMeshAgent.speed = moveSpeed;

                if (motion != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(motion);
                    transform.GetChild(0).rotation = rotation;
                }
            }

            else if (aggroTimer <= 0 && toTarget.magnitude <= 2f && idleTimer <= 0)
            {
                idleTimer = idleTime;
            }

            else
            {
                navMeshAgent.speed = 0f;
                idleTimer -= Time.deltaTime;

                if (idleTimer <= 0)
                {
                    Transform prevTarget = currentTarget;

                    while (currentTarget == prevTarget)
                        currentTarget = movementTargets[Random.Range(0, movementTargets.Length)];

                    navMeshAgent.SetDestination(currentTarget.position);
                }
            }
        }

        else if(!rb.isKinematic)
            navMeshAgent.speed = 0f;
    }

    public void ToggleSpectateCamera()
    {
        cameraRig.SetActive(!cameraRig.activeSelf);
    }
}
