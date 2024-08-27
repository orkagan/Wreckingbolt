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
	float maxAcceleration = 10f;
	[SerializeField]
	AnimationCurve turnSpeedCurve;

	[SerializeField]
	public float slowTorque = 10f;

	[SerializeField]
	float jumpHeight = 2f;

	[SerializeField, Range(0, 5)]
	int maxAirJumps = 0;

	[SerializeField, Range(0, 90)]
	float maxGroundAngle = 25f, maxStairsAngle = 50f;

	[SerializeField, Range(0f, 100f)]
	float maxSnapSpeed = 100f;

	[SerializeField, Min(0f)]
	float probeDistance = 1f;

	[SerializeField]
	LayerMask probeMask = -1, stairsMask = -1;

	[SerializeField, Range(0f, 180)]
	float maxTilt = 100f;

	Vector2 playerInput = Vector2.zero;
	Rigidbody rb;
	WheelCollider wheel;

	Vector3 velocity, desiredVelocity;
	bool desiredJump;
	Vector3 contactNormal, steepNormal;
	int groundContactCount, steepContactCount;
	bool OnGround => groundContactCount > 0;
	bool OnSteep => steepContactCount > 0;
	int jumpPhase;
	float minGroundDotProduct, minStairsDotProduct;
	int stepsSinceLastGrounded, stepsSinceLastJump;

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
		desiredJump |= jump.WasPerformedThisFrame();
	}

	void FixedUpdate()
	{
		UpdateState();
		AdjustVelocity();

		if (desiredJump)
		{
			desiredJump = false;
			Jump();
		}

		rb.velocity = velocity;
		ClearState();

		//visual tilt when turning
		Vector3 tiltSide = Vector3.Cross(transform.forward.normalized, rb.velocity.normalized);
		visualTilt.localRotation = Quaternion.Slerp(visualTilt.localRotation, Quaternion.Euler(0, 0, maxTilt * tiltSide.y), Time.deltaTime *10f);

		Debug.DrawLine(transform.position, transform.position + desiredVelocity, Color.blue);
		Debug.DrawLine(transform.position, transform.position + rb.velocity, Color.red);
		//Debug.Log($"tiltSide: {tiltSide}");
	}
	void ClearState()
	{
		groundContactCount = steepContactCount = 0;
		contactNormal = steepNormal = Vector3.zero;
	}

	void UpdateState()
	{
		stepsSinceLastGrounded += 1;
		stepsSinceLastJump += 1;
		velocity = rb.velocity;
		if (OnGround /*|| SnapToGround()*/ || CheckSteepContacts() || wheel.isGrounded)
		{
			stepsSinceLastGrounded = 0;
			if (stepsSinceLastJump > 1)
			{
				jumpPhase = 0;
			}
			if (groundContactCount > 1)
			{
				contactNormal.Normalize();
			}
		}
		else
		{
			contactNormal = Vector3.up;
		}
	}

	bool SnapToGround()
	{
		if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
		{
			return false;
		}
		float speed = velocity.magnitude;
		if (speed > maxSnapSpeed)
		{
			return false;
		}
		if (!Physics.Raycast(
			rb.position, Vector3.down, out RaycastHit hit,
			probeDistance, probeMask
		))
		{
			return false;
		}
		if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer))
		{
			return false;
		}

		groundContactCount = 1;
		contactNormal = hit.normal;
		float dot = Vector3.Dot(velocity, hit.normal);
		if (dot > 0f)
		{
			velocity = (velocity - hit.normal * dot).normalized * speed;
		}
		return true;
	}

	bool CheckSteepContacts()
	{
		if (steepContactCount > 1)
		{
			steepNormal.Normalize();
			if (steepNormal.y >= minGroundDotProduct)
			{
				steepContactCount = 0;
				groundContactCount = 1;
				contactNormal = steepNormal;
				return true;
			}
		}
		return false;
	}

	void AdjustVelocity()
	{
		/*float maxSpeedChange = maxAcceleration * Time.deltaTime;
		velocity.x =
			Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
		velocity.z =
			Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);*/

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
			float turnSpeed = 0.05f;
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
	}

	void Jump()
	{
		Debug.Log("Jump!");
		Vector3 jumpDirection;
		if (OnGround)
		{
			jumpDirection = contactNormal;
		}
		else if (OnSteep)
		{
			jumpDirection = steepNormal;
			jumpPhase = 0;
		}
		else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
		{
			if (jumpPhase == 0)
			{
				jumpPhase = 1;
			}
			jumpDirection = contactNormal;
		}
		else
		{
			return;
		}

		stepsSinceLastJump = 0;
		jumpPhase += 1;
		float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
		jumpDirection = (jumpDirection + Vector3.up).normalized;
		float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
		if (alignedSpeed > 0f)
		{
			jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
		}
		velocity += jumpDirection * jumpSpeed;
	}

	void OnCollisionEnter(Collision collision)
	{
		EvaluateCollision(collision);
	}

	void OnCollisionStay(Collision collision)
	{
		EvaluateCollision(collision);
	}

	void EvaluateCollision(Collision collision)
	{
		float minDot = GetMinDot(collision.gameObject.layer);
		for (int i = 0; i < collision.contactCount; i++)
		{
			Vector3 normal = collision.GetContact(i).normal;
			if (normal.y >= minDot)
			{
				groundContactCount += 1;
				contactNormal += normal;
			}
			else if (normal.y > -0.01f)
			{
				steepContactCount += 1;
				steepNormal += normal;
			}
		}
	}

	Vector3 ProjectOnContactPlane(Vector3 vector)
	{
		return vector - contactNormal * Vector3.Dot(vector, contactNormal);
	}

	float GetMinDot(int layer)
	{
		/*return (stairsMask & (1 << layer)) == 0 ?
			minGroundDotProduct : minStairsDotProduct;*/
		return minGroundDotProduct;
	}
}
