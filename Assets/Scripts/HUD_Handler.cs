using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD_Handler : MonoBehaviour
{
    public GameObject player;
    public SmashCycle scriptRef;
    public Rigidbody movingTarget;

    public TMP_Text speedometer;
    public Image boostBar;

    [SerializeField] float speed;

	private void Start()
	{
        movingTarget = player.GetComponent<Rigidbody>();
        scriptRef = player.GetComponent<SmashCycle>();
        scriptRef.OnBoostChanged += UpdateBoostBar;
	}

    void Update()
    {
        if (movingTarget == null)
        {
            speedometer.text = "KPH\n0";
            return;
        }
        speed = movingTarget.velocity.magnitude;
        speedometer.text = $"KPH\n{Mathf.Floor(speed * 3.6f)}";
    }

    void UpdateBoostBar(float boostAmt)
	{
        boostBar.fillAmount = boostAmt / scriptRef.BoostMaxAmount;
	}
}
