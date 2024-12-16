using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    public UIManager uiManager;

    [SerializeField] private float  moveSpeed;

    [SerializeField] private GameObject model;
    [SerializeField] private Transform enemies;
    [SerializeField] private Transform directionalArrow;
    [SerializeField] private Transform[] objectives;
    [SerializeField] private Transform exit;
    [SerializeField] private Animator anim;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 motion;
    private float height;
    private bool moving;
    private int gottenObjectives;
    private bool godmode;

    [HideInInspector] public bool gameStopped;
    [HideInInspector] public bool playingDead;

    private void Start()
    {
        controller     = GetComponent<CharacterController>();
        velocity       = Vector3.zero;
        motion         = Vector3.zero;
        moving         = false;
        gottenObjectives = 0;
        uiManager.UpdateObjectiveText(gottenObjectives, objectives.Count());
        gameStopped = false;
        playingDead = false;
        godmode = false;
        moveSpeed /= 18f;
        height = transform.position.y;
    }

    private void Update()
    {
        transform.position = new Vector3(transform.position.x, height, transform.position.z);

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.G) && !godmode)
        {
            godmode = true;

            foreach (EnemyMovement e in enemies.GetComponentsInChildren<EnemyMovement>())
                e.GetComponent<Rigidbody>().isKinematic = true;

            moveSpeed *= 3f;
            Debug.Log("Godmode");
        }


        if (!gameStopped && !uiManager.isPaused)
        {
            if (ControllerInput.Instance.Button2Trigger && !uiManager.Button2Buffer)
            {
                playingDead = true;
                velocity = Vector3.zero;
                controller.enabled = false;
                anim.SetBool("PlayDead",true);
            }

            else
            {
                playingDead = false;
                controller.enabled = true;
                anim.SetBool("PlayDead",false);
            }
            

            
            float distanceToExit = (exit.position - transform.position).magnitude;
            float closestObjectiveDistance = float.MaxValue;
            Transform closestObjective = exit;
            Vector3 arrowTargetPos;

            foreach(Transform t in objectives)
            {
                if(t && (t.position - transform.position).magnitude < closestObjectiveDistance)
                {
                    closestObjective = t;
                    closestObjectiveDistance = (t.position - transform.position).magnitude;
                }
            }

            arrowTargetPos = closestObjective.position - transform.position;

            if(gottenObjectives > 0)
            {
                if(distanceToExit < closestObjectiveDistance)
                    arrowTargetPos = exit.position - transform.position;
            }


            directionalArrow.rotation = Quaternion.LookRotation(arrowTargetPos);
            directionalArrow.rotation = Quaternion.Euler(new Vector3(directionalArrow.rotation.eulerAngles.x, directionalArrow.rotation.eulerAngles.y - 90, directionalArrow.rotation.eulerAngles.z));


            if (!playingDead && controller.enabled)
            {
                model.transform.localRotation = Quaternion.Euler(0, model.transform.eulerAngles.y, 0);
                

                velocity.x = ControllerInput.Instance.Axis_X;
                velocity.z = ControllerInput.Instance.Axis_Y;

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
                {
                    model.transform.localRotation = Quaternion.Euler(0, model.transform.eulerAngles.y, 0);
                    
                }
                else
                {
                    model.transform.localRotation = Quaternion.Euler(0, model.transform.eulerAngles.y, 0);
                   

                }
            }
        }

        else velocity = Vector3.zero;

        motion = transform.TransformVector(motion);
        
        if (controller.enabled)
        {
            controller.Move(motion);
            
        }


        moving = motion.z != 0f || motion.x != 0f;
        if (velocity != Vector3.zero)
        {
            anim.SetTrigger("Walk");
        }
        else if (playingDead == false && moving == false)
        {
            anim.SetTrigger("Idle");
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!gameStopped && !uiManager.isPaused)
        {
            if (collision.transform.tag == "Entrance" && gottenObjectives > 0)
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
            if (collision.transform.tag == "Enemy" && !playingDead && !godmode)
            {
                EnemyMovement collidedEnemy = collision.transform.GetComponent<EnemyMovement>();
                if(!collidedEnemy.sleeping)
                {
                    gameStopped = true;
                    collidedEnemy.ToggleSpectateCamera();
                    gameObject.SetActive(false);
                    uiManager.Lose();
                }
            }

            if (collision.transform.tag == "Objective") 
            {
                gottenObjectives += 1;
                uiManager.GetObjective(collision.gameObject);
                uiManager.UpdateObjectiveText(gottenObjectives, objectives.Count());
            }
        }
    }
}

