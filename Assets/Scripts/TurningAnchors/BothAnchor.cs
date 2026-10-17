using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BothAnchor : CenterAnchor
{
    public override bool CanTurnLeft() => true;
    public override bool CanTurnRight() => true;
}

