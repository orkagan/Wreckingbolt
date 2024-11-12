using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the different menu states
/// </summary>
public enum MenuState
{
    Main,
    Options,
}

public class MenuHandler : Singleton<MenuHandler>
{
    [Serializable]
    public struct Element
    {
        public GameState gameState;
        public GameObject panelGameObj;
        public bool pauses;
        public Button selectedButton;
    }
    
    /// <summary>
    /// Stores the UI panel associated with a state and if it should pause the game.
    /// </summary>
    [SerializeField] Element[] elements;

	private void Start()
	{
        GameManager.Instance.OnGameStateChanged += SwitchPanels;
        SwitchPanels(GameManager.Instance.CurrentGameState);
	}

	// Update is called once per frame
	void Update()
    {
        
    }

    public void SwitchPanels(GameState state)
    {
        GameObject currentPanel = null;
        foreach (Element item in elements)
        {
            //Disable other panels
            item.panelGameObj.SetActive(false);
            if (item.gameState == state)
            {
                currentPanel = item.panelGameObj;
                if (item.selectedButton != null) item.selectedButton.Select();
                //Time.timeScale = item.pauses ? 0 : 1;
				if (item.pauses)
				{
                    Time.timeScale = 0;
                    //Unlock cursor
                    Cursor.lockState = CursorLockMode.None;
                    //Make cursor visible
                    Cursor.visible = true;
                }
				else
				{
                    Time.timeScale = 1;
                    //Lock Cursor to middle of screen
                    Cursor.lockState = CursorLockMode.Locked;
                    //Hide Cursor from view
                    Cursor.visible = false;
                    //PlayerInputHandler.Instance.EnableControls();
				}
            }
        }
        //Enable panel of current state
        currentPanel.SetActive(true);
    }
    public void SwitchMenuPanel()
	{

	}

    public static void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        Debug.Log("ExitGame called.");
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void CursorVisibility(bool visibility)
	{
        Cursor.visible = visibility;
	}
}
