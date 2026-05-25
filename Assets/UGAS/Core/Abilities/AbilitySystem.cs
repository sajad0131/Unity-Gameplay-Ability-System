using System.Collections.Generic;
using System.Linq;
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

        [Header("Level")]
        public int level = 1;

        private bool isCasting;
        private AbilityDefinition currentCast;
        private float castTimeRemaining;
        private GameObject currentTarget;
        private Vector3 lastCastPosition;
        private Dictionary<AbilityDefinition, float> cooldowns = new Dictionary<AbilityDefinition, float>();

        private Queue<QueuedAbility> abilityQueue = new Queue<QueuedAbility>();

        public System.Action<AbilityDefinition> OnAbilityCastStart;
        public System.Action<AbilityDefinition> OnAbilityCastCompleted;
        public System.Action<AbilityDefinition> OnAbilityCastCancelled;

        public bool IsCasting => isCasting;
        public AbilityDefinition CurrentCast => currentCast;
        public float CastProgress => currentCast != null && currentCast.castTime > 0 ? 1f - (castTimeRemaining / currentCast.castTime) : 0f;
        public int QueuedCount => abilityQueue.Count;

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
            HandleMovementCancel();
        }

        public bool TryActivateAbility(AbilityDefinition ability, GameObject target = null)
        {
            if (ability == null || isCasting || IsOnCooldown(ability) || !ability.CanActivate(gameObject, target))
            {
                abilityQueue.Enqueue(new QueuedAbility(ability, target));
                return true;
            }

            if (IsOnCooldown(ability) || !ability.CanActivate(gameObject, target))
            {
                return false;
            }

            if (!runtimeAbilities.Contains(ability))
            {
                Debug.LogWarning($"Ability {ability.abilityName} has not been granted to this character.");
                return false;
            }

            StartCasting(ability, target);
            return true;
        }

        public bool TryActivateAbilityWithLevel(AbilityDefinition ability, int abilityLevel, GameObject target = null)
        {
            int prevLevel = level;
            level = Mathf.Clamp(abilityLevel, 1, ability.maxLevel);
            bool result = TryActivateAbility(ability, target);
            level = prevLevel;
            return result;
        }

        public void GrantAbility(AbilityDefinition ability)
        {
            if (!runtimeAbilities.Contains(ability))
            {
                runtimeAbilities.Add(ability);
                cooldowns[ability] = 0f;
            }
        }

        public void RemoveAbility(AbilityDefinition ability)
        {
            if (runtimeAbilities.Remove(ability))
            {
                cooldowns.Remove(ability);
            }
        }

        public void CancelCasting()
        {
            if (!isCasting) return;
            if (currentCast != null && !currentCast.interruptible) return;

            var cancelledAbility = currentCast;
            isCasting = false;
            currentCast = null;
            currentTarget = null;
            OnAbilityCastCancelled?.Invoke(cancelledAbility);
        }

        public void CancelCastingForce()
        {
            if (!isCasting) return;

            var cancelledAbility = currentCast;
            isCasting = false;
            currentCast = null;
            currentTarget = null;
            OnAbilityCastCancelled?.Invoke(cancelledAbility);
        }

        public void ClearAbilityQueue()
        {
            abilityQueue.Clear();
        }

        public bool IsOnCooldown(AbilityDefinition ability)
        {
            return cooldowns.ContainsKey(ability) && cooldowns[ability] > 0f;
        }

        public float GetCooldownRemaining(AbilityDefinition ability)
        {
            return IsOnCooldown(ability) ? cooldowns[ability] : 0f;
        }

        public AbilityDefinition[] GetGrantedAbilities()
        {
            return runtimeAbilities.ToArray();
        }

        public void RemoveEffectsWithTag(GameplayTag tag, GameObject target)
        {
            if (effectRunner != null)
            {
                effectRunner.RemoveEffectsWithTag(tag, target);
            }
        }

        public bool HasActiveEffect(GameplayEffect effect)
        {
            return effectRunner != null && effectRunner.HasActiveEffect(effect);
        }

        private void StartCasting(AbilityDefinition ability, GameObject target)
        {
            isCasting = true;
            currentCast = ability;
            currentTarget = target;
            castTimeRemaining = ability.castTime;
            lastCastPosition = transform.position;

            PlayAbilityVisuals(ability);

            OnAbilityCastStart?.Invoke(ability);

            if (ability.IsInstant)
            {
                CompleteCast();
            }
        }

        private void HandleMovementCancel()
        {
            if (!isCasting || currentCast == null) return;
            if (currentCast.canCastWhileMoving) return;

            if (Vector3.Distance(transform.position, lastCastPosition) > 0.01f)
            {
                CancelCasting();
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
            PlayImpactVisuals(ability, target);
            StartCooldown(ability);

            isCasting = false;
            currentCast = null;
            currentTarget = null;
            OnAbilityCastCompleted?.Invoke(ability);

            TryActivateQueued();
        }

        private void TryActivateQueued()
        {
            if (abilityQueue.Count == 0) return;

            var queued = abilityQueue.Peek();
            if (queued.ability == null)
            {
                abilityQueue.Dequeue();
                TryActivateQueued();
                return;
            }

            if (!IsOnCooldown(queued.ability) && queued.ability.CanActivate(gameObject, queued.target))
            {
                abilityQueue.Dequeue();
                StartCasting(queued.ability, queued.target);
            }
        }

        private void PlayAbilityVisuals(AbilityDefinition ability)
        {
            if (!string.IsNullOrEmpty(ability.animationTrigger))
            {
                var animator = GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger(ability.animationTriggerHash);
                }
            }

            if (ability.castVFX != null)
            {
                var vfx = Instantiate(ability.castVFX, transform.position, transform.rotation);
                Destroy(vfx, 5f);
            }

            if (ability.castSFX != null)
            {
                var audioSource = GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(ability.castSFX);
                }
            }
        }

        private void PlayImpactVisuals(AbilityDefinition ability, GameObject target)
        {
            if (target == null) return;

            if (ability.impactVFX != null)
            {
                var vfx = Instantiate(ability.impactVFX, target.transform.position, target.transform.rotation);
                Destroy(vfx, 5f);
            }

            if (ability.impactSFX != null)
            {
                var audioSource = target.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(ability.impactSFX);
                }
            }
        }

        private void ExecuteAbility(AbilityDefinition ability, GameObject target)
        {
            var targets = GetTargets(ability, target);
            foreach (var t in targets)
            {
                foreach (var effect in ability.effects)
                {
                    if (effectRunner.CanApplyEffect(effect, t))
                    {
                        effectRunner.ApplyEffect(effect, t, gameObject);
                    }
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
                    else
                    {
                        var colliders = Physics.OverlapSphere(center, ability.radius, ability.targetableLayers);
                        foreach (var col in colliders)
                        {
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

    public class QueuedAbility
    {
        public AbilityDefinition ability;
        public GameObject target;

        public QueuedAbility(AbilityDefinition ability, GameObject target)
        {
            this.ability = ability;
            this.target = target;
        }
    }
}