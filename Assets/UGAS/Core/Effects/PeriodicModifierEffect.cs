using UnityEngine;

namespace UnityGAS
{
    [CreateAssetMenu(fileName = "NewPeriodicModifierEffect", menuName = "GAS/Effects/Periodic Modifier")]
    public class PeriodicModifierEffect : GameplayEffect
    {
        [Header("Periodic Config")]
        public bool revertOnRemove = false;

        public override void Apply(GameObject target, GameObject instigator, int stackCount = 1)
        {
            var attributeSet = target.GetComponent<AttributeSet>();
            if (attributeSet == null) return;

            if (revertOnRemove)
            {
                foreach (var mod in modifiers)
                {
                    if (mod.attribute == null) continue;
                    attributeSet.GetAttribute(mod.attribute)?.RemoveModifiersFromSource(this);

                    var modifier = new AttributeModifier(mod.type, mod.value * stackCount, this, duration);
                    attributeSet.AddModifier(mod.attribute, modifier);
                }
            }
            else
            {
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
        }

        public override void Remove(GameObject target, GameObject instigator)
        {
            if (!revertOnRemove) return;

            var attributeSet = target.GetComponent<AttributeSet>();
            if (attributeSet == null) return;

            foreach (var mod in modifiers)
            {
                if (mod.attribute == null) continue;
                attributeSet.GetAttribute(mod.attribute)?.RemoveModifiersFromSource(this);
            }
        }
    }
}