using UnityEditor;
using UnityEngine;

namespace UnityGAS
{
    [CustomEditor(typeof(AbilityDefinition))]
    public class AbilityDefinitionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Info", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Activation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cooldown"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("castTime"));
            if (serializedObject.FindProperty("castTime").floatValue > 0)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("canCastWhileMoving"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("interruptible"));
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxLevel"));

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Cost", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cost"));
            if (serializedObject.FindProperty("cost").floatValue > 0)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("costAttribute"));
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Targeting", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetingType"));

            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dimension"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useRangeCheck"));

            var targetingType = (TargetingType)serializedObject.FindProperty("targetingType").enumValueIndex;
            var useRangeCheck = serializedObject.FindProperty("useRangeCheck").boolValue;

            if (targetingType != TargetingType.Self && useRangeCheck)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("range"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("targetableLayers"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("occlusionLayers"));
            }
            if (targetingType == TargetingType.Area || targetingType == TargetingType.Ground)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("radius"));
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("effects"), true);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Visuals & Audio", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animationTrigger"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("castVFX"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("impactVFX"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("castSFX"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("impactSFX"));

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("abilityTags"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("requiredTags"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("blockedByTags"), true);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Target Tags", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetRequiredTags"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetBlockedByTags"), true);
            

            serializedObject.ApplyModifiedProperties();
        }
    }
}