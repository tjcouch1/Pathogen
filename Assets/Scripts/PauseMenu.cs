using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class PauseMenu : MonoBehaviour
{
    private VCNetworkManager NM;
    public static bool isOn = false;

    private void Start()
    {
        NM = (VCNetworkManager) NetworkManager.singleton;
    }

    public void LeaveRoom()
    {
        MatchInfo matchInfo = NM.matchInfo;
        if (!NM.isPrivate)
            NM.matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, 0, NM.OnDropConnection);
        NM.StopHost();
    }
}
