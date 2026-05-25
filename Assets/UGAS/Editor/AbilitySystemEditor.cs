using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace UnityGAS
{
    [CustomEditor(typeof(AbilitySystem))]
    public class AbilitySystemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!Application.isPlaying) return;

            var abilitySystem = (AbilitySystem)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Info", EditorStyles.boldLabel);

            object isCastingObj = GetInstanceField(abilitySystem, "isCasting");
            if (isCastingObj is bool isCasting && isCasting)
            {
                var currentCast = (AbilityDefinition)GetInstanceField(abilitySystem, "currentCast");
                if (currentCast == null) return;
                float castProgress = (float)abilitySystem.GetType().GetProperty("CastProgress", BindingFlags.Public | BindingFlags.Instance).GetValue(abilitySystem);

                EditorGUILayout.LabelField("Casting:", currentCast.name);
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), castProgress, "Cast Progress");
            }

            var cooldowns = (System.Collections.IDictionary)GetInstanceField(abilitySystem, "cooldowns");
            if (cooldowns == null) return;
            EditorGUILayout.LabelField("Cooldowns", EditorStyles.boldLabel);
            foreach (System.Collections.DictionaryEntry entry in cooldowns)
            {
                var ability = (AbilityDefinition)entry.Key;
                float remaining = (float)entry.Value;
                if (remaining > 0)
                {
                    float progress = 1f - (remaining / ability.cooldown);
                    EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, $"{ability.name}: {remaining:F1}s");
                }
            }

            Repaint();
        }

        private object GetInstanceField(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(instance);
        }
    }
}