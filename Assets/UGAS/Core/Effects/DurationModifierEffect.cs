using UnityEngine;

namespace UnityGAS
{
    [CreateAssetMenu(fileName = "NewDurationModifierEffect", menuName = "GAS/Effects/Duration Modifier")]
    public class DurationModifierEffect : GameplayEffect
    {
        public override void Apply(GameObject target, GameObject instigator, int stackCount = 1)
        {
            var attributeSet = target.GetComponent<AttributeSet>();
            if (attributeSet == null) return;

            foreach (var mod in modifiers)
            {
                if (mod.attribute == null) continue;
                attributeSet.GetAttribute(mod.attribute)?.RemoveModifiersFromSource(this);

                var modifier = new AttributeModifier(mod.type, mod.value * stackCount, this, duration);
                attributeSet.AddModifier(mod.attribute, modifier);
            }
        }

        public override void Remove(GameObject target, GameObject instigator)
        {
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