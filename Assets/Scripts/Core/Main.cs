using System.IO;
using UnityEngine;
using System.Threading.Tasks;

public class Main : MonoBehaviour
{
    [SerializeField] private FileWorker fileWorker;
    [SerializeField] private LessonManager lesson;
    [SerializeField] private MainUI ui;
    [SerializeField] private InputListener input;
    private DataWriter writer;

    private string SavedTraj => PlayerPrefs.GetString("trajPath", "");
    private string SavedPath => PlayerPrefs.GetString("filePath", "");
    private string SavedHash => PlayerPrefs.GetString("fileHash", "");
    private string ResultsPath => PlayerPrefs.GetString("resultsPath", "");
    private string SavedID => PlayerPrefs.GetString("id", "");


    public void Start()
    {
        input.EscapePressed += OnStop;
        writer = new DataWriter();
        ui.EnableCanvas(true);

        ui.SetID(SavedID);
        ui.SetFile(SavedPath);
        ui.SetTraj(SavedTraj);

        CheckContinue();
        CheckStart();
    }
    /*всё что On.. - подписано на события ui через инспеткор*/
    public async void OnStart()
    {
        writer.CreateFile(ui.GetID());
        lesson.ResetProgress();
        await Lesson();
    }

    public async void OnContinue()
    {
        var currentId = ui.GetID();
        if (!writer.SetPath(ResultsPath) || currentId != SavedID)
        {
            writer.CreateFile(currentId);
        }
        await Lesson();
    }

    private async Task Lesson()
    {
        fileWorker.SetFilePath(ui.GetPath());
        Trajectory_Data.jsonFilePath = ui.GetTraj();
        SetPrefs();
        ui.EnableCanvas(false);
        await lesson.StartLesson(fileWorker.LessonPlan(), writer);
        ui.EnableCanvas(true);
        ui.ContinueButton(lesson.HasProgress());
    }

    public void OnStop()
    {
        lesson.AbortLesson();
        ui.EnableCanvas(true);
        ui.ContinueButton(lesson.HasProgress());
    }

    private void SetPrefs()
    {
        PlayerPrefs.SetString("trajPath", ui.GetTraj());
        PlayerPrefs.SetString("filePath", ui.GetPath());
        PlayerPrefs.SetString("id", ui.GetID());
        PlayerPrefs.Save();
    }

    public void OnChangeInput()
    {
        CheckStart();
        CheckContinue();
    }

    public void CheckStart()
    {
        bool fileExists = File.Exists(ui.GetPath());
        bool id = !string.IsNullOrEmpty(ui.GetID());
        bool traj = File.Exists(ui.GetTraj());

        ui.StartButton(fileExists && id && traj);
    }

    private void CheckContinue()
    {
        var path = ui.GetPath();
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            ui.ContinueButton(false);
            return;
        }

        bool modifiedPath = FileRefreshed();
        bool progress = lesson.HasProgress();
        bool sameId = ui.GetID() == SavedID;

        ui.ContinueButton(!modifiedPath && progress && sameId);
    }

    private bool FileRefreshed()
    {
        if (ui.GetPath() != SavedPath)
        {
            return true;
        }
        return fileWorker.CalculateSHA256(ui.GetPath()) != SavedHash;
    }

    public void OnQuit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}