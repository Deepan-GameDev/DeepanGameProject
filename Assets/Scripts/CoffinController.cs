using UnityEngine;
using System.Collections;

public class CoffinController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform coffinLidPivot;
    [SerializeField] private ZombieAI zombieAI;

    [Header("Opening")]
    [SerializeField] private float openAngle = -110f;
    [SerializeField] private float openSpeed = 2f;

    private bool opened = false;

    public ZombieAI Zombie => zombieAI;

    public void OpenCoffin()
    {
        if (opened || coffinLidPivot == null)
            return;

        opened = true;

        StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        Quaternion startRot = coffinLidPivot.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(openAngle, 0, 0);

        float t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime * openSpeed;

            float easedT = t * t * (3f - 2f * t);
            coffinLidPivot.localRotation = Quaternion.Slerp(startRot, endRot, easedT);

            yield return null;
        }

        coffinLidPivot.localRotation = endRot;

        if (zombieAI != null)
        {
            zombieAI.WakeUpZombie();
        }
    }
}
