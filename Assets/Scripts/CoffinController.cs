using UnityEngine;
using System.Collections;

public class CoffinController : MonoBehaviour
{
    [Header("References")]
    public Transform coffinLidPivot;
    public ZombieAI zombieAI;

    [Header("Opening")]
    public float openAngle = -110f;
    public float openSpeed = 2f;

    private bool opened = false;

    public void OpenCoffin()
    {
        if (opened)
            return;

        opened = true;

        StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        Quaternion startRot = coffinLidPivot.localRotation;
        Quaternion endRot = Quaternion.Euler(openAngle, 0, 0);

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * openSpeed;

            coffinLidPivot.localRotation =
                Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        if (zombieAI != null)
        {
            zombieAI.WakeUpZombie();
        }
    }
}