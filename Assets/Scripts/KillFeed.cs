using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillFeed : MonoBehaviour {

    [SerializeField]
    GameObject killFeedItemPrefab;
    [SerializeField]
    private float killFeedTimeout = 4;

	// Use this for initialization
	void Start () {

        GameManager.singleton.onPlayerKilledCallback += OnKill;
	}

    public void OnKill(string player, string source)
    {
        GameObject go = Instantiate(killFeedItemPrefab, this.transform);
        go.GetComponent<KillFeedElement>().Setup(player, source);
        go.transform.SetAsFirstSibling();

        Destroy(go, killFeedTimeout);
    }
}
