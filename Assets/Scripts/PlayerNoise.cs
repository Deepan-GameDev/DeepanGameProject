using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    public ZombieAI zombie;

    public float walkNoiseInterval = 1.2f;
    public float runNoiseInterval = 0.6f;

    private Player player;

    private float timer;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (player == null || zombie == null)
            return;

        if (!player.GetIsMoving())
        {
            timer = 0f;
            return;
        }

        timer += Time.deltaTime;

        bool running = player.GetIsRunning();

        float interval = running ? runNoiseInterval : walkNoiseInterval;

        if (timer >= interval)
        {
            timer = 0f;

            zombie.HearNoise(transform.position, running);
        }
    }
}
