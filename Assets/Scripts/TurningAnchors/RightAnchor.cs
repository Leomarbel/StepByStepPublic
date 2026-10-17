using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightAnchor : CenterAnchor
{
    public override bool CanTurnLeft() => false;
    public override bool CanTurnRight() => true;
}

