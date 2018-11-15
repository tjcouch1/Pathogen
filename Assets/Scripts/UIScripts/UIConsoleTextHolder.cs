using UnityEngine;
 using System.Collections;

public class UIConsoleTextHolder : MonoBehaviour
 {
    public string Log;

    void Start()
    {
        Object.DontDestroyOnLoad(gameObject);
    }
 }