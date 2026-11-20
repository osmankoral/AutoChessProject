using Photon.Deterministic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum
{
    public unsafe class ArrowSystem : SystemMainThreadFilter<ArrowSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public Arrow* Arrow;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            var unitScript = f.Get<CharacterHandler>(filter.Arrow->Ref);
            var targetUnitScript = f.Get<CharacterHandler>(unitScript.TargetEnemy);

            var tempPos = targetUnitScript.Pos;
            tempPos.Y = filter.Transform->Position.Y;

            filter.Transform->LookAt(tempPos);

            var dis = FPVector3.Distance(filter.Transform->Position, tempPos);

            if (dis <= FP.FromFloat_UNSAFE(0.2f))
            {
                targetUnitScript.Health = targetUnitScript.Health - unitScript.Damage;
                targetUnitScript.Mana = targetUnitScript.Mana + 15;
                f.Set<CharacterHandler>(unitScript.TargetEnemy, targetUnitScript);
                                    
                f.Destroy(filter.Entity);

            }
            else
            {
                filter.Transform->Position = filter.Transform->Position + (filter.Transform->Forward * 5 * f.DeltaTime);
            }
        }
    }
}
