using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WreckingCycle : MonoBehaviour
{
	[SerializeField]
	Transform playerInputSpace = default;

	[SerializeField, Range(0f, 100f)]
	float maxSpeed = 10f;

	Vector2 playerInput = Vector2.zero;
	Rigidbody rb;
	Vector2 velocity, desiredVelocity;

	PlayerInputActions playerControls;
	private InputAction move, jump;

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
		jump.performed += Jump;
	}

	private void OnDisable()
	{
		move.Disable();
		jump.Disable();
	}

	void Start()
    {
		rb = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
		playerInput = Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1f);
		if (playerInputSpace)
		{
			Vector3 forward = playerInputSpace.forward;
			forward.y = 0f;
			forward.Normalize();
			Vector3 right = playerInputSpace.right;
			right.y = 0f;
			right.Normalize();
			desiredVelocity =
				(forward * playerInput.y + right * playerInput.x) * maxSpeed;
		}
		else
		{
			desiredVelocity =
				new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
		}
	}

	void FixedUpdate()
	{
		
	}

	void Jump(InputAction.CallbackContext context)
	{
		Debug.Log("Jump called");
	}
}
