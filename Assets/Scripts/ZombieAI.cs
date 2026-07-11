using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : MonoBehaviour
{
    private enum ZombieState { Patrol, Waiting, Screaming, Chase, Dead }

    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsScreaming = Animator.StringToHash("IsScreaming");
    private static readonly int IsChasing = Animator.StringToHash("IsChasing");
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameOverManager gameOverManager;
    [SerializeField] private Animator zombieAnimator;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 0.9f;
    [SerializeField] private float patrolAcceleration = 2.5f;
    [SerializeField] private float patrolWaitTime = 1.5f;
    [SerializeField] private float patrolPointDistance = 0.35f;

    [Header("Player Detection")]
    [SerializeField] private float detectionRange = 12f;
    [SerializeField, Range(1f, 180f)] private float fieldOfView = 100f;
    [SerializeField] private float eyeHeight = 1.6f;
    [SerializeField] private LayerMask detectionLayers = ~0;

    [Header("Scream")]
    [SerializeField] private float screamDuration = 2.5f;
    [SerializeField] private AudioSource zombieAudioSource;
    [SerializeField] private AudioClip screamSound;

    [Header("Chase")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float chaseAcceleration = 5.5f;
    [SerializeField] private float killDistance = 1f;

    [Header("Movement Animation")]
    [SerializeField] private float turnSpeed = 240f;
    [SerializeField] private float movingAnimationThreshold = 0.08f;

    private NavMeshAgent agent;
    private ZombieState state;
    private int patrolIndex;
    private bool hasScreamed;
    private bool playerDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (zombieAnimator == null) zombieAnimator = GetComponent<Animator>();

        // The agent owns translation only; this script is the single owner of yaw.
        agent.updateRotation = false;
        agent.autoBraking = true;
    }

    private void Start()
    {
        ActivateAnimator();
        state = ZombieState.Patrol;
        ConfigureAgent(patrolSpeed, patrolAcceleration, patrolPointDistance);
        GoToPatrolPoint();
    }

    private void Update()
    {
        if (playerDead || player == null) return;

        switch (state)
        {
            case ZombieState.Patrol:
                UpdatePatrol();
                break;
            case ZombieState.Chase:
                UpdateChase();
                break;
        }
    }

    private void ActivateAnimator()
    {
        if (zombieAnimator == null) return;
        zombieAnimator.enabled = true;
        zombieAnimator.Rebind();
        zombieAnimator.Update(0f);
        SetAnimation(false, false, false, 0f);
    }

    private void UpdatePatrol()
    {
        if (CanSeePlayer())
        {
            StartScream();
            return;
        }

        if (!CanUseAgent() || agent.pathPending) return;

        UpdateMovementAnimation(false);
        RotateTowards(agent.steeringTarget);

        if (agent.remainingDistance > agent.stoppingDistance || agent.velocity.sqrMagnitude > 0.01f) return;

        StopAgent(true);
        StartCoroutine(PatrolPauseRoutine());
    }

    private IEnumerator PatrolPauseRoutine()
    {
        state = ZombieState.Waiting;
        SetAnimation(false, false, false, 0f);
        yield return new WaitForSeconds(patrolWaitTime);

        if (playerDead) yield break;
        patrolIndex = patrolPoints != null && patrolPoints.Length > 0 ? (patrolIndex + 1) % patrolPoints.Length : 0;
        state = ZombieState.Patrol;
        ConfigureAgent(patrolSpeed, patrolAcceleration, patrolPointDistance);
        GoToPatrolPoint();
    }

    private void GoToPatrolPoint()
    {
        if (!CanUseAgent() || patrolPoints == null || patrolPoints.Length == 0) return;
        Transform point = patrolPoints[patrolIndex];
        if (point == null) return;

        if (NavMesh.SamplePosition(point.position, out NavMeshHit hit, 1.5f, agent.areaMask))
            agent.SetDestination(hit.position);
        else
            Debug.LogWarning($"{name}: patrol point '{point.name}' is not on this agent's NavMesh.", this);
    }

    private void StartScream()
    {
        if (hasScreamed) return;
        hasScreamed = true;
        StartCoroutine(ScreamRoutine());
    }

    private IEnumerator ScreamRoutine()
    {
        state = ZombieState.Screaming;
        StopAgent(true);
        SetAnimation(false, true, false, 0f);
        if (zombieAudioSource != null && screamSound != null) zombieAudioSource.PlayOneShot(screamSound);

        float elapsed = 0f;
        while (elapsed < screamDuration)
        {
            if (player != null) RotateTowards(player.position);
            elapsed += Time.deltaTime;
            yield return null;
        }

        StartChase();
    }

    private void StartChase()
    {
        state = ZombieState.Chase;
        ConfigureAgent(chaseSpeed, chaseAcceleration, killDistance);
    }

    private void UpdateChase()
    {
        if (!CanUseAgent()) return;
        agent.SetDestination(player.position);
        RotateTowards(agent.steeringTarget);
        UpdateMovementAnimation(true);

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude <= killDistance * killDistance) KillPlayer();
    }

    private void ConfigureAgent(float speed, float acceleration, float stoppingDistance)
    {
        if (!CanUseAgent()) return;
        agent.isStopped = false;
        agent.speed = speed;
        agent.acceleration = acceleration;
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;
        agent.updateRotation = false;
    }

    private void StopAgent(bool clearPath)
    {
        if (!CanUseAgent()) return;
        agent.isStopped = true;
        if (clearPath) agent.ResetPath();
    }

    private bool CanUseAgent() => agent != null && agent.enabled && agent.isOnNavMesh;

    private void RotateTowards(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
    }

    private void UpdateMovementAnimation(bool chasing)
    {
        float speed = CanUseAgent() ? agent.velocity.magnitude : 0f;
        bool moving = speed > movingAnimationThreshold;
        float referenceSpeed = chasing ? chaseSpeed : patrolSpeed;
        SetAnimation(!chasing && moving, false, chasing && moving, referenceSpeed > 0f ? speed / referenceSpeed : 0f);
    }

    private void SetAnimation(bool walking, bool screaming, bool chasing, float normalizedMoveSpeed)
    {
        if (zombieAnimator == null) return;
        zombieAnimator.SetBool(IsWalking, walking);
        zombieAnimator.SetBool(IsScreaming, screaming);
        zombieAnimator.SetBool(IsChasing, chasing);
        zombieAnimator.SetFloat(MoveSpeed, Mathf.Clamp(normalizedMoveSpeed, 0.01f, 1.25f));
    }

    private bool CanSeePlayer()
    {
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up;
        Vector3 toPlayer = target - eye;
        float distance = toPlayer.magnitude;
        if (distance > detectionRange) return false;

        Vector3 flatDirection = Vector3.ProjectOnPlane(toPlayer, Vector3.up);
        if (flatDirection.sqrMagnitude < 0.0001f || Vector3.Angle(transform.forward, flatDirection) > fieldOfView * 0.5f) return false;

        return Physics.Raycast(eye, toPlayer.normalized, out RaycastHit hit, distance, detectionLayers, QueryTriggerInteraction.Ignore)
            && (hit.transform == player || hit.transform.IsChildOf(player));
    }

    private void KillPlayer()
    {
        playerDead = true;
        state = ZombieState.Dead;
        StopAgent(true);
        SetAnimation(false, false, false, 0f);
        if (gameOverManager != null) gameOverManager.GameOver();
    }
}
