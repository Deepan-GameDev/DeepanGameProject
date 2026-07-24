using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{
    public TMP_Text objectiveText;

    public enum Objective
    {
        FindTorch,
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
        }
    }
}