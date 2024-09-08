using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SmashCycle : MonoBehaviour
{
	#region Parameters
	[Header("Parameters")]
	[SerializeField] float maxSpeed = 20f;
	[SerializeField] float maxAcceleration = 10f;
	#endregion
	#region References
	[Header("References")]
	[SerializeField] Transform playerInputSpace = default;
	public Transform vehicleBody;
	public Transform visualBall;
	public Transform visualSeat;

	Rigidbody rb;
	#endregion
	#region Internal Variables
	Vector2 playerInputMove;
	Vector3 desiredVelocity;
	[SerializeField]
	Vector3 velocity;
	[SerializeField]
	float wheelRollspeed;
	#endregion
	#region Controls/Inputs
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
	#endregion

	private void Start()
    {
		rb = GetComponent<Rigidbody>();

		//unparent hover seat (also keep it near in hierarchy)
		vehicleBody.SetParent(null, true);
		vehicleBody.SetSiblingIndex(transform.GetSiblingIndex());
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

		//Vehicle body copy position
		vehicleBody.position = transform.position;

		//Seat rotation
		Quaternion lookDir = vehicleBody.rotation;
		if (desiredVelocity.magnitude > 0.1f)
		{
			//TODO: replace Vector3.up with contact normal
			lookDir = Quaternion.LookRotation(desiredVelocity, Vector3.up);
		}
		Vector3 tiltSide = Vector3.Cross(desiredVelocity.normalized, rb.velocity.normalized);
		//visualSeat.localEulerAngles = new Vector3(visualSeat.localEulerAngles.x, visualSeat.localEulerAngles.y, 60f * tiltSide.y);
		lookDir.eulerAngles = new Vector3(lookDir.eulerAngles.x, lookDir.eulerAngles.y, 60f * tiltSide.y);

		visualSeat.rotation = Quaternion.Slerp(visualSeat.rotation, lookDir, Time.deltaTime * 2f);

        //Ball rotation
        Vector3 ballTiltSide = Vector3.Cross(vehicleBody.forward.normalized, rb.velocity.normalized);
        float ballTiltAmount = Vector3.Angle(vehicleBody.forward.normalized, rb.velocity.normalized) * velocity.magnitude / 10f;
        Quaternion ballTilt = Quaternion.Euler(0, 0, Mathf.Clamp(ballTiltAmount, 0f, 60f) * ballTiltSide.y);
        visualBall.localRotation = Quaternion.Slerp(visualBall.localRotation, ballTilt, Time.deltaTime * 10f);

        //Ball Roll
        wheelRollspeed = Vector3.Dot(rb.angularVelocity.normalized, vehicleBody.right.normalized) * rb.angularVelocity.magnitude;
		//float rollAmount = rb.velocity.magnitude * (180f / Mathf.PI) / 0.5f;
		/*if (wheelRollspeed > 1f)
		{
			visualBall.Rotate(wheelRollspeed, 0, 0);
		}*/
		//visualBall.Rotate(1f, 0, 0);

        //Debug vectors
        //Debug.DrawLine(transform.position, transform.position + rb.angularVelocity, Color.yellow);
        Debug.DrawLine(transform.position, transform.position + desiredVelocity, Color.blue);
        Debug.DrawLine(transform.position, transform.position + tiltSide, Color.green);
        Debug.DrawLine(transform.position, transform.position + rb.velocity, Color.red);
    }

	private void FixedUpdate()
	{
		velocity = rb.velocity;
		float maxSpeedChange = maxAcceleration * Time.deltaTime;

		if (desiredVelocity.magnitude > 0.1f)
		{
			//Turn to desired direction
			Quaternion lookTarget = Quaternion.LookRotation(desiredVelocity, Vector3.up);
			//float turnSpeed = turnSpeedCurve.Evaluate();
			float turnSpeed = 0.05f;
			vehicleBody.rotation = Quaternion.Lerp(vehicleBody.rotation, lookTarget, turnSpeed);
		}
		velocity.x = Mathf.MoveTowards(velocity.x, vehicleBody.forward.x * desiredVelocity.magnitude, maxSpeedChange);
		velocity.z = Mathf.MoveTowards(velocity.z, vehicleBody.forward.z * desiredVelocity.magnitude, maxSpeedChange);


		//TODO: Tire side friction
		Vector3 friction = Vector3.Dot(rb.velocity.normalized, vehicleBody.right.normalized) * -1 * rb.velocity;

		//Appply forces
		//rb.angularVelocity += vehicleBody.right * desiredVelocity;
		rb.AddTorque(vehicleBody.right * desiredVelocity.magnitude * 100f);
		//rb.velocity = velocity;
	}
}
