using System.Collections.Generic;
using UnityEngine;

namespace UnityGAS
{
    public class AbilitySystem : MonoBehaviour
    {
        [SerializeField] private List<AbilityDefinition> initialAbilities = new List<AbilityDefinition>();
        [SerializeField] private List<AbilityDefinition> runtimeAbilities;

        [Header("Components")]
        [SerializeField] private AttributeSet attributeSet;
        [SerializeField] private GameplayEffectRunner effectRunner;
        [SerializeField] private TagSystem tagSystem;

        private bool isCasting;
        private AbilityDefinition currentCast;
        private float castTimeRemaining;
        private GameObject currentTarget;
        private Dictionary<AbilityDefinition, float> cooldowns = new Dictionary<AbilityDefinition, float>();

        public System.Action<AbilityDefinition> OnAbilityCastStart;
        public System.Action<AbilityDefinition> OnAbilityCastCompleted;
        public System.Action<AbilityDefinition> OnAbilityCastCancelled;

        public bool IsCasting => isCasting;
        public AbilityDefinition CurrentCast => currentCast;
        public float CastProgress => currentCast != null && currentCast.castTime > 0 ? 1f - (castTimeRemaining / currentCast.castTime) : 0f;


        private void Awake()
        {
            if (attributeSet == null) attributeSet = GetComponent<AttributeSet>();
            if (effectRunner == null) effectRunner = GetComponent<GameplayEffectRunner>();
            if (tagSystem == null) tagSystem = GetComponent<TagSystem>();

            runtimeAbilities = new List<AbilityDefinition>(initialAbilities);
            foreach (var ability in runtimeAbilities)
            {
                cooldowns[ability] = 0f;
            }
        }

        private void Update()
        {
            HandleCooldowns();
            HandleCasting();
        }

        public bool TryActivateAbility(AbilityDefinition ability, GameObject target = null)
        {
            if (ability == null || isCasting || IsOnCooldown(ability) || !ability.CanActivate(gameObject, target))
            {
                return false;
            }

            StartCasting(ability, target);
            return true;
        }

        public void CancelCasting()
        {
            if (!isCasting) return;

            var cancelledAbility = currentCast;
            isCasting = false;
            currentCast = null;
            OnAbilityCastCancelled?.Invoke(cancelledAbility);
        }

        public bool IsOnCooldown(AbilityDefinition ability)
        {
            return cooldowns.ContainsKey(ability) && cooldowns[ability] > 0f;
        }

        public float GetCooldownRemaining(AbilityDefinition ability)
        {
            return IsOnCooldown(ability) ? cooldowns[ability] : 0f;
        }

        private void StartCasting(AbilityDefinition ability, GameObject target)
        {
            isCasting = true;
            currentCast = ability;
            currentTarget = target;
            castTimeRemaining = ability.castTime;

            OnAbilityCastStart?.Invoke(ability);

            if (ability.IsInstant)
            {
                CompleteCast();
            }
        }

        private void HandleCasting()
        {
            if (!isCasting) return;

            castTimeRemaining -= Time.deltaTime;
            if (castTimeRemaining <= 0)
            {
                CompleteCast();
            }
        }

        private void CompleteCast()
        {
            if (!isCasting) return;

            var ability = currentCast;
            var target = currentTarget;

            ability.ApplyCost(gameObject);
            ExecuteAbility(ability, target);
            StartCooldown(ability);

            isCasting = false;
            currentCast = null;
            currentTarget = null;
            OnAbilityCastCompleted?.Invoke(ability);
        }

        private void ExecuteAbility(AbilityDefinition ability, GameObject target)
        {
            var targets = GetTargets(ability, target);
            foreach (var t in targets)
            {
                foreach (var effect in ability.effects)
                {
                    effectRunner.ApplyEffect(effect, t, gameObject);
                }
            }
        }

        private List<GameObject> GetTargets(AbilityDefinition ability, GameObject target)
        {
            var targets = new List<GameObject>();
            switch (ability.targetingType)
            {
                case TargetingType.Self:
                    targets.Add(gameObject);
                    break;
                case TargetingType.Target:
                    // The initial CanActivate check is sufficient for a single target.
                    if (target != null) targets.Add(target);
                    break;
                case TargetingType.Area:
                case TargetingType.Ground:
                    var center = target != null ? target.transform.position : transform.position;
                    if (ability.dimension == Dimension.d2D)
                    {
                        var colliders = Physics2D.OverlapCircleAll(center, ability.radius, ability.targetableLayers);
                        foreach (var col in colliders)
                        {
                            if (ability.IsValidTarget(gameObject, col.gameObject))
                            {
                                targets.Add(col.gameObject);
                            }
                        }
                    }
                    else // Original 3D logic
                    {
                        var colliders = Physics.OverlapSphere(center, ability.radius, ability.targetableLayers);
                        foreach (var col in colliders)
                        {
                            // --- THIS IS THE CRUCIAL FIX ---
                            // We must validate that EACH target in the sphere is a valid target.
                            // This will check for required tags on every single one.
                            if (ability.IsValidTarget(gameObject, col.gameObject))
                            {
                                targets.Add(col.gameObject);
                            }
                        }
                    }
                    break;
            }
            return targets;
        }


        private void HandleCooldowns()
        {
            if (cooldowns.Count == 0) return;

            var keys = new List<AbilityDefinition>(cooldowns.Keys);
            foreach (var ability in keys)
            {
                if (cooldowns[ability] > 0)
                {
                    cooldowns[ability] -= Time.deltaTime;
                }
            }
        }

        private void StartCooldown(AbilityDefinition ability)
        {
            if (ability.cooldown > 0)
            {
                cooldowns[ability] = ability.cooldown;
            }
        }
    }
}