using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CenterAnchor : MonoBehaviour
{
    [SerializeField] protected Transform centerPoint;

    public Transform Center => centerPoint;

    // Called by PlayerController
    public virtual void OnPlayerEnter(PlayerController player)
    {
        player.BeginCenterAlignment(this);
    }

    // Polymorphic contract
    public abstract bool CanTurnLeft();
    public abstract bool CanTurnRight();
}

