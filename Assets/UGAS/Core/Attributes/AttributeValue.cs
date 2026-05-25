using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityGAS
{
    [Serializable]
    public class AttributeValue
    {
        public AttributeDefinition Definition { get; }
        private float baseValue;
        public float BaseValue { get{ return baseValue; } set { baseValue = value; RecalculateValue(); } }
        public float CurrentValue { get; private set; }

        private readonly List<AttributeModifier> modifiers = new List<AttributeModifier>();
        private float lastDamageTime;

        public Action<float, float> OnValueChanged;

        public AttributeValue(AttributeDefinition definition)
        {
            Definition = definition;
            BaseValue = definition.defaultBaseValue;
            RecalculateValue();
        }

        public void AddModifier(AttributeModifier modifier)
        {
            modifiers.Add(modifier);
            RecalculateValue();
        }

        public void RemoveModifier(AttributeModifier modifier)
        {
            modifiers.Remove(modifier);
            RecalculateValue();
        }

        public void RemoveModifiersFromSource(UnityEngine.Object source)
        {
            modifiers.RemoveAll(mod => mod.Source == source);
            RecalculateValue();
        }


        public void Update(float deltaTime)
        {
            bool needsRecalculate = false;
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                modifiers[i].Update(deltaTime);
                if (!modifiers[i].IsPermanent && modifiers[i].TimeRemaining <= 0)
                {
                    modifiers.RemoveAt(i);
                    needsRecalculate = true;
                }
            }

            if (Definition.hasRegeneration && CurrentValue < Definition.maxValue)
            {
                if (Time.time - lastDamageTime >= Definition.regenerationDelay)
                {
                    BaseValue += Definition.regenerationRate * deltaTime;
                    needsRecalculate = true;
                }
            }

            if (needsRecalculate)
            {
                RecalculateValue();
            }
        }

        private void RecalculateValue()
        {
            float oldValue = CurrentValue;
            float finalValue = BaseValue;

            var flatModifiers = modifiers.Where(m => m.Type == ModifierType.Flat).Sum(m => m.Value);
            var percentModifiers = modifiers.Where(m => m.Type == ModifierType.Percent).Sum(m => m.Value);

            finalValue += flatModifiers;
            finalValue *= (1f + percentModifiers);

            CurrentValue = Mathf.Clamp(finalValue, Definition.minValue, Definition.maxValue);

            if (Math.Abs(oldValue - CurrentValue) > 0.001f)
            {
                OnValueChanged?.Invoke(oldValue, CurrentValue);
                if (CurrentValue < oldValue)
                {
                    lastDamageTime = Time.time;
                }
            }
        }
    }
}