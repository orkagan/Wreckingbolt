using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WreckingCycle : MonoBehaviour
{
	[SerializeField]
	Transform playerInputSpace = default;
	[SerializeField]
	Transform visualTilt;

	[SerializeField]
	float maxSpeed = 10f;

	[SerializeField]
	float maxAcceleration = 10f, maxAirAcceleration = 1f;
	[SerializeField]
	AnimationCurve turnSpeedCurve;

	[SerializeField]
	public float slowTorque = 10f;

	[SerializeField, Range(0f, 10f)]
	float jumpHeight = 2f;

	[SerializeField, Range(0f, 180)]
	float maxTilt = 100f;

	Vector2 playerInput = Vector2.zero;
	Rigidbody rb;
	WheelCollider wheel;
	Vector3 velocity, desiredVelocity;

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
		wheel = GetComponent<WheelCollider>();
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
		velocity = rb.velocity;
		float maxSpeedChange = maxAcceleration * Time.deltaTime;
		velocity.x =
			Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
		velocity.z =
			Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
		//rb.velocity = velocity;

		//Trying rotational/torque based movement;
		/*Vector3 desiredRotVelocity = new Vector3(desiredVelocity.z, 0, -desiredVelocity.x);
		rb.AddTorque(desiredRotVelocity);*/

		//Trying Wheel collider
		if (desiredVelocity.magnitude > 0.1f)
		{
			/*if (playerInput.y > -0.5f)
			{
				Quaternion lookTarget = Quaternion.LookRotation(desiredVelocity, transform.up);
				transform.rotation = Quaternion.Lerp(transform.rotation, lookTarget, 0.1f);
				wheel.motorTorque = desiredVelocity.magnitude;
			}
			//Reverse if player inputs backwards enough
			else if (playerInput.y < 0.5f)
			{
				Quaternion lookTarget = Quaternion.LookRotation(-desiredVelocity, transform.up);
				transform.rotation = Quaternion.Lerp(transform.rotation, lookTarget, 0.1f);
				wheel.motorTorque = -desiredVelocity.magnitude;
			}*/

			//Turn to desired direction
			Quaternion lookTarget = Quaternion.LookRotation(desiredVelocity, transform.up);
			//float turnSpeed = turnSpeedCurve.Evaluate();
			float turnSpeed = 0.1f;
			transform.rotation = Quaternion.Lerp(transform.rotation, lookTarget, turnSpeed);
			//Accelerate wheel
			wheel.motorTorque = desiredVelocity.magnitude;
			wheel.brakeTorque = 0f;
		}
		else
		{
			visualTilt.localRotation = Quaternion.identity;
			wheel.motorTorque = 0f;
			wheel.brakeTorque = slowTorque;
		}

		Vector3 tiltSide = Vector3.Cross(transform.forward.normalized, rb.velocity.normalized);
		visualTilt.localRotation = Quaternion.Euler(0, 0, maxTilt * tiltSide.y);

		Debug.DrawLine(transform.position, transform.position + desiredVelocity, Color.blue);
		Debug.DrawLine(transform.position, transform.position + rb.velocity, Color.red);
		Debug.Log($"tiltSide: {tiltSide}");
	}

	void Jump(InputAction.CallbackContext context)
	{
		Debug.Log("Jump called");
	}
}
