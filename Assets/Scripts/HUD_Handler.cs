using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD_Handler : MonoBehaviour
{
    public RectTransform canvas_tf;
    public Transform camera_tf;
    public Transform player_tf;
    
    private SmashCycle player_script;
    private OrbitCamera camera_script;
    private Rigidbody movingTarget;

    public TMP_Text speedometer;
    public Image boostBar;

    [SerializeField] float HUDHeightOffset = 0.5f;
    [SerializeField] float speed;

	private void Start()
	{
        movingTarget = player_tf.GetComponent<Rigidbody>();
        player_script = player_tf.GetComponent<SmashCycle>();
        camera_script = camera_tf.GetComponent<OrbitCamera>();
        player_script.OnBoostChanged += UpdateBoostBar;
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


        Vector3 toUp = camera_script.gravityAlignment * Vector3.up;

        canvas_tf.position = player_tf.position - toUp * HUDHeightOffset;
        canvas_tf.rotation = Quaternion.LookRotation(-toUp, canvas_tf.position - camera_tf.position);

        speedometer.transform.rotation = Quaternion.LookRotation(speedometer.transform.position - camera_tf.position, toUp);
    }

    void UpdateBoostBar(float boostAmt)
	{
        boostBar.fillAmount = boostAmt / player_script.BoostMaxAmount;
	}
}
