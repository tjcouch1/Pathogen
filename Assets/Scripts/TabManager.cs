using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//TabManager: handles what to do when the user presses tab. Put on a single object.
public class TabManager : MonoBehaviour {

    public GameObject tabStart;//object that starts with focus

    private EventSystem eSystem;

    // Use this for initialization
    void Start ()
    {
        eSystem = EventSystem.current;
        if (tabStart != null)
            selectWithInput(tabStart.GetComponent<Selectable>());
    }

    //update checks for tab pressed, moves selection
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //tab down
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                //preselected tab object
                Selectable selected = null;
                if (eSystem.currentSelectedGameObject != null)
                    selected = eSystem.currentSelectedGameObject.GetComponent<Selectable>();

                if (selected != null)//if you have a selection
                {
                    Selectable next = null;
                    //get selected object's manual tab choice
                    TabObject selectedTabs = selected.GetComponentInParent<TabObject>();
                    if (selectedTabs != null)
                        if (selectedTabs.tabTo != null)
                            next = selectedTabs.tabTo.GetComponent<Selectable>();
                    if (next == null)//if no manual choice, get automatic choice
                        next = selected.FindSelectableOnDown();

                    //select the choice!
                    if (next != null)
                        selectWithInput(next);
                    //else Debug.Log("next nagivation element not found");
                }
            }
            else//tab up
            {
                //preselected tab object
                Selectable selected = null;
                if (eSystem.currentSelectedGameObject != null)
                    selected = eSystem.currentSelectedGameObject.GetComponent<Selectable>();

                if (selected != null)//if you have a selection
                {
                    Selectable prev = null;
                    //get selected object's manual tab choice
                    TabObject selectedTabs = selected.GetComponentInParent<TabObject>();
                    if (selectedTabs != null)
                        if (selectedTabs.backTabTo != null)
                            prev = selectedTabs.backTabTo.GetComponent<Selectable>();
                    if (prev == null)//if no manual choice, get automatic choice
                        prev = selected.FindSelectableOnDown();

                    //select the choice!
                    if (prev != null)
                        selectWithInput(prev);
                    //else Debug.Log("next nagivation element not found");
                }
            }

        }
    }

    void selectWithInput(Selectable selectObject)
    {
        //if it's an input field, also click into it
        InputField inputfield = selectObject.GetComponent<InputField>();
        if (inputfield != null)
            inputfield.OnPointerClick(new PointerEventData(eSystem));

        //select it
        eSystem.SetSelectedGameObject(selectObject.gameObject, new BaseEventData(eSystem));
    }
}
