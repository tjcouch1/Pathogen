using UnityEngine;
using System;

//To eventually be replaced by some XML or JSON parsing utility if we need to store much more user data
public class UADataTranslator : MonoBehaviour {

    private static string KILLS_SYMBOL = "[KILLS]";
    private static string DEATHS_SYMBOL = "[DEATHS]";
    private static string POINTS_SYMBOL = "[POINTS]";
    private static string VERSION_SYMBOL = "[VERSION]";
    private static string DEPLOY_SYMBOL = "[DEPLOY]";

    public static int DataToKills(string data)
    {
        return int.Parse(FindSymbol(data, KILLS_SYMBOL));
    }
    public static int DataToDeaths(string data)
    {
        return int.Parse(FindSymbol(data, DEATHS_SYMBOL));
    }
    public static int DataToPoints(string data)
    {
        return int.Parse(FindSymbol(data, POINTS_SYMBOL));
    }
    public static string DataToVersion(string data)
    {
        return FindSymbol(data, VERSION_SYMBOL);
    }
    public static bool DataToDeploy(string data)
    {
        return bool.Parse(FindSymbol(data, DEPLOY_SYMBOL));
    }

    public static string FormatDataToString(int kills, int deaths, int points)
    {
        return KILLS_SYMBOL + kills + "/"
             + DEATHS_SYMBOL + deaths + "/"
             + POINTS_SYMBOL + points + "/";
    }

    private static string FindSymbol(string data, string symbol)
    {
        string[] chunks = data.Split('/');
        foreach (string chunk in chunks)
        {
            if (chunk.StartsWith(symbol))
            {
                return chunk.Substring(symbol.Length);
            }

        }

        Debug.LogError("UADataTranslator - " + symbol + " not found in " + data);
        return "";
    }
}
