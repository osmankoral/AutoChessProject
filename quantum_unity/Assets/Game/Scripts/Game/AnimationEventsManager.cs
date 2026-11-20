using Photon.Deterministic;
using Quantum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventsManager : MonoBehaviour
{
    [SerializeField] private EntityView _unitRef;
    [SerializeField] private Transform _position;

    private void Setups(out QuantumGame game, out Frame frame, out CharacterHandler unitScript)
    {
        game = QuantumRunner.Default.Game;
        frame = game.Frames.Verified;

        unitScript = frame.Get<CharacterHandler>(_unitRef.EntityRef);
    }
    public void ElfArcherAttackEvemt()
    {
        Setups(out QuantumGame game, out Frame frame, out CharacterHandler unitScript);

       // Debug.LogError("Elf is Attack");

        if (!game.PlayerIsLocal(unitScript.Player))
            return;

        Vector3 tempPos = _position.position;
        FPVector3 startPos = new FPVector3(FP.FromFloat_UNSAFE(tempPos.x), FP.FromFloat_UNSAFE(tempPos.y), FP.FromFloat_UNSAFE(tempPos.z));

        var command = new ArrowSpawn()
        {
           UnitEntity = _unitRef.EntityRef,
           StartPosition = startPos,
        };
        game.SendCommand(unitScript.Player, command);


    }

    public void HumanWarriorAttackEvent()
    {
        Setups(out QuantumGame game, out Frame frame, out CharacterHandler unitScript);

       // Debug.LogError("Warrior is Attack");

        if (!game.PlayerIsLocal(unitScript.Player))
            return;


        var command = new TakeDamage()
        {
            UnitRef = _unitRef.EntityRef,
            TargetRef = unitScript.TargetEnemy,
            Damage = (int)unitScript.Damage,
        };
        game.SendCommand(unitScript.Player, command);
    }
}
