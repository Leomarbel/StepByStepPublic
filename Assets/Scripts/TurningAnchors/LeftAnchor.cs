using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftAnchor : CenterAnchor
{
    public override bool CanTurnLeft() => true;
    public override bool CanTurnRight() => false;
}

