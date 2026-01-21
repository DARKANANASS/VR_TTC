using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Security.Cryptography;

[Serializable]
public class FileWorker
{
    private const string hashKey = "fileHash";
    private string SavedHash => PlayerPrefs.GetString(hashKey, "");
    [SerializeField] private ConditionManager manager;
    private List<string[]> codes = new List<string[]>();
    [SerializeField] LessonPlanAsset lessonPlan = new LessonPlanAsset();
    private string filePath;

    private void ReadCSV()
    {
        var hash = CalculateSHA256(filePath);
        if (SavedHash == hash && codes.Count != 0) return;

        PlayerPrefs.SetString(hashKey, hash);
        PlayerPrefs.Save();
        
        codes = new List<string[]>();
        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines[1..])
        {
            string[] code = line.Split(";");
            codes.Add(code);
        }
    }

    public LessonPlanAsset LessonPlan()
    {
        lessonPlan.Clear();
        ReadCSV();
        foreach (string[] c in codes)
        {
            var newCond = manager.SetCondition(c[3], c[5], c[7], c[4], c[8]);
            lessonPlan.AddCondition(c[0], newCond);
        }
        return lessonPlan;
    }

    public string CalculateSHA256(string filePath)
    {
        using (var sha256 = SHA256.Create())
        {
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                byte[] hashByte = sha256.ComputeHash(stream);
                return BitConverter.ToString(hashByte).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    public void SetFilePath(string path)
    {
        filePath = path;
    }
}

