using UnityEngine;
using UnityEditor;

public class GravityTube : GravitySource
{

	[SerializeField]
	float gravity = 9.81f;
	[SerializeField, Min(0f)]
	float range = 15f;

	[SerializeField]
	bool falloff = true;

	[SerializeField, Min(0f)]
	float innerFalloffRadius = 1f, innerRadius = 5f;

	[SerializeField, Min(0f)]
	float outerRadius = 10f, outerFalloffRadius = 15f;

	float innerFalloffFactor, outerFalloffFactor;

	public override Vector3 GetGravity(Vector3 position)
	{
		//Vector3 vector = transform.position - position;
		Vector3 direction = transform.up;
		float directionRange = Vector3.Dot(direction, position - transform.position);
		Vector3 pos2origin = transform.position - position;
		Vector3 vector = pos2origin - Vector3.Dot(transform.up, pos2origin) * direction;
		float distance = vector.magnitude;
		//float distance = Vector3.Dot(transform.up, transform.position - position);
		Debug.DrawLine(position, position + vector, Color.magenta);
		if (directionRange > range || directionRange < 0 || distance > outerFalloffRadius || distance < innerFalloffRadius)
		{
			return Vector3.zero;
		}
		if (!falloff)
		{
			return gravity * vector.normalized;
		}
		float g = gravity / distance;
		if (distance > outerRadius)
		{
			g *= 1f - (distance - outerRadius) * outerFalloffFactor;
		}
		else if (distance < innerRadius)
		{
			g *= 1f - (innerRadius - distance) * innerFalloffFactor;
		}
		return g * vector;
	}

	void Awake()
	{
		OnValidate();
	}

	void OnValidate()
	{
		innerFalloffRadius = Mathf.Max(innerFalloffRadius, 0f);
		innerRadius = Mathf.Max(innerRadius, innerFalloffRadius);
		outerRadius = Mathf.Max(outerRadius, innerRadius);
		outerFalloffRadius = Mathf.Max(outerFalloffRadius, outerRadius);

		innerFalloffFactor = 1f / (innerRadius - innerFalloffRadius);
		outerFalloffFactor = 1f / (outerFalloffRadius - outerRadius);
	}

	void OnDrawGizmos()
	{
		Vector3 p = transform.position;
		Vector3 direction = transform.up;
		if (innerFalloffRadius > 0f && innerFalloffRadius < innerRadius)
		{
			Handles.color = Color.cyan;
			Handles.DrawWireDisc(p, transform.up, innerFalloffRadius);
			Handles.DrawWireDisc(p + direction * range, transform.up, innerFalloffRadius);
		}
		Handles.color = Color.yellow;
		if (innerRadius > 0f && innerRadius < outerRadius)
		{
			Handles.DrawWireDisc(p, transform.up, innerRadius);
			Handles.DrawWireDisc(p + direction * range, transform.up, innerRadius);
		}
		Handles.DrawWireDisc(p, transform.up, outerRadius);
		Handles.DrawWireDisc(p + direction * range, transform.up, outerRadius);
		if (outerFalloffRadius > outerRadius)
		{
			Handles.color = Color.cyan;
			Handles.DrawWireDisc(p, transform.up, outerFalloffRadius);
			Handles.DrawWireDisc(p + direction * range, transform.up, outerFalloffRadius);
		}
	}
}