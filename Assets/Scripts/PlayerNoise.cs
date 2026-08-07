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

        // Player.UpdateFootsteps already treats crouching as silent. Keep the
        // AI hearing emitter consistent with it so crouch movement creates no
        // investigation events. Vision remains entirely in ZombieAI.
        if (!player.GetIsMoving() || player.GetIsCrouching())
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
