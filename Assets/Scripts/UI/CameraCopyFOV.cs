using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCopyFOV : MonoBehaviour
{
    Camera thisCamera;
	Camera copyCamera;

	void Start()
	{
		thisCamera = GetComponent<Camera>();
		copyCamera = transform.parent.GetComponent<Camera>();
	}

	void FixedUpdate()
    {
		thisCamera.fieldOfView = copyCamera.fieldOfView;
    }
}
