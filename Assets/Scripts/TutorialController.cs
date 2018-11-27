using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour {

    private int slideNo = 0;
    [SerializeField] private GameObject[] Slides;

    public void advanceSlide()
    {
        Slides[slideNo].SetActive(false);
        if(slideNo+1 >= Slides.Length)
        {
            resetSlides();
        }
        else
        {
            slideNo++;
            Slides[slideNo].SetActive(true);
        }
    }

    public void resetSlides()
    {
        Slides[slideNo].SetActive(false);
        slideNo = 0;
        Slides[slideNo].SetActive(true);
    }
}
