using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Speedometer : MonoBehaviour
{
    public Rigidbody movingTarget;
    public TMP_Text speedometer;

    [SerializeField] float speed;

    void Update()
    {
		if (movingTarget==null)
		{
            speedometer.text = "KPH\n0";
            return;
		}
        speed = movingTarget.velocity.magnitude;
        speedometer.text = $"KPH\n{Mathf.Floor(speed*3.6f)}";
    }
}
