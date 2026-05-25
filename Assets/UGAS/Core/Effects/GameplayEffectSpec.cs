using System.Collections.Generic;
using UnityEngine;

namespace UnityGAS
{
    public class GameplayEffectSpec
    {
        public GameplayEffect EffectDef { get; }
        public GameObject Instigator { get; }
        public int Level { get; set; }
        public int StackCount { get; set; }

        public GameplayEffectSpec(GameplayEffect effectDef, GameObject instigator, int level = 1, int stackCount = 1)
        {
            EffectDef = effectDef ?? throw new System.ArgumentNullException(nameof(effectDef));
            Instigator = instigator;
            Level = level;
            StackCount = stackCount;
        }

        public void Apply(GameObject target)
        {
            EffectDef.Apply(target, Instigator, StackCount);
        }

        public void Remove(GameObject target)
        {
            EffectDef.Remove(target, Instigator);
        }
    }
}