using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public UIManager uiManager;

    [SerializeField] private float  moveSpeed;
    [SerializeField] private float  rotationVelocityFactor;

    [SerializeField] private GameObject model;
    [SerializeField] private Transform enemies;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 motion;
    private float height;
    private bool    moving;
    private bool hasObjective;
    private bool godmode;

    [HideInInspector] public bool gameStopped;
    [HideInInspector] public bool playingDead;

    private void Start()
    {
        controller     = GetComponent<CharacterController>();
        velocity       = Vector3.zero;
        motion         = Vector3.zero;
        moving         = false;
        hasObjective = false;
        gameStopped = false;
        playingDead = false;
        godmode = false;
        moveSpeed /= 18f;
        height = transform.position.y;

        StartCoroutine(FixCollisions());
    }

    private void Update()
    {
        transform.position = new Vector3(transform.position.x, height, transform.position.z);

        if (Input.GetKeyDown(KeyCode.G) && !godmode)
        {
            godmode = true;

            foreach (EnemyMovement e in enemies.GetComponentsInChildren<EnemyMovement>())
                e.GetComponent<Rigidbody>().isKinematic = true;

            moveSpeed *= 3f;
            Debug.Log("Godmode");
        }
    }

    private void FixedUpdate()
    {
        if (!gameStopped && !uiManager.isPaused)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                playingDead = true;
                velocity = Vector3.zero;
            }

            else if (playingDead)
            {
                playingDead = false;
            }


            if (!playingDead && controller.enabled)
            {
                model.transform.localRotation = Quaternion.Euler(0, model.transform.eulerAngles.y, 0);


                velocity.x = Input.GetAxisRaw("Strafe");
                velocity.z = Input.GetAxisRaw("Forward");

                velocity.Normalize();
                motion = velocity * moveSpeed;

                if (velocity != Vector3.zero)
                {
                    float rotX = (velocity.x > 0.5 && velocity.x < 1) || (velocity.x < -0.5 && velocity.x > -1) ? velocity.x / Mathf.Sqrt(Mathf.Pow(velocity.x, 2)) * 0.5f : velocity.x;
                    float rotZ = (velocity.z > 0.5 && velocity.z < 1) || (velocity.z < -0.5 && velocity.z > -1) ? velocity.z / Mathf.Sqrt(Mathf.Pow(velocity.z, 2)) * 0.5f : velocity.z;
                    Quaternion rotation;

                    if (rotX == 0.5 && rotZ == -0.5)
                        rotation = Quaternion.Euler(0, 135, 0);

                    else
                        rotation = Quaternion.Euler(0, rotX * 90 + Mathf.Min(rotZ, 0f) * 180, 0);

                    model.transform.localRotation = rotation;
                }
            }

            else
            {
                motion = Vector3.zero;

                if (playingDead)
                    model.transform.localRotation = Quaternion.Euler(0, model.transform.eulerAngles.y, 90);
                else
                    model.transform.localRotation = Quaternion.Euler(0, model.transform.eulerAngles.y, 0);
            }
        }

        else velocity = Vector3.zero;

        motion = transform.TransformVector(motion);

        if(controller.enabled)
            controller.Move(motion);

        moving = motion.z != 0f || motion.x != 0f;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!gameStopped && !uiManager.isPaused)
        {
            if (collision.transform.tag == "Enemy" && !playingDead && !godmode)
            {
                gameStopped = true;
                collision.transform.GetComponent<EnemyMovement>().ToggleSpectateCamera();
                gameObject.SetActive(false);
                uiManager.Lose();
            }

            if (collision.transform.tag == "Entrance" && hasObjective)
            {
                gameStopped = true;
                uiManager.Win();
            }
        }
    }

    private void OnTriggerStay(Collider collision)
    {
        if (!gameStopped && !uiManager.isPaused)
        {
            if (collision.transform.tag == "Objective") 
            {
                hasObjective = true;
                uiManager.GetObjective(collision.gameObject);
            }
        }
    }

    private IEnumerator FixCollisions()
    {
        while (true)
        {
            controller.enabled = false;

            yield return new WaitForSeconds(0.01f);

            controller.enabled = true;

            yield return new WaitForSeconds(0.1f);
        }
    }
}

