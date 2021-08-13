using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Pathfinding.Util;
using DeadFear.Utility;
using UnityEngine.SceneManagement;
using System;
using WorldStreamer2;
using System.Linq;

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

    private void OnEnable()
    {
        // TODO: Replace with your event handler to catch Scene Unloading
        Opsive.Shared.Events.EventHandler.RegisterEvent<Scene>(dfc.EVT_WORLDSTREAMER_SCENE_UNLOADED, OnWorldStreamerSceneUnloaded);
    }


    private void OnDisable()
    {
        // TODO: Replace with your event handler to catch Scene Unloading
        Opsive.Shared.Events.EventHandler.UnregisterEvent<Scene>(dfc.EVT_WORLDSTREAMER_SCENE_UNLOADED, OnWorldStreamerSceneUnloaded);
    }


    private void OnWorldStreamerSceneUnloaded(Scene scene)
    {
        _instance.terrainRecasts.RemoveAll(r => r.SceneName == scene.name);
        Debug.Log($"Remove TerrainReacast: {scene.name}");
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

    public static List<TerrainRecast> AllTiles
    {
        get { return Instance.terrainRecasts; }
    }


}
