﻿using UnityEngine;
using System;
using System.Collections;

public class ScoreManager : SingletonMonoBehaviour<ScoreManager>
{
    public int Score { get; private set; }

    public int HighScore { get; private set; }

    public bool HasNewHighScore { get; private set; }

    public static event Action<int> ScoreUpdated = delegate { };
    public static event Action<int> HighscoreUpdated = delegate { };

    private const string HIGHSCORE = "HIGHSCORE";
    // key name to store high score in PlayerPrefs

    void Start()
    {
        Reset();
    }

    public void Reset()
    {
        // Initialize score
        Score = 0;

        ScoreUpdated(Score);

        // Initialize highscore
        HighScore = PlayerPrefs.GetInt(HIGHSCORE, 0);
        HasNewHighScore = false;
    }

    public void AddScore(int amount)
    {
        Score += amount;

        // Fire event
        ScoreUpdated(Score);

        if (Score > HighScore)
        {
            UpdateHighScore(Score);
            HasNewHighScore = true;
        }
        else
        {
            HasNewHighScore = false;
        }
    }

    public void UpdateHighScore(int newHighScore)
    {
        // Update highscore if player has made a new one
        if (newHighScore > HighScore)
        {
            HighScore = newHighScore;
            PlayerPrefs.SetInt(HIGHSCORE, HighScore);
            HighscoreUpdated(HighScore);
        }
    }
}