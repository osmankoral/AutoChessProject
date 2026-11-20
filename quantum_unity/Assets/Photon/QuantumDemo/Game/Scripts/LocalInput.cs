using System;
using System.Collections;
using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.UI;

public class LocalInput : MonoBehaviour {

    EntityRef _char;
    
    bool fp;
    int prefabIndex;

    bool _isCharMove;
    bool _isCharActive;
    int _boxIndex;



    private void OnEnable() {
    QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));

        EventManager.CharEntityRef += Charr1;
        EventManager.IsCharMovePlayer += IsCharMovePlayer;
  }

    private void OnDisable()
    {
        EventManager.CharEntityRef -= Charr1;
        EventManager.IsCharMovePlayer -= IsCharMovePlayer;
        
    }

    public void PollInput(CallbackPollInput callback) {
    Quantum.Input i = new Quantum.Input();

        i.Jump = UnityEngine.Input.GetButton("Jump");

        i.Create = UnityEngine.Input.GetKey(KeyCode.K);

        if(fp)
        {
            i.Crreat = true;
            i.PrefabIndex = prefabIndex;
            fp = false;
        }
        Vector2 inputDirection = Vector2.zero;

        inputDirection.x = UnityEngine.Input.GetAxis("Horizontal");
        inputDirection.y = UnityEngine.Input.GetAxis("Vertical");


        i.DirectionX = (short)(inputDirection.x * 10);
        i.DirectionY = (short)(inputDirection.y * 10);

        if(EventManager.IsCharMovePlayer != null)
        {
            i.IsCharMove = _isCharMove;

            var tempPos = EventManager.MousePosition3D.Invoke();
            FPVector3 tempQPos = new FPVector3(FP.FromFloat_UNSAFE(tempPos.x), FP.FromFloat_UNSAFE(tempPos.y + 0.5f), FP.FromFloat_UNSAFE(tempPos.z));

            i.CharPlayerMove = tempQPos;

            i.CharEntity = _char;

            i.BoxIndex = EventManager.BoxIndex.Invoke();

            i.IsCharActiive = EventManager.IsCharActive.Invoke();
        }

    callback.SetInput(i, DeterministicInputFlags.Repeatable);
  }

  public void Prss(int index)
    {
        fp = true;
        prefabIndex = index;
    }


    void Charr1(EntityRef e)
    {
        _char = e;
    }

    private void IsCharMovePlayer( bool b)
    {
        _isCharMove = b;
    }

    private void IsCharActive(bool a)
    {
        _isCharActive = a;
    }
    
    private void BoxIndex(int i)
    {
        _boxIndex = i;
    }

}
