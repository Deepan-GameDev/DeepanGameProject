using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : MonoBehaviour
{
    private enum ZombieState { Idle, Patrol, Waiting, Investigating, Screaming, Chase, Attacking, Dead }
    private enum LocomotionMode { Idle, Walk, Run }

    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsScreaming = Animator.StringToHash("IsScreaming");
    private static readonly int IsChasing = Animator.StringToHash("IsChasing");
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int Attack = Animator.StringToHash("Attack");

    private const string ScreamStateName = "Zombie Scream";
    private const string BiteStateName = "Zombie Neck Bite";

    private bool canMove;

    [Header("Hearing")]
    [SerializeField] private float walkHearingRange = 3f;
    [SerializeField] private float runHearingRange = 8f;

    private Vector3 lastHeardPosition;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameOverManager gameOverManager;
    [SerializeField] private Animator zombieAnimator;

    [Header("Startup")]
    [SerializeField] private float initialIdleTime = 0.75f;

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

    [Header("Investigation")]
    [SerializeField] private float investigateTime = 5f;
    [SerializeField] private float lookRotationSpeed = 120f;
    [SerializeField] private float investigationArriveDistance = 0.6f;

    [Header("Scream")]
    [SerializeField] private float screamDuration = 2.5f;
    [SerializeField] private AudioSource zombieAudioSource;
    [SerializeField] private AudioClip screamSound;

    [Header("Chase")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float chaseAcceleration = 5.5f;
    [SerializeField] private float killDistance = 1f;
    [SerializeField] private float loseSightDelay = 1.25f;

    [Header("Attack")]
    [SerializeField] private AudioSource attackAudioSource;
    [SerializeField] private AudioClip biteClip;
    [SerializeField] private float attackDuration = 1.3f;
    [SerializeField] private float biteSoundDelay = 1f;
    [SerializeField] private Image bloodOverlay;
    [SerializeField] private float bloodFadeInDuration = 0.25f;
    [SerializeField] private AudioSource playerScreamAudioSource;
    [SerializeField] private AudioClip playerScreamClip;

    [Header("Movement Animation")]
    [SerializeField] private float turnSpeed = 240f;
    [SerializeField] private float movingAnimationThreshold = 0.08f;
    [SerializeField] private float moveSpeedDampTime = 0.15f;
    [SerializeField] private float screamCrossFadeDuration = 0.18f;
    [SerializeField] private float biteCrossFadeDuration = 0.12f;

    [Header("Doors")]
    [SerializeField] private float doorOpenDistance = 1.5f;
    [SerializeField] private float doorCheckHeight = 1f;
    [SerializeField] private float doorCheckRadius = 0.3f;
    [SerializeField] private LayerMask doorDetectionLayers = ~0;

    [Header("Patrol Sound")]
    [SerializeField] private AudioSource patrolAudioSource;

    private NavMeshAgent agent;
    private ZombieState state;
    private Coroutine stateRoutine;
    private int patrolIndex;
    private bool playerDead;
    private bool isAttacking;
    private Vector3 lastKnownPlayerPosition;
    private float timeSincePlayerSeen;
    private Coroutine bloodOverlayRoutine;
    private readonly RaycastHit[] doorHits = new RaycastHit[4];

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private LocomotionMode currentAnimationMode;
    private bool currentScreaming;
    private bool animationInitialized;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        if (zombieAnimator == null)
            zombieAnimator = GetComponent<Animator>();

        agent.updateRotation = false;
        agent.autoBraking = true;
    }

    private void Start()
    {
        ActivateAnimator();
        StartStateRoutine(StartupRoutine());
    }

    private void Update()
    {
        if (playerDead || player == null || isAttacking || !canMove)
        {
            return;
        }

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
        animationInitialized = false;
        SetAnimation(LocomotionMode.Idle, 0f);
    }

    private IEnumerator StartupRoutine()
    {
        state = ZombieState.Idle;
        canMove = false;
        StopAgent(true);
        SetAnimation(LocomotionMode.Idle, 0f);

        yield return new WaitForSeconds(initialIdleTime);

        canMove = true;
        BeginPatrol();
    }

    private void UpdatePatrol()
    {
        if (TrySeePlayerAndScream())
        {
            return;
        }

        if (!CanUseAgent() || agent.pathPending)
        {
            return;
        }

        UpdateMovementAnimation(LocomotionMode.Walk, patrolSpeed);
        RotateTowards(agent.steeringTarget);

        if (HasArrived(patrolPointDistance))
        {
            StartStateRoutine(PatrolPauseRoutine());
        }
    }

    private IEnumerator PatrolPauseRoutine()
    {
        state = ZombieState.Waiting;
        StopAgent(true);
        SetAnimation(LocomotionMode.Idle, 0f);

        float elapsed = 0f;
        while (elapsed < patrolWaitTime)
        {
            if (TrySeePlayerAndScream())
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (playerDead)
        {
            yield break;
        }

        patrolIndex = patrolPoints != null && patrolPoints.Length > 0 ? (patrolIndex + 1) % patrolPoints.Length : 0;
        BeginPatrol();
    }

    private void BeginPatrol()
    {
        state = ZombieState.Patrol;
        ConfigureAgent(patrolSpeed, patrolAcceleration, patrolPointDistance);
        GoToPatrolPoint();
        UpdateMovementAnimation(LocomotionMode.Walk, patrolSpeed);
        if (patrolAudioSource != null && !patrolAudioSource.isPlaying)
        {
            patrolAudioSource.Play();
        }
    }

    private void GoToPatrolPoint()
    {
        if (!CanUseAgent() || patrolPoints == null || patrolPoints.Length == 0) return;

        Transform point = patrolPoints[patrolIndex];
        if (point == null) return;

        if (NavMesh.SamplePosition(point.position, out NavMeshHit hit, 1.5f, agent.areaMask))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning($"{name}: patrol point '{point.name}' is not on this agent's NavMesh.", this);
        }
    }

    private bool TrySeePlayerAndScream()
    {
        if (!CanSeePlayer())
        {
            return false;
        }

        lastKnownPlayerPosition = player.position;
        StartScream();
        return true;
    }

    private void StartScream()
    {
        if (state == ZombieState.Screaming || state == ZombieState.Attacking || state == ZombieState.Dead)
        {
            return;
        }

        StartStateRoutine(ScreamRoutine());
    }

    private IEnumerator ScreamRoutine()
    {
        state = ZombieState.Screaming;
        StopAgent(true);
        SetAnimation(LocomotionMode.Idle, 0f, true);

        if (zombieAnimator != null)
        {
            zombieAnimator.CrossFadeInFixedTime(ScreamStateName, screamCrossFadeDuration, 0, 0f);
        }

        if (zombieAudioSource != null && screamSound != null)
        {
            zombieAudioSource.PlayOneShot(screamSound);
        }

        float elapsed = 0f;
        while (elapsed < screamDuration)
        {
            if (player != null)
            {
                FaceTarget(player.position);
                lastKnownPlayerPosition = player.position;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        StartChase();
    }

    private void StartChase()
    {
        if (player == null || playerDead)
        {
            return;
        }

        state = ZombieState.Chase;
        timeSincePlayerSeen = 0f;
        ConfigureAgent(chaseSpeed, chaseAcceleration, killDistance);
        agent.SetDestination(player.position);
        UpdateMovementAnimation(LocomotionMode.Run, chaseSpeed);

        if (patrolAudioSource != null)
        {
            patrolAudioSource.Stop();
        }
    }

    private void UpdateChase()
    {
        if (!CanUseAgent())
        {
            return;
        }

        if (CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;
            timeSincePlayerSeen = 0f;
            agent.SetDestination(lastKnownPlayerPosition);
        }
        else
        {
            timeSincePlayerSeen += Time.deltaTime;
            agent.SetDestination(lastKnownPlayerPosition);

            if (timeSincePlayerSeen >= loseSightDelay && HasArrived(investigationArriveDistance))
            {
                StartStateRoutine(InvestigateRoutine(lastKnownPlayerPosition));
                return;
            }
        }

        TryOpenBlockingDoor();

        RotateTowards(agent.steeringTarget);
        UpdateMovementAnimation(LocomotionMode.Run, chaseSpeed);

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude <= killDistance * killDistance)
        {
            StartStateRoutine(AttackPlayer());
        }
    }

    private IEnumerator InvestigateRoutine(Vector3 position)
    {
        state = ZombieState.Investigating;
        ConfigureAgent(chaseSpeed, chaseAcceleration, investigationArriveDistance);

        if (!CanUseAgent())
        {
            yield break;
        }

        SetAnimation(LocomotionMode.Run, 1f);
        SetDestinationOnNavMesh(position, 1.5f);

        while (CanUseAgent() && (agent.pathPending || !HasArrived(investigationArriveDistance)))
        {
            if (TrySeePlayerAndScream())
            {
                yield break;
            }

            TryOpenBlockingDoor();
            RotateTowards(agent.steeringTarget);
            UpdateMovementAnimation(LocomotionMode.Run, chaseSpeed);
            yield return null;
        }

        StopAgent(true);
        SetAnimation(LocomotionMode.Idle, 0f);

        Quaternion startRotation = transform.rotation;
        Quaternion left = startRotation * Quaternion.Euler(0f, -45f, 0f);
        Quaternion right = startRotation * Quaternion.Euler(0f, 45f, 0f);

        if (yieldReturnIfPlayerSeen()) yield break;
        yield return RotateRoutine(left);
        if (yieldReturnIfPlayerSeen()) yield break;
        yield return RotateRoutine(right);
        if (yieldReturnIfPlayerSeen()) yield break;
        yield return RotateRoutine(startRotation);

        float elapsed = 0f;
        while (elapsed < investigateTime)
        {
            if (TrySeePlayerAndScream())
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        BeginPatrol();

        bool yieldReturnIfPlayerSeen()
        {
            return TrySeePlayerAndScream();
        }
    }

    private IEnumerator RotateRoutine(Quaternion target)
    {
        while (Quaternion.Angle(transform.rotation, target) > 1f)
        {
            if (TrySeePlayerAndScream())
            {
                yield break;
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, lookRotationSpeed * Time.deltaTime);
            yield return null;
        }

        float wait = 0.35f;
        while (wait > 0f)
        {
            if (TrySeePlayerAndScream())
            {
                yield break;
            }

            wait -= Time.deltaTime;
            yield return null;
        }
    }

    public void Investigate(Vector3 position)
    {
        if (!CanRespondToInvestigation())
        {
            return;
        }

        lastHeardPosition = position;
        StartStateRoutine(InvestigateRoutine(lastHeardPosition));
    }

    public void HearNoise(Vector3 noisePosition)
    {
        HearNoise(noisePosition, false);
    }

    public void HearNoise(Vector3 noisePosition, bool isRunningNoise)
    {
        if (!CanRespondToInvestigation())
        {
            return;
        }

        float hearingRange = isRunningNoise ? runHearingRange : walkHearingRange;
        Vector3 toNoise = noisePosition - transform.position;
        toNoise.y = 0f;
        if (toNoise.sqrMagnitude > hearingRange * hearingRange)
        {
            return;
        }

        lastHeardPosition = noisePosition;
        StartStateRoutine(InvestigateRoutine(lastHeardPosition));
    }

    private bool CanRespondToInvestigation()
    {
        return canMove &&
            state != ZombieState.Chase &&
            state != ZombieState.Screaming &&
            state != ZombieState.Attacking &&
            !playerDead &&
            !isAttacking;
    }

    private IEnumerator AttackPlayer()
    {
        if (isAttacking)
        {
            yield break;
        }

        state = ZombieState.Attacking;
        isAttacking = true;
        playerDead = true;

        StopAgent(true);
        FaceTarget(player.position);
        SetAnimation(LocomotionMode.Idle, 0f);
        PlayAttackEffect();

        Player playerComponent = player.GetComponent<Player>();
        if (playerComponent != null)
        {
            playerComponent.enabled = false;
        }

        if (zombieAnimator != null)
        {
            zombieAnimator.ResetTrigger(Attack);
            zombieAnimator.CrossFadeInFixedTime(BiteStateName, biteCrossFadeDuration, 0, 0f);
        }

        float elapsed = 0f;
        bool biteSoundPlayed = false;
        float duration = Mathf.Max(attackDuration, biteSoundDelay);

        while (elapsed < duration)
        {
            FaceTarget(player.position);

            if (!biteSoundPlayed && elapsed >= biteSoundDelay)
            {
                biteSoundPlayed = true;
                PlayBiteSound();
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!biteSoundPlayed)
        {
            PlayBiteSound();
        }

        if (GameManager.Instance != null)
        {
            yield return StartCoroutine(GameManager.Instance.PlayerDiedRoutine());
        }

        yield return null;

        if (GameManager.Instance == null || GameManager.Instance.currentLives <= 0)
        {
            state = ZombieState.Dead;
            yield break;
        }

       playerDead = false;
       isAttacking = false;
       canMove = false;
       ResetBloodOverlay();
       yield break;
    }

    private void PlayBiteSound()
    {
        if (attackAudioSource != null && biteClip != null)
        {
            attackAudioSource.PlayOneShot(biteClip);
        }
    }

    private void PlayAttackEffect()
    {
        if (playerScreamAudioSource != null && playerScreamClip != null)
        {
            playerScreamAudioSource.PlayOneShot(playerScreamClip);
        }

        if (bloodOverlay == null)
        {
            return;
        }

        if (bloodOverlayRoutine != null)
        {
            StopCoroutine(bloodOverlayRoutine);
        }

        bloodOverlay.gameObject.SetActive(true);
        bloodOverlayRoutine = StartCoroutine(FadeBloodOverlayIn());
    }

    private IEnumerator FadeBloodOverlayIn()
    {
        Color color = bloodOverlay.color;
        color.a = 0f;
        bloodOverlay.color = color;

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, bloodFadeInDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsed / duration);
            bloodOverlay.color = color;
            yield return null;
        }

        color.a = 1f;
        bloodOverlay.color = color;
        bloodOverlayRoutine = null;
    }

    private void ResetBloodOverlay()
    {
        if (bloodOverlay == null)
        {
            return;
        }

        if (bloodOverlayRoutine != null)
        {
            StopCoroutine(bloodOverlayRoutine);
            bloodOverlayRoutine = null;
        }

        Color color = bloodOverlay.color;
        color.a = 0f;
        bloodOverlay.color = color;
        bloodOverlay.gameObject.SetActive(false);
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
        agent.velocity = Vector3.zero;
        if (clearPath)
        {
            agent.ResetPath();
        }
    }

    private bool CanUseAgent()
    {
        return agent != null && agent.enabled && agent.isOnNavMesh;
    }

    private bool HasArrived(float arriveDistance)
    {
        if (!CanUseAgent() || agent.pathPending)
        {
            return false;
        }

        float distance = Mathf.Max(arriveDistance, agent.stoppingDistance);
        return agent.remainingDistance <= distance &&
            (!agent.hasPath || agent.velocity.sqrMagnitude <= movingAnimationThreshold * movingAnimationThreshold);
    }

    private bool SetDestinationOnNavMesh(Vector3 position, float sampleDistance)
    {
        if (!CanUseAgent())
        {
            return false;
        }

        if (NavMesh.SamplePosition(position, out NavMeshHit hit, sampleDistance, agent.areaMask))
        {
            return agent.SetDestination(hit.position);
        }

        return agent.SetDestination(position);
    }

    private void RotateTowards(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
    }

    private void FaceTarget(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void UpdateMovementAnimation(LocomotionMode mode, float referenceSpeed)
    {
        if (!CanUseAgent())
        {
            SetAnimation(LocomotionMode.Idle, 0f);
            return;
        }

        bool wantsToMove = agent.hasPath && !agent.isStopped && !HasArrived(agent.stoppingDistance);
        float speed = agent.velocity.magnitude;
        float normalizedSpeed = referenceSpeed > 0f ? speed / referenceSpeed : 0f;

        if (wantsToMove && normalizedSpeed < movingAnimationThreshold)
        {
            normalizedSpeed = movingAnimationThreshold;
        }

        SetAnimation(wantsToMove ? mode : LocomotionMode.Idle, normalizedSpeed);
    }

    private void SetAnimation(LocomotionMode mode, float normalizedMoveSpeed, bool screaming = false)
    {
        if (zombieAnimator == null) return;

        bool walking = mode == LocomotionMode.Walk;
        bool running = mode == LocomotionMode.Run;

        if (!animationInitialized || currentAnimationMode != mode || currentScreaming != screaming)
        {
            zombieAnimator.SetBool(IsWalking, walking);
            zombieAnimator.SetBool(IsScreaming, screaming);
            zombieAnimator.SetBool(IsChasing, running);

            currentAnimationMode = mode;
            currentScreaming = screaming;
            animationInitialized = true;
        }

        float moveSpeed = Mathf.Clamp(normalizedMoveSpeed, 0f, 1.25f);
        if (Time.deltaTime > 0f)
        {
            zombieAnimator.SetFloat(MoveSpeed, moveSpeed, moveSpeedDampTime, Time.deltaTime);
        }
        else
        {
            zombieAnimator.SetFloat(MoveSpeed, moveSpeed);
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null)
        {
            return false;
        }

        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up;
        Vector3 toPlayer = target - eye;
        float distance = toPlayer.magnitude;
        if (distance > detectionRange)
        {
            return false;
        }

        Vector3 flatDirection = Vector3.ProjectOnPlane(toPlayer, Vector3.up);
        if (flatDirection.sqrMagnitude < 0.0001f || Vector3.Angle(transform.forward, flatDirection) > fieldOfView * 0.5f)
        {
            return false;
        }

        return Physics.Raycast(eye, toPlayer.normalized, out RaycastHit hit, distance, detectionLayers, QueryTriggerInteraction.Ignore)
            && (hit.transform == player || hit.transform.IsChildOf(player));
    }

    private void StartStateRoutine(IEnumerator routine)
    {
        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
        }

        stateRoutine = StartCoroutine(routine);
    }
    private void TryOpenBlockingDoor()
    {
        
        Vector3 direction = agent.desiredVelocity.sqrMagnitude > 0.01f
            ? agent.desiredVelocity.normalized
            : (agent.steeringTarget - transform.position).normalized;

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = transform.forward;
        }

        Vector3 origin = transform.position + Vector3.up * doorCheckHeight;
        int hitCount = Physics.SphereCastNonAlloc(origin, doorCheckRadius, direction, doorHits, doorOpenDistance, doorDetectionLayers, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = doorHits[i].collider;
            if (hitCollider == null || hitCollider.transform == transform || hitCollider.transform.IsChildOf(transform))
            {
                continue;
            }

            Door door = hitCollider.GetComponentInParent<Door>();
            if (door != null)
            {
                if (!door.open && (door.requiredKey == Door.DoorKeyType.None || door.canZombieOpenLockedDoor))
                {
                    door.OpenForZombie();
                }

                return;
            }

            LockedDoor lockedDoor = hitCollider.GetComponentInParent<LockedDoor>();
            if (lockedDoor != null)
            {
                if (!lockedDoor.IsLocked() && !lockedDoor.IsOpen())
                {
                    lockedDoor.OpenByZombie();
                }

                return;
            }
        }
    }
    

    public void ResetZombie()
    {
        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
            stateRoutine = null;
        }

        if (bloodOverlayRoutine != null)
        {
            StopCoroutine(bloodOverlayRoutine);
            bloodOverlayRoutine = null;
        }

        if (zombieAudioSource != null)
            zombieAudioSource.Stop();

        if (attackAudioSource != null)
            attackAudioSource.Stop();

        if (patrolAudioSource != null)
            patrolAudioSource.Stop();

        playerDead = false;
        isAttacking = false;
        canMove = false;
        state = ZombieState.Idle;

        lastKnownPlayerPosition = Vector3.zero;
        lastHeardPosition = Vector3.zero;
        timeSincePlayerSeen = 0f;
        patrolIndex = 0;

        ResetBloodOverlay();

        if (CanUseAgent())
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
            agent.Warp(spawnPosition);
        }
        else
        {
            transform.position = spawnPosition;
        }

        transform.rotation = spawnRotation;

        ActivateAnimator();
        StartStateRoutine(StartupRoutine());
    }
    
}
