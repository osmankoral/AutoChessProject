using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum
{
    public class PlayerCommandSystem : SystemMainThread
    {
        public override void Update(Frame f)
        {
            for(int i = 0; i<f.PlayerCount; i++)
            {
                var command = f.GetPlayerCommand(i) as PlayerExpUpdate;
                command?.Execute(f);

                var takeDamageCommand = f.GetPlayerCommand(i) as TakeDamage;
                takeDamageCommand?.Execute(f);

                var arrowSpawnComman = f.GetPlayerCommand(i) as ArrowSpawn;
                arrowSpawnComman?.Execute(f);
                
                var unitMoveComman = f.GetPlayerCommand(i) as UnitMovement;
                unitMoveComman?.Execute(f);
                
                var attackAnimationComman = f.GetPlayerCommand(i) as AttackAnimationSet;
                attackAnimationComman?.Execute(f);

                var useSkillAnimationComman = f.GetPlayerCommand(i) as UseSkillAnimationSet;
                useSkillAnimationComman?.Execute(f);
            }
        }
    }
}
