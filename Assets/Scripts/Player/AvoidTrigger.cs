using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidTrigger : MonoBehaviour
{
    public PlayerController player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
            player.SetObstacleInFront(true);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle"))
            player.SetObstacleInFront(false);
    }
}
