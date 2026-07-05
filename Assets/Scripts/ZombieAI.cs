using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ZombieAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameOverManager gameOverManager;
    public Animator zombieAnimator;

    [Header("Wake Up")]
    public string wakeUpTrigger = "WakeUp";
    public float wakeUpDuration = 4f;
    public bool wakeUpOnStart = true;

    [Header("Detection")]
    public float detectionRange = 12f;

    [Header("Chase")]
    public float chaseSpeed = 3.5f;
    public float killDistance = 1.5f;

    private NavMeshAgent agent;
    private bool isChasing = false;
    private bool playerDead = false;
    private bool isAwake = false;
    private bool hasWokenUp = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        isAwake = false;

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
        if (!isAwake)
            return;

        if (player == null || playerDead)
            return;

        float distanceToPlayer = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distanceToPlayer <= detectionRange)
        {
            isChasing = true;
        }

        if (isChasing)
        {
            ChasePlayer();

            if (distanceToPlayer <= killDistance)
            {
                KillPlayer();
            }
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
        isAwake = false;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        if (zombieAnimator != null)
        {
            zombieAnimator.SetTrigger(wakeUpTrigger);
        }

        yield return new WaitForSeconds(wakeUpDuration);

        isAwake = true;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = chaseSpeed;
        }
    }

    private void ChasePlayer()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        agent.speed = chaseSpeed;

        agent.SetDestination(player.position);
    }

    private void KillPlayer()
    {
        playerDead = true;

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