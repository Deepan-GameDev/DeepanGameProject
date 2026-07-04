using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{
    public TMP_Text objectiveText;

    public enum Objective
    {
        FindTorch,
        FindKey,
        UnlockDoor,
        Escape
    }

    private Objective currentObjective;

    void Start()
    {
        SetObjective(Objective.FindTorch);
    }

    public void CompleteTorchObjective()
    {
        if (currentObjective == Objective.FindTorch)
        {
            SetObjective(Objective.FindKey);
        }
    }

    public void CompleteKeyObjective()
    {
        if (currentObjective == Objective.FindKey)
        {
            SetObjective(Objective.UnlockDoor);
        }
    }

    public void CompleteDoorObjective()
    {
        if (currentObjective == Objective.UnlockDoor)
        {
            SetObjective(Objective.Escape);
        }
    }

    private void SetObjective(Objective objective)
    {
        currentObjective = objective;

        switch (currentObjective)
        {
            case Objective.FindTorch:
                objectiveText.text = "FIND THE TORCH";
                break;

            case Objective.FindKey:
                objectiveText.text = "FIND THE KEY";
                break;

            case Objective.UnlockDoor:
                objectiveText.text = "UNLOCK THE DOOR";
                break;

            case Objective.Escape:
                objectiveText.text = "ESCAPE THE HOUSE";
                break;
        }
    }
}