namespace UnityGAS
{
    public enum ModifierType
    {
        Flat,
        Percent
    }

    [System.Serializable]
    public struct ModifierOverride
    {
        public AttributeDefinition attribute;
        public ModifierType type;
        public float value;

        public ModifierOverride(AttributeDefinition attribute, ModifierType type, float value)
        {
            this.attribute = attribute;
            this.type = type;
            this.value = value;
        }
    }
}