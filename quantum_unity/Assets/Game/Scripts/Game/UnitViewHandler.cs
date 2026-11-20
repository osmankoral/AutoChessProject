using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quantum;
using UnityEditor.Rendering;
using UnityEngine.AI;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Realtime;
using Photon.Deterministic;

public class UnitViewHandler : MonoBehaviour
{
    [SerializeField] private EntityView _entityView;

    [SerializeField] private GameObject _playerUnitGameObject;
    [SerializeField] private BoxCollider _unitCollider;
    [SerializeField] private Transform _effectParent;
    [SerializeField] private Transform _meshParent;
    [SerializeField] private Transform _canvas;
    [SerializeField] private Vector3 _tempCanvas;
    [SerializeField] private NavMeshAgent _unitNavMesh;
    public BoardHandler Squat;

    private void Start()
    {

        var game = QuantumRunner.Default.Game;
        var frame = game.Frames.Verified;

        _tempCanvas = _canvas.position;
    }
    private void Update()
    {
        var game = QuantumRunner.Default.Game;
        if (Untils.TryGetQuantumFrame(out Frame frame))
        {
            if (frame.TryGetSingletonEntityRef<GameSession>(out var entity) == false)
            {
                return;
            }

            var gameSession = frame.GetSingleton<GameSession>();

            var charScript = frame.Get<CharacterHandler>(_entityView.EntityRef);

            var unitId = frame.Get<CharacterId>(_entityView.EntityRef);

            if(unitId.StarLevel > 0)
            {

                switch (unitId.StarLevel)
                {
                    case 1:

                        _effectParent.GetChild(0).gameObject.SetActive(true);
                        _effectParent.GetChild(1).gameObject.SetActive(false);
                        _meshParent.localScale = (Vector3.one * 0.65f) * 1.2f;

                        Vector3 tempPos = _tempCanvas;
                        tempPos.y += 0.2f;
                        _canvas.position = tempPos;

                        break;

                    case 2:

                        _effectParent.GetChild(0).gameObject.SetActive(false);
                        _effectParent.GetChild(1).gameObject.SetActive(true);
                        _meshParent.localScale = (Vector3.one * 0.65f) * 1.4f;

                        Vector3 tempPos2 = _tempCanvas;
                        tempPos2.y += 0.5f;
                        _canvas.position = tempPos2;

                        break;
                }
            }

            if (!charScript.IsCharacterActive)
            {
                if (game.PlayerIsLocal(charScript.Player))
                {
                    _unitCollider.enabled = true;
                    _playerUnitGameObject.transform.GetChild(0).gameObject.SetActive(true);
                }

                else
                {
                    _unitCollider.enabled = false;
                    _playerUnitGameObject.transform.GetChild(0).gameObject.SetActive(false);
                    _playerUnitGameObject.transform.GetChild(2).gameObject.SetActive(false);

                }
                return;
            }

            if(charScript.Health < 0)
            {
                _playerUnitGameObject.transform.GetChild(0).gameObject.SetActive(false);
                _playerUnitGameObject.transform.GetChild(2).gameObject.SetActive(false);
                return;

            }
            

            if(gameSession.State == GameState.Playing)
            {
                _playerUnitGameObject.transform.GetChild(0).gameObject.SetActive(true);
                _playerUnitGameObject.transform.GetChild(2).gameObject.SetActive(false);
                return;
            }

            else if(gameSession.State == GameState.Preparation)
            {
                if (game.PlayerIsLocal(frame.Get<CharacterHandler>(_entityView.EntityRef).Player))
                {
                    _playerUnitGameObject.transform.GetChild(0).gameObject.SetActive(true);
                    _playerUnitGameObject.transform.GetChild(2).gameObject.SetActive(false);
                }

                else
                {
                    _playerUnitGameObject.transform.GetChild(2).gameObject.SetActive(true);
                    _playerUnitGameObject.transform.GetChild(0).gameObject.SetActive(true);
                }
            }

            /*if(charScript.IsCharacterActive && gameSession.State == GameState.Playing)
            {
                var targetScript = frame.Get<CharacterHandler>(charScript.TargetEnemy);
                var tempFPPos = targetScript.Pos;
                Vector3 tempPos = Vector3.zero;
                tempPos.x = (float)tempFPPos.X;
                tempPos.y = (float)tempFPPos.Y;
                tempPos.z = (float)tempFPPos.Z;

                _unitNavMesh.SetDestination(tempPos);

                var Pos = transform.parent.transform.position;

                var PosPF = new FPVector3(FP.FromFloat_UNSAFE(Pos.x), FP.FromFloat_UNSAFE(Pos.x),  FP.FromFloat_UNSAFE(Pos.x));


                var comand = new UnitMovement()
                {
                    UnitEntity = _entityView.EntityRef,
                    NextPosition = PosPF,
                };

                game.SendCommand(comand);
            }*/
        }
    }
}
