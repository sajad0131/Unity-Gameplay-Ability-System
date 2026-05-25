using UnityEngine;

namespace UnityGAS
{
    [CreateAssetMenu(fileName = "NewGameplayCue", menuName = "GAS/Gameplay Cue")]
    public class GameplayCue : ScriptableObject
    {
        [Header("Visual")]
        public GameObject spawnPrefab;
        public bool attachToTarget = false;
        public float lifetime = 3f;

        [Header("Audio")]
        public AudioClip sound;
        public float soundVolume = 1f;

        [Header("Animation")]
        public string animationTrigger;

        public void Execute(GameObject target, GameObject instigator, Vector3 position)
        {
            GameObject spawnTarget = target != null ? target : instigator;
            Vector3 spawnPos = spawnTarget != null ? spawnTarget.transform.position : position;

            if (spawnPrefab != null)
            {
                GameObject instance;
                if (attachToTarget && spawnTarget != null)
                {
                    instance = Object.Instantiate(spawnPrefab, spawnTarget.transform);
                }
                else
                {
                    instance = Object.Instantiate(spawnPrefab, spawnPos, Quaternion.identity);
                }

                if (lifetime > 0)
                {
                    Object.Destroy(instance, lifetime);
                }
            }

            if (sound != null && spawnTarget != null)
            {
                var audioSource = spawnTarget.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(sound, soundVolume);
                }
            }

            if (!string.IsNullOrEmpty(animationTrigger) && spawnTarget != null)
            {
                var animator = spawnTarget.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger(animationTrigger);
                }
            }
        }
    }
}