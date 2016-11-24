using ShapeGame.Utils;
using System;
using System.Windows;
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
        public ThingState State;
        public DateTime TimeLastHit;
        public double AvgTimeBetweenHits;
        public int TouchedBy;               // Last player to touch this thing
        public int Hotness;                 // Score level
        public int FlashCount;
        public Player attachedPlayer;
        public Bone attachedTo;
        public double attachedAt;

        /**
         * Pengecekan apakah spider menabrak segment tubuh. Jika iya, maka
         * hitCenter akan menyimpan titik koordinat segment mengenai spider, dan
         * lineHitLocation akan menyimpan posisi di segment dengan skala 0-1.
         */
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

        /* Change our velocity based on the object's velocity, our velocity, and where we hit.
         *   x1, y1: titik temu benda dengan segment
         *   otherSize: segment radius
         *   fXv, fYv: segment velocity
         */
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

        public Path makelegandhand(Point[] Direction, Brush brush)
        {
            Path temp = new Path();
            temp.Stroke = brush;
            temp.StrokeThickness = 2;

            PathFigure myPathFigure = new PathFigure();
            myPathFigure.StartPoint = Direction[0];

            BezierSegment Bezier = new BezierSegment();
            Bezier.Point1 = Direction[1];
            Bezier.Point2 = Direction[2];
            Bezier.Point3 = Direction[3];

            PathSegmentCollection mySegCollect = new PathSegmentCollection();
            mySegCollect.Add(Bezier);
            myPathFigure.Segments = mySegCollect;
            PathFigureCollection myPathFigCollection = new PathFigureCollection();
            myPathFigCollection.Add(myPathFigure);
            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures = myPathFigCollection;
            temp.Data = myPathGeometry;

            return temp;
        }

        public Shape makeheadandeye(double[] Direction, Brush brush, Brush brushStroke, double size)
        {
            double strokeThickness = 1;
            double opacity = 1;

            Shape temp = new Ellipse { Width = size, Height = size, Stroke = brushStroke };

            if (temp.Stroke != null)
            {
                temp.Stroke.Opacity = opacity;
            }

            temp.StrokeThickness = strokeThickness;
            temp.Fill = brush;

            temp.SetValue(Canvas.LeftProperty, Direction[0]);
            temp.SetValue(Canvas.TopProperty, Direction[1]);

            return temp;
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
            if (this.State == ThingState.Dissolving)
            {
                this.Brush.Opacity = 1.0 - (this.Dissolve * this.Dissolve);
            }

            double size = this.Size * 2;
            double spin = this.Theta;
            System.Windows.Point center = this.Center;
            double vx = 0;
            double vy = 0;
            double resultan = 1;

            Brush brush = this.Brush;
            Brush brush_white = new SolidColorBrush(System.Windows.Media.Color.FromRgb(248,248,255));
            Brush brushStroke = (this.State == ThingState.Dissolving) ? null : this.Brush2;

            // create spiders head
            double[] direction = { center.X + size * vx / resultan - size / 2, center.Y + size * vy / resultan - size / 2};
            Shape head = makeheadandeye(direction, brush, brushStroke, size);
            children.Add(head);

            //create left cornea
            direction = new double[] { center.X + size * vx / resultan - size * 0.32, center.Y + size * vy / resultan - size * 0.2 };
            Shape l_eye = makeheadandeye(direction, brush_white, brush_white, size*0.35);
            children.Add(l_eye);

            //create right cornea
            direction = new double[] { center.X + size * vx / resultan, center.Y + size * vy / resultan - size * 0.2 };
            Shape r_eye = makeheadandeye(direction, brush_white, brush_white, size * 0.35);
            children.Add(r_eye);

            //create left eyeball
            direction = new double[] { center.X + size * vx / resultan - size * 0.25, center.Y + size * vy / resultan - size * 0.15 };
            Shape l_eyeball = makeheadandeye(direction, brush, brushStroke, size*0.13);
            children.Add(l_eyeball);

            //create right eyeball
            direction = new double[] { center.X + size * vx / resultan + size * 0.10, center.Y + size * vy / resultan - size * 0.15 };
            Shape r_eyeball = makeheadandeye(direction, brush, brushStroke, size * 0.13);
            children.Add(r_eyeball);

            //create left hand
            Point[] lh = new Point[4];
            lh[0] = new Point(center.X + size * vx / resultan - size * 0.4, center.Y + size * vy / resultan + size * 0.3);
            lh[1] = new Point(center.X + size * vx / resultan - size * 0.6, center.Y + size * vy / resultan + size * 0.6);
            lh[2] = new Point(center.X + size * vx / resultan - size * 0.6, center.Y + size * vy / resultan + size * 0.61);
            lh[3] = new Point(center.X + size * vx / resultan, center.Y + size * vy / resultan + size * 0.7);
            Path l_hand = makelegandhand(lh, brush);
            children.Add(l_hand);

            //create right hand
            Point[] rh = new Point[4];
            rh[0] = new Point(center.X + size * vx / resultan + size * 0.4, center.Y + size * vy / resultan + size * 0.3);
            rh[1] = new Point(center.X + size * vx / resultan + size * 0.6, center.Y + size * vy / resultan + size * 0.6);
            rh[2] = new Point(center.X + size * vx / resultan + size * 0.6, center.Y + size * vy / resultan + size * 0.61);
            rh[3] = new Point(center.X + size * vx / resultan - size * 0.1, center.Y + size * vy / resultan + size * 0.8);
            Path r_hand = makelegandhand(rh, brush);
            children.Add(r_hand);

            //create left lower limb
            Point[] ll = new Point[4];
            ll[0] = new Point(center.X + size * vx / resultan - size * 0.45, center.Y + size * vy / resultan);
            ll[1] = new Point(center.X + size * vx / resultan - size * 0.9, center.Y + size * vy / resultan - size * 0.1);
            ll[2] = new Point(center.X + size * vx / resultan - size * 0.87, center.Y + size * vy / resultan - size * 0.08);
            ll[3] = new Point(center.X + size * vx / resultan - size * 0.7, center.Y + size * vy / resultan + size * 0.7);
            Path first_leftleg = makelegandhand(ll, brush);
            children.Add(first_leftleg);
            Point[] lf = new Point[4];
            lf[0] = new Point(center.X + size * vx / resultan - size * 0.7, center.Y + size * vy / resultan + size * 0.7);
            lf[1] = new Point(center.X + size * vx / resultan - size * 0.7, center.Y + size * vy / resultan + size * 0.8);
            lf[2] = new Point(center.X + size * vx / resultan - size * 0.83, center.Y + size * vy / resultan + size * 0.81);
            lf[3] = new Point(center.X + size * vx / resultan - size * 0.9, center.Y + size * vy / resultan + size * 0.8);
            Path first_leftfoot = makelegandhand(lf, brush);
            children.Add(first_leftfoot);

            ll[0] = new Point(center.X + size * vx / resultan - size * 0.4, center.Y + size * vy / resultan - size * 0.3);
            ll[1] = new Point(center.X + size * vx / resultan - size * 0.85, center.Y + size * vy / resultan - size * 0.4);
            ll[2] = new Point(center.X + size * vx / resultan - size * 0.82, center.Y + size * vy / resultan - size * 0.38);
            ll[3] = new Point(center.X + size * vx / resultan - size * 0.65, center.Y + size * vy / resultan + size * 0.4);
            Path second_leftleg = makelegandhand(ll, brush);
            children.Add(second_leftleg);
            lf[0] = new Point(center.X + size * vx / resultan - size * 0.65, center.Y + size * vy / resultan + size * 0.4);
            lf[1] = new Point(center.X + size * vx / resultan - size * 0.65, center.Y + size * vy / resultan + size * 0.5);
            lf[2] = new Point(center.X + size * vx / resultan - size * 0.78, center.Y + size * vy / resultan + size * 0.51);
            lf[3] = new Point(center.X + size * vx / resultan - size * 0.9, center.Y + size * vy / resultan + size * 0.5);
            Path second_leftfoot = makelegandhand(lf, brush);
            children.Add(second_leftfoot);

            //create right lower limb
            Point[] rl = new Point[4];
            rl[0] = new Point(center.X + size * vx / resultan + size * 0.45, center.Y + size * vy / resultan);
            rl[1] = new Point(center.X + size * vx / resultan + size * 0.9, center.Y + size * vy / resultan - size * 0.1);
            rl[2] = new Point(center.X + size * vx / resultan + size * 0.87, center.Y + size * vy / resultan - size * 0.08);
            rl[3] = new Point(center.X + size * vx / resultan + size * 0.7, center.Y + size * vy / resultan + size * 0.7);
            Path first_rightleg = makelegandhand(rl, brush);
            children.Add(first_rightleg);
            Point[] rf = new Point[4];
            rf[0] = new Point(center.X + size * vx / resultan + size * 0.7, center.Y + size * vy / resultan + size * 0.7);
            rf[1] = new Point(center.X + size * vx / resultan + size * 0.7, center.Y + size * vy / resultan + size * 0.8);
            rf[2] = new Point(center.X + size * vx / resultan + size * 0.83, center.Y + size * vy / resultan + size * 0.81);
            rf[3] = new Point(center.X + size * vx / resultan + size * 0.9, center.Y + size * vy / resultan + size * 0.8);
            Path first_rightfoot = makelegandhand(rf, brush);
            children.Add(first_rightfoot);

            rl[0] = new Point(center.X + size * vx / resultan + size * 0.4, center.Y + size * vy / resultan - size * 0.3);
            rl[1] = new Point(center.X + size * vx / resultan + size * 0.85, center.Y + size * vy / resultan - size * 0.4);
            rl[2] = new Point(center.X + size * vx / resultan + size * 0.82, center.Y + size * vy / resultan - size * 0.38);
            rl[3] = new Point(center.X + size * vx / resultan + size * 0.65, center.Y + size * vy / resultan + size * 0.4);
            Path second_rightleg = makelegandhand(rl, brush);
            children.Add(second_rightleg);
            rf[0] = new Point(center.X + size * vx / resultan + size * 0.65, center.Y + size * vy / resultan + size * 0.4);
            rf[1] = new Point(center.X + size * vx / resultan + size * 0.65, center.Y + size * vy / resultan + size * 0.5);
            rf[2] = new Point(center.X + size * vx / resultan + size * 0.78, center.Y + size * vy / resultan + size * 0.51);
            rf[3] = new Point(center.X + size * vx / resultan + size * 0.9, center.Y + size * vy / resultan + size * 0.5);
            Path second_rightfoot = makelegandhand(rf, brush);
            children.Add(second_rightfoot);    
        }

    }
}
