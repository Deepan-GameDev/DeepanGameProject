using UnityEngine;
using System.Collections;

public class LadderClimber : MonoBehaviour
{
    public Transform bottomPoint;
    public Transform topPoint;

    public Player player;

    public float climbSpeed = 2f;

    private bool climbing = false;

    public void StartClimb()
    {
        if (climbing)
            return;

        StartCoroutine(ClimbRoutine());
    }

    IEnumerator ClimbRoutine()
    {
        climbing = true;

        player.enabled = false;

        Transform playerTransform = player.transform;

        playerTransform.position = bottomPoint.position;

        while (Vector3.Distance(playerTransform.position, topPoint.position) > 0.02f)
        {
            playerTransform.position = Vector3.MoveTowards(
                playerTransform.position,
                topPoint.position,
                climbSpeed * Time.deltaTime);

            yield return null;
        }

        playerTransform.position = topPoint.position;

        player.enabled = true;

        climbing = false;
    }
}