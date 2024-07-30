using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Speedometer : MonoBehaviour
{
    public Rigidbody movingTarget;
    public TMP_Text speedometer;

    void Update()
    {
		if (movingTarget==null)
		{
            speedometer.text = "KPH\n0";
            return;
		}
        speedometer.text = $"KPH\n{Mathf.Floor(movingTarget.velocity.magnitude *3.6f)}";
    }
}
