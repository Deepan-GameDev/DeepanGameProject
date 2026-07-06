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
    public float patrolSpeed = 1.8f;
    public float patrolWaitTime = 1.5f;
    public float patrolPointDistance = 0.5f;

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

    private NavMeshAgent agent;
    private ZombieState currentState;

    private int currentPatrolIndex;
    private float patrolWaitTimer;

    private bool playerDead;
    private bool hasWokenUp;
    private bool hasScreamed;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        currentState = ZombieState.Sleeping;

        SetAnimation(false, false, false);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
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

        SetAnimation(true, false, false);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed;
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

        if (agent.remainingDistance <= patrolPointDistance)
        {
            patrolWaitTimer += Time.deltaTime;

            if (patrolWaitTimer >= patrolWaitTime)
            {
                patrolWaitTimer = 0f;

                currentPatrolIndex++;

                if (currentPatrolIndex >= patrolPoints.Length)
                {
                    currentPatrolIndex = 0;
                }

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

        Transform targetPoint = patrolPoints[currentPatrolIndex];

        if (targetPoint == null)
            return;

        agent.isStopped = false;
        agent.speed = patrolSpeed;

        agent.SetDestination(targetPoint.position);
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

        if (zombieAudioSource != null && screamSound != null)
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
        }
    }

    private void UpdateChase()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        agent.SetDestination(player.position);

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

    private void SetAnimation(
        bool walking,
        bool screaming,
        bool chasing)
    {
        if (zombieAnimator == null)
            return;

        zombieAnimator.SetBool("IsWalking", walking);
        zombieAnimator.SetBool("IsScreaming", screaming);
        zombieAnimator.SetBool("IsChasing", chasing);
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