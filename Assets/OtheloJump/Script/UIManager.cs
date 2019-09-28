using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Object References")]
    public GameManager gameManager;
    public GameObject titleLabelA;
    public GameObject titleLabelB;
    public GameObject tapToStart;
    public GameObject continueButton;
    public TextMeshProUGUI continueText;
    public GameObject scoreLabel;
    public TextMeshProUGUI scoreText;
    public GameObject retryButton;
    public TextMeshProUGUI retryPosText;
    public GameObject speedUpLabel;
    public GameObject dangerLabel;
    public float moveSpeedUpTime;
    public float showSpeedUpTime;
    public GameObject volumeSlider;
    private int highScore;
    // Animator scoreAnimator;

    void Awake()
    {
        highScore = PlayerPrefs.GetInt("HIGHSCORE", 0);
    }

    void OnEnable()
    {
        GameManager.GameStateChanged += GameManager_GameStateChanged;
        ScoreManager.ScoreUpdated += OnScoreUpdated;
    }

    void OnDisable()
    {
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
        ScoreManager.ScoreUpdated -= OnScoreUpdated;
    }

    // Use this for initialization
    void Start()
    {
        Reset();
        ShowStartUI();
    }

    void GameManager_GameStateChanged(GameState newState, GameState oldState)
    {
        if (newState == GameState.Playing)
        {
            ShowGameUI();
        }
        else if (newState == GameState.PreGameOver)
        {
            // Before game over, i.e. game potentially will be recovered
        }
        else if (newState == GameState.GameOver)
        {
            if (ScoreManager.Instance.Score < 100)
            {
                retryPosText.text = "";
            }
            else if (ScoreManager.Instance.Score < 300)
            {
                retryPosText.text = "(100m~)";
            }
            else if (ScoreManager.Instance.Score < 600)
            {
                retryPosText.text = "(300m~)";
            }
            else if (ScoreManager.Instance.Score < 1000)
            {
                retryPosText.text = "(600m~)";
            }
            else
            {
                retryPosText.text = "(1000m~)";
            }
            Invoke("ShowGameOverUI", 0.2f);
        }
    }

    void OnScoreUpdated(int newScore)
    {
        scoreText.text = newScore.ToString();
    }

    void Reset()
    {
        titleLabelA.SetActive(false);
        titleLabelB.SetActive(false);
        tapToStart.SetActive(false);
        continueButton.SetActive(false);
        scoreLabel.SetActive(false);
        scoreText.gameObject.SetActive(false);
        retryButton.SetActive(false);
        retryPosText.gameObject.SetActive(false);
        speedUpLabel.SetActive(false);
        dangerLabel.SetActive(false);
        volumeSlider.SetActive(false);
    }

    public void StartGame()
    {
        AudioManager.Instance.PlaySE("Button");
        AudioManager.Instance.PlayBGM("Highlands");
        gameManager.StartGame();
    }

    public void ContinueGame()
    {
        AudioManager.Instance.PlaySE("Button");
        if (highScore < 1000)
        {
            AudioManager.Instance.PlayBGM("Highlands");
        }
        else
        {
            AudioManager.Instance.PlayBGM("Pooh");
        }

        if (highScore < 100)
        { }
        else if (highScore < 300)
        {
            ScoreManager.Instance.AddScore(100);
        }
        else if (highScore < 600)
        {
            ScoreManager.Instance.AddScore(300);
        }
        else if (highScore < 1000)
        {
            ScoreManager.Instance.AddScore(600);
        }
        else
        {
            ScoreManager.Instance.AddScore(1000);
        }

        gameManager.StartGame();
    }

    public void RetryGame()
    {
        AudioManager.Instance.PlaySE("Button");
        gameManager.Retry();
    }

    public void EndGame()
    {
        gameManager.GameOver();
    }

    public void ShowStartUI()
    {
        titleLabelA.SetActive(true);
        titleLabelB.SetActive(true);
        tapToStart.SetActive(true);
        volumeSlider.SetActive(true);

        if (highScore >= 100)
        {
            continueButton.SetActive(true);
            if (highScore < 300)
            {
                continueText.text = "100m~";
            }
            else if (highScore < 600)
            {
                continueText.text = "300m~";
            }
            else if (highScore < 1000)
            {
                continueText.text = "600m~";
            }
            else
            {
                continueText.text = "1000m~";
            }
        }
    }

    public void ShowGameUI()
    {
        titleLabelA.SetActive(false);
        titleLabelB.SetActive(false);
        tapToStart.SetActive(false);
        continueButton.SetActive(false);
        scoreLabel.SetActive(true);
        scoreText.gameObject.SetActive(true);
        retryButton.SetActive(false);
        retryPosText.gameObject.SetActive(false);
        volumeSlider.SetActive(false);
    }

    public void ShowGameOverUI()
    {
        naichilab.RankingLoader.Instance.SendScoreAndShowRanking(ScoreManager.Instance.Score);
        retryButton.SetActive(true);
        retryPosText.gameObject.SetActive(true);
        volumeSlider.SetActive(true);
    }

    public void SetSpeedUpAnimation()
    {
        speedUpLabel.SetActive(true);
        LeanTween.moveLocalX(speedUpLabel, 1440.0f, 0.0f).setOnComplete(() =>
        {
            LeanTween.moveLocalX(speedUpLabel, 0.0f, moveSpeedUpTime).setOnComplete(() =>
            {
                LeanTween.moveLocalX(speedUpLabel, -1440.0f, moveSpeedUpTime).setDelay(showSpeedUpTime).setOnComplete(() =>
                {
                    speedUpLabel.SetActive(false);
                });
            });
        });
    }

    public void SetDangerAnimation()
    {
        dangerLabel.SetActive(true);
        LeanTween.moveLocalX(dangerLabel, 1440.0f, 0.0f).setOnComplete(() =>
        {
            LeanTween.moveLocalX(dangerLabel, 0.0f, moveSpeedUpTime).setOnComplete(() =>
            {
                LeanTween.moveLocalX(dangerLabel, -1440.0f, moveSpeedUpTime).setDelay(showSpeedUpTime).setOnComplete(() =>
                {
                    dangerLabel.SetActive(false);
                });
            });
        });
    }

    public void OnChangeVolumeSlider(float volume)
    {
        AudioManager.Instance.ChangeVolume(0.8f * volume, 1.0f * volume);
        AudioManager.Instance.PlaySE("Jump");
    }

    //     public void ShowSettingsUI()
    //     {
    //         settingsUI.SetActive(true);
    //     }

    //     public void HideSettingsUI()
    //     {
    //         settingsUI.SetActive(false);
    //     }

}
