using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class LadderTrigger : MonoBehaviour
{
    public GameObject climbButton;
    public Button climbUIButton;
    public LadderClimber ladder;

    private bool playerInside;

    private void Awake()
    {
        Collider triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;

        if (climbUIButton == null && climbButton != null)
        {
            climbUIButton = climbButton.GetComponent<Button>();
        }

        if (climbUIButton != null)
        {
            climbUIButton.onClick.RemoveListener(OnClimbButtonPressed);
            climbUIButton.onClick.AddListener(OnClimbButtonPressed);
        }
    }

    private void Start()
    {
        SetButtonVisible(false);
    }

    private void Update()
    {
        SetButtonVisible(playerInside && ladder != null && !ladder.IsClimbing);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = true;
        SetButtonVisible(ladder != null && !ladder.IsClimbing);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = false;
        SetButtonVisible(false);
    }

    public void OnClimbButtonPressed()
    {
        if (!playerInside || ladder == null || ladder.IsClimbing)
        {
            return;
        }

        SetButtonVisible(false);
        ladder.StartClimb();
    }

    private void SetButtonVisible(bool visible)
    {
        if (climbButton != null)
        {
            climbButton.SetActive(visible);
        }
    }
}
