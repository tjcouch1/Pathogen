using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
[ExecuteInEditMode]
public class VersionNoUpdater : MonoBehaviour {

    private void Start()
    {
        Text t = gameObject.GetComponent<Text>();
        t.text = "Version " + Application.version;
    }
}
