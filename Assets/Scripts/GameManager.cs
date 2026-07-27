using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Lives")]
    public int maxLives = 2;
    public int currentLives;

    [Header("References")]
    public GameOverManager gameOverManager;
    public Transform player;
    public Transform startPoint;

    private CharacterController controller;
    private Player playerScript;
    private Rigidbody playerRigidbody;

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
    currentLives--;

    if (currentLives == 1)
    {
        Debug.Log("One life remaining...");
        RespawnPlayer();
        return;
    }

    if(currentLives <= 0)
{
    SaveManager.DeleteSave();

    gameOverManager.GameOver();
}
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

    Vector3 respawnPosition = PlayerPrefs.GetInt("HasSave", 0) == 1
        ? SaveManager.LoadCheckpointPosition(startPoint.position)
        : startPoint.position;

    Quaternion respawnRotation = PlayerPrefs.GetInt("HasSave", 0) == 1
        ? SaveManager.LoadCheckpointRotation(startPoint.rotation)
        : startPoint.rotation;

    TeleportPlayer(respawnPosition, respawnRotation);

    // Enable again
    if (cc != null)
        cc.enabled = true;

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
    
}
