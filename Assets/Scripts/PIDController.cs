using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class PIDController : MonoBehaviour
{
    // Transform of a target
    [SerializeField]
    protected Transform m_target = null;

    // class for pid gains
    [System.Serializable]
    public class Gain
    {
        public float p = 1.0f;

        public float i = 0.0f;

        public float d = 1.0f;
    }

    // [BoxGroup("RotationGain")]
    [SerializeField]
    // [SerializeField, InlineProperty, HideLabel]
    protected Gain m_rotGain = new Gain();

    [SerializeField]
    protected bool m_gravityCompensation = true;

    // rigidbody of an attached gameobject
    private Rigidbody rb_;

    // rigidbody of an attached gameobject
    private Transform tf_;

    // Rotation error
    private Quaternion rotError_ = Quaternion.identity;

    private Quaternion prevRotError_ = Quaternion.identity;

    private Quaternion diffRotError_ = Quaternion.identity;

    private Quaternion intRotError_ = Quaternion.identity;

    private float angleError_ = 0f;

    private Vector3 errorAxis_;

    private Vector3 diffErrorAxis_;

    private Vector3 intErrorAxis_;

    // private float prevAngleError_ = 0f;
    private float diffAngleError_ = 0f;

    private float intAngleError_ = 0f;

    private Vector3 force_ = Vector3.zero;

    /**
    * @brief Rotation gains
    * 
    */
    public Gain rotationGains
    {
        get
        {
            return m_rotGain;
        }
    }

    /**
    * @brief rotation axis
    * 
    */
    Vector3 RotAxis(Quaternion q)
    {
        var n = new Vector3(q.x, q.y, q.z);
        return n.normalized;
    }

    /**
    *
    */
    protected Quaternion RotOptimize(Quaternion q)
    {
        if (q.w < 0.0f)
        {
            q.x *= -1.0f;
            q.y *= -1.0f;
            q.z *= -1.0f;
            q.w *= -1.0f;
        }

        return q;
    }

    /**
    * @brief Set a target object
    * 
    * @param target target transform
    */
    public void SetTarget(Transform target)
    {
        m_target = target;
    }

    /**
    * @brief Set pid gains for rotation control
    * 
    * @param p proportional gain
    * @param i integral gain
    * @param d differential gain
    */
    public void SetRotationGains(float p, float i, float d)
    {
        m_rotGain.p = p;
        m_rotGain.i = i;
        m_rotGain.d = d;
    }

    void Awake()
    {
        rb_ = GetComponent<Rigidbody>();
        tf_ = transform;
    }

    void FixedUpdate()
    {
        if (m_target == null)
        {
            return;
        }

        /// rotation ///
        rotError_ =
            RotOptimize(m_target.rotation *
            Quaternion.Inverse(tf_.rotation));

        // diffRotError_ = rotError_ * Quaternion.Inverse(prevRotError_);
        // intRotError_ = intRotError_ * rotError_;
        rotError_.ToAngleAxis(out angleError_, out errorAxis_);

        // diffRotError_.ToAngleAxis(out diffAngleError_, out diffErrorAxis_);
        // intRotError_.ToAngleAxis(out intAngleError_, out intErrorAxis_);
        // var trq = errorAxis_ * (m_rotGain.p*angleError_)
        //   +diffErrorAxis_*(m_rotGain.i*diffAngleError_)
        //   +intErrorAxis_*(m_rotGain.d*intAngleError_);
        angleError_ *= Mathf.Deg2Rad; // deg to rad
        var angVel_ = m_rotGain.p * angleError_ * errorAxis_;

        if (angleError_ * angleError_ > 0.01f)
        {
            rb_.angularVelocity = angVel_;
        }
        prevRotError_ = rotError_;
    }
}
