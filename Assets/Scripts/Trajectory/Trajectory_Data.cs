using UnityEngine;
using System.IO;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif
public class Trajectory_Data : MonoBehaviour
{
    [System.Serializable]
    public class TrajectoryCollection
    {
        public string name;
        public Vector3[] points;
    }
    [System.Serializable]
    public class ExportList
    {
        //�� ������ �������� savedPoints, � ������� ��������� ����� �� ��� ��� �������
        public List<TrajectoryCollection> savedPoints;
    }
    
    [Header("���� � ����� JSON")]
    public static string jsonFilePath;

    [SerializedDictionary("Name", "Points")]
    public static SerializedDictionary<string, Vector3[]> TrajectoryDictionary = new SerializedDictionary<string, Vector3[]>{ };

    public ConditionManager manager;

    public void Start()
    {
        LoadFromJSON(TrajectoryDictionary);
    }
    /// ��������� ����� �� JSON �����
    [ContextMenu("Load from JSON File")]
    public void LoadFromJSON(Dictionary<string, Vector3[]> Dic)
    {
        string filePath = jsonFilePath;
        try
        {
            // ������ ����
            string json = File.ReadAllText(filePath);
            // ����������� � �������
            ExportList importData = JsonUtility.FromJson<ExportList>(json);

            foreach (var collection in importData.savedPoints)
            {
                Dic[collection.name] = collection.points;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"������ �������� �����: {e.Message}");
        }
    }
    
    public static void SetPath(string path)
    {
        jsonFilePath = path;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Trajectory_Data))]
public class Trajectory_DictionaryEditor : Editor
{
    private Vector2 scrollPos;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Trajectory_Data script = (Trajectory_Data)target;

        // ���������� � �����
        EditorGUILayout.LabelField("����:", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField($"����: {Trajectory_Data.jsonFilePath}", EditorStyles.miniLabel);

        GUILayout.Space(5);

        // ��������� ���������� �� JSON        
        if (GUILayout.Button("��������� ���������� �� JSON", GUILayout.Height(25)))
        {
             script.LoadFromJSON(script.manager.vectors);
            EditorUtility.SetDirty(script);
        }        
    }
}
#endif