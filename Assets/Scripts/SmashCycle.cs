using System;
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
	[SerializeField] float maxAirAcceleration = 2f;
	[SerializeField, Range(0,1)] float frictionCoefficient = 0.02f;
	[SerializeField] float turnSpeed = 0.05f;
	[SerializeField] float jumpHeight = 10f;

	[SerializeField] float boostPower = 5000f;
	[SerializeField] float boostMaxAmount = 10f; //burns 1 a second
	[SerializeField] float boostRechargeRate = 1f;
	[SerializeField] float boostRechargeDelay = 1f;
	public float BoostMaxAmount
	{
		get
		{
			return boostMaxAmount;
		}
		private set
		{
			boostMaxAmount = value;
		}
	}

	[Header("Surface Contact")]
	[SerializeField, Range(0, 90)]
	float maxGroundAngle = 25f;
	[SerializeField, Range(0f, 100f)]
	float maxSnapSpeed = 100f;
	[SerializeField, Min(0f)]
	float probeDistance = 1f;
	[SerializeField]
	LayerMask probeMask = -1;

	#endregion

	#region References
	[Header("References")]
	[SerializeField] Transform playerInputSpace = default;
	[SerializeField] PlayerInputHandler controls;
	public Transform vehicleBody_tf;
	public Transform tilt_tf;
	public Transform wheel_tf;
	public Transform seat_tf;
	public ParticleSystem jetThrustFX;

	Rigidbody rb;
	#endregion

	#region Internal Variables
	float boostRechargeTime;
	Vector2 playerInputMove;
	Vector3 desiredVelocity;
	Vector3 sideFriction;
	[SerializeField]
	Vector3 velocity;
	Vector3 upAxis, rightAxis, forwardAxis;
	[SerializeField]
	float wheelRollSpeed;
	bool desiredJump;
	bool OnGround => groundContactCount > 0;
	bool OnSteep => steepContactCount > 0;
	int groundContactCount, steepContactCount;
	int stepsSinceLastJump, stepsSinceLastGrounded;
	float minGroundDotProduct;
	Vector3 contactNormal, steepNormal;
	
	bool desiredBoost;
	public event Action<float> OnBoostChanged;
	[SerializeField]
	float boostAmount;
	public float BoostAmount
	{
		get
		{
			return boostAmount;
		}
		private set
		{
			boostAmount = value;
			OnBoostChanged?.Invoke(BoostAmount);
		}
	}
	#endregion

	private void OnValidate()
	{
		rb = GetComponent<Rigidbody>();
		rb.maxAngularVelocity = maxAngularVelocity;
		rb.useGravity = false;

		controls = GetComponent<PlayerInputHandler>();

		minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
	}
	private void Awake()
	{
		OnValidate();
	}
	private void Start()
    {
		BoostAmount = BoostMaxAmount;
		boostRechargeTime = 0;
		jetThrustFX.Stop();
	}

    private void Update()
    {
		if (GameManager.Instance.CurrentGameState!=GameState.Playing)
		{
			return;
		}
		//Get player move input
		playerInputMove = Vector2.ClampMagnitude(controls.move.ReadValue<Vector2>(), 1f);
		//Convert input to camera space, else world space
		if (playerInputSpace)
		{
			rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
			forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
		}
		else
		{
			rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
			forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
		}
		desiredVelocity = (rightAxis* playerInputMove.x + forwardAxis * playerInputMove.y) * maxSpeed;

		desiredJump |= controls.jump.WasPressedThisFrame();
		desiredBoost |= controls.boost.WasPressedThisFrame();

		//Vehicle body copy position
		vehicleBody_tf.position = transform.position;
		#region Visuals using exterrnal transforms

		//Target look direction
		Vector3 lookDir = (desiredVelocity.magnitude > 0.1f) ? desiredVelocity : vehicleBody_tf.forward;
		//Seat rotation
		Quaternion lookTarget = Quaternion.LookRotation(lookDir, upAxis);
		//Seat tilt
		float seatTilt = Mathf.Clamp(-Vector3.SignedAngle(rb.velocity, desiredVelocity, upAxis), -60, 60);
		lookTarget *= Quaternion.Euler(0, 0, seatTilt);
		//Apply rotation with smoothing
		seat_tf.rotation = Quaternion.Lerp(seat_tf.rotation, lookTarget, Time.deltaTime * 2f);

		//Ball Tilt
		Vector3 up = (OnGround) ? contactNormal : upAxis;
		float ballTiltSide = Mathf.Clamp(Vector3.SignedAngle(vehicleBody_tf.forward.normalized, rb.velocity.normalized, up) / 90f, -1, 1);
		float ballTiltAmount = (!OnGround) ? 0 : Mathf.Clamp(sideFriction.magnitude * 360f, 0f, 60f);
		tilt_tf.localRotation = Quaternion.Lerp(tilt_tf.localRotation, Quaternion.Euler(0, 0, ballTiltAmount * ballTiltSide), Time.deltaTime * 4);

		//Wheel Roll
		wheelRollSpeed = Mathf.Rad2Deg * Vector3.Dot(rb.angularVelocity, vehicleBody_tf.right.normalized);
		wheel_tf.Rotate(Vector3.right, wheelRollSpeed * Time.deltaTime);
		#endregion

		//Debug vectors
		Debug.DrawLine(transform.position, transform.position + desiredVelocity, Color.blue);
        Debug.DrawLine(transform.position, transform.position + sideFriction * 10f, Color.yellow);
        Debug.DrawLine(transform.position, transform.position + rb.velocity, Color.red);
        Debug.DrawLine(transform.position, transform.position + -upAxis, Color.magenta);
    }

	private void FixedUpdate()
	{
		//Vector3 gravity = Physics.gravity; upAxis = Vector3.up;
		Vector3 gravity = CustomGravity.GetGravity(rb.position, out upAxis);
		if (gravity == Vector3.zero) upAxis = playerInputSpace.up;
		//Debug.Log($"Gravity: {gravity}\nUpAxis: {upAxis}");
		//Debug.DrawLine(rb.position, rb.position + upAxis, Color.magenta);
		UpdateState();
		AdjustVelocity();

		//Boost
		//if (Gamepad.current.rightShoulder.IsPressed())
		/*if (boost.IsPressed())
		{
			BoostHold();
		}
		else
		{
			jetThrustFX.Stop();
		}*/

		if (desiredJump)
        {
			desiredJump = false;
			Jump(gravity);
        }

		Boost();

		velocity += gravity * Time.deltaTime;

		rb.velocity = velocity;
		//Debug.Log($"OnGround: {OnGround}");
		ClearState();
	}
	
	void UpdateState()
	{
		stepsSinceLastGrounded += 1;
		stepsSinceLastJump += 1;
		velocity = rb.velocity;
		if (OnGround || SnapToGround() || CheckSteepContacts())
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

	void AdjustVelocity()
	{
		//float maxSpeedChange = maxAcceleration * Time.deltaTime;

		//Direction Vehicle is facing
		Vector3 upDir = OnGround ? contactNormal : upAxis;
		Quaternion lookTarget = Quaternion.LookRotation(vehicleBody_tf.forward, upDir);
		//(0.1f is kinda hardcoded deadzone)
		if (desiredVelocity.magnitude > 0.1f)
		{
			//Turn to desired direction
			lookTarget = Quaternion.LookRotation(desiredVelocity, upDir);
			//float turnSpeed = turnSpeedCurve.Evaluate(); //TODO: reduce turning capabilities as speed increases
		}
		else
		{
			lookTarget = Quaternion.LookRotation(ProjectDirectionOnPlane(vehicleBody_tf.forward,upDir),upDir);
		}
		vehicleBody_tf.rotation = Quaternion.Lerp(vehicleBody_tf.rotation, lookTarget, turnSpeed);

		//Tire side friction
		if (OnGround)
		{
			//TODO: make friction asymptotic curve
			sideFriction = Vector3.Dot(velocity, vehicleBody_tf.right.normalized) * -frictionCoefficient * vehicleBody_tf.right.normalized;
			velocity += sideFriction;
		}
		else
		{
			sideFriction = Vector3.zero;
			velocity += desiredVelocity.normalized * maxAirAcceleration * Time.deltaTime;
		}

		//Appply forces
		//rb.velocity = velocity;
		//rb.angularVelocity += vehicleBody_tf.right * desiredVelocity.magnitude;
		rb.AddTorque(vehicleBody_tf.right * desiredVelocity.magnitude * 100f);
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
		Debug.DrawLine(rb.position, rb.position - upAxis.normalized*probeDistance, Color.white);
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

	/*private void BoostBurst(InputAction.CallbackContext context)
	{
		Debug.Log("Boost Burst called");
		rb.AddForce(desiredVelocity.normalized * 200f * boostPower);
	}*/
	private void Boost()
	{
		if(!controls.boost.IsPressed() || BoostAmount <= 0)
		{
			desiredBoost = false;
		}

		if (desiredBoost)
		{
			jetThrustFX.Play();
			rb.AddForce(desiredVelocity.normalized * boostPower);
			BoostAmount -= 1f * Time.deltaTime;
			boostRechargeTime = boostRechargeDelay;
		}
		else
		{
			jetThrustFX.Stop();
		}

		if (boostRechargeTime <= 0 & BoostAmount<BoostMaxAmount)
		{
			BoostAmount += boostRechargeRate * Time.deltaTime;
		}
		else
		{
			boostRechargeTime -= Time.deltaTime;
		}
	}

	void Jump(Vector3 gravity)
	{
		Vector3 jumpDirection;
		if (OnGround)
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
		//Debug.Log("Collision Enter");
	}

	void OnCollisionExit(Collision collision)
	{
		EvaluateCollision(collision);
		//Debug.Log("Collision Exit");
	}

	void OnCollisionStay(Collision collision)
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

	Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
	{
		return (direction - normal * Vector3.Dot(direction, normal)).normalized;
	}
}
