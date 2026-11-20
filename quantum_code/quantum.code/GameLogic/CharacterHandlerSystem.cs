using Photon.Deterministic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Quantum.Navigation;

namespace Quantum
{
    public unsafe class CharacterHandlerSystem : SystemMainThreadFilter<CharacterHandlerSystem.Filter>
    {
        
        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public CharacterHandler* Character;
            public CharacterId* CharacterId;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            
            GameSession* gameSession = f.Unsafe.GetPointerSingleton<GameSession>();

            if (gameSession == null)
                return;

            if(gameSession->State == GameState.Preparation)
            {
                filter.Character->State = CharacterState.Standby;
            }

           /* if(filter.Character->IsReset)
            {
                  
                  filter.Character->IsReset = false;
            }*/
            if (filter.Character->State == CharacterState.Standby)
            {
                var player = f.Unsafe.GetPointer<playerLink>(filter.Character->PlayerEntityRef);

                filter.Transform->Position = filter.Character->GuardPos;
                filter.Character->Health = 100;

                var input = f.GetPlayerInput(filter.Character->Player);

                if (input->IsCharMove)
                {
                    if (input->CharEntity == filter.Entity)
                    {
                        filter.Transform->Position = input->CharPlayerMove;
                        filter.Character->GuardPos = input->CharPlayerMove;

                        if(input->BoxIndex < 9)
                        {
                            if(filter.Character->OnBoxIndex < 9)
                            {
                                player->Bachs[filter.Character->OnBoxIndex] = false;
                            }
                            player->Bachs[input->BoxIndex] = true;
                        }

                        else
                        {
                            if(filter.Character->OnBoxIndex < 9)
                            {
                                player->Bachs[input->BoxIndex] = false;

                            }
                        }

                        filter.Character->IsCharacterActive = input->IsCharActiive;
                        filter.Character->OnBoxIndex = input->BoxIndex;
                    }
                }
            }

            if (gameSession->State != GameState.Playing)
                return;

            if (filter.Character->IsCharacterActive)
            {
                filter.Character->State = CharacterState.Playing;
            }

            if (!filter.Character->IsCharacterActive)
            {
                filter.Character->IsMove = false;
                return;
            }


            if (filter.Character->Health < 0)
            {
                //f.Destroy(filter.Entity);
                return;
            }
 
            filter.Character->Pos = filter.Transform->Position;
            filter.Character->entityRef = filter.Entity;

            bool isEnemyExist = false;

            foreach(var character in f.GetComponentIterator<CharacterHandler>())
            {
                if (!character.Component.IsCharacterActive)
                    continue;
                if (character.Component.TeamNumber != filter.Character->TeamNumber && character.Component.Health > 0)
                {
                    isEnemyExist = true;
                
                    SetTarget(f, filter, character);
                }


            }
            
            if(!isEnemyExist)
            {
                filter.Character->IsMove = false;
                filter.Character->IsAttack = false;
                return;
            }

            if(filter.Character->Mana > 100)
            {
                //Skill(filter);
            }

            if (filter.Character->IsMove)
            {
                var nearestEnemy = f.Get<CharacterHandler>(filter.Character->TargetEnemy);

                filter.Character->AttackTimer = 0;
                filter.Transform->LookAt(nearestEnemy.Pos);
                filter.Transform->Position += (nearestEnemy.Pos - filter.Transform->Position).Normalized * f.DeltaTime * 1;

               /* var navmesh = f.Map.NavMeshes["Navmesh"];

                var agent = f.Get<NavMeshPathfinder>(filter.Entity);
                agent.SetTarget(f, nearestEnemy.Pos, navmesh);

                f.Set(filter.Entity, agent);*/

            }

            else
            {
                Attack(filter, f);
            }  
        }

        private void SetTarget(Frame f, Filter filter, EntityComponentPair<CharacterHandler> target)
        {
            if (filter.Character->TargetEnemy.ToString() == EntityRef.None.ToString())
                filter.Character->TargetEnemy = target.Entity;
                
            var e = f.Get<CharacterHandler>(filter.Character->TargetEnemy);


            if(e.Health < 0)
            {
                filter.Character->TargetEnemy = target.Entity;
            }

            var temp = f.Get<CharacterHandler>(filter.Character->TargetEnemy);

            if (FPVector3.Distance(filter.Character->Pos, target.Component.Pos) < FPVector3.Distance(temp.Pos, filter.Character->Pos))
            {
                filter.Character->TargetEnemy = target.Entity;

            }

            
            var temp2 = f.Get<CharacterHandler>(filter.Character->TargetEnemy);
                var dis = FPVector3.Distance(temp2.Pos, filter.Character->Pos);
                
                if(dis > filter.Character->AttackRange)
                    filter.Character->IsMove = true;

                else
                    filter.Character->IsMove = false;
        }

        private void Attack(Filter filter, Frame f)
        {
            filter.Character->IsAttack = false;
            filter.Character->AttackTimer += f.DeltaTime;

            if(filter.Character->AttackTimer > filter.Character->AttackCD)
            {
                filter.Character->IsAttack = true;
                foreach(var enemy in f.GetComponentIterator<CharacterHandler>())
                {
                    if(enemy.Entity == filter.Character->TargetEnemy)
                    {

                        filter.Transform->LookAt(enemy.Component.Pos);
                        //SetHealth(f, filter, enemy, filter.Character->IsUseSkill);
                    }
                }

                filter.Character->AttackTimer = 0;
               // filter.Character->Mana = filter.Character->Mana + 10;
            }
        }

        private void SetHealth(Frame f, Filter filter, EntityComponentPair<CharacterHandler> target, bool isUseSkill)
        {
            var hel = f.Unsafe.GetPointer<CharacterHandler>(target.Entity);
            var tempDamage = filter.Character->Damage;

            if(isUseSkill)
            {
                tempDamage *= 2;
                filter.Character->IsUseSkill = false;
            }

            hel->Health = hel->Health - tempDamage;
        }

        private void Skill(Filter filter)
        {
            filter.Character->IsUseSkill = true;

            filter.Character->Mana = 0;

        }



    }

}
