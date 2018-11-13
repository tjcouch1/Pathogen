﻿using System;
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

        Player s = GameManager.getPlayer(source);
        if (s != null)
        {
            GameObject go = Instantiate(killFeedItemPrefab, this.transform);
            go.GetComponent<KillFeedElement>().Setup(p.username, s.username);
            go.transform.SetAsFirstSibling();
            Destroy(go, killFeedTimeout);
        }
        else
        {
            GameObject go = Instantiate(killFeedItemPrefab, this.transform);
            go.GetComponent<KillFeedElement>().Setup(p.username, source);
            go.transform.SetAsFirstSibling();
            Destroy(go, killFeedTimeout);
        }

    }
}
