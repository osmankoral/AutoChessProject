using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Quantum
{
    public static class QUntils
    {
        public static String PrefabGUID(int index)
        {
            string[] guilds =
            {
                "Resources/DB/Characters/ElfWarrior/ElfWarriorPrefab|EntityPrototype",
                "Resources/DB/Characters/ElfArcher/ElfArcherPrefab|EntityPrototype",
                "Resources/DB/Characters/ElfAssasin/ElfAssasinPrefab|EntityPrototype",
                "Resources/DB/Characters/ElfKnight/ElfKnightPrefab|EntityPrototype",
                "Resources/DB/Characters/ElfKing/ElfKingPrefab|EntityPrototype",
            };
            return guilds[index];
        }

        public static void Exp(Frame f, EntityRef entityRef, int exp)
        {
            var script = f.Get<playerLink>(entityRef);

            script.Exp = script.Exp + exp;

            if (script.Exp >= script.MaxExp)
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

            f.Set(entityRef, script);

            if(script.Exp >= script.MaxExp)
                Exp(f, entityRef, 0);
        }

        public static void GoldandExp(Frame f, EntityRef entityRef, int exp, int gold)
        {
            Exp(f, entityRef, exp);

            var script = f.Get<playerLink>(entityRef);

            script.Gold = script.Gold - gold;

            f.Set(entityRef, script);
        }

        public static void UnitUpdate(Frame f,  EntityRef playerRef)
        {
            var playerScript = f.Get<playerLink>(playerRef);

            List<EntityRef> unitList = new List<EntityRef>();
            
            for(int i = 0; i< playerScript.CharacterCount; i++)
            {
                unitList.Add(playerScript.UnitList[i]);
            }

            foreach( var currentUnit in unitList )
            {

                List<EntityRef> units = new List<EntityRef>();

                units.Add(currentUnit);

                var currentUnitID = f.Get<CharacterId>(currentUnit);

                foreach(var tempUnit in unitList)
                {
                    if (currentUnit.Equals(tempUnit))
                        continue;
                    

                    var tempUnitID = f.Get<CharacterId>(tempUnit);

                    if(currentUnitID.UnitID == tempUnitID.UnitID && currentUnitID.StarLevel == tempUnitID.StarLevel)
                    {
                        units.Add(tempUnit);
                    }

                }

                if(units.Count >= 3)
                {
                    List<EntityRef> destroyingUnitList = new List<EntityRef>();
                    List<EntityRef> undestroyingUnitList = new List<EntityRef>();


                    foreach(var unit in units)
                    {
                        var unitScript = f.Get<CharacterHandler>(unit);
                        
                        if(unitScript.IsCharacterActive)
                            undestroyingUnitList.Add(unit);
                        
                        if(!unitScript.IsCharacterActive)
                            destroyingUnitList.Add(unit);
                    }

                    if(undestroyingUnitList.Count > 0)
                    {

                    }

                    else if(undestroyingUnitList.Count == 0 && destroyingUnitList.Count > 2)
                    {
                        var unitScript = f.Get<CharacterId>(destroyingUnitList[0]);

                        unitScript.StarLevel = unitScript.StarLevel + 1;
                        f.Set(destroyingUnitList[0], unitScript);

                        for (int j = 0; j < playerScript.CharacterCount; j++)
                        {
                            if (playerScript.UnitList[j].Equals(destroyingUnitList[1]) || playerScript.UnitList[j].Equals(destroyingUnitList[2]))
                            {
                                var script = f.Get<CharacterHandler>(playerScript.UnitList[j]);
                                playerScript.Bachs[script.OnBoxIndex] = false;
                                f.Destroy(playerScript.UnitList[j]);


                                for(int z = j; z < 17; z++)
                                {
                                    playerScript.UnitList[z] = playerScript.UnitList[z + 1];
                                }
                                playerScript.UnitList[17] = EntityRef.None;
                                j = j - 1;
                            }
                        }

                        playerScript.CharacterCount = playerScript.CharacterCount - 2;
                    }
                    
                    f.Set(playerRef, playerScript);
                }

                /*for(int k = 0; k<9; k++)
                {
                    if (playerScript.ActiveCharacters[k].ToString() == EntityRef.None.ToString())
                    {
                        continue;
                    }

                    var tempUnit = f.Get<CharacterId>(playerScript.ActiveCharacters[k]);

                    if (baseUnit.UnitID == tempUnit.UnitID && baseUnit.StarLevel == tempUnit.StarLevel)
                    {
                        units.Add(playerScript.ActiveCharacters[k]);
                    }
                }

                if (units.Count >= 3)
                {
                    
                    var gameSession = f.GetSingleton<GameSession>();

                    if(gameSession.State == GameState.Playing)
                    {
                        playerScript.IsReView = true;
                        f.Set(playerRef, playerScript);

                        break;
                    }

                    for(int w = 0; w<units.Count; w++)
                    {
                        var tempActiveUnit = f.Get<CharacterHandler>(units[w]);
                        if(tempActiveUnit.IsCharacterActive)
                        {
                            var a = units[0];
                            var b = units[1];

                            units[0] = units[w];
                            units[1] = a;
                            units[2] = b;
                            
                        }
                    }


                    var baseUnit2 = f.Get<CharacterId>(units[0]);
                    baseUnit2.StarLevel = baseUnit.StarLevel + 1;
                    f.Set(units[0], baseUnit2);

                    for (int q = 0; q < 9; q++)
                    {
                        if (playerScript.BachCharacters[q].ToString().Equals(units[1].ToString()) || playerScript.BachCharacters[q].ToString().Equals(units[2].ToString()))
                        {
                            f.Destroy(playerScript.BachCharacters[q]);
                            playerScript.BachCharacters[q] = EntityRef.None;
                        }
                    }
                    f.Set(playerRef, playerScript);
                }*/

                
            }
        }
    } 
}
