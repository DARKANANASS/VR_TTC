using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LessonUI : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private List<TextMeshProUGUI> allDescriptions;
    [SerializeField] private TextMeshProUGUI occuracy;

    void Awake()
    {
        HideAll();
    }

    public void SetImage(Sprite sprite)
    {
        image.sprite = sprite;
        Image(true);
    }

    public void Image(bool val)
    {
        image.enabled = val;
    }

    public void EndScreen(bool val)
    {
        textMesh.enabled = val;
    }
    
    public void Description(TextMeshProUGUI desc, bool val)
    {
        desc.enabled = val;
    }

    public void HideAll()
    {
        Image(false);
        EndScreen(false);
        occuracy.text = "";
        
        foreach (TextMeshProUGUI d in allDescriptions)
        {
            d.enabled = false;
        }
    }

    public void ShowAccuracy(string res)
    {
        occuracy.text = res;
    }
    
    public void HideAccuracy()
    {
        occuracy.text = "";
    }
}
