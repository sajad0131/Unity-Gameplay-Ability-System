using UnityEditor;
using UnityEngine;

namespace UnityGAS
{
    [CustomEditor(typeof(GameplayEffect), true)]
    public class GameplayEffectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Info", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("effectName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Duration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"));

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Periodic", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isPeriodic"));
            if (serializedObject.FindProperty("isPeriodic").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("period"));
                if (target is PeriodicModifierEffect)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("revertOnRemove"));
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Stacking", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canStack"));
            if (serializedObject.FindProperty("canStack").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxStacks"));
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Modifiers", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("modifiers"), true);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Gameplay Cues", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cues"), true);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Effect Tags", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("grantedTags"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ongoingTags"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("applicationRequiredTags"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("applicationBlockedByTags"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("removeOnApplicationTags"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("removeOnRemoveTags"), true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}