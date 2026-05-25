using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityGAS
{
    public class GameplayEffectRunner : MonoBehaviour
    {
        private readonly List<ActiveGameplayEffect> activeEffects = new List<ActiveGameplayEffect>();
        private TagSystem tagSystem;

        public System.Action<GameplayEffect, GameObject, GameObject> OnEffectApplied;
        public System.Action<GameplayEffect, GameObject, GameObject> OnEffectRemoved;

        private void Awake()
        {
            tagSystem = GetComponent<TagSystem>();
        }

        private void Update()
        {
            if (activeEffects.Count == 0) return;

            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                var activeEffect = activeEffects[i];

                if (!activeEffect.Effect.IsInfinite)
                {
                    activeEffect.TimeRemaining -= Time.deltaTime;
                }

                if (activeEffect.Effect.isPeriodic && activeEffect.Effect.period > 0f)
                {
                    activeEffect.TimeSinceLastPeriod += Time.deltaTime;
                    if (activeEffect.TimeSinceLastPeriod >= activeEffect.Effect.period)
                    {
                        activeEffect.TimeSinceLastPeriod -= activeEffect.Effect.period;
                        activeEffect.Effect.Apply(activeEffect.Target, activeEffect.Instigator, activeEffect.StackCount);
                    }
                }

                if (!activeEffect.Effect.IsInfinite && activeEffect.TimeRemaining <= 0)
                {
                    EndEffect(activeEffect);
                    activeEffects.RemoveAt(i);
                }
            }
        }

        public bool CanApplyEffect(GameplayEffect effect, GameObject target)
        {
            var targetTags = target.GetComponent<TagSystem>();
            if (targetTags == null) return true;

            if (effect.applicationBlockedByTags.Count > 0 && targetTags.HasAnyTag(effect.applicationBlockedByTags))
                return false;

            if (effect.applicationRequiredTags.Count > 0 && !targetTags.HasAllTags(effect.applicationRequiredTags))
                return false;

            return true;
        }

        public void ApplyEffect(GameplayEffect effect, GameObject target, GameObject instigator)
        {
            if (effect == null || target == null) return;

            var existingEffect = activeEffects.FirstOrDefault(e => e.Effect == effect && e.Target == target);

            if (existingEffect != null)
            {
                if (effect.canStack && existingEffect.StackCount < effect.maxStacks)
                {
                    existingEffect.StackCount++;
                }
                existingEffect.TimeRemaining = effect.duration;
                existingEffect.TimeSinceLastPeriod = 0f;
                effect.Apply(target, instigator, existingEffect.StackCount);

                var targetTags = target.GetComponent<TagSystem>();
                if (targetTags != null)
                {
                    targetTags.AddTags(effect.grantedTags);
                    targetTags.AddTags(effect.ongoingTags);
                }
            }
            else
            {
                if (effect.IsInstant)
                {
                    effect.Apply(target, instigator);
                }
                else
                {
                    var newActiveEffect = new ActiveGameplayEffect(effect, target, instigator);
                    activeEffects.Add(newActiveEffect);
                    effect.Apply(target, instigator);

                    var targetTags = target.GetComponent<TagSystem>();
                    if (targetTags != null)
                    {
                        targetTags.AddTags(effect.grantedTags);
                        targetTags.AddTags(effect.ongoingTags);
                    }
                }
            }

            var effectTargetTags = target.GetComponent<TagSystem>();
            if (effectTargetTags != null && effect.removeOnApplicationTags.Count > 0)
            {
                effectTargetTags.RemoveTags(effect.removeOnApplicationTags);
            }

            ExecuteCues(effect, target, instigator);
            OnEffectApplied?.Invoke(effect, target, instigator);
        }

        private void ExecuteCues(GameplayEffect effect, GameObject target, GameObject instigator)
        {
            foreach (var cue in effect.cues)
            {
                if (cue != null)
                {
                    cue.Execute(target, instigator, target != null ? target.transform.position : Vector3.zero);
                }
            }
        }

        public void RemoveEffect(GameplayEffect effect, GameObject target)
        {
            var activeEffect = activeEffects.FirstOrDefault(e => e.Effect == effect && e.Target == target);
            if (activeEffect != null)
            {
                EndEffect(activeEffect);
                activeEffects.Remove(activeEffect);
            }
        }

        public void RemoveEffectsWithTag(GameplayTag tag, GameObject target)
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                var activeEffect = activeEffects[i];
                if (activeEffect.Target == target && (activeEffect.Effect.ongoingTags.Contains(tag) || activeEffect.Effect.grantedTags.Contains(tag)))
                {
                    EndEffect(activeEffect);
                    activeEffects.RemoveAt(i);
                }
            }
        }

        public bool HasActiveEffect(GameplayEffect effect)
        {
            return activeEffects.Any(e => e.Effect == effect);
        }

        public bool HasActiveEffectOnTarget(GameplayEffect effect, GameObject target)
        {
            return activeEffects.Any(e => e.Effect == effect && e.Target == target);
        }

        public ActiveGameplayEffect[] GetActiveEffects()
        {
            return activeEffects.ToArray();
        }

        public void RemoveAllEffectsOnTarget(GameObject target)
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i].Target == target)
                {
                    EndEffect(activeEffects[i]);
                    activeEffects.RemoveAt(i);
                }
            }
        }

        private void EndEffect(ActiveGameplayEffect activeEffect)
        {
            activeEffect.Effect.Remove(activeEffect.Target, activeEffect.Instigator);

            var targetTags = activeEffect.Target.GetComponent<TagSystem>();
            if (targetTags != null)
            {
                targetTags.RemoveTags(activeEffect.Effect.grantedTags);
                targetTags.RemoveTags(activeEffect.Effect.ongoingTags);
                targetTags.RemoveTags(activeEffect.Effect.removeOnRemoveTags);
            }

            OnEffectRemoved?.Invoke(activeEffect.Effect, activeEffect.Target, activeEffect.Instigator);
        }
    }

    public class ActiveGameplayEffect
    {
        public GameplayEffect Effect { get; }
        public GameObject Target { get; }
        public GameObject Instigator { get; }
        public float TimeRemaining { get; set; }
        public int StackCount { get; set; }
        public float TimeSinceLastPeriod { get; set; }

        public ActiveGameplayEffect(GameplayEffect effect, GameObject target, GameObject instigator)
        {
            Effect = effect;
            Target = target;
            Instigator = instigator;
            TimeRemaining = effect.duration;
            StackCount = 1;
            TimeSinceLastPeriod = 0f;
        }
    }
}