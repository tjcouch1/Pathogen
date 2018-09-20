using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillFeed : MonoBehaviour {

    [SerializeField]
    GameObject killFeedItemPrefab;
    [SerializeField]
    private float killFeedTimeout = 8;

	// Use this for initialization
	void Start () {

        GameManager.singleton.onPlayerKilledCallbacks.Add(OnKill);
	}

    public void OnKill(string player, string source)
    {
        Player p = GameManager.getPlayer(player);
        GameObject go = Instantiate(killFeedItemPrefab, this.transform);
        go.GetComponent<KillFeedElement>().Setup(p.username, source);
        go.transform.SetAsFirstSibling();

        Destroy(go, killFeedTimeout);
    }
}
