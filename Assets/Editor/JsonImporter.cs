using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(SpeechManager))]
public class LevelScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SpeechManager myTarget = (SpeechManager)target;
        
        if (GUILayout.Button("Export to JSON"))
        {
            string jsonString = JsonUtility.ToJson(myTarget.m_currentSpeech, true);
            System.IO.File.WriteAllText(EditorUtility.OpenFilePanel("select JSON file", Application.dataPath, "json"), jsonString);
            Debug.Log(jsonString);
        }

            
        if (GUILayout.Button("Import from JSON"))
        {
            string jsonString = System.IO.File.ReadAllText(EditorUtility.OpenFilePanel("select JSON file", Application.dataPath, "json"));
            myTarget.m_currentSpeech = JsonUtility.FromJson<SpeechManager.Speech>(jsonString);
        }
    }
}
