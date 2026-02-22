using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("Regenerate Level", GUILayout.Height(40)))
        {
            ((LevelGenerator)target).GenerateLevel();
        }
    }
}