using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof (CanvasGroup))]
public class UIFader : MonoBehaviour
{
    public CanvasGroup canvas;
    private Coroutine coroutine;

    private bool isShow;
    public UnityEvent OnClosed = new UnityEvent();

    public bool IsShow
    {
        private set
        {
            canvas.interactable = value;
            isShow = value;
        }
        get { return isShow; }
    }

    public virtual void OnClose()
    {
    }

    public IEnumerator AwaitClose()
    {
        yield return new WaitWhile(() => IsShow);
    }

    public void Start()
    {
        canvas.alpha = 0.0f;
        canvas.blocksRaycasts = false;
    }

    public void Reset()
    {
        canvas = GetComponent<CanvasGroup>();
    }

    public UnityEvent Show(bool showFlag = true)
    {
        if (showFlag != IsShow)
        {
            if (showFlag)
            {
                IsShow = true;
            }
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            coroutine = StartCoroutine(ShowIterator(showFlag));
        }
        return OnClosed;
    }

    private IEnumerator ShowIterator(bool showFlag)
    {
        var showSpeed = showFlag ? 3.0f : -3.0f;
        while (true)
        {
            canvas.alpha += Time.deltaTime*showSpeed;
            if (showFlag)
            {
                canvas.blocksRaycasts = true;
                if (canvas.alpha >= 1.0f)
                {
                    canvas.alpha = 1.0f;
                    break;
                }
            }
            else
            {
                if (canvas.alpha <= 0.0f)
                {
                    canvas.alpha = 0.0f;
                    canvas.blocksRaycasts = false;
                    IsShow = false;
                    OnClose();
                    OnClosed.Invoke();
                    OnClosed.RemoveAllListeners();
                    break;
                }
            }
            yield return null;
        }
        coroutine = null;
    }
}