using UnityEngine;
using UnityEngine.AI;

public class MonsterChase : MonoBehaviour
{
    public Transform target;
    public float detectionRange = 10f;
    public float wanderRadius = 5f;
    public float wanderInterval = 4f;

    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource footsteps;
    private float wanderTimer;
    private Vector3 startPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        footsteps = GetComponent<AudioSource>();
        startPosition = transform.position;
        wanderTimer = wanderInterval;
    }

    void Update()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= detectionRange)
        {
            // Chase player at running speed
            agent.speed = 2.5f;
            agent.SetDestination(target.position);
            agent.isStopped = false;
        }
        else
        {
            // Wander at walking speed
            agent.speed = 1f;

            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                wanderTimer += Time.deltaTime;
                if (wanderTimer >= wanderInterval)
                {
                    Vector3 randomPoint = startPosition + Random.insideUnitSphere * wanderRadius;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(randomPoint, out hit, wanderRadius, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                        agent.isStopped = false;
                    }
                    wanderTimer = 0f;
                }
            }
        }

        // Update animator based on actual intent, not residual velocity
        float speed = agent.desiredVelocity.magnitude;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            speed = 0;
        }
        animator.SetFloat("Speed", speed);

        // Footsteps based on actual movement
        if (agent.velocity.magnitude > 0.1f && !footsteps.isPlaying)
        {
            footsteps.Play();
        }
        else if (agent.velocity.magnitude <= 0.1f && footsteps.isPlaying)
        {
            footsteps.Stop();
        }
    }
}