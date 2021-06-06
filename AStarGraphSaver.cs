using DeadFear.Utility;
using Pathfinding;
using Pathfinding.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.AddressableAssets;


/// <summary>
/// In Large open world games the world is too large to store the navmesh for the entire world
/// This component will facilitate loading a recast graph for each terrain tile loaded additively
/// to a master world scene.  
/// 
/// Usage In Editor: 
/// Add this component to a scene with A* Pathfinding recast graph.  Scan the graph, then use the Save button on
/// this component to store the recast graph into a file.  Add the saved file into an addressables group, then
/// Set the _NavMeshData property on this component to the saved file.  The AstarPath Component in this scene 
/// should then be disabled and the scene saved.
/// 
/// Usage In Game:
/// A master world scene is requried that will additively load and unload scenes containing single terrain tiles.
/// This master world scene must have a AstarPath component with a configured tiled recast graph that has bounds large enough
/// to contain all the sub-scenes that are loaded at any time.  This recast graph will be moved to center around the player.
/// The graph tile sizes for both the master and child scnes must be exactly the same size.
///
/// </summary>
public class AStarGraphSaver : MonoBehaviour
{
    /// <summary>
    /// Path to save serialized Graph data
    /// </summary>
    [SerializeField] public string SavePath;

    /// <summary>
    /// This objects name will be used as the save file name
    /// </summary>
    [SerializeField] public GameObject ParentObject;

    /// <summary>
    /// Asset Reference for 
    /// </summary>
    [SerializeField] private AssetReference _NavMeshData;
    public TerrainRecast _data;

    private void Start()
    {
        // Load the recast graph when this scene is loaded
        _NavMeshData.LoadAssetAsync<TextAsset>().Completed += handle =>
        {
            LoadNavMesh(handle.Result);

        };
    }

    /// <summary>
    /// Deserialize this scenes recast graph data
    /// And load into the master graph
    /// </summary>
    /// <param name="textAsset"></param>
    private void LoadNavMesh(TextAsset textAsset)
    {
        _data = DesializeTile(textAsset.bytes);

        var graph = AstarPath.active.data.recastGraph;
        if (ValidateData(graph))
        {
            CheckGraphPosition(graph);
            LoadGraphTiles(graph);
        }
    }

