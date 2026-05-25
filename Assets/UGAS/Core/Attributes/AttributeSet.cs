using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityGAS
{
    public class AttributeSet : MonoBehaviour
    {
        [SerializeField] private List<AttributeDefinition> initialAttributes = new List<AttributeDefinition>();
        private readonly Dictionary<AttributeDefinition, AttributeValue> attributes = new Dictionary<AttributeDefinition, AttributeValue>();

        public delegate void AttributeChangedDelegate(AttributeDefinition attribute, float oldValue, float newValue);
        public delegate float AttributePreChangeDelegate(AttributeDefinition attribute, float currentValue, float incomingChange);
        public event AttributeChangedDelegate OnAttributeChanged;
        public event AttributePreChangeDelegate OnAttributePreChange;

        private void Awake()
        {
            foreach (var attributeDef in initialAttributes)
            {
                attributes[attributeDef] = new AttributeValue(attributeDef);
                attributes[attributeDef].OnValueChanged += (oldVal, newVal) => OnAttributeChanged?.Invoke(attributeDef, oldVal, newVal);
            }
        }

        private void Update()
        {
            if (attributes.Count == 0) return;
            var snapshot = attributes.Values.ToArray();
            foreach (var attributeValue in snapshot)
            {
                attributeValue.Update(Time.deltaTime);
            }
        }

        public AttributeValue GetAttribute(AttributeDefinition definition)
        {
            if (definition == null) return null;
            AttributeValue value;
            attributes.TryGetValue(definition, out value);
            return value;
        }

        public float GetAttributeValue(AttributeDefinition definition)
        {
            return GetAttribute(definition)?.CurrentValue ?? 0f;
        }

        public void SetAttributeBaseValue(AttributeDefinition definition, float value)
        {
            var attr = GetAttribute(definition);
            if (attr != null)
            {
                attr.BaseValue = value;
            }
        }

        public float GetAttributeBaseValue(AttributeDefinition definition)
        {
            return GetAttribute(definition)?.BaseValue ?? 0f;
        }

        public void ModifyAttributeValue(AttributeDefinition definition, float amount, Object source)
        {
            var attr = GetAttribute(definition);
            if (attr == null) return;

            if (OnAttributePreChange != null)
            {
                amount = OnAttributePreChange.Invoke(definition, attr.CurrentValue, amount);
            }

            attr.BaseValue += amount;
        }

        public void AddModifier(AttributeDefinition definition, AttributeModifier modifier)
        {
            var attr = GetAttribute(definition);
            if (attr != null)
            {
                attr.AddModifier(modifier);
            }
        }

        public void RemoveModifiersFromSource(Object source)
        {
            foreach (var attribute in attributes.Values)
            {
                attribute.RemoveModifiersFromSource(source);
            }
        }
    }
}