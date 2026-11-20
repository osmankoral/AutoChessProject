using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum
{
    public unsafe class GameSessionStateSystem : SystemMainThreadFilter<GameSessionStateSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public GameSession* GameSession;
        }

        public override void OnInit(Frame f)
        {
            Log.Debug("Quantum GameSessionStateSystem On Init");

        }

        public override void Update(Frame f, ref Filter filter)
        {
            GameSession* gameSession = f.Unsafe.GetPointerSingleton<GameSession>();

            if (gameSession == null)
                return;
            if(gameSession->State == GameState.Countdown)
                gameSession->TimeUntilStat = gameSession->TimeUntilStat - f.DeltaTime;
            
            else if(gameSession->State == GameState.Preparation)
                gameSession->PrepartionCooldown = gameSession->PrepartionCooldown - f.DeltaTime;

            else if (gameSession->State == GameState.Playing)
                gameSession->GameTimeCooldown = gameSession->GameTimeCooldown - f.DeltaTime;

            if (gameSession->TimeUntilStat < 0 && gameSession->State == GameState.Countdown)
            {
                Log.Debug("Quantum GameSessionStateSystem Countdown completed");
                StateChangePrepartion(gameSession);
            }
            
            if (gameSession->PrepartionCooldown < 0 && gameSession->State == GameState.Preparation)
            {
                Log.Debug("Quantum GameSessionStateSystem Preparation completed");
                gameSession->EndRound = false;
                StateChangePlay(gameSession);
            }

            if (gameSession->GameTimeCooldown < 0 && gameSession->State == GameState.Playing)
            {
                Log.Debug("Quantum GameSessionStateSystem Playing completed");

                
                DealDamages();
                StateChangePrepartion(gameSession);     

                foreach(var character in f.GetComponentIterator<playerLink>())
                {
                    GoldUpdate(f, character.Entity);
                    //CharSetup(f, character.Entity);
                }
               
            }
        }

        private void StateChangePrepartion(GameSession* gameSession)
        {
            gameSession->State = GameState.Preparation;
            gameSession->PrepartionCooldown = 15;
            gameSession->RoundCount = gameSession->RoundCount + 1;
        }

        private void StateChangePlay(GameSession* gameSession)
        {
            gameSession->State = GameState.Playing;
            gameSession->GameTimeCooldown = 45;
        }

        private void DealDamages()
        {

        }

        private void GoldUpdate(Frame f, EntityRef entityRef)
        {
            var script = f.Get<playerLink>(entityRef);

            script.Gold = script.Gold + 4;
            f.Set(entityRef, script);
            QUntils.Exp(f, entityRef, 2);
            
        }

        /*private void CharSetup(Frame f, EntityRef entity)
        {
            var script = f.Get<playerLink>(entity);
            
            for(int i = 0; i<9; i++)
            {
                var chScript = f.Get<CharacterHandler>(script.ActiveCharacters[i]);
                chScript.IsReset = true;

                f.Set(script.ActiveCharacters[i], chScript);

            }

            f.Set(entity, script);
            
        }*/
    }
}
