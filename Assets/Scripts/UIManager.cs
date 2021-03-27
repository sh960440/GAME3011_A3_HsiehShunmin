using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour 
{
	public static UIManager instance;
	
	public Text scoreText;
	public Text targetScoreText;
	public Text moveCounterText;
	public Text timerText;
	public Text gameoverText;
	public GameObject gameOverPanel;

	private int score;
	private int targetScore;
	private int moveCounter;
	private float timer;

	private bool isTiming;

	public int Score 
	{
		get 
		{
			return score;
		}

		set 
		{
			score = value;
			scoreText.text = score.ToString();
			if (score >= targetScore)
			{
				StartCoroutine(WaitForShifting(true));
			}
		}
	}

	public int MoveCounter 
	{
		get 
		{
			return moveCounter;
		}

		set 
		{
			moveCounter = value;
			if (moveCounter <= 0) 
			{
				moveCounter = 0;
				StartCoroutine(WaitForShifting(score >= targetScore));
			}
			moveCounterText.text = moveCounter.ToString();
		}
	}

	void Awake() 
	{
		isTiming = true;
		moveCounter = 5;
		moveCounterText.text = moveCounter.ToString();
		instance = GetComponent<UIManager>();
	}

	void Update()
	{
		if (isTiming)
		{
			if (timer > 0)
			{
				timer -= Time.deltaTime;
				timerText.text = timer.ToString("0.0");
				if (timer <= 0)
				{
					timer = 0;
					timerText.text = "0";
					StartCoroutine(WaitForShifting(false));
				}
			}
		}
	}

	public void GameOver(bool won) 
	{
		isTiming = false;
		BoardManager.ClearBoard();
		gameOverPanel.SetActive(true);
		gameoverText.text = won ? "YOU WIN" : "YOU LOSE";
	}

	private IEnumerator WaitForShifting(bool won)
	{
		yield return new WaitUntil(()=> !BoardManager.instance.IsShifting);
		yield return new WaitForSeconds(0.5f);
		GameOver(won);
	}

	public void SetTargetScore(int value)
	{
		targetScore = value;
		targetScoreText.text = targetScore.ToString();
	}

	public void SetMoveAmount(int value)
	{
		moveCounter = value;
		moveCounterText.text = moveCounter.ToString();
	}

	public void SetTimer(float value)
	{
		timer = value;
		timerText.text = timer.ToString("0.0");
	}

	public void Reset()
	{
		score = 0;
		scoreText.text = score.ToString();

		isTiming = true;
	}
}
