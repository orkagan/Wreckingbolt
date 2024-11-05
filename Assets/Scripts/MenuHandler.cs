using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHandler : Singleton<MenuHandler>
{
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
            if (item.panelState == state)
            {
                currentPanel = item.panelGameObj;
                //Time.timeScale = item.pauses ? 0 : 1;
				if (item.pauses)
				{
                    Time.timeScale = 0;
                    //Lock Cursor to middle of screen
                    Cursor.lockState = CursorLockMode.None;
                    //Hide Cursor from view
                    Cursor.visible = true;
                }
				else
				{
                    Time.timeScale = 1;
                    //Lock Cursor to middle of screen
                    Cursor.lockState = CursorLockMode.Locked;
                    //Hide Cursor from view
                    Cursor.visible = false;
				}
            }
        }
        //Enable panel of current state
        currentPanel.SetActive(true);
    }
}
