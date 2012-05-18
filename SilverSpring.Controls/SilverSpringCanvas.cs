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
using System.Linq;
using System.ComponentModel;
using System.Collections;

namespace SilverSpring.Controls
{
    /// <summary>
    /// A canvas that incorporates spring layout functionality
    /// </summary>
    public class SilverSpringCanvas : Canvas
    {
        private BackgroundWorker _worker = null;
        private bool _messageShown = false;
        /// <summary>
        /// Run the layout algorithm on this control
        /// </summary>
        public void RunLayoutAlgorithm()
        {
            ForceLayout layout = new ForceLayout(GetKeyFromNode, 
                                                 GetSourceNodeKeyFromEdge, 
                                                 GetDestinationNodeKeyFromEdge, 
                                                 SetNodeCoordinates);

            layout.MaximumScaledX = this.ActualWidth * .95;
            layout.MaximumScaledY = this.ActualHeight * .95;
            layout.MaximumSeconds = 120;
           
            IEnumerable nodes = this.Children.Where(ch => !(ch is IEdge)).ToList();
            IEnumerable edges = this.Children.Where(ch => ch is IEdge).ToList();

            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.DoWork += (w, data) =>
            {
                // Run the layout algorithm
                // every child that is not an edge is considered a node
                layout.Layout(nodes,
                              edges);
            };

            _worker.ProgressChanged += new ProgressChangedEventHandler(_worker_ProgressChanged);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);
            _worker.RunWorkerAsync();
        }

        void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                MessageBox.Show(e.Error.StackTrace);
            }
        }

        #region Private Methods

        /// <summary>
        /// Handle progress changed event, update node coordinates
        /// </summary>
        private void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            NodePointList nodeCoordinateData = e.UserState as NodePointList;

            if (nodeCoordinateData != null)
            {
                foreach (NodePoint nodePoint in nodeCoordinateData)
                {
                    FrameworkElement nodeAsFrameworkElement = nodePoint.Node as FrameworkElement;

                    if (nodeAsFrameworkElement != null)
                    {
                        try
                        {

                            if (nodePoint.Coordinate.X >= 0 && nodePoint.Coordinate.X <= this.ActualWidth && !double.IsNaN(nodePoint.Coordinate.X))
                            {
                                nodeAsFrameworkElement.SetValue(Canvas.LeftProperty, nodePoint.Coordinate.X);
                            }

                            if (nodePoint.Coordinate.Y >= 0 && nodePoint.Coordinate.Y <= this.ActualHeight && !double.IsNaN(nodePoint.Coordinate.Y))
                            {
                                nodeAsFrameworkElement.SetValue(Canvas.TopProperty, nodePoint.Coordinate.Y);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!_messageShown)
                            {
                                _messageShown = true;
                                MessageBox.Show(ex.Message);
                                MessageBox.Show(nodePoint.Coordinate.X.ToString());
                                MessageBox.Show(nodePoint.Coordinate.Y.ToString());
                            }

                        }
                    }
                }
            }

            IEnumerable edges = this.Children.Where(ch => ch is Edge).ToList();

            foreach (Edge edge in edges)
            {
                edge.UpdateCoordinates();
            }
        }
                
        /// <summary>
        /// Delegate used to get a key from a node
        /// </summary>
        private object GetKeyFromNode(object node)
        {
            // use the node itself as the key
            return node;
        }

        /// <summary>
        /// Delegate used to get the key of the source node of an edge
        /// </summary>
        private object GetSourceNodeKeyFromEdge(object edge)
        {
            object nodeKey = null;

            if (edge is IEdge)
            {
                nodeKey = ((IEdge)edge).LastSetSourceNode;
            }

            return nodeKey;
        }

        /// <summary>
        /// Delegate used to get the key of the destination node of an edge
        /// </summary>
        private object GetDestinationNodeKeyFromEdge(object edge)
        {
            object nodeKey = null;

            if (edge is IEdge)
            {
                nodeKey = ((IEdge)edge).LastSetDestinationNode;
            }

            return nodeKey;
        }

        /// <summary>
        /// Delegate used to get the coordinates of a nodes
        /// </summary>
        private void SetNodeCoordinates(NodePointList nodeCoordinateData)
        {
            _worker.ReportProgress(0, nodeCoordinateData);
        }

        #endregion
    }
}
