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
    public Transform[] movementTargets;
    public PlayerMovement player;
    public GameObject cameraRig;
    [SerializeField] private ParticleSystem sleepParticles;
    [SerializeField] private Slider moveSpeedSlider;
    [SerializeField] private AudioClip detectAudio;
    [SerializeField] private AudioClip moveAudio;
    [SerializeField] private Animator anim;

    private Rigidbody rb;
    private NavMeshAgent navMeshAgent;
    private AudioSource audioSource;
    private UIManager UIManager;
    private float aggroTimer;
    private float idleTimer;
    private Transform currentTarget;
    private bool originallySleeping;
    public bool OriginallySleeping {get => originallySleeping;}
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        UIManager = player.uiManager;
        aggroTimer = 0f;
        idleTimer = 0f;
        currentTarget = movementTargets[Random.Range(0, movementTargets.Length)];
        moveSpeed *= 3f;
        originallySleeping = sleeping;

        navMeshAgent.SetDestination(currentTarget.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.gameStopped && !UIManager.isPaused && !rb.isKinematic && !sleeping)
        {
            anim.SetBool("Sleep", false);

            if(sleepParticles.isPlaying)
                sleepParticles.Stop();


            if (idleTimer > 0f)
            {
                anim.SetBool("Idle", true);
                anim.SetBool("Walk", false);
            }

            else
            {
                anim.SetBool("Walk", true);
                anim.SetBool("Idle", false);
            }
            Vector3 toTarget = currentTarget.position - transform.position;

            if ((player.transform.position - transform.position).magnitude <= aggroDistance && !player.playingDead)
            {
                if (currentTarget != player.transform)
                {
                    audioSource.PlayOneShot(detectAudio);
                    anim.SetTrigger("Spot");
                }
                
                aggroTimer = aggroTime;
                idleTimer = 0f;
                currentTarget = player.transform;
                toTarget = currentTarget.position - transform.position;

                navMeshAgent.SetDestination(currentTarget.position);
            }

            if (aggroTimer > 0 && toTarget.magnitude > 2f && (!player.playingDead || toTarget.magnitude > aggroDistance))
            {
                anim.SetBool("Walk", true);
                anim.SetBool("Idle", false);
                
                Vector3 motion = toTarget;
                motion.Normalize();
                navMeshAgent.speed = moveSpeed * (moveSpeedSlider.value/100);

                if (motion != Vector3.zero)
                {
                    Vector3 rotation = Quaternion.LookRotation(motion).eulerAngles;
                    rotation.x = 0f;
                    transform.GetChild(0).rotation = Quaternion.Euler(rotation);
                    
                }
            }

            else if (aggroTimer > 0 && player.playingDead && toTarget.magnitude <= aggroDistance)
            {
                anim.SetBool("Idle", true);
                anim.SetBool("Walk", false);

                navMeshAgent.speed = 0f;
                aggroTimer -= Time.deltaTime;

                if (aggroTimer <= 0)
                {
                    Transform prevTarget = currentTarget;

                    while (currentTarget == prevTarget)
                        currentTarget = movementTargets[Random.Range(0, movementTargets.Length)];
                    
                    anim.SetBool("Walk", true);
                    anim.SetBool("Idle", false);
                    navMeshAgent.SetDestination(currentTarget.position);
                    audioSource.PlayOneShot(moveAudio);
                }

            }

            else if (aggroTimer <= 0 && toTarget.magnitude > 2f && idleTimer <= 0)
            {
                anim.SetBool("Walk", true);
                anim.SetBool("Idle", false);

                Vector3 motion = toTarget;
                motion.Normalize();
                navMeshAgent.speed = moveSpeed * (moveSpeedSlider.value/100);

                if (motion != Vector3.zero)
                {
                    Vector3 rotation = Quaternion.LookRotation(motion).eulerAngles;
                    rotation.x = 0f;
                    transform.GetChild(0).rotation = Quaternion.Euler(rotation);
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
                    audioSource.PlayOneShot(moveAudio);
                }
            }
        }

        else if(!rb.isKinematic || sleeping)
        {
            navMeshAgent.speed = 0f;
            anim.SetBool("Sleep", true);

            if(!sleepParticles.isPlaying)
                sleepParticles.Play();
        }
    }

    public void ToggleSpectateCamera()
    {
        cameraRig.SetActive(!cameraRig.activeSelf);
    }
}
