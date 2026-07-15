using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Lives")]
    public int maxLives = 3;
    public int currentLives;

    [Header("References")]
    public GameOverManager gameOverManager;
    public Transform player;
    public Transform startPoint;

    private CharacterController controller;
    private Player playerScript;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        currentLives = maxLives;
    }

    private void Start()
    {
        controller = player.GetComponent<CharacterController>();
        playerScript = player.GetComponent<Player>();
    }

    public void PlayerDied()
    {
        currentLives--;

        if (currentLives <= 0)
        {
            gameOverManager.GameOver();
            return;
        }

        RespawnPlayer();
    }

    private void RespawnPlayer()
{
    StartCoroutine(RespawnRoutine());
}

private IEnumerator RespawnRoutine()
{
    CharacterController cc = player.GetComponent<CharacterController>();
    Player playerScript = player.GetComponent<Player>();

    // Disable player
    if (playerScript != null)
        playerScript.enabled = false;

    if (cc != null)
        cc.enabled = false;

    yield return null;

    // Move to start position
    player.position = startPoint.position;
    player.rotation = startPoint.rotation;

    player.GetComponent<Player>().enabled = true;

    // Enable again
    if (cc != null)
        cc.enabled = true;

    if (playerScript != null)
        playerScript.enabled = true;
}
}