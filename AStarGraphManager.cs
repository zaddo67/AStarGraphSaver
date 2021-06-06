using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Pathfinding.Util;
using UnityEngine.SceneManagement;
using System;
using WorldStreamer2;
using System.Linq;

/// <summary>
/// This Class will need to be set up for your project
/// It Provides a link to the Player so that the pathfinding can be centered around 
/// the players movements
/// This class also keeps track of the recast graphs that should be loaded in the world.
/// </summary>
public class AStarGraphManager : MonoBehaviour
{

    //RecastGraph _graph;
    StreamerZ _streamer;
    public List<TerrainRecast> terrainRecasts = new List<TerrainRecast>();

    private static AStarGraphManager _instance;

    public static AStarGraphManager Instance
    {
        get { return _instance; }
    }

    public static Vector3 PlayerPosition
    {
        get
        {
            if (_instance == null || _instance._streamer == null || _instance._streamer.player == null) return Vector3.zero;
            return _instance._streamer.player.position;
        }
    }

    void Start()
    {
        _instance = this;
        _streamer = gameObject.GetComponent<StreamerZ>();

    }

    public static void AddTerrainData(TerrainRecast data)
    {
        if (_instance == null) return;

        if (!_instance.terrainRecasts.Any(r => r.TerrainName == data.TerrainName))
            _instance.terrainRecasts.Add(data);
    }


    public static void RemoveTerrainData(TerrainRecast data)
    {
        if (_instance == null) return;

        _instance.terrainRecasts.RemoveAll(r => r.TerrainName == data.TerrainName);
    }

}
