using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obs_Detection : MonoBehaviour
{
    [SerializeField] private AObstacles obj;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            obj.Appear();
        }
    }
}
