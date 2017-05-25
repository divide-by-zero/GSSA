using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DialogPanel : MonoBehaviour
{
    [SerializeField] private UIFader fader;
    [SerializeField] private Text titleLabel, contentLabel, descriptionLabel;
    [SerializeField] private GameObject contents;

    public string Title
    {
        set { titleLabel.text = value; }
    }

    public string Content
    {
        set { contentLabel.text = value; }
    }

    public string Description
    {
        set { descriptionLabel.text = value; }
    }

    public void Show(string title, string content, string description)
    {
        Title = title;
        Content = content;
        Description = description;
        fader.Show(true);
    }

    public void Hide()
    {
        fader.Show(false);
    }
}
