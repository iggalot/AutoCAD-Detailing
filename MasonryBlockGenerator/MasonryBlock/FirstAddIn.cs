using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;

namespace MasonryBlock
{

    public class FirstAddIn
    {
        [CommandMethod("Command1")]
        public void Command1()
        {
            MessageBox.Show("Test Command");
        }

        [CommandMethod("Command2",CommandFlags.Session)]
        public void Command2()
        {
            MessageBox.Show("Session Flag means the command can cross multiple documents.");
        }

        [CommandMethod("Command3",CommandFlags.UsePickSet)]
        public void Command3()
        {
            Document myDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor myEd = myDoc.Editor;
            PromptSelectionResult myPSR = myEd.SelectImplied();
            if(myPSR.Status == PromptStatus.OK)
            {
                MessageBox.Show(myPSR.Value.Count.ToString() + " selected.");
            } else
            {
                MessageBox.Show("0 selected.");
            }
        }

        [CommandMethod("Command4")]
        public void Command4()
        {
            // Uses Autodesk.AutoCAD.DatabaseServices
            Database myDB;
            myDB = HostApplicationServices.WorkingDatabase;
            using (Transaction myTrans = myDB.TransactionManager.StartTransaction())
            {
                Autodesk.AutoCAD.Geometry.Point3d startPoint = new Autodesk.AutoCAD.Geometry.Point3d(1, 2, 3);
                Autodesk.AutoCAD.Geometry.Point3d endPoint = new Autodesk.AutoCAD.Geometry.Point3d(4, 20, 6);
                Line myLine = new Line(startPoint, endPoint);
                BlockTableRecord myBTR = (BlockTableRecord)myDB.CurrentSpaceId.GetObject(OpenMode.ForWrite);
                myBTR.AppendEntity(myLine);
                myTrans.AddNewlyCreatedDBObject(myLine, true);
                myTrans.Commit();
            }
        }

        [CommandMethod("Command6")]
        public void Command6()
        {
            Database myDB;
            myDB = HostApplicationServices.WorkingDatabase;
            using (Transaction myTrans = myDB.TransactionManager.StartTransaction())
            {
                Editor myEd = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                Point3d startPoint = myEd.GetPoint("First Point:").Value;
                Point3d endPoint = myEd.GetPoint("Second Point:").Value;
                Line myLine = new Line(startPoint, endPoint);
                BlockTableRecord myBTR = (BlockTableRecord)myDB.CurrentSpaceId.GetObject(OpenMode.ForWrite);
                myBTR.AppendEntity(myLine);
                myTrans.AddNewlyCreatedDBObject(myLine, true);
                myTrans.Commit();
            }
        }

        List<string> GetBlockNames(Database DBIn)
        {
            List<string> retList = new List<string>();
            using(Transaction myTrans = DBIn.TransactionManager.StartTransaction())
            {
                BlockTable myBT = (BlockTable)DBIn.BlockTableId.GetObject(OpenMode.ForRead);
                foreach (ObjectId myOID in myBT)
                {
                    BlockTableRecord myBTR = (BlockTableRecord)myOID.GetObject(OpenMode.ForRead);
                    if (myBTR.IsLayout == false | myBTR.IsAnonymous == false)
                    {
                        retList.Add(myBTR.Name);
                    }
                }
            }

            return retList;
        }

        ObjectIdCollection GetBlockIDs(Database DBIn, string BlockName)
        {
            ObjectIdCollection retCollection = new ObjectIdCollection();
            using (Transaction myTrans = DBIn.TransactionManager.StartTransaction())
            {
                BlockTable myBT = (BlockTable)DBIn.BlockTableId.GetObject(OpenMode.ForRead);
                if (myBT.Has(BlockName))
                {
                    BlockTableRecord myBTR = (BlockTableRecord)myBT[BlockName].GetObject(OpenMode.ForRead);
                    retCollection = (ObjectIdCollection)myBTR.GetBlockReferenceIds(true, true);
                    myTrans.Commit();
                    return retCollection;
                } else
                {
                    myTrans.Commit();
                    return retCollection;
                }
            }
        }

        Dictionary<string,string> GetAttributes(ObjectId BlockRefID)
        {
            Dictionary<string, string> retDictionary = new Dictionary<string, string>();
            using (Transaction myTrans = BlockRefID.Database.TransactionManager.StartTransaction())
            {
                BlockReference MyBref = (BlockReference)BlockRefID.GetObject(OpenMode.ForRead);
                if(MyBref.AttributeCollection.Count == 0)
                {
                    return retDictionary;
                } else
                {
                    foreach (ObjectId myBRefID in MyBref.AttributeCollection)
                    {
                        AttributeReference myAttRef = (AttributeReference)myBRefID.GetObject(OpenMode.ForRead);
                        if (retDictionary.ContainsKey(myAttRef.Tag) == false)
                        {
                            retDictionary.Add(myAttRef.Tag, myAttRef.TextString);
                        }
                    }
                    return retDictionary;
                }
            }
        }

