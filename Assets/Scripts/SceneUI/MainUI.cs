using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Button contButton;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_InputField fileField;
    [SerializeField] private TMP_InputField idField;
    [SerializeField] private TMP_InputField trajField;

    public void EnableCanvas(bool val)
    {
        canvas.gameObject.SetActive(val);
    }

    public void ContinueButton(bool val)
    {
        contButton.gameObject.SetActive(val);
    }

    public string GetPath()
    {
        return fileField.text;
    }

    public string GetID()
    {
        return idField.text;
    }

    public void StartButton(bool val)
    {
        startButton.interactable = val;
    }

    public void SetTraj(string trajPath)
    {
        trajField.text = trajPath;
    }

    public string GetTraj()
    {
        return trajField.text;
    }

    public void SetID(string id)
    {
        idField.text = id;
    }

    public void SetFile(string path)
    {
        fileField.text = path;
    }
}
