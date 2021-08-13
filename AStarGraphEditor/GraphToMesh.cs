using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools.AStarGraphEditor
{

    public class GraphToMesh : MonoBehaviour
    {
        public Material mat;

        /// <summary>
        /// Generate Game Objects with a Mesh for each graph tile
        /// </summary>
        public void GenerateMesh()
        {
            // Load the graph data
            var data = LoadGraph();

            // Create game objects with mesh
            CreateMesh(data);
        }

        private void CreateMesh(GraphData data)
        {
            for (int i = 0; i < data.tiles.Length; i++)
            {
                GameObject obj = new GameObject($"AStar Mesh Tile [{data.tiles[i].x},{data.tiles[i].z}]");
                var filter = obj.AddComponent<MeshFilter>();
                var renderer = obj.AddComponent<MeshRenderer>();
                var info = obj.AddComponent<GraphInfo>();
                info.TileId = i;
                info.x = data.tiles[i].x;
                info.z = data.tiles[i].z;
                obj.transform.position = Vector3.zero;

                var mesh = new Mesh();
                mesh.Clear();
                mesh.vertices = data.tiles[i].verts;
                mesh.triangles = data.tiles[i].tris;
                filter.mesh = mesh;
                renderer.material = mat;
                obj.transform.parent = transform;
            }
        }

        /// <summary>
        /// Clear the child objects
        /// </summary>
        public void ClearMesh()
        {
            Transform[] children = new Transform[transform.childCount];

            int i = 0;
            foreach (Transform child in transform)
            {
                children[i] = child;
                i++;
            }

            for (int b = 0; b < children.Length; b++)
            {
                DestroyImmediate(children[b].gameObject);
            }
        }

        /// <summary>
        /// Load the graph tiles into 
        /// </summary>
        /// <returns></returns>
        public GraphData LoadGraph()
        {
            GraphData data = new GraphData();

            var graph = AstarPath.active.data.recastGraph;
            data.tiles = new TileData[graph.tileXCount * graph.tileZCount];


            for (int x = 0; x < graph.tileXCount; x++)
            {
                for (int z = 0; z < graph.tileZCount; z++)
                {
                    Bounds b = graph.GetTileBounds(x, z);
                    var tile = graph.GetTile(x, z);

                    data.tiles[x + graph.tileXCount * z] = new TileData
                    {
                        x = x,
                        z = z,
                        verts = Int3ToVert3(graph, tile.vertsInGraphSpace),
                        tris = tile.tris
                    };

                    var xx = graph.CalculateTransform();

                }
            }

            return data;
        }

        /// <summary>
        /// Convert A* Int3 type array into a Vector3 array
        /// </summary>
        /// <param name="graph">Reference to the recast graph</param>
        /// <param name="int3Verts">Array of Int3 verticies</param>
        /// <returns></returns>
        public static Vector3[] Int3ToVert3(RecastGraph graph, Int3[] int3Verts)
        {

            Vector3[] vectorVerts = new Vector3[int3Verts.Length];
            for (int v = 0; v < int3Verts.Length; v++)
            {
                Vector3 worldPos = graph.transform.Transform((Vector3)int3Verts[v]);
                vectorVerts[v] = worldPos;
            }
            return vectorVerts;
        }

        /// <summary>
        /// Update Recast graph from mesh
        /// </summary>
        /// <param name="info"></param>
        public void MakeGraph(GraphInfo info)
        {

            var graph = AstarPath.active.data.recastGraph;


            AstarPath.active.AddWorkItem((context) =>
            {
                graph.StartBatchTileUpdate();

                var filter = info.transform.GetComponent<MeshFilter>();
                var tile = graph.GetTile(info.x, info.z);
                if (info.x == 0 && info.z == 0)
                {
                    graph.ReplaceTile(info.x, info.z, VectToInt3(graph, filter.sharedMesh.vertices), filter.sharedMesh.triangles);
                }

                graph.EndBatchTileUpdate();
            });

        }

        /// <summary>
        /// Convert array of Vector3 into Int3 
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="verts"></param>
        /// <returns></returns>
        public static Int3[] VectToInt3(RecastGraph graph, Vector3[] verts)
        {
            Int3[] int3Verts = new Int3[verts.Length];
            for (int v = 0; v < verts.Length; v++)
            {
                int3Verts[v] = (Int3)graph.transform.InverseTransform(verts[v]);
            }
            return int3Verts;
        }



    }

    /// <summary>
    /// Class for storing graph tiles
    /// </summary>
    public class GraphData
    {
        public TileData[] tiles;
    }

    /// <summary>
    /// Class for storing graph vertices and triangles
    /// </summary>
    public class TileData
    {
        public int x;
        public int z;

        public Vector3[] verts;
        public int[] tris;
    }
}