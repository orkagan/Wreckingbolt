using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smashable : MonoBehaviour
{
    [SerializeField] private float forceMultiplier = 1f;
    [SerializeField] private float radius = 1f;
    [SerializeField] private float upwardsMod = 0.5f;

    [SerializeField] private GameObject player;
    Fracture fracture_ref;
	private void Start()
	{
        fracture_ref = GetComponent<Fracture>();
		//JANK way to find player's rigidbody
		foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
		{
			if (obj.GetComponent<Rigidbody>()) player = obj;
		}
	}
	private void OnCollisionEnter(Collision collision)
	{
		Debug.Log("Bonked Speed: " + collision.relativeVelocity.magnitude);
		if (collision.gameObject.CompareTag("Player") & collision.relativeVelocity.magnitude > 8f)
		{
            //GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.7f, 1f);
            fracture_ref.CauseFracture();
            //HitForce(collision.transform.position, collision.relativeVelocity.magnitude);
		}
	}
	private void OnTriggerEnter(Collider other)
	{
		//HitForce(other.transform.position, force);
		if (other.CompareTag("Player"))
		{;
			fracture_ref.callbackOptions.onCompleted.AddListener(HitForce);
			fracture_ref.triggerOptions.triggerType = TriggerType.Trigger;
			fracture_ref.triggerOptions.filterCollisionsByTag = true;
			fracture_ref.CauseFracture();
		}
	}

	public void HitForce()
    {
        Debug.Log("HitForce called");
		Collider[] hittingObjects = Physics.OverlapSphere(player.transform.position, radius);

		for (int i = 0; i < hittingObjects.Length; i++)
		{
			Rigidbody rb = hittingObjects[i].GetComponent<Rigidbody>();

			if (!rb) continue;
			rb.AddExplosionForce(player.GetComponent<Rigidbody>().velocity.magnitude * forceMultiplier, player.transform.position, radius, upwardsMod);
			Debug.Log($"{rb.gameObject.name}");
		}
	}
}
