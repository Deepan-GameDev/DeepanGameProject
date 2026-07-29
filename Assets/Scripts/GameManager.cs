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
    private Rigidbody playerRigidbody;

    [Header("References")]
    public ZombieAI zombie;

    [Header("Death Transition")]
    public DeathTransitionUI deathTransitionUI;

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
        playerRigidbody = player.GetComponent<Rigidbody>();
        
        LoadPlayerPosition();
    }

    public void PlayerDied()
{
    StartCoroutine(PlayerDiedRoutine());
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

    // Play second chance transition
    if (deathTransitionUI != null)
    {
        Debug.Log("1 - Respawn Started");
        yield return null;
        Debug.Log("2 - Before Transition");

        int chanceNumber = maxLives - currentLives + 1;
        yield return StartCoroutine(deathTransitionUI.PlaySecondChance(chanceNumber));
    }
    // Respawn player
   TeleportPlayer(
    SaveManager.LoadCheckpointPosition(startPoint.position),
    SaveManager.LoadCheckpointRotation(startPoint.rotation));


    if (zombie == null)
        zombie = FindAnyObjectByType<ZombieAI>();

    if (zombie != null)
    {
        zombie.ResetZombie();
    }

    yield return null;

    if (cc != null)
        cc.enabled = true;

    if (deathTransitionUI != null)
    {
        yield return StartCoroutine(deathTransitionUI.FadeBackToGameplay());
    }

    if (playerScript != null)
        playerScript.enabled = true;
}
    
    public void ResetGame()
{
    currentLives = maxLives;
}
    
    private void LoadPlayerPosition()
{
    if (PlayerPrefs.GetInt("HasSave", 0) == 0)
        return;

    TeleportPlayer(
        SaveManager.LoadCheckpointPosition(startPoint.position),
        SaveManager.LoadCheckpointRotation(startPoint.rotation));
}

    private void TeleportPlayer(Vector3 position, Quaternion rotation)
{
    if (controller != null)
        controller.enabled = false;

    if (playerRigidbody != null)
    {
        playerRigidbody.linearVelocity = Vector3.zero;
        playerRigidbody.angularVelocity = Vector3.zero;
        playerRigidbody.position = position;
        playerRigidbody.rotation = rotation;
    }

    player.position = position;
    player.rotation = rotation;

    Physics.SyncTransforms();

    if (controller != null)
        controller.enabled = true;
}   
    
    public IEnumerator PlayerDiedRoutine()
{
    currentLives--;

    // First death → Second Chance
    if (currentLives > 0)
    {
        yield return StartCoroutine(RespawnRoutine());
        yield break;
    }

    // Final death
    SaveManager.DeleteSave();

    gameOverManager.GameOver();
}
    
}
