using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIShowHide : MonoBehaviour {

    public bool StartHidden = false;

    private bool _hidden = false;

    public bool Hidden
    {
        get { return _hidden; }
        set
        {
            if (!value)
            {
                Show();
                _hidden = false;
            }
            else
            {
                Hide();
                _hidden = true;
            }
        }
    }

    CanvasGroup canvasGroup;

    private void Start()
    {
        canvasGroup = GetComponentInParent<CanvasGroup>();

        Hidden = StartHidden;
    }

    private void Show()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;//does stuff
        canvasGroup.blocksRaycasts = true;//inputs
    }

    private void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;//does nothing
        canvasGroup.blocksRaycasts = false;//no inputs
    }
}
