using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class OnlineScoreUI : MonoBehaviour
{
    [Header("Names")]
    [SerializeField] private string player1Name = "玩家1";
    [SerializeField] private string player2Name = "玩家2";

    private Text scoreText;

    private int lastPlayer1Score = -1;
    private int lastPlayer2Score = -1;

    private void Awake()
    {
        scoreText = GetComponent<Text>();
    }

    private void Update()
    {
        if (scoreText == null)
        {
            scoreText = GetComponent<Text>();
        }

        OnlineMatchManager matchManager = OnlineMatchManager.Instance;

        if (matchManager == null)
        {
            scoreText.text = player1Name + "：0    " + player2Name + "：0";
            return;
        }

        int player1Score = matchManager.Player1Score;
        int player2Score = matchManager.Player2Score;

        if (player1Score == lastPlayer1Score &&
            player2Score == lastPlayer2Score)
        {
            return;
        }

        lastPlayer1Score = player1Score;
        lastPlayer2Score = player2Score;

        scoreText.text =
            player1Name + "：" + player1Score +
            "    " +
            player2Name + "：" + player2Score;
    }
}