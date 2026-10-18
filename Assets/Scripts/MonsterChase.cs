using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MonsterChase : MonoBehaviour
{
    [Header("Movement")]
    public float wanderRadius = 5f;
    public float wanderInterval = 4f;
    public float walkSpeed = 1f;
    public float runSpeed = 2.5f;

    [Header("Sight")]
    public float detectionRange = 10f;
    public float viewAngle = 120f;
    public float eyeHeight = 1.6f;
    public float playerAimHeight = 1.0f;
    public float loseTrackTime = 3f;
    public LayerMask sightMask = ~0;

    [Header("Attack")]
    public float attackRange = 1.8f;
    public float attackCooldown = 2f;
    public float attackDuration = 1.5f;
    public float hitDelay = 0.6f;
    public float hitRange = 2.2f;
    public float hitAngle = 60f;
    public int damage = 34;

    [Header("Audio")]
    public AudioSource voiceSource;     
    public AudioClip spottedSting;
    public float stingCooldown = 8f;
    public AudioClip swingClip;
    public float swingSoundDelay = 0.4f;
    public AudioClip impactClip;
    public AudioClip deathClip;
    public AudioClip bodyFallClip;
    public float bodyFallDelay = 1.0f;   // time into the Death clip when the body lands

    [Header("Ambient")]
    public AudioSource ambientSource;   
    public float ambientVolume = 0.6f;
    public float ambientFadeSpeed = 1.5f;

    [Header("Chase Sounds")]
    public AudioClip[] chaseClips;
    public Vector2 chaseSoundInterval = new Vector2(6f, 12f); // random pause between sounds

    [Header("Hearing")]
    public float hearingMultiplier = 1f;      // >1 hears from farther, <1 is partly deaf
    public float investigateSpeed = 1.5f;
    public float investigateWaitTime = 4f;    // how long it lingers at the noise

    [Header("Hit Reaction")]
    public float staggerDuration = 0.8f;   // roughly the Hit clip length

    private bool wasChasing;
    private float lastStingTime = -Mathf.Infinity;
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource footsteps;
    private Transform target;
    private PlayerHealth playerHealth;
    private Vector3 startPosition;
    private float wanderTimer;
    private float lastSeenTime = -Mathf.Infinity;
    private Vector3 lastKnownPosition;
    private bool isAttacking;
    private float lastAttackTime = -Mathf.Infinity;
    private float nextChaseSoundTime;
    private bool isInvestigating;
    private float investigateTimer;
    private EnemyHealth health;
    private bool isStaggered;
    private Coroutine attackCoroutine;
    private Coroutine staggerCoroutine;

    void OnEnable()
    {
        NoiseSystem.OnNoise += HearNoise;
        if (health == null) health = GetComponent<EnemyHealth>();
        if (health != null) health.OnDamaged += HandleDamaged;
        if (health != null) health.OnDied += HandleDied;
    }

    void OnDisable()
    {
        NoiseSystem.OnNoise -= HearNoise;
        if (health != null) health.OnDamaged -= HandleDamaged;
        if (health != null) health.OnDied -= HandleDied;
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        footsteps = GetComponent<AudioSource>();
        startPosition = transform.position;
        wanderTimer = wanderInterval;

        target = BootstrapLoader.Instance.Player;
        if (target != null) playerHealth = target.GetComponent<PlayerHealth>();

        if (ambientSource != null)
        {
            ambientSource.loop = true;
            ambientSource.volume = ambientVolume;
            ambientSource.Play();
        }
    }

    void Update()
    {
        if (isAttacking || isStaggered) return;
        if (isAttacking) return;

        float distance = target != null
            ? Vector3.Distance(transform.position, target.position)
            : Mathf.Infinity;

        bool canSee = CanSeePlayer(distance);
        if (canSee)
        {
            lastSeenTime = Time.time;
            lastKnownPosition = target.position;
        }

        if (canSee) isInvestigating = false;

        bool chasing = Time.time - lastSeenTime <= loseTrackTime;
        if (chasing && !wasChasing) OnSpotted();
        wasChasing = chasing;

        // Ambient fades out while chasing, back in when wandering
        if (ambientSource != null)
        {
            float targetVol = chasing ? 0f : ambientVolume;
            ambientSource.volume = Mathf.MoveTowards(ambientSource.volume, targetVol,
                ambientFadeSpeed * Time.deltaTime);
        }

        // Chase sounds with long random pauses
        if (chasing && chaseClips != null && chaseClips.Length > 0 && Time.time >= nextChaseSoundTime)
        {
            voiceSource.PlayOneShot(chaseClips[Random.Range(0, chaseClips.Length)]);
            nextChaseSoundTime = Time.time + Random.Range(chaseSoundInterval.x, chaseSoundInterval.y);
        }

        bool playerAlive = playerHealth == null || !playerHealth.IsDead;

        if (canSee && playerAlive && distance <= attackRange)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            FaceTarget();

            if (Time.time >= lastAttackTime + attackCooldown)
                attackCoroutine = StartCoroutine(AttackRoutine());
        }
        else if (chasing && playerAlive)
        {
            agent.speed = runSpeed;
            agent.isStopped = false;
            agent.SetDestination(canSee ? target.position : lastKnownPosition);
        }
        else if (isInvestigating)
        {
            Investigate();
        }
        else
        {
            Wander();
        }

        UpdateAnimator();
        UpdateFootsteps();
    }

    void Wander()
    {
        agent.speed = walkSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            wanderTimer += Time.deltaTime;
            if (wanderTimer >= wanderInterval)
            {
                Vector3 randomPoint = startPosition + Random.insideUnitSphere * wanderRadius;
                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                    agent.isStopped = false;
                }
                wanderTimer = 0f;
            }
        }
    }

    bool CanSeePlayer(float distance)
    {
        if (target == null || distance > detectionRange) return false;

        Vector3 eyes = transform.position + Vector3.up * eyeHeight;
        Vector3 aim = target.position + Vector3.up * playerAimHeight;
        Vector3 dir = aim - eyes;

        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
        if (Vector3.Angle(transform.forward, flatDir) > viewAngle * 0.5f) return false;

        if (Physics.Raycast(eyes, dir.normalized, out RaycastHit hit, dir.magnitude + 0.5f,
            sightMask, QueryTriggerInteraction.Ignore))
        {
            return hit.transform == target || hit.transform.IsChildOf(target);
        }

        return false;
    }

    void FaceTarget()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * 8f);
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        animator.SetFloat("Speed", 0f);
        animator.SetTrigger("Attack");

        // Swing whoosh, timed to the downswing
        float soundDelay = Mathf.Min(swingSoundDelay, hitDelay);
        yield return new WaitForSeconds(soundDelay);
        if (voiceSource != null && swingClip != null)
            voiceSource.PlayOneShot(swingClip);

        // Hit check when the hammer lands
        yield return new WaitForSeconds(hitDelay - soundDelay);
        CheckHit();

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - hitDelay));

        lastAttackTime = Time.time;
        isAttacking = false;
        agent.isStopped = false;
    }

    void CheckHit()
    {
        if (target == null) return;

        Vector3 toPlayer = target.position - transform.position;
        toPlayer.y = 0f;

        bool inRange = toPlayer.magnitude <= hitRange;
        bool inFront = Vector3.Angle(transform.forward, toPlayer) <= hitAngle;

        if (inRange && inFront && playerHealth != null)
        {
            if (voiceSource != null && impactClip != null)
                voiceSource.PlayOneShot(impactClip);
            playerHealth.TakeDamage(damage, transform.position);
        }
    }

    void UpdateAnimator()
    {
        float speed = agent.isStopped ? 0f : agent.desiredVelocity.magnitude;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            speed = 0f;
        animator.SetFloat("Speed", speed);
    }

    void UpdateFootsteps()
    {
        if (footsteps == null) return;

        bool moving = agent.velocity.magnitude > 0.1f;
        if (moving && !footsteps.isPlaying) footsteps.Play();
        else if (!moving && footsteps.isPlaying) footsteps.Stop();
    }

    void OnDrawGizmosSelected()
    {
        Vector3 eyes = transform.position + Vector3.up * eyeHeight;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 left = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle * 0.5f, 0) * transform.forward;
        Gizmos.DrawRay(eyes, left * detectionRange);
        Gizmos.DrawRay(eyes, right * detectionRange);
    }

    void OnSpotted()
    {
        nextChaseSoundTime = Time.time + Random.Range(chaseSoundInterval.x, chaseSoundInterval.y);
        if (voiceSource == null || spottedSting == null) return;
        if (Time.time < lastStingTime + stingCooldown) return;
        voiceSource.PlayOneShot(spottedSting);
        lastStingTime = Time.time;
    }
    void HearNoise(Vector3 position, float radius)
    {
        if (Time.time - lastSeenTime <= loseTrackTime) return; // already chasing, ignore
        if (Vector3.Distance(transform.position, position) > radius * hearingMultiplier) return;

        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            isInvestigating = true;
            investigateTimer = 0f;
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }

    void Investigate()
    {
        agent.speed = investigateSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            investigateTimer += Time.deltaTime;
            if (investigateTimer >= investigateWaitTime)
            {
                isInvestigating = false;
                wanderTimer = 0f;
            }
        }
    }

    void HandleDamaged(int amount, Vector3 hitPoint)
    {
        // Getting shot reveals the player's position
        if (target != null)
        {
            lastSeenTime = Time.time;
            lastKnownPosition = target.position;
            isInvestigating = false;
        }

        // Cancel a swing in progress
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
            isAttacking = false;
            lastAttackTime = Time.time;
            animator.ResetTrigger("Attack");
        }

        if (staggerCoroutine != null) StopCoroutine(staggerCoroutine);
        staggerCoroutine = StartCoroutine(StaggerRoutine());
    }

    IEnumerator StaggerRoutine()
    {
        isStaggered = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        animator.SetFloat("Speed", 0f);
        animator.SetTrigger("Hit");

        yield return new WaitForSeconds(staggerDuration);

        isStaggered = false;
        agent.isStopped = false;
        staggerCoroutine = null;
    }

    void HandleDied()
    {
        StopAllCoroutines();
        isAttacking = false;
        isStaggered = false;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.enabled = false;

        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Hit");
        animator.SetFloat("Speed", 0f);
        animator.SetTrigger("Die");

        if (voiceSource != null && deathClip != null)
        {
            voiceSource.Stop();                 // cut any chase sound mid-play
            voiceSource.PlayOneShot(deathClip);
        }

        if (ambientSource != null) ambientSource.Stop();

        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        StartCoroutine(BodyFallRoutine());

        enabled = false;

    }

    IEnumerator BodyFallRoutine()
    {
        yield return new WaitForSeconds(bodyFallDelay);
        if (voiceSource != null && bodyFallClip != null)
            voiceSource.PlayOneShot(bodyFallClip);
    }
}