using UnityEngine;
using UnityEngine.UI;

public class LadderTrigger : MonoBehaviour
{
    public GameObject climbButton;
    public LadderClimber ladder;

    private bool playerInside;

    private void Start()
    {
        climbButton.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
        climbButton.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        climbButton.SetActive(false);
    }

    public void OnClimbButtonPressed()
    {
        if (!playerInside)
            return;

        climbButton.SetActive(false);

        ladder.StartClimb();
    }
}