using UnityEditor;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

/// <summary>
/// クラスの拡張
/// フライングエネミーのプロペラ回転用クラスの拡張
/// Scripts/Editor/の中に必ず入れないと動かない！
/// </summary>
[CustomEditor(typeof(Rotator))]
[CanEditMultipleObjects]
public class RotatorEditor : UnityEditor.Editor
{
    private SerializedProperty _runningProp;
    private SerializedProperty _axisProp;
    private SerializedProperty _customAxisProp;
    private SerializedProperty _speedTypeProp;
    private SerializedProperty _initialProp;
    private SerializedProperty _accelerationProp;
    private SerializedProperty _referenceProp;
    private SerializedProperty _coefficientProp;
    private SerializedProperty _limitProp;


    private void OnEnable()
    {
        // Fetch the objects from the GameObject script to display in the inspector
        _runningProp = serializedObject.FindProperty("running");
        _axisProp = serializedObject.FindProperty("axis");
        _customAxisProp = serializedObject.FindProperty("customAxis");
        _speedTypeProp = serializedObject.FindProperty("speedType");
        _initialProp = serializedObject.FindProperty("initial");
        _accelerationProp = serializedObject.FindProperty("acceleration");
        _referenceProp = serializedObject.FindProperty("reference");
        _coefficientProp = serializedObject.FindProperty("coefficient");
        _limitProp = serializedObject.FindProperty("limit");

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var rotator = target as Rotator;
        Debug.Assert(rotator != null, nameof(rotator) + " != null");

        // Auto Run
        EditorGUILayout.PropertyField(_runningProp, new GUIContent("Auto Run"));

        // Axis
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(_axisProp);
        if (rotator.axis == Rotator.Axis.Custom)
        {
            EditorGUILayout.PropertyField(_customAxisProp, GUIContent.none);
        }

        EditorGUILayout.EndHorizontal();

        // AngleSpeed
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(_speedTypeProp);
        switch (rotator.speedType)
        {
            case Rotator.SpeedType.Fixed:
                EditorGUILayout.PropertyField(_initialProp, GUIContent.none);
                break;
            case Rotator.SpeedType.Linear:
                EditorGUILayout.LabelField("Initial", GUILayout.Width(40));
                EditorGUILayout.PropertyField(_initialProp, GUIContent.none);
                EditorGUILayout.LabelField("Acceleration", GUILayout.Width(80));
                EditorGUILayout.PropertyField(_accelerationProp, GUIContent.none);
                break;
            case Rotator.SpeedType.Sync:
                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Reference", GUILayout.Width(70));
                EditorGUILayout.PropertyField(_referenceProp, GUIContent.none);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Initial", GUILayout.Width(70));
                EditorGUILayout.PropertyField(_initialProp, GUIContent.none);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Coefficient", GUILayout.Width(70));
                EditorGUILayout.PropertyField(_coefficientProp, GUIContent.none);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                break;
        }
        EditorGUILayout.EndHorizontal();

        // Limit
        if (rotator.speedType != Rotator.SpeedType.Fixed)
        {
            EditorGUILayout.PropertyField(_limitProp);
        }

        // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();
    }
}