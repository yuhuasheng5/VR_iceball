using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GoalZoneFeedbackUI : MonoBehaviour
{
    [Header("References")]
    public Text feedbackText;
    public GoalZoneManager goalZoneManager;

    [Header("Display Settings")]
    public bool showFeedback = true;
    public float displayTime = 1.2f;

    private Coroutine hideCoroutine;

    private IEnumerator Start()
    {
        if (feedbackText == null)
        {
            feedbackText = GetComponent<Text>();
        }

        while (goalZoneManager == null)
        {
            goalZoneManager = GoalZoneManager.Instance;
            yield return null;
        }

        goalZoneManager.OnZoneScored += ShowZoneFeedback;

        ClearFeedbackText();
    }

    private void OnDestroy()
    {
        if (goalZoneManager != null)
        {
            goalZoneManager.OnZoneScored -= ShowZoneFeedback;
        }
    }

    private void ShowZoneFeedback(string zoneName, int scoreValue)
    {
        if (!showFeedback)
        {
            ClearFeedbackText();
            return;
        }

        if (feedbackText == null)
        {
            return;
        }

        feedbackText.text = "命中" + zoneName + "  +" + scoreValue;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);

        ClearFeedbackText();

        hideCoroutine = null;
    }

    private void ClearFeedbackText()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }
}