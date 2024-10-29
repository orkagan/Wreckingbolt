using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
	PlayerInputActions playerControls;
	public InputAction move, jump, boost;

	private void Awake()
	{
		playerControls = new PlayerInputActions();
	}

	private void OnEnable()
	{
		move = playerControls.Player.Move;
		move.Enable();

		jump = playerControls.Player.Jump;
		jump.Enable();

		boost = playerControls.Player.Boost;
		boost.Enable();
		//boost.performed += BoostBurst;
	}

	private void OnDisable()
	{
		move.Disable();
		jump.Disable();
		boost.Disable();
	}
}
