using UnityEngine;
 using System.Collections;
 
 public class UIConsoleLog : MonoBehaviour
 {
     string myLog;
		 public UnityEngine.UI.Text logText;
     Queue myLogQueue = new Queue();
 
     void OnEnable () {
         Application.logMessageReceived += HandleLog;
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

				 logText.text = myLog;
     }
 }