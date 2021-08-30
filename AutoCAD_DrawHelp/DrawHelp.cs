using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCAD_DrawHelp
{
    public static class DrawHelp
    {
        /// <summary>
        /// Draws an AutoCAD rectangular polyline
        /// </summary>
        /// <param name="pt1">lower left point of the block</param>
        /// <param name="blockWid">width of the rectangle</param>
        /// <param name="blockHt">height of the rectangle</param>
        /// <param name="offset">vertical offset (mortar thickness of CMU)</param>
        /// <param name="lineWt">thickness of the line to be drawn</param>
        /// <returns>Returns an AutoCAD polyline object</returns>
        public static Polyline DrawRectangle(Point2d pt1, double blockWid, double blockHt, double offset, double lineWt)
        {
            Polyline myPline2 = new Polyline();

            myPline2.SetDatabaseDefaults();

            // Draw the bottom of the block
            myPline2.AddVertexAt(0, new Point2d(pt1.X, pt1.Y + offset), 0, lineWt, lineWt);
            myPline2.AddVertexAt(1, new Point2d(pt1.X + blockWid, pt1.Y + offset), 0, lineWt, lineWt);
        
            // draw normal full block
            myPline2.AddVertexAt(2, new Point2d(pt1.X + blockWid, pt1.Y + offset + blockHt), 0, lineWt, lineWt);
            myPline2.AddVertexAt(3, new Point2d(pt1.X, pt1.Y + offset + blockHt), 0, lineWt, lineWt);
            myPline2.Closed = true; // Close the polyline

            return myPline2;
        }

        /// <summary>
        /// Draws an AutoCAD polyline rectangle with a cut section at the top
        /// </summary>
        /// <param name="pt1">lower left point of the block</param>
        /// <param name="blockWid">width of the rectangle</param>
        /// <param name="blockHt">height of the rectangle</param>
        /// <param name="offset">vertical offset (mortar thickness of CMU)</param>
        /// <param name="lineWt">thickness of the line to be drawn</param>
        /// <returns>Returns an AutoCAD polyline object</returns>
        public static Polyline DrawRectangleClippedTop(Point2d pt1, double blockWid, double blockHt, double offset, double lineWt)
        {
            Polyline myPline2 = new Polyline();


            myPline2.SetDatabaseDefaults();

            // Draw the bottom of the block
            myPline2.AddVertexAt(0, new Point2d(pt1.X, pt1.Y + offset), 0, lineWt, lineWt);
            myPline2.AddVertexAt(1, new Point2d(pt1.X + blockWid, pt1.Y + offset), 0, lineWt, lineWt);

            // draw the section block
            myPline2.AddVertexAt(2, new Point2d(pt1.X + blockWid, pt1.Y + offset + (0.2 + 0.3) * blockHt), 0, lineWt, lineWt);
            myPline2.AddVertexAt(3, new Point2d(pt1.X + (0.57 * blockWid), pt1.Y + offset + (0.2 + 0.57 * 0.3) * blockHt), 0, lineWt, lineWt);

            myPline2.AddVertexAt(4, new Point2d(pt1.X + (0.5 * blockWid), pt1.Y + offset + (0.2 + 0.5 * 0.3 + 0.15) * blockHt), 0, lineWt, lineWt);
            myPline2.AddVertexAt(5, new Point2d(pt1.X + (0.5 * blockWid), pt1.Y + offset + (0.2 + 0.5 * 0.3 - 0.15) * blockHt), 0, lineWt, lineWt);

            myPline2.AddVertexAt(6, new Point2d(pt1.X + (0.43 * blockWid), pt1.Y + offset + (0.2 + 0.43 * 0.3) * blockHt), 0, lineWt, lineWt);
            myPline2.AddVertexAt(7, new Point2d(pt1.X, pt1.Y + offset + 0.2 * blockHt), 0, lineWt, lineWt);
            myPline2.Closed = true; // Close the polyline

            if (!(myPline2.Closed || myPline2.GetPoint2dAt(0).IsEqualTo(myPline2.GetPoint2dAt(myPline2.NumberOfVertices - 1))))
                throw new InvalidOperationException("Opened polyline in myPline2.");

            return myPline2;
        }

        public static Polyline DrawUniformMortarJoint(Point2d insertPoint, double blockWid, double blockHt, double offset, Matrix3d ucs, double lineWt)
        {
            Polyline myPline1 = new Polyline();

            // for points
            //   pt4 ------------------- pt3
            //    |                       |
            //   pt1 ------------------- pt2
            Point2d pt1 = new Point2d(insertPoint.X, insertPoint.Y);
            Point2d pt2 = new Point2d(insertPoint.X + blockWid, insertPoint.Y);
            Point2d pt3 = new Point2d(insertPoint.X + blockWid, insertPoint.Y + offset);
            Point2d pt4 = new Point2d(insertPoint.X, insertPoint.Y + offset);

            // draw the mortar layer
            myPline1.SetDatabaseDefaults();
            myPline1.AddVertexAt(0, pt1, 0, lineWt, lineWt);
            myPline1.AddVertexAt(1, pt2, 0, lineWt, lineWt);

            using (Arc myArc1 = new Arc())
            {
                Point3d startPoint = new Point3d(pt2.X, pt2.Y, 0);
                Point3d endPoint = new Point3d(pt3.X, pt3.Y, 0);

                myArc1.TransformBy(ucs);

                myArc1.Center = new Point3d(pt2.X + (pt3.X - pt2.X) / 2.0, pt2.Y + (pt3.Y - pt2.Y) / 2.0, 0);
                myArc1.Radius = 0.5 * offset;

                Matrix3d ocs2wcs = Matrix3d.PlaneToWorld(myArc1.Normal);
                Plane plane = new Plane(ocs2wcs.CoordinateSystem3d.Origin, ocs2wcs.CoordinateSystem3d.Xaxis, ocs2wcs.CoordinateSystem3d.Yaxis);

                myArc1.StartAngle = -(startPoint - myArc1.Center).AngleOnPlane(plane);
                myArc1.EndAngle = -(endPoint - myArc1.Center).AngleOnPlane(plane);

                myPline1.JoinEntity(myArc1);
            }

            myPline1.AddVertexAt(2, pt3, 0, lineWt, lineWt);
            myPline1.AddVertexAt(3, pt4, 0, lineWt, lineWt);

            using (Arc myArc1 = new Arc())
            {
                Point3d startPoint = new Point3d(pt4.X, pt4.Y, 0);
                Point3d endPoint = new Point3d(pt1.X, pt1.Y, 0);

                myArc1.TransformBy(ucs);

                myArc1.Center = new Point3d(pt4.X + (pt1.X - pt4.X) / 2.0, pt4.Y + (pt1.Y - pt4.Y) / 2.0, 0);
                myArc1.Radius = 0.5 * offset;

                Matrix3d ocs2wcs = Matrix3d.PlaneToWorld(myArc1.Normal);
                Plane plane = new Plane(ocs2wcs.CoordinateSystem3d.Origin, ocs2wcs.CoordinateSystem3d.Xaxis, ocs2wcs.CoordinateSystem3d.Yaxis);

                myArc1.StartAngle = -(startPoint - myArc1.Center).AngleOnPlane(plane);
                myArc1.EndAngle = -(endPoint - myArc1.Center).AngleOnPlane(plane);

                myPline1.JoinEntity(myArc1);
            }

            myPline1.Closed = true; ; // Close the polyline

            return myPline1;
        }
    }
}
