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
    public Transform speedometer_tf;
    public Transform timer_tf;
    
    private OrbitCamera camera_script;
    private SmashCycle player_script;

    public Image boostBar;

    [SerializeField] float HUDHeightOffset = 0.5f;
    
    Vector3 upAxis;

	private void Start()
	{
        player_script = player_tf.GetComponent<SmashCycle>();
        camera_script = camera_tf.GetComponent<OrbitCamera>();

        player_script.OnBoostChanged += UpdateBoostBar;
    }

    void Update()
    {
        upAxis = camera_script.gravityAlignment * Vector3.up;

        canvas_tf.position = player_tf.position - upAxis * HUDHeightOffset;
        canvas_tf.rotation = Quaternion.LookRotation(-upAxis, canvas_tf.position - camera_tf.position);

        /*speedometer.transform.rotation = Quaternion.LookRotation(speedometer.transform.position - camera_tf.position, upAxis);
        timer_tf.rotation = Quaternion.LookRotation(timer_tf.position - camera_tf.position, upAxis);*/
        LookAtCamera(speedometer_tf);
        LookAtCamera(timer_tf);
    }

    void LookAtCamera(Transform tf)
	{
        tf.rotation = Quaternion.LookRotation(tf.position - camera_tf.position, upAxis);
    }

    void UpdateBoostBar(float boostAmt)
    {
        boostBar.fillAmount = boostAmt / player_script.BoostMaxAmount;
    }
}
