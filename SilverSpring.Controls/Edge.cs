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

namespace SilverSpring.Controls
{
    /// <summary>
    /// An edge between 2 nodes
    /// </summary>
    public class Edge : Canvas, IEdge
    {
        /// <summary>
        /// Property used to specify whether the edge is directed or not
        /// </summary>
        public static DependencyProperty IsDirectedProperty = DependencyProperty.Register("IsDirected",
                                                                typeof(bool),
                                                                typeof(Edge),
                                                                new PropertyMetadata(true));

        /// <summary>
        /// Property used to specify the source of the edge
        /// </summary>
        public static DependencyProperty SourceNodeProperty = DependencyProperty.Register("SourceNode",
                                                                typeof(object),
                                                                typeof(Edge),
                                                                new PropertyMetadata(null, new PropertyChangedCallback(OnSourceNodeChanged)));

        /// <summary>
        /// Property used to specify the destination of the edge
        /// </summary>
        public static DependencyProperty DestinationNodeProperty = DependencyProperty.Register("DestinationNode",
                                                                typeof(object),
                                                                typeof(Edge),
                                                                new PropertyMetadata(null, new PropertyChangedCallback(OnDestinationNodeChanged)));

        
        #region Private Fields

        private object _lastSetDestinationNode;
        private object _lastSetSourceNode;
        private Path _line;
        private Path _lineEnd;

        #endregion

        /// <summary>
        /// Handler fired when source is changed
        /// </summary>
        private static void OnSourceNodeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Edge senderAsEdge = sender as Edge;

            if (senderAsEdge != null)
            {
                senderAsEdge._lastSetSourceNode = e.NewValue;
            }

            if (e.OldValue != null && e.OldValue is FrameworkElement)
            {
                ((FrameworkElement)e.OldValue).LayoutUpdated -= new EventHandler(senderAsEdge.node_LayoutUpdated);
            }

            if (e.NewValue != null)
            {
                ((FrameworkElement)e.NewValue).LayoutUpdated += new EventHandler(senderAsEdge.node_LayoutUpdated);
            }
        }

        /// <summary>
        /// Handler fired when destination is changed
        /// </summary>
        private static void OnDestinationNodeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Edge senderAsEdge = sender as Edge;

            if (senderAsEdge != null)
            {
                senderAsEdge._lastSetDestinationNode = e.NewValue;
            }

            if (e.OldValue != null && e.OldValue is FrameworkElement)
            {
                ((FrameworkElement)e.OldValue).LayoutUpdated -= new EventHandler(senderAsEdge.node_LayoutUpdated);
            }

