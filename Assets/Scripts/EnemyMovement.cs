using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyMovement : MonoBehaviour
{
    public bool sleeping = false;
    public float moveSpeed = 4f;
    public float idleTime = 5f;
    public float aggroTime = 2f;
    public float aggroDistance = 4f;
    public float detectReactionTime = 2f;
    public Transform[] movementTargets;
    public PlayerMovement player;
    public GameObject cameraRig;
    [SerializeField] private ParticleSystem sleepParticles;
    [SerializeField] private Slider moveSpeedSlider;
    [SerializeField] private AudioClip detectAudio;
    [SerializeField] private AudioClip moveAudio;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject detectSymbol;

    private Rigidbody rb;
    private NavMeshAgent navMeshAgent;
    private AudioSource audioSource;
    private UIManager UIManager;
    private float aggroTimer;
    private float idleTimer;
    private float detectReactionTimer;
    private Transform currentTarget;
    private bool originallySleeping;
    public bool OriginallySleeping {get => originallySleeping;}
    


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        UIManager = player.uiManager;
        aggroTimer = 0f;
        idleTimer = 0f;
        detectReactionTimer = 0f;
        currentTarget = movementTargets[Random.Range(0, movementTargets.Length)];
        originallySleeping = sleeping;
        detectSymbol.SetActive(false);

        // Initializes the movement by setting the first target as the spawn position
        navMeshAgent.SetDestination(currentTarget.position);
    }


    private void Update()
    {
        // Checks if the game isn't paused/over, if the player hasn't activated godmode, and if this enemy isn't sleeping
        if (!player.gameStopped && !UIManager.isPaused && !rb.isKinematic && !sleeping)
        {
            // Makes sure the sleep animation and particles are disabled
            anim.SetBool("Sleep", false);

            if(sleepParticles.isPlaying)
                sleepParticles.Stop();


            // Plays the idle animation while the enemy is at its destination
            if (idleTimer > 0f)
            {
                anim.SetBool("Idle", true);
                anim.SetBool("Walk", false);
            }

            // Plays the walking animation while the enemy is moving
            else
            {
                anim.SetBool("Walk", true);
                anim.SetBool("Idle", false);
            }

            // Calculates the distance and direction to the destination target
            Vector3 toTarget = currentTarget.position - transform.position;

            // Checks if the player is within detection distance or if this enemy is already alarmed but far from the player
            if (((player.transform.position - transform.position).magnitude <= aggroDistance)
                || (aggroTimer > 0 && toTarget.magnitude > aggroDistance))
            {
                // Checks if the player isn't playing dead
                if(!player.playingDead)
                {
                    // If it's the first time seeing the player, plays the spotting animation and alarms this enemy
                    if (currentTarget != player.transform)
                    {
                        audioSource.PlayOneShot(detectAudio);
                        anim.SetBool("EndSpot", false);
                        anim.SetTrigger("Spot");

                        // Begins a short timer where the enemy is stopped for a bit after detecting the player
                        detectSymbol.SetActive(true);
                        detectReactionTimer = detectReactionTime;
                        navMeshAgent.speed = 0f;
                        navMeshAgent.SetDestination(transform.position);
                    }
                    
                    // Resets/Begins the timer for the enemy chasing the player, ends the idle timer,
                    // and updates the destination as the player's current position
                    aggroTimer = aggroTime;
                    idleTimer = 0f;
                    currentTarget = player.transform;
                    toTarget = currentTarget.position - transform.position;
                }

                // While the enemy is still stopped after detecting the player
                if(detectReactionTimer > 0)
                {
                    // Plays the idle animation
                    anim.SetBool("Idle", true);
                    anim.SetBool("Walk", false);

                    // Lowers the timer
                    detectReactionTimer -= Time.deltaTime;

                    // Stops the navMeshAgent from moving to its target but still looks at it
                    navMeshAgent.speed = 0f;

                    // Once the timer hits 0, the movement resumes and the animations return to normal
                    if(detectReactionTimer <= 0)
                    {
                        anim.SetBool("Walk", true);
                        anim.SetBool("Idle", false);
                        anim.SetBool("EndSpot", true);

                        navMeshAgent.speed = moveSpeed * (moveSpeedSlider.value/100);
                    }
                }

                // If this enemy is not stopped from the spotting, it moves towards its target
                else
                {
                    navMeshAgent.SetDestination(currentTarget.position);
                }
            }


            // If this enemy is alarmed but isn't within stopping distance of the player, and the player is able to be caught
            if (aggroTimer > 0 && toTarget.magnitude > 1.5f &&
                (!player.playingDead || toTarget.magnitude > aggroDistance))
            {
                // Plays the walking animation
                anim.SetBool("Walk", true);
                anim.SetBool("Idle", false);
                
                // Calculates the distance and direction to move and the move speed
                Vector3 motion = toTarget;
                motion.Normalize();
                navMeshAgent.speed = moveSpeed * (moveSpeedSlider.value/100);

                // As long as the direction and distance aren't zero, the enemy looks towards its target
                if (motion != Vector3.zero)
                {
                    Vector3 rotation = Quaternion.LookRotation(motion).eulerAngles;
                    rotation.x = 0f;
                    transform.GetChild(0).rotation = Quaternion.Euler(rotation);
                    
                }
            }

            // If the enemy is alarmed but the player is playing dead 
            else if (aggroTimer > 0 && player.playingDead && toTarget.magnitude <= aggroDistance)
            {
                // Plays the idle animation
                anim.SetBool("Idle", true);
                anim.SetBool("Walk", false);

                // Stops the enemy in place and lowers the timer
                navMeshAgent.speed = 0f;
                aggroTimer -= Time.deltaTime;

                // Once the enemy is no longer alarmed, returns the enemy to normal
                if (aggroTimer <= 0)
                {
                    Transform prevTarget = currentTarget;
                    detectReactionTimer = 0;
                    anim.SetBool("EndSpot", true);
                    detectSymbol.SetActive(false);

                    // Resets the movement targets
                    while (currentTarget == prevTarget)
                        currentTarget = movementTargets[Random.Range(0, movementTargets.Length)];
                    
                    // Plays the walking animation and moves to a random target position
                    anim.SetBool("Walk", true);
                    anim.SetBool("Idle", false);
                    navMeshAgent.SetDestination(currentTarget.position);
                    audioSource.PlayOneShot(moveAudio);
                }

            }

            // If the enemy isn't alarmed and isn't withing stopping distance of its destination
            else if (aggroTimer <= 0 && toTarget.magnitude > 1.5f && idleTimer <= 0)
            {
                // Plays the walking animation
                anim.SetBool("Walk", true);
                anim.SetBool("Idle", false);

                // Sets forward as the direction to move and calculates the move speed
                Vector3 motion = transform.forward;
                motion.Normalize();
                navMeshAgent.speed = moveSpeed * (moveSpeedSlider.value/100);

                // The enemy looks forward while moving
                if (motion != Vector3.zero)
                {
                    Vector3 rotation = Quaternion.LookRotation(motion).eulerAngles;
                    rotation.x = 0f;
                    transform.GetChild(0).rotation = Quaternion.Euler(rotation);
                }
            }

            // If the enemy isn't alarmed, is within stopping distance of its target and isn't idle
            else if (aggroTimer <= 0 && toTarget.magnitude <= 2f && idleTimer <= 0)
            {
                // Begins the idle timer for the previous methods to detect this enemy as idle
                idleTimer = idleTime;
            }

            // In any other case, this enemy is counted as idle
            else
            {
                // Stops the enemy in place and decreases its idle timer
                navMeshAgent.speed = 0f;
                idleTimer -= Time.deltaTime;

                // If the idle timer reaches 0, picks a new movement target to move to
                if (idleTimer <= 0)
                {
                    Transform prevTarget = currentTarget;

                    while (currentTarget == prevTarget)
                        currentTarget = movementTargets[Random.Range(0, movementTargets.Length)];

                    
                    // Moves towards the new target
                    navMeshAgent.SetDestination(currentTarget.position);
                    audioSource.PlayOneShot(moveAudio);
                }
            }
        }

        // If the player is in godmode or if this enemy is sleeping
        else if(!rb.isKinematic || sleeping)
        {
            // Stops this enemy in place, plays the sleeping animation, and plays the particles
            navMeshAgent.speed = 0f;
            anim.SetBool("Sleep", true);

            if(!sleepParticles.isPlaying)
                sleepParticles.Play();
        }
    }

    // Changes the camera to the spectator vision when the player is caught
    public void ToggleSpectateCamera()
    {
        cameraRig.SetActive(!cameraRig.activeSelf);
    }
}
