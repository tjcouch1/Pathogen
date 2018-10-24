using UnityEngine;

public class Util {

    public static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if(obj == null)
        {
            return;
        }

        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if(child == null)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}

public class timerEvent
{
    public delegate void eventCallback();
    public eventCallback eventCallbackFunction;
    public int timeToEvoke;
   
    public timerEvent(eventCallback callback, int _timeToEvoke)
    {
        eventCallbackFunction = callback;
        timeToEvoke = _timeToEvoke;
    }
}

public enum Direction
{
    up,
    down,
    left,
    right
};
