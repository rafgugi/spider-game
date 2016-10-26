using ShapeGame.Utils;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ShapeGame
{

    // The Thing struct represents a single object that is flying through the air, and
    // all of its properties.
    public class Spider
    {
        public System.Windows.Point Center;
        public double Size;
        public double Theta;             // sekarang lagi dirotasi sejauh brp
        public double SpinRate;          // perubahan theta per frame
        public double YVelocity;
        public double XVelocity;
        public PolyType Shape;
        public System.Windows.Media.Color Color;
        public System.Windows.Media.Brush Brush;
        public System.Windows.Media.Brush Brush2;
        public System.Windows.Media.Brush BrushPulse;
        public double Dissolve;
        public FallingThings.ThingState State;
        public DateTime TimeLastHit;
        public double AvgTimeBetweenHits;
        public int TouchedBy;               // Last player to touch this thing
        public int Hotness;                 // Score level
        public int FlashCount;

        // Hit testing between this thing and a single segment.  If hit, the center point on
        // the segment being hit is returned, along with the spot on the line from 0 to 1 if
        // a line segment was hit.
        public bool Hit(Segment seg, ref System.Windows.Point hitCenter, ref double lineHitLocation)
        {
            double minDxSquared = this.Size + seg.Radius;
            minDxSquared *= minDxSquared;

            // See if falling thing hit this body segment
            if (seg.IsCircle())
            {
                if (Helper.SquaredDistance(this.Center.X, this.Center.Y, seg.X1, seg.Y1) <= minDxSquared)
                {
                    hitCenter.X = seg.X1;
                    hitCenter.Y = seg.Y1;
                    lineHitLocation = 0;
                    return true;
                }
            }
            else
            {
                double sqrLineSize = Helper.SquaredDistance(seg.X1, seg.Y1, seg.X2, seg.Y2);
                if (sqrLineSize < 0.5)
                {
                    // if less than 1/2 pixel apart, just check dx to an endpoint
                    //return SquaredDistance(this.Center.X, this.Center.Y, seg.X1, seg.Y1) < minDxSquared;
                    if (Helper.SquaredDistance(this.Center.X, this.Center.Y, seg.X1, seg.Y1) <= minDxSquared)
                    {
                        hitCenter.X = seg.X1;
                        hitCenter.Y = seg.Y1;
                        lineHitLocation = 0;
                        return true;
                    }
                    else
                        return false;
                }

                // Find dx from center to line
                double u = ((this.Center.X - seg.X1) * (seg.X2 - seg.X1) + (this.Center.Y - seg.Y1) * (seg.Y2 - seg.Y1)) / sqrLineSize;
                if ((u >= 0) && (u <= 1.0))
                {   // Tangent within line endpoints, see if we're close enough
                    double intersectX = seg.X1 + ((seg.X2 - seg.X1) * u);
                    double intersectY = seg.Y1 + ((seg.Y2 - seg.Y1) * u);

                    if (Helper.SquaredDistance(this.Center.X, this.Center.Y, intersectX, intersectY) < minDxSquared)
                    {
                        lineHitLocation = u;
                        hitCenter.X = intersectX;
                        hitCenter.Y = intersectY;
                        return true;
                    }
                }
                else
                {
                    // See how close we are to an endpoint
                    if (u < 0)
                    {
                        if (Helper.SquaredDistance(this.Center.X, this.Center.Y, seg.X1, seg.Y1) < minDxSquared)
                        {
                            lineHitLocation = 0;
                            hitCenter.X = seg.X1;
                            hitCenter.Y = seg.Y1;
                            return true;
                        }
                    }
                    else
                    {
                        if (Helper.SquaredDistance(this.Center.X, this.Center.Y, seg.X2, seg.Y2) < minDxSquared)
                        {
                            lineHitLocation = 1;
                            hitCenter.X = seg.X2;
                            hitCenter.Y = seg.Y2;
                            return true;
                        }
                    }
                }

                return false;
            }

            return false;
        }

        // Change our velocity based on the object's velocity, our velocity, and where we hit.
        public void BounceOff(double x1, double y1, double otherSize, double fXv, double fYv)
        {
            double x0 = this.Center.X;
            double y0 = this.Center.Y;
            double xv0 = this.XVelocity - fXv;
            double yv0 = this.YVelocity - fYv;
            double dist = otherSize + this.Size;
            double dx = Math.Sqrt(((x1 - x0) * (x1 - x0)) + ((y1 - y0) * (y1 - y0)));
            double xdif = x1 - x0;
            double ydif = y1 - y0;
            double newvx1 = 0;
            double newvy1 = 0;

            x0 = x1 - (xdif / dx * dist);
            y0 = y1 - (ydif / dx * dist);
            xdif = x1 - x0;
            ydif = y1 - y0;

            double bsq = dist * dist;
            double b = dist;
            double asq = (xv0 * xv0) + (yv0 * yv0);
            double a = Math.Sqrt(asq);
            if (a > 0.000001)
            {
                // if moving much at all...
                double cx = x0 + xv0;
                double cy = y0 + yv0;
                double csq = ((x1 - cx) * (x1 - cx)) + ((y1 - cy) * (y1 - cy));
                double tt = asq + bsq - csq;
                double bb = 2 * a * b;
                double power = a * (tt / bb);
                newvx1 -= 2 * (xdif / dist * power);
                newvy1 -= 2 * (ydif / dist * power);
            }

            this.XVelocity += newvx1;
            this.YVelocity += newvy1;
            this.Center.X = x0;
            this.Center.Y = y0;
        }

        public void Draw(UIElementCollection children)
        {
            // init bruh if not yet defined
            if (this.Brush == null)
            {
                this.Brush = new SolidColorBrush(this.Color);
                double factor = 0.4 + (((double)this.Color.R + this.Color.G + this.Color.B) / 1600);
                this.Brush2 =
                    new SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            (byte)(255 - ((255 - this.Color.R) * factor)),
                            (byte)(255 - ((255 - this.Color.G) * factor)),
                            (byte)(255 - ((255 - this.Color.B) * factor))));
                this.BrushPulse = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            }

            // Disolving gradually
            if (this.State == FallingThings.ThingState.Dissolving)
            {
                this.Brush.Opacity = 1.0 - (this.Dissolve * this.Dissolve);
            }

            double size = this.Size;
            double spin = this.Theta;
            System.Windows.Point center = this.Center;
            Brush brush = this.Brush;
            Brush brushStroke = (this.State == FallingThings.ThingState.Dissolving) ? null : this.Brush2;
            double strokeThickness = 1;
            double opacity = 1;

            Shape circle = new Ellipse { Width = size * 2, Height = size * 2, Stroke = brushStroke };

            if (circle.Stroke != null)
            {
                circle.Stroke.Opacity = opacity;
            }

            circle.StrokeThickness = strokeThickness;
            circle.Fill = brush;
            // locate the object
            circle.SetValue(Canvas.LeftProperty, center.X - size);
            circle.SetValue(Canvas.TopProperty, center.Y - size);

            children.Add(circle);

            // create spiders head
            Shape head = new Ellipse { Width = size, Height = size, Stroke = brushStroke };

            if (head.Stroke != null)
            {
                head.Stroke.Opacity = opacity;
            }

            head.StrokeThickness = strokeThickness;
            head.Fill = brush;
            // locate the object
            double vx = this.XVelocity;
            double vy = this.YVelocity;
            double resultan = Math.Sqrt(vx * vx + vy * vy);
            head.SetValue(Canvas.LeftProperty, center.X + size * vx / resultan - size / 2);
            head.SetValue(Canvas.TopProperty, center.Y + size * vy / resultan - size / 2);

            children.Add(head);
        }

    }
}
