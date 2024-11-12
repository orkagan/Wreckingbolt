using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// States for gameplay
/// </summary>
public enum GameState
{
    Starting,
    Playing,
    Paused,
    Win,
    Lose
}
public class GameManager : Singleton<GameManager>
{
    public event Action<GameState> OnGameStateChanged;
    [SerializeField]
    private GameState gameState = GameState.Playing;
    public GameState CurrentGameState
    {
        get
        {
            return gameState;
        }
        set
        {
            gameState = value;
            OnGameStateChanged?.Invoke(CurrentGameState);
        }
    }
    void Update()
    {
		//Placeholder Controls
        //if (Input.GetKeyDown(KeyCode.P))
        if (PlayerInputHandler.Instance.pause.WasPressedThisFrame())
		{
			if (CurrentGameState == GameState.Playing)
			{
				CurrentGameState = GameState.Paused;
			}
			else if (CurrentGameState == GameState.Paused)
			{
				CurrentGameState = GameState.Playing;
			}
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
            Debug.Log("Retry");
            Retry();
		}
	}

    public void Win()
    {
        CurrentGameState = GameState.Win;
    }
    public void Lose()
    {
        CurrentGameState = GameState.Lose;
    }
    public void ChangeGameState(string targetState)
    {
        CurrentGameState = (GameState)System.Enum.Parse(typeof(GameState), targetState);
    }
    public void ChangeGameState(GameState gameState)
    {
        
		switch (CurrentGameState)
		{
			case GameState.Starting:
				if (gameState==GameState.Playing)
				{

				}
				break;
			case GameState.Playing:
				break;
			case GameState.Paused:
				break;
			case GameState.Win:
				break;
			case GameState.Lose:
				break;
			default:
				break;
		}
        CurrentGameState = gameState;
    }

    public static void NextScene()
    {
        // Get the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        // Load the next scene after the current scene (last scene loops back around to first)
        SceneManager.LoadScene((currentScene.buildIndex + 1) % SceneManager.sceneCountInBuildSettings);
    }

    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public static void Retry()
    {
        //reloads current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
