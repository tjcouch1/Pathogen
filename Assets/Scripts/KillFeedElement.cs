using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillFeedElement : MonoBehaviour {

    [SerializeField]
    Text text;

    public void Setup(string player, string source)
    {
        text.text = "<b>" + "<color=yellow> " + source + "</color>" + "</b>" + " killed " + "<b>" + "<color=red>" + player + "</color>" + "</b>";
    }
}
