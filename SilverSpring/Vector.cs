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

namespace SilverSpring
{
    /// <summary>
    /// Represents a vector
    /// </summary>
    public class Vector
    {
        /// <summary>
        /// Gets or sets the distance in the x direction
        /// </summary>
        public double Dx
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the distance in the y direction
        /// </summary>
        public double Dy
        {
            get;
            set;
        }

        /// <summary>
        /// Add another vector to this vector
        /// </summary>
        /// <param name="other">other vector to add</param>
        public void Add(Vector other)
        {
            Dx = Dx + other.Dx;
            Dy = Dy + other.Dy;
        }

        /// <summary>
        /// Apply a multiplier to the vector
        /// </summary>
        /// <param name="multiplier">multiplier to be applied</param>
        public void ApplyMultiplier(double multiplier)
        {
            Dx = Dx * multiplier;
            Dy = Dy * multiplier;
        }

        /// <summary>
        /// Add another vector with a multiplier
        /// </summary>
        /// <param name="other">other vector to add</param>
        /// <param name="multiplier">multiplier to apply during add</param>
        public void AddWithMultiplier(Vector other, double multiplier)
        {
            Dx = Dx + other.Dx * multiplier;
            Dy = Dy + other.Dy * multiplier;
        }

        /// <summary>
        /// Constructor that accepts Vector x and y distances
        /// </summary>
        /// <param name="dx">distance in the x direction</param>
        /// <param name="dy">distance in the y direction</param>
        public Vector(double dx, double dy)
        {
            Dx = dx;
            Dy = dy;
        }
    }
}
