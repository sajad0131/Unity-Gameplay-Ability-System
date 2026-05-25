using System.Collections;
using UnityEngine;

namespace UnityGAS
{
    public abstract class AbilityTask
    {
        public bool IsRunning { get; protected set; }
        public System.Action OnTaskCompleted;
        public System.Action OnTaskFailed;

        protected MonoBehaviour owner;
        protected AbilityDefinition ability;

        public virtual void Start(MonoBehaviour owner, AbilityDefinition ability)
        {
            this.owner = owner;
            this.ability = ability;
            IsRunning = true;
        }

        public virtual void Cancel()
        {
            IsRunning = false;
        }

        protected void Complete()
        {
            IsRunning = false;
            OnTaskCompleted?.Invoke();
        }

        protected void Fail()
        {
            IsRunning = false;
            OnTaskFailed?.Invoke();
        }
    }

    public class WaitDelayTask : AbilityTask
    {
        private float delay;

        public WaitDelayTask(float delay)
        {
            this.delay = delay;
        }

        public override void Start(MonoBehaviour owner, AbilityDefinition ability)
        {
            base.Start(owner, ability);
            owner.StartCoroutine(WaitRoutine());
        }

        private IEnumerator WaitRoutine()
        {
            yield return new WaitForSeconds(delay);
            if (IsRunning) Complete();
        }
    }

    public class WaitForEventTask : AbilityTask
    {
        private string eventName;

        public WaitForEventTask(string eventName)
        {
            this.eventName = eventName;
        }

        public override void Start(MonoBehaviour owner, AbilityDefinition ability)
        {
            base.Start(owner, ability);
            GameplayEventSystem.Listen(eventName, OnEvent);
        }

        public override void Cancel()
        {
            GameplayEventSystem.Unlisten(eventName, OnEvent);
            base.Cancel();
        }

        private void OnEvent(GameplayEventData data)
        {
            if (IsRunning)
            {
                GameplayEventSystem.Unlisten(eventName, OnEvent);
                Complete();
            }
        }
    }

    public class ApplyEffectTask : AbilityTask
    {
        private GameplayEffect effect;
        private GameObject target;

        public ApplyEffectTask(GameplayEffect effect, GameObject target)
        {
            this.effect = effect;
            this.target = target;
        }

        public override void Start(MonoBehaviour owner, AbilityDefinition ability)
        {
            base.Start(owner, ability);
            var effectRunner = owner.GetComponent<GameplayEffectRunner>();
            if (effectRunner != null && target != null)
            {
                effectRunner.ApplyEffect(effect, target, owner.gameObject);
            }
            Complete();
        }
    }
}