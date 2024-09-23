using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SmashCycle : MonoBehaviour
{
	#region Parameters
	[Header("Parameters")]
	[SerializeField] float maxSpeed = 20f;
	[SerializeField] float maxAngularVelocity = 20f;
	[SerializeField] float maxAcceleration = 10f;
	[SerializeField, Range(0,1)] float frictionCoefficient = 0.02f;

	[Header("Colliison Contacts")]
	[SerializeField, Range(0, 90)]
	float maxGroundAngle = 25f;
	[SerializeField, Range(0f, 100f)]
	float maxSnapSpeed = 100f;
	[SerializeField, Min(0f)]
	float probeDistance = 1f;
	[SerializeField]
	LayerMask probeMask = -1;

	[SerializeField] float jumpHeight = 10f;
	[SerializeField] float boostPower = 5000f;
	#endregion
	#region References
	[Header("References")]
	[SerializeField] Transform playerInputSpace = default;
	public Transform vehicleBody_tf;
	public Transform tilt_tf;
	public Transform wheel_tf;
	public Transform seat_tf;
	public ParticleSystem jetThrustFX;

	Rigidbody rb;
	#endregion
	#region Internal Variables
	Vector2 playerInputMove;
	Vector3 desiredVelocity;
	Vector3 sideFriction;
	[SerializeField]
	Vector3 velocity;
	Vector3 upAxis, rightAxis, forwardAxis;
	[SerializeField]
	float wheelRollSpeed;
	bool desiredJump;
	[SerializeField] bool onGround;
	[SerializeField] int groundContactCount;
	[SerializeField] int steepContactCount;
	[SerializeField] int stepsSinceLastJump;
	Vector3 contactNormal;
	Vector3 steepNormal;
	[SerializeField] int stepsSinceLastGrounded;
	float minGroundDotProduct;
	#endregion
	#region Controls/Inputs
	PlayerInputActions playerControls;
	private InputAction move, jump, boost;

	private void Awake()
	{
		playerControls = new PlayerInputActions();
		OnValidate();
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
	#endregion

	private void OnValidate()
	{
		rb = GetComponent<Rigidbody>();
		rb.maxAngularVelocity = maxAngularVelocity;

		minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
	}
	private void Start()
    {
		//unparent hover seat (also keep it near in hierarchy)
		/*vehicleBody_tf.SetParent(null, true);
		vehicleBody_tf.SetSiblingIndex(transform.GetSiblingIndex());*/
    }

    private void Update()
    {
		//Get player move input
		playerInputMove = Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1f);
		//Convert input to camera space, else world space
		if (playerInputSpace)
		{
			Vector3 forward = playerInputSpace.forward;
			forward.y = 0f;
			forward.Normalize();
			Vector3 right = playerInputSpace.right;
			right.y = 0f;
			right.Normalize();
			desiredVelocity = (forward * playerInputMove.y + right * playerInputMove.x) * maxSpeed;
		}
		else
		{
			desiredVelocity = new Vector3(playerInputMove.x, 0f, playerInputMove.y) * maxSpeed;
		}

		desiredJump |= jump.WasPressedThisFrame();

        #region Visuals using external transforms
        //Vehicle body copy position
        vehicleBody_tf.position = transform.position;

		//Seat rotation
		Quaternion lookDir = vehicleBody_tf.rotation;
		if (desiredVelocity.magnitude > 0.1f)
		{
			//TODO: replace Vector3.up with contact normal
			lookDir = Quaternion.LookRotation(desiredVelocity, Vector3.up);
		}
		Vector3 tiltSide = Vector3.Cross(desiredVelocity.normalized, rb.velocity.normalized);
		lookDir.eulerAngles = new Vector3(lookDir.eulerAngles.x, lookDir.eulerAngles.y, 60f * tiltSide.y);

		seat_tf.rotation = Quaternion.Lerp(seat_tf.rotation, lookDir, Time.deltaTime * 2f);

		//Ball Tilt
        Vector3 ballTiltSide = Vector3.Cross(vehicleBody_tf.forward.normalized, rb.velocity.normalized);
        float ballTiltAmount = Vector3.Angle(vehicleBody_tf.forward.normalized, rb.velocity.normalized) * velocity.magnitude / 10f;
		Quaternion ballTilt = Quaternion.Euler(tilt_tf.eulerAngles.x, 0, Mathf.Clamp(ballTiltAmount, 0f, 60f) * ballTiltSide.y);
		tilt_tf.localRotation = Quaternion.Lerp(tilt_tf.localRotation, ballTilt, Time.deltaTime * 10f);

		//Wheel Roll
		wheelRollSpeed = Mathf.Rad2Deg * Vector3.Dot(rb.angularVelocity, vehicleBody_tf.right.normalized);
		wheel_tf.Rotate(Vector3.right, wheelRollSpeed * Time.deltaTime);
        #endregion

        //Debug vectors
        //Debug.DrawLine(transform.position, transform.position + rb.angularVelocity, Color.yellow);
        Debug.DrawLine(transform.position, transform.position + desiredVelocity, Color.blue);
        Debug.DrawLine(transform.position, transform.position + sideFriction * 10f, Color.yellow);
        //Debug.DrawLine(transform.position, transform.position + tiltSide, Color.green);
        Debug.DrawLine(transform.position, transform.position + rb.velocity, Color.red);
    }

	private void FixedUpdate()
	{
		//Vector3 gravity = CustomGravity.GetGravity(rb.position, out upAxis);
		Vector3 gravity = Physics.gravity;
		UpdateState();
		
		float maxSpeedChange = maxAcceleration * Time.deltaTime;

		//0.1f is kinda hardcoded deadzone
		if (desiredVelocity.magnitude > 0.1f)
		{
			//Turn to desired direction
			//TODO: replace Vector3.up with contact normal
			Quaternion lookTarget = Quaternion.LookRotation(desiredVelocity, Vector3.up);
			//float turnSpeed = turnSpeedCurve.Evaluate();
			float turnSpeed = 0.05f;
			vehicleBody_tf.rotation = Quaternion.Lerp(vehicleBody_tf.rotation, lookTarget, turnSpeed);
		}
		velocity.x = Mathf.MoveTowards(velocity.x, vehicleBody_tf.forward.x * desiredVelocity.magnitude, maxSpeedChange);
		velocity.z = Mathf.MoveTowards(velocity.z, vehicleBody_tf.forward.z * desiredVelocity.magnitude, maxSpeedChange);


		//Tire side friction
		//TODO: make friction asymptotic curve
		sideFriction = Vector3.Dot(rb.velocity, vehicleBody_tf.right.normalized) * -frictionCoefficient * vehicleBody_tf.right.normalized;
		rb.velocity += sideFriction;

		//Appply forces
		//rb.velocity = velocity;
		//rb.angularVelocity += vehicleBody_tf.right * desiredVelocity.magnitude;
		rb.AddTorque(vehicleBody_tf.right * desiredVelocity.magnitude * 100f);

		//Boost
		//if (Gamepad.current.rightShoulder.IsPressed())
		if (boost.IsPressed())
		{
			BoostHold();
			jetThrustFX.Play();
        }
        else
        {
			jetThrustFX.Stop();
        }

        if (desiredJump)
        {
			desiredJump = false;
			Jump(gravity);
        }
		ClearState();
	}
	
	void UpdateState()
	{
		stepsSinceLastGrounded += 1;
		stepsSinceLastJump += 1;
		velocity = rb.velocity;
		if (onGround || SnapToGround() || CheckSteepContacts())
		{
			stepsSinceLastGrounded = 0;
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
			rb.position, -upAxis, out RaycastHit hit,
			probeDistance, probeMask
		))
		{
			return false;
		}

		float upDot = Vector3.Dot(upAxis, hit.normal);
		if (upDot < minGroundDotProduct)
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

	void ClearState()
	{
		groundContactCount = steepContactCount = 0;
		contactNormal = steepNormal = Vector3.zero;
	}

	private void BoostBurst(InputAction.CallbackContext context)
	{
		Debug.Log("Boost Burst called");
		rb.AddForce(desiredVelocity.normalized * 200f * boostPower);
	}
	private void BoostHold()
	{
		Debug.Log("Boost Hold called");
		rb.AddForce(desiredVelocity.normalized * boostPower);
	}

	void Jump(Vector3 gravity)
	{
		Debug.Log("Jump called");
		Vector3 jumpDirection;
		if (onGround)
		{
			jumpDirection = contactNormal;
		}
		else
		{
			return;
		}

		stepsSinceLastJump = 0;
		float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
		jumpDirection = (jumpDirection + upAxis).normalized;
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

	void OnCollisionExit(Collision collision)
	{
		EvaluateCollision(collision);
	}

	void EvaluateCollision(Collision collision)
	{
		float minDot = minGroundDotProduct;
		for (int i = 0; i < collision.contactCount; i++)
		{
			Vector3 normal = collision.GetContact(i).normal;
			float upDot = Vector3.Dot(upAxis, normal);
			if (upDot >= minDot)
			{
				groundContactCount += 1;
				contactNormal += normal;
			}
			else if (upDot > -0.01f)
			{
				steepContactCount += 1;
				steepNormal += normal;
			}
		}
	}

	bool CheckSteepContacts()
	{
		if (steepContactCount > 1)
		{
			steepNormal.Normalize();
			float upDot = Vector3.Dot(upAxis, steepNormal);
			if (upDot >= minGroundDotProduct)
			{
				steepContactCount = 0;
				groundContactCount = 1;
				contactNormal = steepNormal;
				return true;
			}
		}
		return false;
	}
}
