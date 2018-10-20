using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This Script should be placed on the trigger object of the scanner
[RequireComponent(typeof(Collider))]
public class Scanner : MonoBehaviour {

    [SerializeField] private int processingTime;
    [SerializeField] private Image display;
    [SerializeField] private MonitorGraphics Monitor;

    private bool isProcessing = false;
    private Player player;

    private void Start()
    {
        display.sprite = Monitor.defaultSprite;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")){
            if (isProcessing)
            {
                Debug.LogWarning("Player entered while processing was running");
                processingFailure();
                return;
            }
            else
            {
                player = other.gameObject.GetComponent<Player>();
                StartCoroutine(processingLoop());
            }           
        }
        else
        {
            Debug.Log("Scanner cannot scan non-player objects");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isProcessing)
        {
            Debug.Log("Player exited during processing!");
            processingFailure();
        }
    }

    private void processingSuccess()
    {
        if (player.GetInfectedState() == true)
        {
            display.sprite = Monitor.infectedSprite;
        }
        else
        {
            display.sprite = Monitor.healthySprite;
        }
        Invoke("Reset", processingTime);
    }

    private void processingFailure()
    {
        StopAllCoroutines();
        isProcessing = false;
        display.sprite = Monitor.errorSprite;
        Monitor.loadingGraphic.SetActive(false);
        Invoke("Reset", processingTime);
    }

    IEnumerator processingLoop()
    {
        isProcessing = true;
        display.sprite = Monitor.processingSprite;
        Monitor.loadingGraphic.SetActive(true);
        Debug.Log("Processing player " + player.name);
        yield return new WaitForSeconds(processingTime);

        isProcessing = false;
        Monitor.loadingGraphic.SetActive(false);
        Debug.Log("Done processing player " + player.name);
        processingSuccess();
    }

    private void Reset()
    {
        //We only want to reset if the scanner isn't currently active
        if (!isProcessing)
        {
            display.sprite = Monitor.defaultSprite;
            player = null;
        }
    }

    public void disableScanner()
    {
        display.sprite = Monitor.disabledSprite;
        this.enabled = false;
    }

}

[System.Serializable]
class MonitorGraphics
{
    public GameObject loadingGraphic;
    public Sprite defaultSprite;
    public Sprite processingSprite;
    public Sprite healthySprite;
    public Sprite infectedSprite;
    public Sprite disabledSprite;
    public Sprite errorSprite;
}