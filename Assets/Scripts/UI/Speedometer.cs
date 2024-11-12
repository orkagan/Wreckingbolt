using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Speedometer : MonoBehaviour
{
    public Transform player_tf;
    public TMP_Text speedometer;

    [SerializeField] float speed;
    private SmashCycle player_script;
    private Rigidbody movingTarget;

    private void Start()
    {
        player_script = player_tf.GetComponent<SmashCycle>();
        movingTarget = player_tf.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (movingTarget == null)
        {
            speedometer.text = "KPH\n0";
            return;
        }
        speed = movingTarget.velocity.magnitude;
        speedometer.text = $"KPH\n{Mathf.Floor(speed * 3.6f)}";
    }
}
