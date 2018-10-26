using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillFeed : MonoBehaviour {

    [SerializeField]
    GameObject killFeedItemPrefab;
    [SerializeField]
    private float killFeedTimeout = 10;

	public void SetupKillFeed()
    {
        GameManager.singleton.onPlayerKilledCallbacks.Add(OnKill);
    }

    public void OnKill(string player, string source)
    {
        Player p = GameManager.getPlayer(player);

        try
        {
           Player s = GameManager.getPlayer(source);

            GameObject go = Instantiate(killFeedItemPrefab, this.transform);
            go.GetComponent<KillFeedElement>().Setup(p.username, s.username);
            go.transform.SetAsFirstSibling();

            Destroy(go, killFeedTimeout);
        }
        catch (KeyNotFoundException)
        {
            GameObject go = Instantiate(killFeedItemPrefab, this.transform);
            go.GetComponent<KillFeedElement>().Setup(p.username, source);
            go.transform.SetAsFirstSibling();
        }
    }
}