        [CommandMethod("Command7")]
        public void Command7()
        {
            System.IO.FileInfo myFIO = new System.IO.FileInfo("C:\\Users\\jallen\\Documents\\Programming\\AutoCAD\\blocks.txt");
            if(myFIO.Directory.Exists==false)
            {
                myFIO.Directory.Create();
            }
            Database dbToUse = HostApplicationServices.WorkingDatabase;
            System.IO.StreamWriter mySW = new System.IO.StreamWriter(myFIO.FullName);
            foreach (string myName in GetBlockNames(dbToUse))
            {
                foreach (ObjectId myBrefID in GetBlockIDs(dbToUse, myName))
                {
                    mySW.WriteLine(" " + myName);
                    foreach (KeyValuePair<string,string> myKVP in GetAttributes(myBrefID))
                    {
                        mySW.WriteLine("   " + myKVP.Key + "   " + myKVP.Value);
                    }
                }
            }
            mySW.Close();
            mySW.Dispose();
        }

        [CommandMethod("CMU")]
        public void CMU()
        {

            double lineWt = 0.03;
            int numBlocksHigh = 4;
            double blockHt = 5.625;
            double blockWid = 5.625;
            double blockLen = 15.625;
            double mortarThick = 0.375;


            // Uses Autodesk.AutoCAD.DatabaseServices
            Database myDB;
            myDB = HostApplicationServices.WorkingDatabase;
            using (Transaction myTrans = myDB.TransactionManager.StartTransaction())
            {
                // repair the blockname in case decimal points appear
                string blockName = SymbolUtilityServices.RepairSymbolName(blockWid + "X" + blockHt + "X" + blockLen + "_" + numBlocksHigh + "H", false); ;

                //BlockTable bt = (BlockTable)myTrans.GetObject(myDB.BlockTableId, OpenMode.ForRead);
                //BlockTableRecord block;
                //if (bt.Has(blockName))
                //{
                //    MessageBox.Show("Block name " + blockName + "already exists");
                //    return;
                //}

                Editor myEd = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                Point3d insertPoint = myEd.GetPoint("InsertPoint:").Value;
                BlockTableRecord myBTR = (BlockTableRecord)myDB.CurrentSpaceId.GetObject(OpenMode.ForWrite);

                //// Create a block entity;
                //myTrans.GetObject(myDB.BlockTableId, OpenMode.ForWrite);
                //block = new BlockTableRecord();
                //block.Name = blockName;
                //var blockId = bt.Add(block);
                //myTrans.AddNewlyCreatedDBObject(block, true);

                // draw the blocks
                for (int i = 0; i < numBlocksHigh; i++)
                {
                    // for points
                    //   pt4 ------------------- pt3
                    //    |                       |
                    //   pt1 ------------------- pt2
                    Point2d pt1 = new Point2d(insertPoint.X, insertPoint.Y + i * (blockHt + mortarThick));
                    Point2d pt2 = new Point2d(insertPoint.X + blockWid, insertPoint.Y + i * (blockHt + mortarThick));
                    Point2d pt3 = new Point2d(insertPoint.X + blockWid, insertPoint.Y + mortarThick + i * (blockHt + mortarThick));
                    Point2d pt4 = new Point2d(insertPoint.X, insertPoint.Y + mortarThick + i * (blockHt + mortarThick));

                    // draw the mortar layer
                    Polyline myPline1 = new Polyline();
                    myPline1.SetDatabaseDefaults();
                    myPline1.AddVertexAt(0, pt1, 0, lineWt, lineWt);
                    myPline1.AddVertexAt(1, pt2, 0, lineWt, lineWt);

                    using (Arc myArc1 = new Arc())
                    {
                        Point3d startPoint = new Point3d(pt2.X, pt2.Y, 0);
                        Point3d endPoint = new Point3d(pt3.X, pt3.Y, 0);

                        Matrix3d ucs = myEd.CurrentUserCoordinateSystem;
                        myArc1.TransformBy(ucs);

                        myArc1.Center = new Point3d(pt2.X + (pt3.X - pt2.X) / 2.0, pt2.Y + (pt3.Y - pt2.Y) / 2.0, 0);
                        myArc1.Radius = 0.5 * mortarThick;

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

                        Matrix3d ucs = myEd.CurrentUserCoordinateSystem;
                        myArc1.TransformBy(ucs);

                        myArc1.Center = new Point3d(pt4.X + (pt1.X - pt4.X) / 2.0, pt4.Y + (pt1.Y - pt4.Y) / 2.0, 0);
                        myArc1.Radius = 0.5 * mortarThick;

                        Matrix3d ocs2wcs = Matrix3d.PlaneToWorld(myArc1.Normal);
                        Plane plane = new Plane(ocs2wcs.CoordinateSystem3d.Origin, ocs2wcs.CoordinateSystem3d.Xaxis, ocs2wcs.CoordinateSystem3d.Yaxis);

                        myArc1.StartAngle = -(startPoint - myArc1.Center).AngleOnPlane(plane);
                        myArc1.EndAngle = -(endPoint - myArc1.Center).AngleOnPlane(plane);

                        myPline1.JoinEntity(myArc1);
                    }

                    myPline1.Closed = true; ; // Close the polyline

                    // adds to our block object
                    //block.AppendEntity(myPline1);

                    // adds to current space (either model or paper)
                    myBTR.AppendEntity(myPline1);
                    myTrans.AddNewlyCreatedDBObject(myPline1, true);

                    // Draw the CMU block
                    using (Polyline myPline2 = new Polyline())
                    {
                        myPline2.SetDatabaseDefaults();
                        myPline2.AddVertexAt(0, new Point2d(pt1.X, pt1.Y + mortarThick), 0, lineWt, lineWt);
                        myPline2.AddVertexAt(1, new Point2d(pt1.X + blockWid, pt1.Y + mortarThick), 0, lineWt, lineWt);

                        // the top block is a section
                        if (i < numBlocksHigh - 1)
                        {
                            // draw normal full block
                            myPline2.AddVertexAt(2, new Point2d(pt1.X + blockWid, pt1.Y + mortarThick + blockHt), 0, lineWt, lineWt);
                            myPline2.AddVertexAt(3, new Point2d(pt1.X, pt1.Y + mortarThick + blockHt), 0, lineWt, lineWt);
                            myPline2.Closed = true; // Close the polyline
                        }
                        else
                        {
                            // draw the section block
                            myPline2.AddVertexAt(2, new Point2d(pt1.X + blockWid, pt1.Y + mortarThick + (0.2 + 0.3) * blockHt), 0, lineWt, lineWt);
                            myPline2.AddVertexAt(3, new Point2d(pt1.X + (0.57 * blockWid), pt1.Y + mortarThick + (0.2 + 0.57 * 0.3) * blockHt), 0, lineWt, lineWt);

                            myPline2.AddVertexAt(4, new Point2d(pt1.X + (0.5 * blockWid), pt1.Y + mortarThick + (0.2 + 0.5 * 0.3 + 0.15) * blockHt), 0, lineWt, lineWt);
                            myPline2.AddVertexAt(5, new Point2d(pt1.X + (0.5 * blockWid), pt1.Y + mortarThick + (0.2 + 0.5 * 0.3 - 0.15) * blockHt), 0, lineWt, lineWt);

                            myPline2.AddVertexAt(6, new Point2d(pt1.X + (0.43 * blockWid), pt1.Y + mortarThick + (0.2 + 0.43 * 0.3) * blockHt), 0, lineWt, lineWt);
                            myPline2.AddVertexAt(7, new Point2d(pt1.X, pt1.Y + mortarThick + 0.2 * blockHt), 0, lineWt, lineWt);
                            myPline2.Closed = true; // Close the polyline

                            if (!(myPline2.Closed || myPline2.GetPoint2dAt(0).IsEqualTo(myPline2.GetPoint2dAt(myPline2.NumberOfVertices - 1))))
                                throw new InvalidOperationException("Opened polyline in myPline2.");
                        }

                        //block.AppendEntity(myPline2);
                        myBTR.AppendEntity(myPline2);
                        myTrans.AddNewlyCreatedDBObject(myPline2, true);

                        //var ids = new ObjectIdCollection();
                        //ids.Add(myPline2.ObjectId);

                        // Create the hatch
                        //Hatch hatch = new Hatch() { Layer = "0", PatternScale = 0.5, ColorIndex = 1 };
                        //hatch.SetHatchPattern(HatchPatternType.PreDefined, "ANSI31");
                        //block.AppendEntity(hatch);
                        //myTrans.AddNewlyCreatedDBObject(hatch, true);
                        //hatch.Associative = true;
                        //hatch.AppendLoop(HatchLoopTypes.Default, ids);
                        //hatch.EvaluateHatch(true);
                    }
                }

                // Commit the changes
                myTrans.Commit();
            }
        }
    } 
}
