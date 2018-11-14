using UnityEngine;
 using System.Collections;

//credit to https://answers.unity.com/questions/1020051/print-debuglog-to-screen-c.html
public class UIConsoleLog : MonoBehaviour
 {
     string myLog;
     public UnityEngine.UI.Text logText;
     private UIConsoleTextHolder uiConsoleTextHolder;
     Queue myLogQueue = new Queue();
 
     void OnEnable () {
         Application.logMessageReceived += HandleLog;

         GameObject consoleTextHolder = GameObject.Find("ConsoleTextHolder");
         if (consoleTextHolder != null)
         {
            uiConsoleTextHolder = consoleTextHolder.GetComponent<UIConsoleTextHolder>();
            myLogQueue.Enqueue(uiConsoleTextHolder.Log);
         }
     }
     
     void OnDisable () {
         Application.logMessageReceived -= HandleLog;
     }
 
     void HandleLog(string logString, string stackTrace, LogType type){
         myLog = logString;
         string newString = "\n [" + type + "] : " + myLog;
         myLogQueue.Enqueue(newString);
         if (type == LogType.Exception)
         {
             newString = "\n" + stackTrace;
             myLogQueue.Enqueue(newString);
         }
         myLog = string.Empty;
         foreach(string mylog in myLogQueue){
             myLog += mylog;
         }

        int subsStart = Mathf.Max(0, myLog.Length - 10000);
        myLog = myLog == null ? string.Empty : myLog.Substring(subsStart, myLog.Length - subsStart);
		logText.text = myLog;
        uiConsoleTextHolder.Log = myLog;
     }
 }