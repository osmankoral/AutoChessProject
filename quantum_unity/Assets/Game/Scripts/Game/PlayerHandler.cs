using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;
using Cinemachine;
using Photon.Deterministic;

public class PlayerHandler : MonoBehaviour
{
    [SerializeField] EntityView entityView;
    
    [SerializeField] private GameObject BoardObject;
    [SerializeField] private Transform BackStart1Position;
    [SerializeField] private Transform BackStart2Position;

    GameSessionHandler gameSessionHandler;


    public void OnEntityInstantiated()
    {
        Debug.Log("Player character controller OnEntityInstantiated");

        QuantumGame game = QuantumRunner.Default.Game;

        Frame frame = game.Frames.Verified;

        var gameManager = GameObject.FindObjectOfType<GameSessionHandler>();

        if(frame.TryGet(entityView.EntityRef, out playerLink playerLink))
        {
            if(game.PlayerIsLocal(playerLink.Player))
            {
                CinemachineVirtualCamera virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
                
                if ((int)playerLink.Player == 0 || (int)playerLink.Player == 2 || (int)playerLink.Player == 4)
                {
                    virtualCamera.transform.position = gameManager.Position1.position;
                    virtualCamera.transform.rotation = gameManager.Position1.rotation;
                }

                else if ((int)playerLink.Player == 1 || (int)playerLink.Player == 3 || (int)playerLink.Player == 5)
                {
                    virtualCamera.transform.position = gameManager.Position2.position;
                    virtualCamera.transform.rotation = gameManager.Position2.rotation;
                }
            }
        }
    }

    private void Start()
    {
        /* QuantumGame game = QuantumRunner.Default.Game;

         Frame frame = game.Frames.Verified;
         if (frame.TryGet(entityView.EntityRef, out playerLink playerLink))
         {

             if (game.PlayerIsLocal(playerLink.Player))
                 EventManager.BoardLoad.Invoke(BoardObject);
         }*/

        gameSessionHandler = FindAnyObjectByType<GameSessionHandler>();
    }

    private void Update()
    {
        BackStart1Position = gameSessionHandler.BackPosition1;
        BackStart2Position = gameSessionHandler.BackPosition2;

        QuantumGame game = QuantumRunner.Default.Game;

        Frame frame = game.Frames.Verified;

        var a = frame.Get<playerLink>(entityView.EntityRef);

        FPVector3 tempPosition1 = new FPVector3();
        tempPosition1.X = FP.FromFloat_UNSAFE(BackStart1Position.position.x);
        tempPosition1.Y = FP.FromFloat_UNSAFE(BackStart1Position.position.y + 0.5f);
        tempPosition1.Z = FP.FromFloat_UNSAFE(BackStart1Position.position.z);

        a.BackSet1 = tempPosition1;

        FPVector3 tempPosition2 = new FPVector3();
        tempPosition2.X = FP.FromFloat_UNSAFE(BackStart2Position.position.x);
        tempPosition2.Y = FP.FromFloat_UNSAFE(BackStart2Position.position.y + 0.5f);
        tempPosition2.Z = FP.FromFloat_UNSAFE(BackStart2Position.position.z);

        a.BackSet2 = tempPosition2;

        frame.Set(entityView.EntityRef, a);
    }
}
