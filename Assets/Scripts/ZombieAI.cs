using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameOverManager gameOverManager;

    [Header("Detection")]
    public float detectionRange = 12f;

    [Header("Chase")]
    public float chaseSpeed = 3.5f;
    public float killDistance = 1.5f;

    private NavMeshAgent agent;
    private bool isChasing = false;
    private bool playerDead = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
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