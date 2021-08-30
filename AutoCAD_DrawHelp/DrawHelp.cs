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
        const double WALL_SECTION_GAP = 0.1;         // percentage of block height to make gaps on wall sections
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
        /// Draws an AutoCAD polyline rectangle with a cut section at the top of a block
        /// </summary>
        /// <param name="pt1">lower left point of the block</param>
        /// <param name="blockWid">width of the rectangle</param>
        /// <param name="blockHt">height of the rectangle</param>
        /// <param name="offset">vertical offset (mortar thickness of CMU)</param>
        /// <param name="lineWt">thickness of the line to be drawn</param>
        /// <returns>Returns an AutoCAD polyline object</returns>
        public static Polyline DrawRectangleClippedTop(Point2d ins, double blockWid, double blockHt, double offset, double lineWt)
        {
            Polyline myPline2 = new Polyline();

            myPline2.SetDatabaseDefaults();

            // Draw the bottom of the block
            myPline2.AddVertexAt(0, new Point2d(ins.X + (0.0 * blockWid), ins.Y + offset + 0.0 * blockHt), 0, lineWt, lineWt);
            myPline2.AddVertexAt(1, new Point2d(ins.X + (1.0 * blockWid), ins.Y + offset + 0.0 * blockHt), 0, lineWt, lineWt);
            myPline2.AddVertexAt(2, new Point2d(ins.X + (1.0 * blockWid),  ins.Y + offset + (0.35 - WALL_SECTION_GAP / 2.0 + 1.0 * 0.3 ) * blockHt), 0, lineWt, lineWt);

            // Draw the breakline at the top
            Polyline myPline3 = AutoCAD_DrawHelp.DrawHelp.DrawBreakLine(
                new Point2d(ins.X + (0.0 * blockWid), ins.Y + offset + (0.35 - WALL_SECTION_GAP / 2.0 + 0.0 * 0.3) * blockHt),
                new Point2d(ins.X + (1.0 * blockWid), ins.Y + offset + (0.35 - WALL_SECTION_GAP / 2.0 + 1.0 * 0.3) * blockHt), lineWt);
            myPline2.JoinEntity(myPline3);

            myPline2.Closed = true; // Close the polyline

            if (!(myPline2.Closed || myPline2.GetPoint2dAt(0).IsEqualTo(myPline2.GetPoint2dAt(myPline2.NumberOfVertices - 1))))
                throw new InvalidOperationException("Opened polyline in myPline2.");

            return myPline2;
        }

        /// <summary>
        /// Draws an AutoCAD polyline rectangle with a cut section at the bottom of a block
        /// </summary>
        /// <param name="pt1">lower left point of the block</param>
        /// <param name="blockWid">width of the rectangle</param>
        /// <param name="blockHt">height of the rectangle</param>
        /// <param name="offset">vertical offset (mortar thickness of CMU)</param>
        /// <param name="lineWt">thickness of the line to be drawn</param>
        /// <returns>Returns an AutoCAD polyline object</returns>
        public static Polyline DrawRectangleClippedBottom(Point2d ins, double blockWid, double blockHt, double offset, double lineWt)
        {
            Polyline myPline2 = new Polyline();

            myPline2.SetDatabaseDefaults();

            // Draw the break line at the bottom of the block
            myPline2 = AutoCAD_DrawHelp.DrawHelp.DrawBreakLine(
                new Point2d(ins.X + (0.0 * blockWid), ins.Y + offset + (0.35 + WALL_SECTION_GAP / 2.0 + 0.0 * 0.3) * blockHt),
                new Point2d(ins.X + (1.0 * blockWid), ins.Y + offset + (0.35 + WALL_SECTION_GAP / 2.0 + 1.0 * 0.3) * blockHt), lineWt);

            myPline2.AddVertexAt(7, new Point2d(ins.X + (1.0 * blockWid), ins.Y + offset + (1.0 * blockHt)), 0, lineWt, lineWt);
            myPline2.AddVertexAt(8, new Point2d(ins.X + (0.0 * blockWid), ins.Y + offset + (1.0 * blockHt)), 0, lineWt, lineWt);

            myPline2.Closed = true; // Close the polyline

            if (!(myPline2.Closed || myPline2.GetPoint2dAt(0).IsEqualTo(myPline2.GetPoint2dAt(myPline2.NumberOfVertices - 1))))
                throw new InvalidOperationException("Opened polyline in myPline2.");

            return myPline2;
        }

        /// <summary>
        /// Draws a horizontal mortar joint of thickness 'offset' at the insert point
        /// </summary>
        /// <param name="insertPoint">the lower left corner of the mortar joint</param>
        /// <param name="blockWid">the width of the CMU block</param>
        /// <param name="blockHt">the height og the CMU block</param>
        /// <param name="offset">thickness of the mortar joint</param>
        /// <param name="ucs">UCS for the drawing system.  Needed for orienting the arcs in space.  Typically an identiy matrix.</param>
        /// <param name="lineWt">weight of the lines when drawn</param>
        /// <returns></returns>
        public static Polyline DrawUniformMortarJointHorizontal(Point2d insertPoint, double blockWid, double blockHt, double offset, Matrix3d ucs, double lineWt)
        {
            Polyline myPline1 = new Polyline();

            // for points
            //   pt4 ------------------- pt3
            //   \                       /
            //   /                       \
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

        /// <summary>
        /// Draws a vertical mortar joint of thickness 'offset' at the insert point
        /// </summary>
        /// <param name="insertPoint">the lower left corner of the mortar joint</param>
        /// <param name="blockWid">the width of the CMU block</param>
        /// <param name="blockHt">the height og the CMU block</param>
        /// <param name="mortarThick">thickness of the mortar joint</param>
        /// <param name="ucs">UCS for the drawing system.  Needed for orienting the arcs in space.  Typically an identiy matrix.</param>
        /// <param name="lineWt">weight of the lines when drawn</param>
        /// <returns></returns>
        public static Polyline DrawUniformMortarJointVertical(Point2d insertPoint, double blockWid, double blockHt, double mortarThick, Matrix3d ucs, double lineWt)
        {
            Polyline myPline1 = new Polyline();

            // for points
            //   pt4 -\/- pt3
            //   |         |              
            //   |         |              
            //   pt1 -/\- pt2
            Point2d pt1 = new Point2d(insertPoint.X, insertPoint.Y);
            Point2d pt2 = new Point2d(insertPoint.X + mortarThick, insertPoint.Y);
            Point2d pt3 = new Point2d(insertPoint.X + mortarThick, insertPoint.Y + blockWid);
            Point2d pt4 = new Point2d(insertPoint.X, insertPoint.Y + blockWid);

            // draw the mortar layer
            myPline1.SetDatabaseDefaults();

            myPline1.AddVertexAt(0, pt4, 0, lineWt, lineWt);
            myPline1.AddVertexAt(1, pt1, 0, lineWt, lineWt);

            using (Arc myArc1 = new Arc())
            {
                Point3d endPoint = new Point3d(pt1.X, pt1.Y, 0);
                Point3d startPoint = new Point3d(pt2.X, pt2.Y, 0);

                myArc1.TransformBy(ucs);

                myArc1.Center = new Point3d(pt1.X + (pt2.X - pt1.X) / 2.0, pt1.Y + (pt2.Y - pt1.Y) / 2.0, 0);
                myArc1.Radius = 0.5 * mortarThick;

                Matrix3d ocs2wcs = Matrix3d.PlaneToWorld(myArc1.Normal);
                Plane plane = new Plane(ocs2wcs.CoordinateSystem3d.Origin, ocs2wcs.CoordinateSystem3d.Xaxis, ocs2wcs.CoordinateSystem3d.Yaxis);

                myArc1.StartAngle = -(startPoint - myArc1.Center).AngleOnPlane(plane);
                myArc1.EndAngle = -(endPoint - myArc1.Center).AngleOnPlane(plane);

                myPline1.JoinEntity(myArc1);
            }

            myPline1.AddVertexAt(2, pt2, 0, lineWt, lineWt);
            myPline1.AddVertexAt(3, pt3, 0, lineWt, lineWt);

            using (Arc myArc1 = new Arc())
            {
                Point3d endPoint = new Point3d(pt3.X, pt3.Y, 0);
                Point3d startPoint = new Point3d(pt4.X, pt4.Y, 0);

                myArc1.TransformBy(ucs);

                myArc1.Center = new Point3d(pt3.X + (pt4.X - pt3.X) / 2.0, pt3.Y + (pt4.Y - pt3.Y) / 2.0, 0);
                myArc1.Radius = 0.5 * mortarThick;

                Matrix3d ocs2wcs = Matrix3d.PlaneToWorld(myArc1.Normal);
                Plane plane = new Plane(ocs2wcs.CoordinateSystem3d.Origin, ocs2wcs.CoordinateSystem3d.Xaxis, ocs2wcs.CoordinateSystem3d.Yaxis);

                myArc1.StartAngle = -(startPoint - myArc1.Center).AngleOnPlane(plane);
                myArc1.EndAngle = -(endPoint - myArc1.Center).AngleOnPlane(plane);

                myPline1.JoinEntity(myArc1);
            }



            myPline1.Closed = true; ; // Close the polyline

            return myPline1;
        }

        /// <summary>
        /// Draws a section break line between two points
        /// </summary>
        /// <param name="start">start point</param>
        /// <param name="end">end point</param>
        /// <param name="lineWt">weight of the lines to be drawn</param>
        /// <returns></returns>
        public static Polyline DrawBreakLine(Point2d start, Point2d end, double lineWt)
        { 
            double hor = end.X - start.X;
            double ver = end.Y - start.Y;

            double breakOffsetVert = Math.Max(0.05 * ver, 0.5);
            double breakOffsetHoriz = Math.Max(0.05 * hor, 0.5);

            double[] h = { 0.00 * hor, 0.46 * hor, 0.46 * hor, 0.50 * hor, 0.54 * hor, 0.54 * hor, 1.00 * hor };
            double[] v = { 0.00 * ver, 0.46 * ver, 0.46 * ver - breakOffsetVert, 0.50 * ver, 0.54 * ver + breakOffsetVert, 0.54 * ver, 1.00 * ver };

            Polyline myPline2 = new Polyline();

            // Start of cut line
            myPline2.AddVertexAt(0, new Point2d(start.X + h[0], start.Y + v[0]), 0, lineWt, lineWt);

            myPline2.AddVertexAt(1, new Point2d(start.X + h[1], start.Y + v[1]), 0, lineWt, lineWt);
            myPline2.AddVertexAt(2, new Point2d(start.X + h[2], start.Y + v[2]), 0, lineWt, lineWt);

            // Midpoint of cut line
            myPline2.AddVertexAt(3, new Point2d(start.X + h[3], start.Y + v[3]), 0, lineWt, lineWt);
            
            myPline2.AddVertexAt(4, new Point2d(start.X + h[4], start.Y + v[4]), 0, lineWt, lineWt);
            myPline2.AddVertexAt(5, new Point2d(start.X + h[5], start.Y + v[5]), 0, lineWt, lineWt);

            // End of cut line
            myPline2.AddVertexAt(6, new Point2d(start.X + h[6], start.Y + v[6]), 0, lineWt, lineWt);

            return myPline2;
        }

        /// <summary>
        /// Routine to generate a list of the polylines of multiple core (along length) CMU units
        /// </summary>
        /// <param name="ins">insert point</param>
        /// <param name="num_cores">cores to draw</param>
        /// <param name="wid">width of the block (usually the short dimension in plan)</param>
        /// <param name="len">length of the block (long dim.)</param>
        /// <param name="shell_thick">outer wall (shell) thickness of the block</param>
        /// <param name="web_thick">cell divider thickness (usually half of the shell thickness)</param>
        /// <param name="lineWt">weight of the line to draw.</param>
        /// <returns>List of polyline objects</returns>
        public static List<Polyline> DrawCMUTop(Point2d ins, int num_cores, double wid, double len, double shell_thick, double web_thick, double lineWt)
        {
            double coreWidth = wid - 2.0 * shell_thick;
            double coreLen = (len - 2.0 * shell_thick - (num_cores - 1) * web_thick) / num_cores;

            if(coreLen < 0)
            {
                throw new InvalidOperationException("Core length of CMU cannot be negative ( " + coreLen + ")! Revise cell or shell thicknesses");
            }

            if (coreWidth < 0)
            {
                throw new InvalidOperationException("Core width of CMU cannot be negative ( " + coreWidth + ")! Revise shell thicknesses");
            }

            List<Polyline> list = new List<Polyline>();
            list.Add(AutoCAD_DrawHelp.DrawHelp.DrawRectangle(ins, len, wid, 0, lineWt));  // outer shell

            // draw the cores
            for (int i = 0; i < num_cores; i++)
            {
                list.Add(AutoCAD_DrawHelp.DrawHelp.DrawRectangle(new Point2d(ins.X + shell_thick + i * (coreLen + web_thick), ins.Y + shell_thick), coreLen, coreWidth, 0, lineWt)); // cores
            }

            return list;
        }
    }
}
