using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Photon.Deterministic;

namespace Quantum
{
    public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>, ISignalOnPlayerDataSet, ISignalOnPlayerDisconnected
    {
        public struct Filter
        {
            public EntityRef Entity;
            //public CharacterController3D* CharacterController;
            public Transform3D* Transform;
            public playerLink* Link;
        }
        //bool _selectedChar;

        public override void Update(Frame f, ref Filter filter)
        {
            

            GameSession* gameSession = f.Unsafe.GetPointerSingleton<GameSession>();

            if (gameSession == null)
                return;

            

            if(gameSession->State == GameState.Preparation && !filter.Link->IsCharactersRes)
            {
                
            }

            var input = f.GetPlayerInput(filter.Link->Player);
            
            var inputVector = new FPVector2((FP)input->DirectionX/10, (FP)input->DirectionY/10);

            if (inputVector.SqrMagnitude > 1)
                inputVector = inputVector.Normalized;

            if (input->Crreat)
            {
                CreateCharacter(f, filter, input->PrefabIndex);
                input->Crreat = false;
            }

            filter.Link->RefPos = filter.Transform->Position;
        
        }

        public void OnPlayerDataSet(Frame f, PlayerRef player)
        {
            var data = f.GetPlayerData(player);

            var prototypeEntity = f.FindAsset<EntityPrototype>(data.CharacterPrototype.Id);

            var createEntity = f.Create(prototypeEntity);

            if(f.Unsafe.TryGetPointer<playerLink>(createEntity, out var playerLink))
            {
                playerLink->Player = player;
            }
            

        }


        public void OnPlayerDisconnected(Frame f, PlayerRef player)
        {
            foreach(var playerLink in f.GetComponentIterator<playerLink>())
            {
                if (playerLink.Component.Player != player)
                    continue;
                Log.Debug("Quantum : Player Disconnect");
                f.Destroy(playerLink.Entity);
            }
        }

        private void CreateCharacter(Frame f, Filter filter, int index)
        {
            bool isFull = false;
            int banchCount = 0;

            for(int i = 0; i<9; i++)
            {
                if(filter.Link->Bachs[i])
                {
                    if (i == 8)
                        isFull = true;

                    continue;
                }

                else
                {
                    banchCount = i;
                    filter.Link->Bachs[i] = true;
                    break;
                }
            }

            if (isFull)
            {
                Log.Error("banch is full");
                return;
            }

            var characterGUID = f.FindAsset<EntityPrototype>(QUntils.PrefabGUID(index));
            

            var character = f.Create(characterGUID);

            if (f.Unsafe.TryGetPointer<CharacterHandler>(character, out CharacterHandler* charScript))
            {

                charScript->Player = filter.Link->Player;

                EntityRef emptyEntity = EntityRef.None;



                filter.Link->CharacterCount++;

                charScript->CharacterID = ((filter.Link->Player + 1) * 100) + filter.Link->CharacterCount;
                charScript->Damage = filter.Link->Player + 1;
                charScript->PlayerEntityRef = filter.Entity;
                charScript->State = CharacterState.Standby;
                charScript->IsCharacterActive = false;
                charScript->OnBoxIndex = banchCount;

                if (filter.Link->Player == 0 || filter.Link->Player == 2 || filter.Link->Player == 4)
                {
                    charScript->TeamNumber = 1;
                }

                else if (filter.Link->Player == 1 || filter.Link->Player == 3 || filter.Link->Player == 5)
                {
                    charScript->TeamNumber = 2;
                }


                
                if (f.TryGet<Transform3D>(character, out Transform3D charTransform))
                {
                    var tempPos = FPVector3.Zero;

                    if(filter.Link->Player == 0 || filter.Link->Player == 2 || filter.Link->Player == 4)
                    {
                        tempPos = filter.Link->BackSet1;
                        tempPos.X += banchCount * 2;  
                    }
                    
                    else if(filter.Link->Player == 1 || filter.Link->Player == 3 || filter.Link->Player == 5)
                    {
                            
                        tempPos = filter.Link->BackSet2;
                        tempPos.X += banchCount * -2;
                        /*var tempRot = tempPos;
                        tempRot.Z -= 1;
                        charTransform.LookAt(tempRot);*/
                            
                    }

                    charTransform.Position = tempPos;
                    charTransform.Rotation = filter.Transform->Rotation;
                    charScript->GuardPos = tempPos; 
                    
                    f.Set(character, charTransform);
                }
                filter.Link->UnitList[filter.Link->CharacterCount - 1] = character;
            }

            QUntils.UnitUpdate(f, filter.Entity);
        }

    }
}
