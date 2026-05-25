using UnityEngine;
using System.Collections.Generic;

namespace UnityGAS
{
    public abstract class GameplayEffect : ScriptableObject
    {
        [Header("Info")]
        public string effectName = "New Effect";
        [TextArea] public string description = "Effect description.";
        public Sprite icon;

        [Header("Duration")]
        public float duration = 0f;

        [Header("Periodic")]
        public bool isPeriodic = false;
        public float period = 1f;

        [Header("Stacking")]
        public bool canStack = false;
        public int maxStacks = 1;

        [Header("Modifiers")]
        public List<ModifierOverride> modifiers = new List<ModifierOverride>();

        [Header("Cues")]
        public List<GameplayCue> cues = new List<GameplayCue>();

        [Header("Tags")]
        public List<GameplayTag> grantedTags = new List<GameplayTag>();
        public List<GameplayTag> ongoingTags = new List<GameplayTag>();
        public List<GameplayTag> removeOnApplicationTags = new List<GameplayTag>();
        public List<GameplayTag> removeOnRemoveTags = new List<GameplayTag>();
        public List<GameplayTag> applicationRequiredTags = new List<GameplayTag>();
        public List<GameplayTag> applicationBlockedByTags = new List<GameplayTag>();

        public bool IsInfinite => duration < 0f;
        public bool IsInstant => duration <= 0f && !isPeriodic && !IsInfinite;
        public bool IsDuration => duration > 0f || isPeriodic || IsInfinite;

        public abstract void Apply(GameObject target, GameObject instigator, int stackCount = 1);
        public abstract void Remove(GameObject target, GameObject instigator);
    }
}