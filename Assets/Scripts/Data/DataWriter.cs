using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataWriter
{
    private string file;
    private string mainDir = Path.Combine(Application.persistentDataPath, "LessonResults");
    private readonly string[] headers = new string[] { "Номер блока",
                                                        "Номер попытки внутри блока",
                                                        "Количество осей",
                                                        "Тип условия",
                                                        "Длительность (T1-TX)",
                                                        "Скорость",
                                                        "Вариант траектории",
                                                        "Время начала движения в попытке",
                                                        "Время окончания движения",
                                                        "Время начала предъявления стимула в попытке",
                                                        "Время окончания предъявления стимула",
                                                        "Время начала интервала между стимулами",
                                                        "Время окончания интервала между стимулами",
                                                        "Время движения под окклюдером",
                                                        "Эталонное время от начала движения до целевой зоны",
                                                        "Эталонное время предъявления стимула",
                                                        "Эталонное время прохождения траектории",
                                                        "Фактическое время от момента начала движения до остановки движения",
                                                        "Фактическое время от момента предъявления стимула до момента ответа"
                                                        };
    private Dictionary<string, int> columns = new Dictionary<string, int>{  {"b", 0},
                                                                            {"c", 1},
                                                                            {"axes", 2},
                                                                            {"C", 3},
                                                                            {"D", 4},
                                                                            {"S", 5},
                                                                            {"V", 6},

                                                                            {"move_start", 7},
                                                                            {"move_end", 8},
                                                                            {"stim_start", 9},
                                                                            {"stim_end", 10},

                                                                            {"isi_start", 11},
                                                                            {"isi_end", 12},

                                                                            {"occluder_moving", 13},

                                                                            {"ref_move_to_target", 14},
                                                                            {"ref_stim", 15},
                                                                            {"ref_path", 16},

                                                                            {"actual_move_duration", 17},
                                                                            {"actual_stim_to_response", 18}};
    private string[] results;

    public void CreateFile(string id)
    {
        Directory.CreateDirectory(mainDir);
        string userDir = Path.Combine(mainDir, id);
        Directory.CreateDirectory(userDir);
        file = Path.Combine(userDir, DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".csv");

        PlayerPrefs.SetString("resultsPath", file);
        PlayerPrefs.Save();

        using (File.Create(file)) { }
        ;
        File.AppendAllText(file, string.Join(";", headers) + "\n", System.Text.Encoding.UTF8);
    }

    public void CreateRowAndFlush(Results res)
    {
        results = new string[19];
        foreach (TimeEvent ev in res.Events)
        {
            WriteResToList(ev.Name, ev.Time.ToString());
        }
        foreach (KeyValuePair<string, object> pair in res.Values)
        {
            WriteResToList(pair.Key, pair.Value.ToString());
        }
        FlushRowToFile();
    }

    private void FlushRowToFile()
    {
        File.AppendAllText(file, string.Join(";", results) + "\n", System.Text.Encoding.UTF8);
    }

    private void WriteResToList(string key, string value)
    {
        results[columns[key]] = value;
    }
    
    public bool SetPath(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }
        file = path;
        return true;
    }
}
