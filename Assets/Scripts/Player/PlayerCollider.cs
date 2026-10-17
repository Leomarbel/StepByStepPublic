using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollider : MonoBehaviour
{
    public PlayerController player;
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<AObstacles>(out var obstacle))
        {
            obstacle.OnPlayerHit(player);
            
        }
    }
}
