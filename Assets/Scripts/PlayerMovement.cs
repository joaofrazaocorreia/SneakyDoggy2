using System.Linq;
using UnityEngine;

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
        height = transform.position.y;
    }

    private void Update()
    {
        // Makes sure the player is always at the same Y level every frame (overriding gravity)
        transform.position = new Vector3(transform.position.x, height, transform.position.z);

        // When Alt + G is pressed, the player triggers Godmode, disabling all enemies and gaining a speed boost
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.G) && !godmode)
        {
            godmode = true;

            foreach (EnemyMovement e in enemies.GetComponentsInChildren<EnemyMovement>())
                e.GetComponent<Rigidbody>().isKinematic = true;

            moveSpeed *= 3f;
            Debug.Log("Godmode");
        }


        // While the game isn't paused or over
        if (!gameStopped && !uiManager.isPaused)
        {
            // If the blue button is pressed, the player plays dead and stops moving
            if (ControllerInput.Instance.Button2Trigger && !uiManager.Button2Buffer)
            {
                playingDead = true;
                velocity = Vector3.zero;
                controller.enabled = false;
                anim.SetBool("PlayDead",true);
            }

            // If the blue button is not pressed, makes sure the player is not playing dead and can move
            else
            {
                playingDead = false;
                controller.enabled = true;
                anim.SetBool("PlayDead",false);
            }
            

            // Calculates the distance to the exit and to the closest objective in the level
            float distanceToExit = (exit.position - transform.position).magnitude;
            float closestObjectiveDistance = float.MaxValue;
            Transform closestObjective = exit;
            Vector3 arrowTargetPos;

            // Checks which objective is the closest to the player
            foreach(Transform t in objectives)
            {
                if(t && (t.position - transform.position).magnitude < closestObjectiveDistance)
                {
                    closestObjective = t;
                    closestObjectiveDistance = (t.position - transform.position).magnitude;
                }
            }

            // Calculates the direction of the directional guide
            arrowTargetPos = closestObjective.position - transform.position;

            // If the player has at least one objective and the
            // distance to the exit is lower than any other objective
            if(gottenObjectives > 0)
            {
                if(distanceToExit < closestObjectiveDistance)
                    arrowTargetPos = exit.position - transform.position;
            }

            // Rotates the directional guide towards the closest target
            directionalArrow.rotation = Quaternion.LookRotation(arrowTargetPos);
            directionalArrow.rotation = Quaternion.Euler(new Vector3(directionalArrow.rotation.eulerAngles.x, directionalArrow.rotation.eulerAngles.y - 90, directionalArrow.rotation.eulerAngles.z));


            // If the player isn't playing dead and can move
            if (!playingDead && controller.enabled)
            {
                // Rotates the player's model on the Y axis only (resets the X and Z axis)
                model.transform.localRotation = Quaternion.Euler(0, model.transform.eulerAngles.y, 0);
                
                // Reads the player's velocity from the joystick's position
                velocity.x = ControllerInput.Instance.Axis_X;
                velocity.z = ControllerInput.Instance.Axis_Y;

                // Determines the direction of the movement and calculates the speed
                velocity.Normalize();
                motion = velocity * moveSpeed * Time.deltaTime;

                // As long as the player is moving
                if (velocity != Vector3.zero)
                {
                    // Calculates the rotations of the player depending on the direction of the movement
                    float rotX = (velocity.x > 0.5 && velocity.x < 1) || (velocity.x < -0.5 && velocity.x > -1) ? velocity.x / Mathf.Sqrt(Mathf.Pow(velocity.x, 2)) * 0.5f : velocity.x;
                    float rotZ = (velocity.z > 0.5 && velocity.z < 1) || (velocity.z < -0.5 && velocity.z > -1) ? velocity.z / Mathf.Sqrt(Mathf.Pow(velocity.z, 2)) * 0.5f : velocity.z;
                    Quaternion rotation;
                    
                    // Due to a weird bug with the axis, manually sets the rotation of this specific position (walking up and left)
                    if (rotX == 0.5 && rotZ == -0.5)
                        rotation = Quaternion.Euler(0, 135, 0);

                    // Otherwise calculates the rotation as normal
                    else
                        rotation = Quaternion.Euler(0, rotX * 90 + Mathf.Min(rotZ, 0f) * 180, 0);

                    // Lastly, rotates the player's model to the calculated direction
                    model.transform.localRotation = rotation;
                }
            }

            // If the player is playing dead or can't move
            else
            {
                // Stops the movement and rotation
                motion = Vector3.zero;
                model.transform.localRotation = Quaternion.Euler(0, model.transform.eulerAngles.y, 0);
            }
        }

        // While the game is paused or over, stops the player
        else velocity = Vector3.zero;


        // Claculates the vector of the direction and distance of the movement in world space
        motion = transform.TransformVector(motion);
        
        // Moves the player using that vector
        if (controller.enabled)
        {
            controller.Move(motion);
        }

        // Checks if the player is moving and updates the animations accordingly
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

    // If the player touches the entrance of the level
    private void OnTriggerEnter(Collider collision)
    {
        // If the game isn't paused or over
        if (!gameStopped && !uiManager.isPaused)
        {
            // If the player touches the entrance and has at least one objective, the level is passed
            if (collision.transform.tag == "Entrance" && gottenObjectives > 0)
            {
                gameStopped = true;
                uiManager.Win();
            }
        }
    }

    // If the player is in contact with an object
    private void OnTriggerStay(Collider collision)
    {
        // If the game isn't paused or over
        if (!gameStopped && !uiManager.isPaused)
        {
            // If the player is touching an enemy and is not playing dead nor in godmode
            if (collision.transform.tag == "Enemy" && !playingDead && !godmode)
            {
                // If that enemy isn't sleeping, the level is lost
                EnemyMovement collidedEnemy = collision.transform.GetComponent<EnemyMovement>();
                if(!collidedEnemy.sleeping)
                {
                    gameStopped = true;
                    collidedEnemy.ToggleSpectateCamera();
                    gameObject.SetActive(false);
                    uiManager.Lose();
                }
            }

            // If the player is touching an objective, they grab it and it's registered
            if (collision.transform.tag == "Objective") 
            {
                gottenObjectives += 1;
                uiManager.GetObjective(collision.gameObject);
                uiManager.UpdateObjectiveText(gottenObjectives, objectives.Count());
            }
        }
    }
}

