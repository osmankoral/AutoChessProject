using Quantum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Deterministic;
using UnityEngine.TextCore.Text;

public class CharacterUIHandler : MonoBehaviour, IAction
{
    [SerializeField] private Slider _healthBar;
    [SerializeField] private Slider _manaBar;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] EntityView entityView;

    [SerializeField] private Animator animator;

    private bool _isLocalPlayer;
    private bool _isEntery;

    private void Start()
    {
        _healthBar.maxValue = 100;
        _healthBar.maxValue = 100;


    }

    private void StartUpdate()
    {
        var game = QuantumRunner.Default.Game;
        var frame = game.Frames.Verified;
        if(frame.TryGet<CharacterHandler>(entityView.EntityRef, out CharacterHandler scrt))
        {

            if (game.PlayerIsLocal(scrt.Player))
            {
                _isLocalPlayer = true;
            }

            _isEntery = true;
        }

    }

    public void Update()
    {
        if(!_isEntery)
        {
            StartUpdate();
            return;
        }

        var game = QuantumRunner.Default.Game;
        var frame = game.Frames.Verified;

        if (frame.TryGet(entityView.EntityRef, out CharacterHandler character))
        {
            SetAnimation();

            _nameText.text = $"{character.Player} - {character.CharacterID}";
            _healthBar.value = (float)character.Health;
            _manaBar.value = (float)character.Mana;
            

        }
    }

    private void SetAnimation()
    {
        var game = QuantumRunner.Default.Game;
        var frame = (_isLocalPlayer) ? game.Frames.Predicted : game.Frames.Verified;
        var unitScript = frame.Get<CharacterHandler>(entityView.EntityRef);

        

        if (!unitScript.IsCharacterActive)
        {
            animator.SetBool("IsMove", false);
            return;
        }

        if(unitScript.Mana >= 90)
        {
            //animator.SetTrigger("SkillUse");

            if (_isLocalPlayer)
            {

                var command = new UseSkillAnimationSet()
                {
                    UnitEntity = entityView.EntityRef,
                    ManaValue = 0,
                };
                game.SendCommand(command);
            }
        }

        if (unitScript.IsMove)
        {
            animator.SetBool("IsMove", true);
        }

        else if (!unitScript.IsMove)
        {
            animator.SetBool("IsMove", false);
        }

        if (unitScript.IsAttack)
        {
            animator.SetTrigger("IsAttack");

            if(_isLocalPlayer)
            {
                QuantumAttackAnimationSet();
            }
        }

        
    }

    private void QuantumAttackAnimationSet()
    {
        var game = QuantumRunner.Default.Game;
        var frame = game.Frames.Verified;


        var command = new AttackAnimationSet()
        {
            UnitEntity = entityView.EntityRef,
            IsAttack = false,
        };
        game.SendCommand(command);
    }

    public void Action()
    {
        EventManager.IsCharMovePlayer.Invoke(true);

       EventManager.CharEntityRef.Invoke(entityView.EntityRef);

    }

    public void DeActive()
    {
        EventManager.IsCharMovePlayer.Invoke(false);
    }

    public bool IsCharacterActive()
    {
        bool tempActive = false;
        var game = QuantumRunner.Default.Game;
        var frame = game.Frames.Verified;

        if (frame.TryGet(entityView.EntityRef, out CharacterHandler character))
        {
            tempActive = character.IsCharacterActive;
        }

        return tempActive;
    }

    public bool IsCharacterHasPlayer()
    {
        var game = QuantumRunner.Default.Game;
        var frame = game.Frames.Verified;

        bool isCharacterHasPlayer = false;
        
        if (frame.TryGet(entityView.EntityRef, out CharacterHandler character))
        {
            isCharacterHasPlayer = game.PlayerIsLocal(character.Player);
        }

        return isCharacterHasPlayer;
    }
}
