using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ZombieAI : MonoBehaviour
{
    private enum ZombieState
    {
        Sleeping,
        Patrol,
        Screaming,
        Chase
    }

    [Header("References")]
    public Transform player;
    public GameOverManager gameOverManager;
    public Animator zombieAnimator;

    [Header("Wake Up")]
    public string wakeUpTrigger = "WakeUp";
    public float wakeUpDuration = 3.2f;
    public bool wakeUpOnStart = true;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 1.3f;
    public float patrolWaitTime = 1.5f;
    public float patrolPointDistance = 0.35f;

    [Header("Player Detection")]
    public float detectionRange = 12f;
    public float fieldOfView = 100f;
    public float eyeHeight = 1.6f;
    public LayerMask detectionLayers = ~0;

    [Header("Scream")]
    public float screamDuration = 2.5f;
    public AudioSource zombieAudioSource;
    public AudioClip screamSound;

    [Header("Chase")]
    public float chaseSpeed = 3.5f;
    public float killDistance = 1.5f;

    [Header("Animation")]
    public float walkAnimationSpeedMultiplier = 1f;
    public float runAnimationSpeedMultiplier = 1f;
    public float turnSpeed = 360f;
    public float turnBeforeMoveAngle = 55f;

    private NavMeshAgent agent;
    private ZombieState currentState;

    private int currentPatrolIndex;
    private float patrolWaitTimer;

    private bool playerDead;
    private bool hasWokenUp;
    private bool hasScreamed;
    private bool reachedPatrolPoint;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (zombieAnimator == null)
        {
            zombieAnimator = GetComponent<Animator>();
        }
    }

    void Start()
    {
        currentState = ZombieState.Sleeping;

        SetAnimation(false, false, false);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.updateRotation = false;
        }

        if (wakeUpOnStart)
        {
            WakeUp();
        }
    }

    void Update()
    {
        if (player == null || playerDead)
            return;


        switch (currentState)
        {
            case ZombieState.Sleeping:
                break;

            case ZombieState.Patrol:
                UpdatePatrol();
                break;

            case ZombieState.Screaming:
                break;

            case ZombieState.Chase:
                UpdateChase();
                break;
        }
    }

    public void WakeUp()
    {
        if (hasWokenUp)
            return;

        hasWokenUp = true;

        StartCoroutine(WakeUpRoutine());
    }

    private IEnumerator WakeUpRoutine()
    {
        currentState = ZombieState.Sleeping;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        if (zombieAnimator != null)
        {
            zombieAnimator.SetTrigger(wakeUpTrigger);
        }

        yield return new WaitForSeconds(wakeUpDuration);

        StartPatrol();
    }

    private void StartPatrol()
    {
        currentState = ZombieState.Patrol;

        patrolWaitTimer = 0f;
        reachedPatrolPoint = false;

        SetAnimation(true, false, false);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed;
            agent.stoppingDistance = 0.1f;
            agent.autoBraking = true;
            agent.updateRotation = false;
        }

        GoToPatrolPoint();
    }

    private void UpdatePatrol()
    {
        if (CanSeePlayer())
        {
            StartScream();
            return;
        }

        if (agent == null || !agent.isOnNavMesh)
            return;

        if (agent.pathPending)
            return;

        UpdateFacingAndMovement(agent.steeringTarget, false);

        bool reachedDestination =
            !agent.hasPath ||
            agent.remainingDistance <= patrolPointDistance;

        if (!reachedPatrolPoint && reachedDestination)
        {
            reachedPatrolPoint = true;

            agent.isStopped = true;
            agent.ResetPath();

            SetAnimation(false, false, false);

            patrolWaitTimer = 0f;

        }

        if (reachedPatrolPoint)
        {
            patrolWaitTimer += Time.deltaTime;

            if (patrolWaitTimer >= patrolWaitTime)
            {
                currentPatrolIndex++;

                if (currentPatrolIndex >= patrolPoints.Length)
                {
                    currentPatrolIndex = 0;
                }

                reachedPatrolPoint = false;
                patrolWaitTimer = 0f;

                GoToPatrolPoint();
            }
        }
    }

    private void GoToPatrolPoint()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Transform targetPoint =
            patrolPoints[currentPatrolIndex];

        if (targetPoint == null)
            return;

        NavMeshHit navMeshHit;

        if (!NavMesh.SamplePosition(
            targetPoint.position,
            out navMeshHit,
            1.5f,
            NavMesh.AllAreas))
        {
            Debug.LogWarning(
                targetPoint.name +
                " NOT FOUND ON NAVMESH"
            );

            return;
        }

        NavMeshPath path = new NavMeshPath();

        bool pathFound = agent.CalculatePath(
            navMeshHit.position,
            path
        );

        if (!pathFound ||
            path.status != NavMeshPathStatus.PathComplete)
        {
            Debug.LogWarning(
                "NO COMPLETE PATH TO: " +
                targetPoint.name
            );

            return;
        }

        agent.isStopped = false;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.1f;
        agent.updateRotation = false;

        reachedPatrolPoint = false;

        agent.SetPath(path);

    }

    private void StartScream()
    {
        if (hasScreamed)
            return;

        hasScreamed = true;

        StartCoroutine(ScreamRoutine());
    }

    private IEnumerator ScreamRoutine()
    {
        currentState = ZombieState.Screaming;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        FacePlayer();

        SetAnimation(false, true, false);

        if (zombieAudioSource != null &&
            screamSound != null)
        {
            zombieAudioSource.PlayOneShot(screamSound);
        }

        yield return new WaitForSeconds(screamDuration);

        StartChase();
    }

    private void StartChase()
    {
        currentState = ZombieState.Chase;

        SetAnimation(false, false, true);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = chaseSpeed;
            agent.stoppingDistance = killDistance;
            agent.updateRotation = false;
        }
    }

    private void UpdateChase()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        agent.SetDestination(player.position);

        UpdateFacingAndMovement(player.position, true);

        float distanceToPlayer = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distanceToPlayer <= killDistance)
        {
            KillPlayer();
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 eyePosition =
            transform.position + Vector3.up * eyeHeight;

        Vector3 playerPosition =
            player.position + Vector3.up;

        Vector3 directionToPlayer =
            playerPosition - eyePosition;

        float distanceToPlayer =
            directionToPlayer.magnitude;

        if (distanceToPlayer > detectionRange)
            return false;

        Vector3 flatDirection = directionToPlayer;
        flatDirection.y = 0f;

        float angle = Vector3.Angle(
            transform.forward,
            flatDirection
        );

        if (angle > fieldOfView * 0.5f)
            return false;

        if (Physics.Raycast(
            eyePosition,
            directionToPlayer.normalized,
            out RaycastHit hit,
            distanceToPlayer,
            detectionLayers,
            QueryTriggerInteraction.Ignore))
        {
            return hit.transform == player ||
                   hit.transform.IsChildOf(player);
        }

        return false;
    }

    private void FacePlayer()
    {
        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void UpdateFacingAndMovement(Vector3 lookTarget, bool chasing)
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        Vector3 direction = lookTarget - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        float turnAngle = Vector3.Angle(transform.forward, direction);

        RotateTowards(direction);

        bool shouldMove = turnAngle <= turnBeforeMoveAngle;

        agent.isStopped = !shouldMove;

        SetAnimation(!chasing && shouldMove, false, chasing && shouldMove);
    }

    private void RotateTowards(Vector3 direction)
    {
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(direction),
            turnSpeed * Time.deltaTime);
    }

    private void SetAnimation(
        bool walking,
        bool screaming,
        bool chasing)
    {
        if (zombieAnimator == null)
            return;

        zombieAnimator.SetBool(
            "IsWalking",
            walking
        );

        zombieAnimator.SetBool(
            "IsScreaming",
            screaming
        );

        zombieAnimator.SetBool(
            "IsChasing",
            chasing
        );

        float movementRatio = GetMovementAnimationRatio(chasing);

        if (chasing)
        {
            zombieAnimator.speed = runAnimationSpeedMultiplier * movementRatio;
        }
        else if (walking)
        {
            zombieAnimator.speed = walkAnimationSpeedMultiplier * movementRatio;
        }
        else
        {
            zombieAnimator.speed = 1f;
        }
    }

    private float GetMovementAnimationRatio(bool chasing)
    {
        if (agent == null || !agent.isOnNavMesh)
            return 1f;

        float referenceSpeed = chasing ? chaseSpeed : patrolSpeed;

        if (referenceSpeed <= 0.01f)
            return 1f;

        float speedRatio = agent.velocity.magnitude / referenceSpeed;

        return Mathf.Clamp(speedRatio, 0.85f, 1.25f);
    }
    private void KillPlayer()
    {
        playerDead = true;

        SetAnimation(false, false, false);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        if (gameOverManager != null)
        {
            gameOverManager.GameOver();
        }
    }
}
