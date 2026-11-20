using Cinemachine;
using Quantum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSessionHandler : MonoBehaviour
{
    [SerializeField] EntityView entityView;
    [SerializeField] Transform pos1;
    [SerializeField] Transform pos2;
    [SerializeField] Transform pos3;
    [SerializeField] private GameObject BoardObject;
    [SerializeField] private Transform backStartPosition1;
    [SerializeField] private Transform backStartPosition2;

    public int target = 30;

    public Transform Position1 { get => pos1; }
    public Transform Position2 { get => pos2; }

    public GameObject GetBoardObject {  get => BoardObject; }
    
    public Transform BackPosition1 { get => backStartPosition1; }
    public Transform BackPosition2 { get => backStartPosition2; }

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = target;
    }

    public void OnEntityInstantiated()
    {
        Debug.Log("Player character controller OnEntityInstantiated");

        QuantumGame game = QuantumRunner.Default.Game;

        Frame frame = game.Frames.Verified;

        if (frame.TryGet(entityView.EntityRef, out playerLink playerLink))
        {
            if (game.PlayerIsLocal(playerLink.Player))
            {
                
            }
        }
    }

    void Update()
    {
        if (Application.targetFrameRate != target)
            Application.targetFrameRate = target;
    }
}

