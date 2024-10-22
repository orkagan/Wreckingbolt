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
            SwitchPanels(gameState);
        }
    }
    //public Dictionary<GameState, GameObject> panels;
    [Serializable]
    public struct Element
    {
        public GameState panelState;
        public GameObject panelGameObj;
        public bool pauses;
    }
    /// <summary>
    /// Stores the UI panel associated with a state and if it should pause the game.
    /// </summary>
    public Element[] elements;

    private void Start()
    {
        SwitchPanels(CurrentGameState);
    }
    void Update()
    {
		//Placeholder Controls
        if (Input.GetKeyDown(KeyCode.Escape))
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
    public void SwitchPanels(GameState state)
    {
        GameObject currentPanel = null;
        foreach (Element item in elements)
        {
            //Disable other panels
            item.panelGameObj.SetActive(false);
            if (item.panelState == CurrentGameState)
            {
                currentPanel = item.panelGameObj;
                Time.timeScale = item.pauses ? 0 : 1;
            }
        }
        //Enable panel of current state
        currentPanel.SetActive(true);
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
