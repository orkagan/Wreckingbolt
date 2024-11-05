using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public enum GameState
{
    MainMenu,
    LevelSelect,
    Options,
    Playing,
    Paused,
    Win
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

    private void Start()
    {
        
    }
    void Update()
    {
		//Placeholder Controls
        if (Input.GetKeyDown(KeyCode.P))
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
    public void ChangeGameState(string targetState)
    {
        CurrentGameState = (GameState)System.Enum.Parse(typeof(GameState), targetState);
    }

    public void ChangeGameState(GameState gameState)
    {
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


    public static void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        Debug.Log("ExitGame called.");
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
