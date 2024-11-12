using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GoalBehaviour : MonoBehaviour
{
    public UnityEvent OnWin;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            GameManager.Instance.Win();
            OnWin.Invoke();
        }
    }
}
