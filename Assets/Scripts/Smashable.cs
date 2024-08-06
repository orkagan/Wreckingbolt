using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smashable : MonoBehaviour
{
	private void OnCollisionEnter(Collision collision)
	{
		Debug.Log("Bonked Speed: " + collision.relativeVelocity.magnitude);
		if (collision.gameObject.CompareTag("Player") & collision.relativeVelocity.magnitude > 8f)
		{
			GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.7f, 1f);
		}
	}
}
