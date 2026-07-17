using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private Text scoreText;

    private IEnumerator Start()
    {
        if (scoreText == null)
        {
            scoreText = GetComponent<Text>();
        }

        while (ScoreManager.Instance == null)
        {
            yield return null;
        }

        ScoreManager.Instance.OnScoreChanged += UpdateScoreText;
        UpdateScoreText(ScoreManager.Instance.Score);
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreText;
        }
    }

    private void UpdateScoreText(int score)
    {
        if (scoreText == null)
        {
            return;
        }

        scoreText.text = "得分：" + score;
    }
}