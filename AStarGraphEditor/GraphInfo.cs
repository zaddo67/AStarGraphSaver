using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools.AStarGraphEditor
{
    public class GraphInfo : MonoBehaviour
    {
        public int TileId;
        public int x;
        public int z;

        /// <summary>
        /// Update Recast graph with this components mesh
        /// </summary>
        public void MakeGraph()
        {
            transform.parent.GetComponent<GraphToMesh>().MakeGraph(this);
        }
    }
}
