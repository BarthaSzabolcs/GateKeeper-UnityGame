﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Events

    public delegate void SceneChange();
    public event SceneChange OnSceneChange;

    public delegate void GameStateChange(GameState state);
    public event GameStateChange OnGameStateChange;

    public delegate void MoneyChanged(int money);
    public event MoneyChanged OnMoneyChaned;

    #endregion

    #region Show In Editor

    [SerializeField] string baseName;

    [Header("Money:")]
    [SerializeField] int startingMoney;

    #endregion
    #region Hide In Editor

    public enum GameState { NextWaveReady, InCombat, TrapShop, GunShop };

    private GameObject player;
    private bool inBuildMode = false;
    private int money = 0;
    private GameState currentGameState;

    public static GameManager Instance { get; private set; }
    public GameObject Player
    {
        get
        {
            if(player == null)
            {
                player = GameObject.Find("Player");
                if(player != null)
                {
                    player.GetComponent<Health>().OnDeath += HandlePlayerDeath;
                }
            }
            return player;
        }
    }
    public GameState CurrentGameState
    {
        get
        {
            return currentGameState;
        }
        set
        {
            if (value != currentGameState)
            {
                OnGameStateChange?.Invoke(value);
            }
            currentGameState = value;
        }
    }
    public int Money
    {
        get
        {
            return money;
        }
        set
        {
            money = value;
            OnMoneyChaned?.Invoke(value);
        }
    }

    #endregion

    #region UnityFunctions

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }

    }
    private void Start()
    {
        SceneManager.sceneLoaded += StartLevel;
        SpawnManager_Szabolcs.Instance.OnWaveEnded += HandleWaveEnded;
    }

    #endregion
    #region Combat

    public void StartWave()
    {
        if(CurrentGameState == GameState.NextWaveReady)
        {
            SpawnManager_Szabolcs.Instance.StartWave();
            CurrentGameState = GameState.InCombat;
        }
    }

    private void HandleWaveEnded()
    {
        CurrentGameState = GameState.NextWaveReady;
    }

    #endregion
    #region CusomFunctions

    public void ChangeScene(int index)
    {
        OnSceneChange?.Invoke();
        SceneManager.LoadScene(index);
    }
    public void ReloadScene()
    {
        ChangeScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void StartLevel(Scene scene, LoadSceneMode mode)
    {
        SetTimeScale(1);

        player = GameObject.Find("Player");

        var portal = GameObject.Find(baseName);

        if (portal != null)
        {
            portal.GetComponent<Health>().OnDeath += HandlePlayerDeath;
        }

        if (player != null)
        {
            player.GetComponent<Health>().OnDeath += HandlePlayerDeath;
        }

        UserInterface.Instance.InitializeLevel();
        ObjectPool.Instance.InitializeLevel();
        SpawnManager_Szabolcs.Instance.InitializeLevel();

        Money = startingMoney;

        CurrentGameState = GameState.NextWaveReady;
    }

    public void SlowTime(float slowRate = 1f)
    {
        SetTimeScale(slowRate);
    }
    public void ContinueGame()
    {
        SetTimeScale(1);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    private void SetTimeScale(float timeScale = Constants.timeSlowScale)
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
    private void HandlePlayerDeath(GameObject sender)
    {
        ReloadScene();
    }

    #endregion
}
