using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAction
{
    public void Action();
    public void DeActive();

    public bool IsCharacterActive();

    public bool IsCharacterHasPlayer();
    
}
