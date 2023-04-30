using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using Codice.Client.Common.GameUI;

[CustomEditor(typeof(SpeechManager))]
public class LevelScriptEditor : Editor
{
    private float newTime = 0.0f;

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

        if (GUILayout.Button("Export Debug to JSON"))
        {
            string jsonString = JsonUtility.ToJson(myTarget.m_debugSpeech, true);
            System.IO.File.WriteAllText(EditorUtility.OpenFilePanel("select JSON file", Application.dataPath, "json"), jsonString);
            Debug.Log(jsonString);

        }

        if (GUILayout.Button("Import from JSON"))
        {
            string jsonString = System.IO.File.ReadAllText(EditorUtility.OpenFilePanel("select JSON file", Application.dataPath, "json"));
            myTarget.m_currentSpeech = JsonUtility.FromJson<SpeechManager.Speech>(jsonString);
        }

        if (GUILayout.Button("Import GameData from JSON"))
        {
            string jsonString = System.IO.File.ReadAllText(EditorUtility.OpenFilePanel("select JSON file", Application.dataPath, "json"));
            myTarget.m_gameData = JsonUtility.FromJson<SpeechManager.GameData>(jsonString);
        }

        if (GUILayout.Button("Export GameData to JSON"))
        {
            string jsonString = JsonUtility.ToJson(myTarget.m_gameData, true);
            System.IO.File.WriteAllText(EditorUtility.OpenFilePanel("select JSON file", Application.dataPath, "json"), jsonString);
            Debug.Log(jsonString);
        }

        newTime = EditorGUILayout.FloatField("Set Time To:", newTime);

        if (GUILayout.Button("Go!"))
        {
            myTarget.SetTime(newTime);
        }
    }
}
