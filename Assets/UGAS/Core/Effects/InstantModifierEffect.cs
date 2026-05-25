using UnityEngine;

namespace UnityGAS
{
    [CreateAssetMenu(fileName = "NewInstantModifierEffect", menuName = "GAS/Effects/Instant Modifier")]
    public class InstantModifierEffect : GameplayEffect
    {
        public override void Apply(GameObject target, GameObject instigator, int stackCount = 1)
        {
            var attributeSet = target.GetComponent<AttributeSet>();
            if (attributeSet == null) return;

            foreach (var mod in modifiers)
            {
                if (mod.attribute == null) continue;
                var attrValue = attributeSet.GetAttribute(mod.attribute);
                if (attrValue == null) continue;

                float modValue = mod.value * stackCount;
                if (mod.type == ModifierType.Flat)
                {
                    attrValue.BaseValue += modValue;
                }
                else if (mod.type == ModifierType.Percent)
                {
                    attrValue.BaseValue *= (1 + modValue);
                }
            }
        }

        public override void Remove(GameObject target, GameObject instigator)
        {
        }
    }
}