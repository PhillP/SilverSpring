using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace SilverSpring
{
    /// <summary>
    /// A set of layout data for a node
    /// </summary>
    /// <remarks>This data is required during the layout process</remarks>
    public class NodeLayoutData
    {
        /// <summary>
        /// The node this data relates to
        /// </summary>
        public object Node
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a score used for sorting nodes
        /// </summary>
        public double SortScore
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of nodes that have links into this node
        /// </summary>
        public List<NodeLayoutData> InputNodesLayoutData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of nodes that this node links to
        /// </summary>
        public List<NodeLayoutData> NextNodesLayoutData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a vector that represents a velocity
        /// </summary>
        public Vector Velocity
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the coordinates for this node
        /// </summary>
        public ChangeablePoint Coordinates
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor that accepts the node the layout data is related to
        /// </summary>
        /// <param name="node"></param>
        public NodeLayoutData(object node)
        {
            Node = node;
            InputNodesLayoutData = new List<NodeLayoutData>();
            NextNodesLayoutData = new List<NodeLayoutData>();
            Velocity = new Vector(0, 0);
            Coordinates = new ChangeablePoint();
        }
    }
}
