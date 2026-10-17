using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AObstacles : MonoBehaviour
{
    public abstract void OnPlayerHit(PlayerController player);
    public abstract void Appear();
}
