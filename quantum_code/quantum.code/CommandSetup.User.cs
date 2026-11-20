using System;
using System.Collections.Generic;
using Photon.Deterministic;

namespace Quantum {
  public static partial class DeterministicCommandSetup {
    static partial void AddCommandFactoriesUser(ICollection<IDeterministicCommandFactory> factories, RuntimeConfig gameConfig, SimulationConfig simulationConfig) {
            // user commands go here

            factories.Add(new PlayerExpUpdate());
            factories.Add(new TakeDamage());
            factories.Add(new ArrowSpawn());
            factories.Add(new UnitMovement());
            factories.Add(new AttackAnimationSet());
            factories.Add(new UseSkillAnimationSet());
    }
  }
}
