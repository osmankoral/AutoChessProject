using Photon.Deterministic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Quantum
{
    public class PlayerExpUpdate : DeterministicCommand
    {
        public EntityRef PlayerRef;
        public int Exp;
        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref PlayerRef);
            stream.Serialize(ref Exp);
        }

        public void Execute(Frame f)
        {
            Log.Error(11);
            var script = f.Get<playerLink>(PlayerRef);

            script.Exp = script.Exp + Exp;

            if(script.Exp > script.MaxExp)
            {
                script.Level = script.Level + 1;
                script.Exp = script.Exp - script.MaxExp;

                switch (script.Level)
                {
                    case 2:
                        script.MaxExp = 2;
                        break;

                    case 3:
                        script.MaxExp = 6;
                        break;

                    case 4:
                        script.MaxExp = 10;
                        break;

                    case 5:
                        script.MaxExp = 20;
                        break;

                    case 6:
                        script.MaxExp = 36;
                        break;

                    case 7:
                        script.MaxExp = 46;
                        break;

                    case 8:
                        script.MaxExp = 76;
                        break;

                    case 9:
                        script.MaxExp = 86;
                        break;

                }
            }
            script.Gold = script.Gold - 4;

            f.Set(PlayerRef, script);

        }
    }

    public class TakeDamage : DeterministicCommand
    {
        public EntityRef TargetRef;
        public EntityRef UnitRef;
        public int Damage;
        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref TargetRef);
            stream.Serialize(ref UnitRef);
            stream.Serialize(ref Damage);
        }

        public void Execute(Frame f)
        {
            var targetScript = f.Get<CharacterHandler>(TargetRef);
            var unitScript = f.Get<CharacterHandler>(UnitRef);

            targetScript.Health = targetScript.Health - Damage;
            targetScript.Mana = targetScript.Mana + 15;

            unitScript.Mana = unitScript.Mana + 10;

            f.Set(TargetRef, targetScript);
            f.Set(UnitRef, unitScript);
        }
    }

    public class ArrowSpawn : DeterministicCommand
    {
        public EntityRef UnitEntity;
        public FPVector3 StartPosition;
        public override void Serialize(BitStream stream)
        {

            stream.Serialize(ref UnitEntity);
            stream.Serialize(ref StartPosition);
        }

        public void Execute(Frame f)
        {
            var entityPrototype = f.FindAsset<EntityPrototype>("Resources/DB/Arrow|EntityPrototype");
            var arrow = f.Create(entityPrototype);

            var transform = f.Get<Transform3D>(arrow);
            transform.Position = StartPosition;

            var arrowScript = f.Get<Arrow>(arrow);
            arrowScript.Ref = UnitEntity;

            var unitScript = f.Get<CharacterHandler>(UnitEntity);
            unitScript.Mana = unitScript.Mana + 10;

            f.Set(arrow, transform);
            f.Set(arrow, arrowScript);
            f.Set(UnitEntity, unitScript);
        }
    }

    public class AttackAnimationSet : DeterministicCommand
    {
        public EntityRef UnitEntity;
        public bool IsAttack;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref UnitEntity);
            stream.Serialize(ref IsAttack);
        }

        public void Execute(Frame f)
        {
            var unitScript = f.Get<CharacterHandler>(UnitEntity);
            unitScript.IsAttack = IsAttack;
            f.Set(UnitEntity, unitScript);


        }
    }

    public class UseSkillAnimationSet : DeterministicCommand
    {
        public EntityRef UnitEntity;
        public int ManaValue;

        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref UnitEntity);
        }

        public void Execute(Frame f)
        {
            var unitScript = f.Get<CharacterHandler>(UnitEntity);
            unitScript.Mana = ManaValue;

            f.Set(UnitEntity, unitScript);

        }
    }

    public class UnitMovement : DeterministicCommand
    {
        public EntityRef UnitEntity;
        public FPVector3 NextPosition;
        public override void Serialize(BitStream stream)
        {

            stream.Serialize(ref UnitEntity);
            stream.Serialize(ref NextPosition);
        }

        public void Execute(Frame f)
        {
            var transform = f.Get<Transform3D>(UnitEntity);
            transform.Position = NextPosition;
            f.Set(UnitEntity, transform);
        }
    }


}