    /// <summary>
    /// Validate that the World graph is compatible with the terrain graph data
    /// </summary>
    /// <param name="graph">World recast graph</param>
    /// <returns>true if they are compatable</returns>
    private bool ValidateData(RecastGraph graph)
    {
        if (graph.TileWorldSizeX != _data.TileWorldSizeX)
        {
            Debug.LogWarning($"World graph tile sizeX [{graph.TileWorldSizeX}] does not match Terrain tile sizeX [{_data.TileWorldSizeX}]");
            return false;
        }

        if (graph.TileWorldSizeZ != _data.TileWorldSizeZ)
        {
            Debug.LogWarning($"World graph tile sizeX [{graph.TileWorldSizeZ}] does not match Terrain tile sizeZ [{_data.TileWorldSizeZ}]");
            return false;
        }

        if ((graph.tileXCount % _data.tileXCount) != 0)
        {
            Debug.LogWarning($"World tile X count [{graph.tileXCount}] is not a multiple of terrain tile X count [{_data.tileXCount}]");
            return false;
        }

        if ((graph.tileZCount % _data.tileZCount) != 0)
        {
            Debug.LogWarning($"World tile Z count [{graph.tileZCount}] is not a multiple of terrain tile Z count [{_data.tileZCount}]");
            return false;
        }

        if (graph.tileSizeX != _data.tileSizeX || graph.tileSizeZ != _data.tileSizeZ)
        {
            Debug.LogWarning($"World tile size [{graph.tileSizeX},{graph.tileSizeZ}] does not match terrain tile size [{_data.tileSizeX},{_data.tileSizeZ}]");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Load this scenes recast graph into the master scenes recast graph,
    /// replacing the tiles in the appropriate position
    /// </summary>
    /// <param name="graph"></param>
    private void LoadGraphTiles(RecastGraph graph)
    {

        AstarPath.active.AddWorkItem((context) =>
        {

            // Get the relative offset between the center of the first tile from terrain
            // And the origin of the RecastGraph
            float relativeX = _data.tiles[0].CenterX - (graph.forcedBoundsCenter.x - (graph.forcedBoundsSize.x/2));
            float relativeZ = _data.tiles[0].CenterZ - (graph.forcedBoundsCenter.z - (graph.forcedBoundsSize.z/2));

            // Find the first matching tile in RecastGraph
            int xOffset = Mathf.FloorToInt(relativeX / graph.TileWorldSizeX);
            int zOffset = Mathf.FloorToInt(relativeZ / graph.TileWorldSizeZ);

            graph.StartBatchTileUpdate();
            for (int x = 0; x < _data.tileXCount; x++)
            {
                for (int z = 0; z < _data.tileZCount; z++)
                {
                    int i = x + _data.tileXCount * z;
                    graph.ReplaceTile(x + xOffset, z + zOffset, AStarGraphSaver.Vert3toInt3(_data.tiles[i].verts), _data.tiles[i].tris);
                    //context.QueueFloodFill();
                }
            }
            graph.EndBatchTileUpdate();
        });
    }

    /// <summary>
    /// Check that the player is center in central area of the World recast graph
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="data"></param>
    private void CheckGraphPosition(RecastGraph graph)
    {

        // Get Players current position
        Vector3 playerPos = AStarGraphManager.PlayerPosition;

        // Calculate Bounds of Central area of Recast Graph, excluding outer tile 
        Bounds playerBounds =
            new Bounds(graph.forcedBoundsCenter,
            graph.forcedBoundsSize - new Vector3(
                2 * _data.tileXCount * _data.TileWorldSizeX,
                0,
                2 * _data.tileZCount * _data.TileWorldSizeZ));

        // If player is not within central area of graph, then reposition graph
        if (!playerBounds.Contains(playerPos))
        {
            RecenterGraph(graph, playerPos);
        }

    }

    /// <summary>
    /// Recenter graph so that the player is at the center
    /// </summary>
    private void RecenterGraph(RecastGraph graph, Vector3 playerPos)
    {
        // Assume Origin is (0,0,0) for Tile at (0,0)

        AstarPath.active.AddWorkItem((context) =>
        {
            // Calculate the Terrain Co-ordinates
            int terrainX = (int)Mathf.Floor(playerPos.x / (_data.tileXCount * _data.TileWorldSizeX));
            int terrainZ = (int)Mathf.Floor(playerPos.z / (_data.tileZCount * _data.TileWorldSizeZ));

            // Calculate the position for players terrain tile
            float terrainTileX = terrainX * _data.tileXCount * _data.TileWorldSizeX;
            float terrainTileZ = terrainZ * _data.tileZCount * _data.TileWorldSizeZ;

            float saveY = graph.forcedBoundsCenter.y;

            // Move the Graph
            graph.forcedBoundsCenter = new Vector3(
                terrainTileX + (0.5f * _data.tileXCount * _data.TileWorldSizeX),
                saveY,
                terrainTileZ + (0.5f * _data.tileZCount * _data.TileWorldSizeZ));
            graph.transform = graph.CalculateTransform();
        });
    }

    /// <summary>
    /// Serialise this scenes recast graph into a file
    /// </summary>
    public void SaveGraph()
    {
        TerrainRecast data = new TerrainRecast();

        var graph = AstarPath.active.data.recastGraph;
        data.TerrainName = this.ParentObject.name;
        data.tileSizeX = graph.tileSizeX;
        data.tileSizeZ = graph.tileSizeZ;
        data.TileWorldSizeX = graph.TileWorldSizeX;
        data.TileWorldSizeZ = graph.TileWorldSizeZ;
        data.tileXCount = graph.tileXCount;
        data.tileZCount = graph.tileZCount;
        data.tiles = new TileData[graph.tileXCount * graph.tileZCount];
        for (int x = 0; x < data.tileXCount; x++)
        {
            for (int z = 0; z < data.tileZCount; z++)
            {
                Bounds b = graph.GetTileBounds(x, z);
                var tile = graph.GetTile(x, z);

                // Vertex offset. Applied to all verts
                Int3 offset = (Int3)new Vector3(
                    x * data.TileWorldSizeX, 
                    (graph.forcedBoundsSize.y / 2) - graph.forcedBoundsCenter.y, 
                    z * data.TileWorldSizeZ);

                data.tiles[x + data.tileXCount * z] = new TileData
                {
                    CenterX = b.center.x,
                    CenterY = b.center.y,
                    CenterZ = b.center.z,
                    SizeX = b.size.x,
                    SizeY = b.size.y,
                    SizeZ = b.size.z,
                    verts = Int3ToVert3(tile.vertsInGraphSpace, offset),
                    tris = tile.tris
                };
            }
        }
        SerializeTile(data);
    }

    /// <summary>
    /// Deserialize the saved graph
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    TerrainRecast DesializeTile(byte[] data)
    {
        IFormatter formatter = new BinaryFormatter();
        Stream stream = new MemoryStream(data);
        stream.Seek(0, SeekOrigin.Begin);
        object obj = formatter.Deserialize(stream);
        return (TerrainRecast)obj;
    }

    /// <summary>
    /// Serialise the class to a file
    /// </summary>
    /// <param name="tile"></param>
    void SerializeTile(TerrainRecast tile)
    {
        IFormatter formatter = new BinaryFormatter();
        string fileName = string.Format("{0}.bytes", System.IO.Path.Combine(Application.dataPath, this.SavePath, this.ParentObject.name).ToString());
        Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        formatter.Serialize(stream, tile);
        stream.Close();
    }

    /// <summary>
    /// Convert A* Int3 type into a custom Vert3 type so it can be serialized
    /// </summary>
    /// <param name="int3Verts"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static Vert3[] Int3ToVert3(Int3[] int3Verts, Int3 offset)
    {
        Vert3[] vectorVerts = new Vert3[int3Verts.Length];
        for (int v = 0; v < int3Verts.Length; v++)
        {
            vectorVerts[v] = new Vert3(int3Verts[v].x - offset.x, int3Verts[v].y - offset.y, int3Verts[v].z - offset.z);
        }
        return vectorVerts;
    }

    /// <summary>
    /// Convert the Vert3 type back to A* Int3 type
    /// </summary>
    /// <param name="vert3Verts"></param>
    /// <returns></returns>
    public static Int3[] Vert3toInt3(Vert3[] vert3Verts)
    {
        Int3[] int3Verts = new Int3[vert3Verts.Length];
        for (int v = 0; v < vert3Verts.Length; v++)
        {
            int3Verts[v] = new Int3(vert3Verts[v].x, vert3Verts[v].y, vert3Verts[v].z);
        }
        return int3Verts;
    }
}


/// <summary>
/// Classes used to serialize the recast graph
/// </summary>

[Serializable]
public class TerrainRecast
{
    public string TerrainName;
    public int tileSizeX;
    public int tileSizeZ;
    public float TileWorldSizeX;
    public float TileWorldSizeZ;
    public int tileXCount;
    public int tileZCount;
    public TileData[] tiles;
}

[Serializable]
public class TileData
{
    public float CenterX;
    public float CenterY;
    public float CenterZ;
    public float SizeX;
    public float SizeY;
    public float SizeZ;

    public Vert3[] verts;
    public int[] tris;
}

[Serializable]
public class Vert3
{
    public Vert3(int x, int y, int z)
    {
        this.x = x; this.y = y; this.z = z;
    }
    public int x;
    public int y;
    public int z;
}