            if (e.NewValue != null)
            {
                ((FrameworkElement)e.NewValue).LayoutUpdated += new EventHandler(senderAsEdge.node_LayoutUpdated);
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Edge()
        {
            Loaded += new RoutedEventHandler(Edge_Loaded);
        }
        
        /// <summary>
        /// Handler for load event
        /// </summary>
        private void Edge_Loaded(object sender, RoutedEventArgs e)
        {
            _line = new Path() { Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 1 };
            _lineEnd = new Path() { Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 1 };

            this.Children.Add(_line);
            this.Children.Add(_lineEnd);
        }

        /// <summary>
        /// Handle layout update of source or destination node
        /// </summary>
        private void node_LayoutUpdated(object sender, EventArgs e)
        {
            UpdateCoordinates();
        }

        /// <summary>
        /// Update the coordinates
        /// </summary>
        public void UpdateCoordinates()
        {
            if (SourceNode != null && DestinationNode != null)
            {
                Point lineSource;
                Point lineDestination;

                GetLinePoints(out lineSource, out lineDestination);

                // draw the main line
                PathFigure pathFigure = new PathFigure();
                pathFigure.StartPoint = lineSource;
                LineSegment lineSegment1 = new LineSegment();
                lineSegment1.Point = lineDestination;
                pathFigure.Segments.Add(lineSegment1);
                PathGeometry pathGeometry = new PathGeometry();
                pathGeometry.Figures = new PathFigureCollection();
                pathGeometry.Figures.Add(pathFigure);

                _line.Data = pathGeometry;

                // this section courtesy of stack overflow (http://stackoverflow.com/questions/1563285/how-to-draw-an-arrow-in-silverlight)
                double theta = Math.Atan2((lineDestination.Y - lineSource.Y), (lineDestination.X - lineSource.X)) * 180 / Math.PI;
                PathGeometry lineEndGeometry = new PathGeometry();
                
                Point lpoint = new Point(lineDestination.X + 2, lineDestination.Y - 10);
                Point rpoint = new Point(lineDestination.X - 2, lineDestination.Y - 10);

                PathFigure lineEndFigure = new PathFigure();
                lineEndFigure.StartPoint = lpoint;
                
                LineSegment seg1 = new LineSegment();
                seg1.Point = lineDestination;
                lineEndFigure.Segments.Add(seg1);
                LineSegment seg2 = new LineSegment();
                seg2.Point = rpoint;
                lineEndFigure.Segments.Add(seg2);
                lineEndGeometry.Figures.Add(lineEndFigure);
                RotateTransform transform = new RotateTransform();
                transform.Angle = theta - 90;
                transform.CenterX = lineDestination.X;
                transform.CenterY = lineDestination.Y;
                lineEndGeometry.Transform = transform;

                _lineEnd.Data = lineEndGeometry;
                _lineEnd.Visibility = (IsDirected) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private List<Point> GetNodeConnectionPoints(FrameworkElement node)
        {
            double x = 0;
            double y = 0;
            double width = node.ActualWidth;
            double height = node.ActualHeight;

            object xObj = node.GetValue(Canvas.LeftProperty);
            if (xObj != null)
            {
                x = (double)xObj;
            }

            object yObj = node.GetValue(Canvas.TopProperty);
            if (yObj != null)
            {
                y = (double)yObj;
            }

            List<Point> points = new List<Point>();

            points.Add(new Point(x, y + height / 3));
            points.Add(new Point(x, y + height / 2));
            points.Add(new Point(x, y + (height / 3) * 2));

            points.Add(new Point(x + width, y + height / 3));
            points.Add(new Point(x + width, y + height / 2));
            points.Add(new Point(x + width, y + (height / 3) * 2));

            points.Add(new Point(x + width / 3, y));
            points.Add(new Point(x + width / 2, y));
            points.Add(new Point(x + (width / 3) * 2, y));

            points.Add(new Point(x + width / 3, y + height));
            points.Add(new Point(x + width / 2, y + height));
            points.Add(new Point(x + (width / 3) * 2, y + height));

            return points;
        }

        private void GetLinePoints(out Point lineSource, out Point lineDestination)
        {
            lineSource = new Point();
            lineDestination = new Point();

            FrameworkElement sourceNodeAsFrameworkElement = SourceNode as FrameworkElement;
            FrameworkElement destinationNodeAsFrameworkElement = DestinationNode as FrameworkElement;

            if (sourceNodeAsFrameworkElement != null && destinationNodeAsFrameworkElement != null)
            {

                List<Point> sourcePoints = GetNodeConnectionPoints(sourceNodeAsFrameworkElement);
                List<Point> destinationPoints = GetNodeConnectionPoints(destinationNodeAsFrameworkElement);

                Point selectedSourcePoint = sourcePoints[0];
                Point selectedDestinationPoint = destinationPoints[0];
                double minDistance = double.MaxValue;

                foreach (Point sourcePoint in sourcePoints)
                {
                    foreach (Point destinationPoint in destinationPoints)
                    {
                        double distance = CalculateDistanceBetweenPoints(sourcePoint, destinationPoint);

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            selectedSourcePoint = sourcePoint;
                            selectedDestinationPoint = destinationPoint;
                        }
                    }
                }

                lineSource = selectedSourcePoint;
                lineDestination = selectedDestinationPoint;
            }
        }

        private double CalculateDistanceBetweenPoints(Point sourcePoint, Point destinationPoint)
        {
            double dx = Math.Abs(sourcePoint.X - destinationPoint.X);
            double dy = Math.Abs(sourcePoint.Y - destinationPoint.Y);

            double dist = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

            return dist;
        }

        /// <summary>
        /// Gets the last destination node
        /// </summary>
        /// <remarks>This is done without accessing dependency object GetValue to avoid cross-threading issues</remarks>
        public object LastSetDestinationNode
        {
            get
            {
                return _lastSetDestinationNode;
            }
        }

        /// <summary>
        /// Gets the last destination node
        /// </summary>
        /// <remarks>This is done without accessing dependency object GetValue to avoid cross-threading issues</remarks>
        public object LastSetSourceNode
        {
            get
            {
                return _lastSetSourceNode;
            }
        }

        /// <summary>
        /// Gets or sets whether this edge is directed
        /// </summary>
        public bool IsDirected
        {
            get
            {
                return (bool)GetValue(IsDirectedProperty);
            }
            set
            {
                SetValue(IsDirectedProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the source node of this edge
        /// </summary>
        public object SourceNode
        {
            get
            {
                return GetValue(SourceNodeProperty);
            }
            set
            {
                SetValue(SourceNodeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the destination node of this edge
        /// </summary>
        public object DestinationNode
        {
            get
            {
                return GetValue(DestinationNodeProperty);
            }
            set
            {
                SetValue(DestinationNodeProperty, value);
            }
        }
    }
}
