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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SilverSpring
{
    /// <summary>
    /// Delegate used to get a key from a node
    /// </summary>
    public delegate object GetKeyFromNode(object node);

    /// <summary>
    /// Delegate used to get the key of the source node of an edge
    /// </summary>
    public delegate object GetSourceNodeKeyFromEdge(object edge);

    /// <summary>
    /// Delegate used to get the key of the destination node of an edge
    /// </summary>
    public delegate object GetDestinationNodeKeyFromEdge(object edge);

    /// <summary>
    /// Delegate used to get the coordinates of nodes
    /// </summary>
    public delegate void SetNodeCoordinates(NodePointList nodeCoordinateData);

    /// <summary>
    /// A force based layout algorithm
    /// </summary>
    public class ForceLayout
    {
        #region Private Constants

        /// <summary>
        /// A default damping applied to node velocity
        /// </summary>
        private const double _defaultDamping = 0.4;

        /// <summary>
        /// Default energy level at which the algorithm will stop
        /// </summary>
        private const double _defaultStoppingKineticEnergyLevel = 0.000005;

        /// <summary>
        /// Default maximum iterations
        /// </summary>
        private const int _defaultMaximumIterations = 500000;

        /// <summary>
        /// Default maximum seconds
        /// </summary>
        private const int _defaultMaximumSeconds = 30;

        /// <summary>
        /// Default repulse constant value
        /// </summary>
        private const double _defaultRepulseConstant = 0.0005;

        /// <summary>
        /// Default spring constant value
        /// </summary>
        private const double _defaultSpringConstant = 1000;

        /// <summary>
        /// Default spring amplifier
        /// </summary>
        private const double _defaultSpringAmplifier = 2;

        /// <summary>
        /// Default spring stable distance
        /// </summary>
        private const double _defaultSpringStableDistance = 300;

        /// <summary>
        /// Default spring multiplier cap
        /// </summary>
        private const double _defaultSpringMultiplierCap = 0.7;

        /// <summary>
        /// X distance move size used by pre-solve
        /// </summary>
        private const double _preSolveXStep = 5;

        /// <summary>
        /// Y distance move size used by pre-solve
        /// </summary>
        private const double _preSolveYStep = 3;

        /// <summary>
        /// Maximum Y value assigned during pre-solve
        /// </summary>
        private const double _preSolveMaxY = 100;

        /// <summary>
        /// small X distance move size used by pre-solve
        /// </summary>
        private const double _preSolveXStepSmall = 2;

        /// <summary>
        /// Default maximum x to assign after node coordinate scaling
        /// </summary>
        private const double _defaultMaximumScaledX = 100;

        /// <summary>
        /// Default maximum y to assign after node coordinate scaling
        /// </summary>
        private const double _defaultMaximumScaledY = 100;

        #endregion

        #region Private Fields

        /// <summary>
        /// Maximum seconds allowed for processing
        /// </summary>
        private int? _maximumSeconds = null;

        /// <summary>
        /// Maximum x value to assign after scaling
        /// </summary>
        private double? _maximumScaledX = null;

        /// <summary>
        /// Maximum y value to assign after scaling
        /// </summary>
        private double? _maximumScaledY = null;

        /// <summary>
        /// The number of iterations between draws
        /// </summary>
        private double? _iterationDrawMiliseconds = 150;

        /// <summary>
        /// Reference to delegate used to get a key from a node
        /// </summary>
        private GetKeyFromNode _getKeyFromNodeDelegate;

        /// <summary>
        /// Reference to delegate used to get the key of the source node of an edge
        /// </summary>
        private GetSourceNodeKeyFromEdge _getSourceNodeKeyFromEdgeDelegate;

        /// <summary>
        /// Reference to delegate used to get the key of the destination node of an edge
        /// </summary>
        private GetDestinationNodeKeyFromEdge _getDestinationNodeKeyFromEdgeDelegate;
        
        /// <summary>
        /// Reference to delegate used to set the coordinates of a node
        /// </summary>
        private SetNodeCoordinates _setNodeCoordinatesDelegate;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the maximum seconds allowed for processing
        /// </summary>
        public int MaximumSeconds
        {
            get
            {
                return _maximumSeconds ?? _defaultMaximumSeconds;
            }
            set
            {
                _maximumSeconds = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum x value to assign after scaling
        /// </summary>
        public double MaximumScaledX
        {
            get
            {
                return _maximumScaledX ?? _defaultMaximumScaledX;
            }
            set
            {
                _maximumScaledX = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum y value to assign after scaling
        /// </summary>
        public double MaximumScaledY
        {
            get
            {
                return _maximumScaledY ?? _defaultMaximumScaledY;
            }
            set
            {
                _maximumScaledY = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor that accepts delegates for required operations
        /// </summary>
        /// <param name="getDestinationNodeKeyFromEdgeDelegate">delegate used to get the key of the destination node of an edge</param>
        /// <param name="getKeyFromNodeDelegate">delegate used to get a key from a node</param>
        /// <param name="getSourceNodeKeyFromEdgeDelegate">delegate used to get the key of the source node of an edge</param>
        /// <param name="setNodeCoordinatesDelegates">delegate used to set the coordinates of a node</param>
        public ForceLayout( GetKeyFromNode getKeyFromNodeDelegate,
                            GetSourceNodeKeyFromEdge getSourceNodeKeyFromEdgeDelegate,
                            GetDestinationNodeKeyFromEdge getDestinationNodeKeyFromEdgeDelegate,
                            SetNodeCoordinates setNodeCoordinatesDelegate)
        {
            _getKeyFromNodeDelegate = getKeyFromNodeDelegate;
            _getSourceNodeKeyFromEdgeDelegate = getSourceNodeKeyFromEdgeDelegate;
            _getDestinationNodeKeyFromEdgeDelegate = getDestinationNodeKeyFromEdgeDelegate;
            _setNodeCoordinatesDelegate = setNodeCoordinatesDelegate;
        }

        #endregion

        /// <summary>
        /// Layout the nodes and endges
        /// </summary>
        /// <param name="nodes">list of nodes to process</param>
        /// <param name="edges">list of edges to process</param>
        public void Layout(IEnumerable nodes,
                            IEnumerable edges
                          )
        {

            Dictionary<object, NodeLayoutData> nodeLayoutDataByKey = new Dictionary<object, NodeLayoutData>();

            // build a dictionary of node layout data by node key
            foreach (object node in nodes)
            {
                object key = _getKeyFromNodeDelegate(node);
                nodeLayoutDataByKey.Add(key, new NodeLayoutData(node));
            }

            foreach (object edge in edges)
            {
                object sourceNodeKey = _getSourceNodeKeyFromEdgeDelegate(edge);
                object destinationNodeKey = _getDestinationNodeKeyFromEdgeDelegate(edge);

                if (sourceNodeKey != null && destinationNodeKey != null)
                {
                    NodeLayoutData sourceNodeLayoutData = null;
                    NodeLayoutData destinationNodeLayoutData = null;

                    nodeLayoutDataByKey.TryGetValue(sourceNodeKey, out sourceNodeLayoutData);
                    nodeLayoutDataByKey.TryGetValue(destinationNodeKey, out destinationNodeLayoutData);

                    if (sourceNodeLayoutData != null && destinationNodeLayoutData != null && sourceNodeLayoutData != destinationNodeLayoutData)
                    {
                        destinationNodeLayoutData.InputNodesLayoutData.Add(sourceNodeLayoutData);
                        sourceNodeLayoutData.NextNodesLayoutData.Add(destinationNodeLayoutData);
                    }
                }
            }

            PreSolve(nodeLayoutDataByKey);

            bool finished = false;

            double damping = _defaultDamping;
            double timestep = 0;
            double minKineticEnergy = _defaultStoppingKineticEnergyLevel;
            
            int maximumIterations = _defaultMaximumIterations;
            int maxElapsedSeconds = MaximumSeconds;

            DateTime startTime = DateTime.Now;
            DateTime lastRenderTime = DateTime.Now;

            while (!finished)
            {
                timestep++;
                double totalKineticEnergy = 0;

                foreach (NodeLayoutData nodeData in nodeLayoutDataByKey.Values)
                {
                    Vector netForceOnNode = new Vector(0, 0);

                    foreach (NodeLayoutData otherNodeData in nodeLayoutDataByKey.Values)
                    {
                        if (nodeData != otherNodeData)
                        {
                            Vector repulsiveForceBetweenNodes = CalculateRepulsiveForceBetweeNodes(nodeData, otherNodeData);
                            netForceOnNode.Add(repulsiveForceBetweenNodes);
                        }
                    }

                    foreach(NodeLayoutData inputNodeData in nodeData.InputNodesLayoutData)
                    {
                        Vector attractiveForceBetweenNodes = CalculateAttractiveForceBetweenNodes(nodeData, inputNodeData);
                        netForceOnNode.Add(attractiveForceBetweenNodes);
                    }

                    nodeData.Velocity.AddWithMultiplier(netForceOnNode, 1);
                    nodeData.Velocity.ApplyMultiplier(damping);

                    nodeData.Coordinates.X = nodeData.Coordinates.X + nodeData.Velocity.Dx;
                    nodeData.Coordinates.Y = nodeData.Coordinates.Y + nodeData.Velocity.Dy;

                    totalKineticEnergy = totalKineticEnergy + Math.Pow((Math.Abs(nodeData.Velocity.Dx) + Math.Abs(nodeData.Velocity.Dy)) / 2.0,2);
                }

                double elapsedSeconds = DateTime.Now.Subtract(startTime).TotalSeconds;

                double elapsedMilisecondsSinceLastRender = DateTime.Now.Subtract(lastRenderTime).TotalMilliseconds;

                if (elapsedMilisecondsSinceLastRender > _iterationDrawMiliseconds)
                {
                    SetNodeCoordinates(nodeLayoutDataByKey.Values);
                   
                    lastRenderTime = DateTime.Now;
                }

                if (timestep >= maximumIterations || totalKineticEnergy < minKineticEnergy || elapsedSeconds > maxElapsedSeconds)
                {
                    finished = true;
                }
            }

            SetNodeCoordinates(nodeLayoutDataByKey.Values);
        }

        #region Private Methods

        /// <summary>
        /// Set node coordinates on all nodes
        /// </summary>
        /// <param name="nodeLayoutDataList">list of nodes to set coordinates on</param>
        private void SetNodeCoordinates(IEnumerable<NodeLayoutData> nodeLayoutDataList)
        {
            double? minX = 0;
            double? maxX = 0;
            double? minY = 0;
            double? maxY = 0;

            NodePointList nodeCoordinateData = new NodePointList();

            foreach (NodeLayoutData nodeData in nodeLayoutDataList)
            {
                if (minX == null || minX > nodeData.Coordinates.X)
                {
                    minX = nodeData.Coordinates.X;
                }

                if (maxX == null || maxX < nodeData.Coordinates.X)
                {
                    maxX = nodeData.Coordinates.X;
                }

                if (minY == null || minY > nodeData.Coordinates.Y)
                {
                    minY = nodeData.Coordinates.Y;
                }

                if (maxY == null || maxY < nodeData.Coordinates.Y)
                {
                    maxY = nodeData.Coordinates.Y;
                }
            }

            if (minX != null && maxX != null && minY != null && maxX != null)
            {
                double xRange = maxX.Value - minX.Value;
                double yRange = maxY.Value - minY.Value;

                foreach (NodeLayoutData nodeData in nodeLayoutDataList)
                {
                    double x = ((nodeData.Coordinates.X - minX.Value) / xRange) * MaximumScaledX;
                    double y = ((nodeData.Coordinates.Y - minY.Value) / yRange) * MaximumScaledY;

                    nodeCoordinateData.Add(new NodePoint() { Node = nodeData.Node, Coordinate = new Point(x, y) });
                }
            }

            _setNodeCoordinatesDelegate(nodeCoordinateData);
        }

        /// <summary>
        /// Calculate the repulsive force between nodes
        /// </summary>
        /// <param name="first">the first node</param>
        /// <param name="second">the second node</param>
        /// <returns>A vector representing a repulsive first</returns>
        private Vector CalculateRepulsiveForceBetweeNodes(NodeLayoutData first, NodeLayoutData second)
        {
            double repulseConstant = _defaultRepulseConstant;
            double dx = first.Coordinates.X - second.Coordinates.X;
            double dy = first.Coordinates.Y - second.Coordinates.Y;
            
            double distance = Math.Sqrt((Math.Pow(dx, 2) + Math.Pow(dy, 2)));

            double repulseMultiplier = 0;

            if (distance != 0)
            {
                repulseMultiplier = repulseConstant / distance;
            }

            return new Vector(repulseMultiplier * dx, repulseMultiplier * dy);
        }

        /// <summary>
        /// Calculate the attractive force between nodes
        /// </summary>
        /// <param name="first">the first node</param>
        /// <param name="second">the second node</param>
        /// <returns>attractive force between nodes</returns>
        private Vector CalculateAttractiveForceBetweenNodes(NodeLayoutData first, NodeLayoutData second)
        {
            double springConstant = _defaultSpringConstant;
            double amplifier = _defaultSpringAmplifier;
            double springStableDistance = _defaultSpringStableDistance;

            double dx = first.Coordinates.X - second.Coordinates.X;
            double dy = first.Coordinates.Y - second.Coordinates.Y;
        
            double distance = Math.Sqrt((Math.Pow(dx, 2) + Math.Pow(dy, 2)));

            double springMultiplier = 0;

            double distanceFromStablePoint = Math.Abs(distance - springStableDistance);
            int directionModifier = -1;

            if (distance < springStableDistance)
            {
                directionModifier = 1;
            }

            springMultiplier = (distanceFromStablePoint / springConstant);

            if (springMultiplier > _defaultSpringMultiplierCap)
            {
                springMultiplier = _defaultSpringMultiplierCap;
            }
            springMultiplier = springMultiplier * directionModifier * amplifier;
            
            return new Vector(springMultiplier * dx, springMultiplier * dy);
        }
        
        /// <summary>
        /// Increment the sort score of a nodes destinations
        /// </summary>
        /// <param name="nodeData">the current node</param>
        /// <param name="visitedNodes">a list of nodes already visited, important to prevent circular processing</param>
        private void AddSortScoreToDestinations(NodeLayoutData nodeData, List<NodeLayoutData> visitedNodes)
        {
            visitedNodes.Add(nodeData);

            if (nodeData.NextNodesLayoutData != null)
            {
                foreach (NodeLayoutData nextNode in nodeData.NextNodesLayoutData)
                {
                    nextNode.SortScore = nextNode.SortScore + 1;
                    
                    if (!visitedNodes.Contains(nextNode))
                    {
                        AddSortScoreToDestinations(nextNode, visitedNodes);
                    }
                }
            }
        }

        /// <summary>
        /// Perform a pre-solve run
        /// </summary>
        private void PreSolve(Dictionary<object, NodeLayoutData> nodeLayoutDataByKey)
        {
            foreach (NodeLayoutData nodeData in nodeLayoutDataByKey.Values)
            {
                nodeData.SortScore = 1;
            }

            foreach (NodeLayoutData nodeData in nodeLayoutDataByKey.Values)
            {
                AddSortScoreToDestinations(nodeData, new List<NodeLayoutData>());
            }

            List<NodeLayoutData> sorted = nodeLayoutDataByKey.Values.OrderBy(nd => nd.SortScore).ToList();

            double currentX = _preSolveXStepSmall;
            double currentY = 0;

            double lastSortScore = 1;

            foreach (NodeLayoutData nodeData in sorted)
            {
                if (nodeData.SortScore == lastSortScore)
                {
                    currentY = (currentY + _preSolveYStep) % _preSolveMaxY;
                    currentX = currentX + _preSolveXStepSmall;
                }
                else
                {
                    currentX = currentX + _preSolveXStep;
                    if (currentY > _preSolveYStep)
                    {
                        currentY = currentY - _preSolveYStep;
                    }
                }

                nodeData.Coordinates.X = currentX;
                nodeData.Coordinates.Y = currentY;
            }
        }

        /// <summary>
        /// Scale coordinates within the solution
        /// </summary>
        private void ScaleSolution(Dictionary<object, NodeLayoutData> nodeLayoutDataByKey)
        {
            double? minX = 0;
            double? maxX = 0;
            double? minY = 0;
            double? maxY = 0;

            foreach (NodeLayoutData nodeData in nodeLayoutDataByKey.Values)
            {
                if (minX == null || minX > nodeData.Coordinates.X)
                {
                    minX = nodeData.Coordinates.X;
                }

                if (maxX == null || maxX < nodeData.Coordinates.X)
                {
                    maxX = nodeData.Coordinates.X;
                }

                if (minY == null || minY > nodeData.Coordinates.Y)
                {
                    minY = nodeData.Coordinates.Y;
                }

                if (maxY == null || maxY < nodeData.Coordinates.Y)
                {
                    maxY = nodeData.Coordinates.Y;
                }
            }

            if (minX != null && maxX != null && minY != null && maxX != null)
            {
                double xRange = maxX.Value - minX.Value;
                double yRange = maxY.Value - minY.Value;

                foreach (NodeLayoutData nodeData in nodeLayoutDataByKey.Values)
                {
                    nodeData.Coordinates.X = ((nodeData.Coordinates.X - minX.Value) / xRange) * MaximumScaledX;
                    nodeData.Coordinates.Y = ((nodeData.Coordinates.Y - minY.Value) / yRange) * MaximumScaledY;
                }
            }
        }

        #endregion
    }
}
