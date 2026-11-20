using Quantum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class EventManager 
{
    public static Action CamPosAc;

    public static Func<Vector3> MousePosition3D;

    public static Action<bool> IsCharMovePlayer;

    public static Func<int> BoxIndex;

    public static Func<bool> IsCharActive;

    public static Action<EntityRef> CharEntityRef;

    public static Action<GameObject> BoardLoad;
}

