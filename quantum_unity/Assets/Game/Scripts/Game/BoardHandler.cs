using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardHandler : MonoBehaviour
{
    [SerializeField] private bool _isActiveChar;
    public int Index;
    public bool IsActiveChar { get => _isActiveChar; }

    public bool IsFull;

}
