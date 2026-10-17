using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObstacles
{
    void OnPlayerHit(PlayerController player);
    void Appear();
}