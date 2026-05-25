using UnityEngine;
using System.Collections.Generic;

namespace UnityGAS
{
    // Define the new enums at the top of the namespace
    public enum TargetingType
    {
        Self,
        Target,
        Area,
        Ground
    }

    public enum Dimension
    {
        d2D,
        d3D
    }

    [CreateAssetMenu(fileName = "NewAbility", menuName = "GAS/Ability Definition")]
    public class AbilityDefinition : ScriptableObject
    {
        [Header("Info")]
        public string abilityName = "New Ability";
        public Sprite icon;
        [TextArea] public string description = "Ability description.";

        [Header("Activation")]
        public float cooldown = 0f;
        public float castTime = 0f;
        public bool canCastWhileMoving = true;
        public bool interruptible = true;
        public int maxLevel = 1;

        [Header("Cost")]
        public float cost = 0f;
        public AttributeDefinition costAttribute;

        [Header("Targeting")]
        public TargetingType targetingType = TargetingType.Self;
        public Dimension dimension = Dimension.d2D;
        public bool useRangeCheck = true; // << NEW: Added range check toggle
        public float range = 5f;
        public float radius = 3f;
        public LayerMask targetableLayers = ~0;
        public LayerMask occlusionLayers = 1; // Default layer

        [Header("Effects")]
        public List<GameplayEffect> effects = new List<GameplayEffect>();

        [Header("Visuals & Audio")]
        public string animationTrigger;
        [HideInInspector] public int animationTriggerHash;
        public GameObject castVFX;
        public GameObject impactVFX;
        public AudioClip castSFX;
        public AudioClip impactSFX;

        [Header("Tags")]
        public List<GameplayTag> abilityTags = new List<GameplayTag>();
        public List<GameplayTag> requiredTags = new List<GameplayTag>();
        public List<GameplayTag> blockedByTags = new List<GameplayTag>();


        [Header("Target Tags")]
        public List<GameplayTag> targetRequiredTags = new List<GameplayTag>();
        public List<GameplayTag> targetBlockedByTags = new List<GameplayTag>();



        public bool IsInstant => castTime <= 0f;
        public bool HasCost => cost > 0f && costAttribute != null;

        private void OnValidate()
        {
            if (!string.IsNullOrEmpty(animationTrigger))
            {
                animationTriggerHash = Animator.StringToHash(animationTrigger);
            }
        }

        public bool CanActivate(GameObject caster, GameObject target)
        {
            var attributeSet = caster.GetComponent<AttributeSet>();
            if (attributeSet == null) return false;

            if (HasCost && attributeSet.GetAttributeValue(costAttribute) < cost)
            {
                return false;
            }

            if (!IsValidTarget(caster, target))
            {
                return false;
            }

            var tags = caster.GetComponent<TagSystem>();
            if (tags != null)
            {
                if (tags.HasAnyTag(blockedByTags)) return false;
                if (!tags.HasAllTags(requiredTags)) return false;
            }

            

            return true;
        }

        public bool IsValidTarget(GameObject caster, GameObject target)
        {
            var targetTags = target?.GetComponent<TagSystem>();

            switch (targetingType)
            {
                case TargetingType.Self:
                    return true;

                case TargetingType.Target:
                    if (target == null) return false;
                    if (((1 << target.layer) & targetableLayers) == 0) return false;

                    if (targetTags != null)
                    {
                        if (targetTags.HasAnyTag(targetBlockedByTags)) return false;
                        if (targetRequiredTags.Count > 0 && !targetTags.HasAllTags(targetRequiredTags)) return false;
                    }

                    if (useRangeCheck)
                    {
                        if (Vector3.Distance(caster.transform.position, target.transform.position) > range) return false;
                        if (dimension == Dimension.d3D && Physics.Linecast(caster.transform.position, target.transform.position, occlusionLayers)) return false;
                        // Note: 2D occlusion check (Physics2D.Linecast) might be needed here for 2D projects
                    }
                    return true;

                case TargetingType.Area:
                case TargetingType.Ground:
                    if (target == null) return false;
                    if (targetTags != null)
                    {
                        if (targetTags.HasAnyTag(targetBlockedByTags)) return false;
                        if (targetRequiredTags.Count > 0 && !targetTags.HasAllTags(targetRequiredTags)) return false;
                    }
                    return true;

                default:
                    return false;
            }
        }

        public void ApplyCost(GameObject caster)
        {
            if (!HasCost) return;
            var attributeSet = caster.GetComponent<AttributeSet>();
            if (attributeSet != null)
            {
                attributeSet.ModifyAttributeValue(costAttribute, -cost, this);
            }
        }
    }
}