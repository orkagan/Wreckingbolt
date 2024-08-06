using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState
{
    Menu,
    Playing,
}

public class GameManager : MonoBehaviour
{
    #region Singleton Setup
    //Staticly typed property setup for EnemySpawner.Instance
    private static GameManager _instance;
    public static GameManager Instance
    {
        get => _instance;
        private set
        {
            //check if instance of this class already exists and if so keep orignal existing instance
            if (_instance == null)
            {
                _instance = value;
            }
            else if (_instance != value)
            {
                Debug.Log($"{nameof(GameManager)} instance already exists, destroy the duplicate!");
                Destroy(value);
            }
        }
    }
    private void Awake()
    {
        Instance = this; //sets the static class instance
    }
    #endregion

    public GameState gameState;

    void Start()
    {
        UpdateGameState();
    }

    void Update()
    {
		if (Input.GetKeyDown(KeyCode.R))
		{
            Retry();
		}
    }

    public void UpdateGameState()
	{
        switch (gameState)
        {
            case GameState.Menu:
                if (Cursor.visible == false)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                break;
            case GameState.Playing:
                if (Cursor.visible == true)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                break;
            default:
                break;
        }
    }

    public void NextScene()
    {
        // Get the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        // Load the next scene after the current scene (or loop back around to 1st)
        SceneManager.LoadScene((currentScene.buildIndex + 1) % SceneManager.sceneCountInBuildSettings);
    }

    public void Retry()
    {
        //reloads current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        //quits application
        Application.Quit();
        //or if running in editor then stop play mode
#if UNITY_EDITOR
        Debug.Log("ExitGame attempted.");
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
