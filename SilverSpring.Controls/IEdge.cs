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

namespace SilverSpring.Controls
{
    /// <summary>
    /// An edge between nodes
    /// </summary>
    public interface IEdge
    {
        /// <summary>
        /// Gets or sets whether this edge is directed
        /// </summary>
        bool IsDirected
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the source node associated with the edge
        /// </summary>
        object SourceNode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the destination node associated with the edge
        /// </summary>
        object DestinationNode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the source node associated with the edge
        /// </summary>
        object LastSetSourceNode
        {
            get;
        }

        /// <summary>
        /// Gets or sets the destination node associated with the edge
        /// </summary>
        object LastSetDestinationNode
        {
            get;
        }
    }
}
