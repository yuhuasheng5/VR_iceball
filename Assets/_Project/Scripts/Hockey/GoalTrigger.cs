using System.Collections;
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [Header("Goal Settings")]
    [SerializeField] private int scoreValue = 1;
    [SerializeField] private float resetDelay = 0.3f;

    private bool canScore = true;

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        if (!canScore)
        {
            return;
        }

        if (!other.CompareTag("Puck"))
        {
            return;
        }

        PuckController puckController = other.GetComponent<PuckController>();

        if (puckController == null)
        {
            return;
        }

        StartCoroutine(HandleGoal(puckController));
    }

    private IEnumerator HandleGoal(PuckController puckController)
    {
        canScore = false;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        yield return new WaitForSeconds(resetDelay);

        puckController.ResetPuck();

        yield return new WaitForSeconds(0.3f);

        canScore = true;
    }
}