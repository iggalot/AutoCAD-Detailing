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

        [CommandMethod("Command2", CommandFlags.Session)]
        public void Command2()
        {
            MessageBox.Show("Session Flag means the command can cross multiple documents.");
        }

        [CommandMethod("Command3", CommandFlags.UsePickSet)]
        public void Command3()
        {
            Document myDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor myEd = myDoc.Editor;
            PromptSelectionResult myPSR = myEd.SelectImplied();
            if (myPSR.Status == PromptStatus.OK)
            {
                MessageBox.Show(myPSR.Value.Count.ToString() + " selected.");
            }
            else
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
            using (Transaction myTrans = DBIn.TransactionManager.StartTransaction())
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
                }
                else
                {
                    myTrans.Commit();
                    return retCollection;
                }
            }
        }

        Dictionary<string, string> GetAttributes(ObjectId BlockRefID)
        {
            Dictionary<string, string> retDictionary = new Dictionary<string, string>();
            using (Transaction myTrans = BlockRefID.Database.TransactionManager.StartTransaction())
            {
                BlockReference MyBref = (BlockReference)BlockRefID.GetObject(OpenMode.ForRead);
                if (MyBref.AttributeCollection.Count == 0)
                {
                    return retDictionary;
                }
                else
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
            if (myFIO.Directory.Exists == false)
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
                    foreach (KeyValuePair<string, string> myKVP in GetAttributes(myBrefID))
                    {
                        mySW.WriteLine("   " + myKVP.Key + "   " + myKVP.Value);
                    }
                }
            }
            mySW.Close();
            mySW.Dispose();
        }

        [CommandMethod("SBR")]
        public void SBR()
        {
            double lineWt = 0.03;

            // Uses Autodesk.AutoCAD.DatabaseServices
            Database myDB;
            myDB = HostApplicationServices.WorkingDatabase;

            Editor myEd = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            Point3d pt1 = myEd.GetPoint("First point:").Value;
            Point3d pt2 = myEd.GetPoint("Second point:").Value;
            Point3d temp;

            // if pt2 is left and below pt1 swap the points.
            if(pt1.X >= pt2.X)
            {
                if(pt1.Y > pt2.Y)
                {
                    temp = pt1;
                    pt1 = pt2;
                    pt2 = temp;
                }
            }

            // Draw the break line between the two points
            using (Transaction myTrans = myDB.TransactionManager.StartTransaction())
            {
                // This call must be inside a Transaction
                BlockTableRecord myBTR = (BlockTableRecord)myDB.CurrentSpaceId.GetObject(OpenMode.ForWrite);

                // Draw the breakline
                Polyline myPline = AutoCAD_DrawHelp.DrawHelp.DrawBreakLine(
                    new Point2d(pt1.X, pt1.Y), new Point2d(pt2.X, pt2.Y), lineWt);
                myBTR.AppendEntity(myPline);
                myTrans.AddNewlyCreatedDBObject(myPline, true);
                myTrans.Commit();
            }
        }

        [CommandMethod("CMU")]
        public void CMU()
        {
            double lineWt = 0.03;
            int numBlocksHigh = 4;
            double blockHt = 7.625;
            double blockWid = 7.625;
            double blockLen = 15.625;
            double blockShellThick = 1.25;
            double blockWebThick = 0.75;
            double mortarThick = 0.375;

            // Uses Autodesk.AutoCAD.DatabaseServices
            Database myDB;
            myDB = HostApplicationServices.WorkingDatabase;

            Editor myEd = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            Point3d insertPoint = myEd.GetPoint("InsertPoint:").Value;

            // Draws the lower portion of the CMU wall.
            using (Transaction myTrans = myDB.TransactionManager.StartTransaction())
            {
                // This call must be inside a Transaction
                BlockTableRecord myBTR = (BlockTableRecord)myDB.CurrentSpaceId.GetObject(OpenMode.ForWrite);

                // repair the blockname in case decimal points appear
                string blockName = SymbolUtilityServices.RepairSymbolName(blockWid + "X" + blockHt + "X" + blockLen + "_" + numBlocksHigh + "H_Lower", false);

                //BlockTable bt = (BlockTable)myTrans.GetObject(myDB.BlockTableId, OpenMode.ForRead);
                //BlockTableRecord block;
                //if (bt.Has(blockName))
                //{
                //    MessageBox.Show("Block name " + blockName + "already exists");
                //    return;
                //}



                //// Create a block entity;
                //myTrans.GetObject(myDB.BlockTableId, OpenMode.ForWrite);
                //block = new BlockTableRecord();
                //block.Name = blockName;
                //var blockId = bt.Add(block);
                //myTrans.AddNewlyCreatedDBObject(block, true);

                // draw the blocks
                for (int i = 0; i < numBlocksHigh; i++)
                {
                    Point2d ins = new Point2d(insertPoint.X, insertPoint.Y + i * (blockHt + mortarThick));
                    Matrix3d ucs = myEd.CurrentUserCoordinateSystem;

                    // Draw the mortar joint
                    Polyline myPline1 = AutoCAD_DrawHelp.DrawHelp.DrawUniformMortarJointHorizontal(ins, blockWid, blockHt, mortarThick, ucs, lineWt);
                    myBTR.AppendEntity(myPline1);
                    myTrans.AddNewlyCreatedDBObject(myPline1, true);

                    // Draw the block


                    // the top block is a section
                    if (i < numBlocksHigh - 1)
                    {
                        // Draw full rectangle
                        Polyline myPline2 = AutoCAD_DrawHelp.DrawHelp.DrawRectangle(ins, blockWid, blockHt, mortarThick, lineWt);
                        myBTR.AppendEntity(myPline2);
                        myTrans.AddNewlyCreatedDBObject(myPline2, true);
                    }
                    else
                    {
                        // Draw the clipped block
                        Polyline myPline2 = AutoCAD_DrawHelp.DrawHelp.DrawRectangleClippedTop(ins, blockWid, blockHt, mortarThick, lineWt);
                        myBTR.AppendEntity(myPline2);
                        myTrans.AddNewlyCreatedDBObject(myPline2, true);
                    }

                    ////block.AppendEntity(myPline2);
                    //myBTR.AppendEntity(myPline2);
                    //myTrans.AddNewlyCreatedDBObject(myPline2, true);

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

                // Commit the changes
                myTrans.Commit();
            }

            // Draws the upper portion of the CMU wall.
            using (Transaction myTrans2 = myDB.TransactionManager.StartTransaction())
            {
                // repair the blockname in case decimal points appear
                string blockName = SymbolUtilityServices.RepairSymbolName(blockWid + "X" + blockHt + "X" + blockLen + "_" + numBlocksHigh + "H_Upper", false); ;

                // This call must be inside a Transaction
                BlockTableRecord myBTR = (BlockTableRecord)myDB.CurrentSpaceId.GetObject(OpenMode.ForWrite);

                //BlockTable bt = (BlockTable)myTrans.GetObject(myDB.BlockTableId, OpenMode.ForRead);
                //BlockTableRecord block;
                //if (bt.Has(blockName))
                //{
                //    MessageBox.Show("Block name " + blockName + "already exists");
                //    return;
                //

                //// Create a block entity;
                //myTrans.GetObject(myDB.BlockTableId, OpenMode.ForWrite);
                //block = new BlockTableRecord();
                //block.Name = blockName;
                //var blockId = bt.Add(block);
                //myTrans.AddNewlyCreatedDBObject(block, true);

                // draw the blocks
                for (int i = numBlocksHigh-1; i < 2 * numBlocksHigh - 1; i++)
                {
                    Point2d ins = new Point2d(insertPoint.X, insertPoint.Y + i * (blockHt + mortarThick));
                    Matrix3d ucs = myEd.CurrentUserCoordinateSystem;

                    // the bottom block is a section
                    if (i != numBlocksHigh-1)
                    {
                        // Draw full rectangle
                        Polyline myPline3 = AutoCAD_DrawHelp.DrawHelp.DrawRectangle(ins, blockWid, blockHt, mortarThick, lineWt);
                        myBTR.AppendEntity(myPline3);
                        myTrans2.AddNewlyCreatedDBObject(myPline3, true);

                        // Draw the mortar joint
                        Polyline myPline4 = AutoCAD_DrawHelp.DrawHelp.DrawUniformMortarJointHorizontal(ins, blockWid, blockHt, mortarThick, ucs, lineWt);
                        myBTR.AppendEntity(myPline4);
                        myTrans2.AddNewlyCreatedDBObject(myPline4, true);
                    }
                    else
                    {
                        // Draw the clipped block on the bottom first
                        Polyline myPline3 = AutoCAD_DrawHelp.DrawHelp.DrawRectangleClippedBottom(ins, blockWid, blockHt, mortarThick, lineWt);
                        myBTR.AppendEntity(myPline3);
                        myTrans2.AddNewlyCreatedDBObject(myPline3, true);
                    }

                    ////block.AppendEntity(myPline2);
                    //myBTR.AppendEntity(myPline2);
                    //myTrans.AddNewlyCreatedDBObject(myPline2, true);

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

                // Commit the changes
                myTrans2.Commit();
            }

            // Draws the cross section.
            using (Transaction myTrans3 = myDB.TransactionManager.StartTransaction())
            {
                // repair the blockname in case decimal points appear
                string blockName = SymbolUtilityServices.RepairSymbolName(blockWid + "X" + blockHt + "X" + blockLen + "_" + numBlocksHigh + "H_CrossSection", false); ;

                // This call must be inside a Transaction
                BlockTableRecord myBTR = (BlockTableRecord)myDB.CurrentSpaceId.GetObject(OpenMode.ForWrite);

                //BlockTable bt = (BlockTable)myTrans.GetObject(myDB.BlockTableId, OpenMode.ForRead);
                //BlockTableRecord block;
                //if (bt.Has(blockName))
                //{
                //    MessageBox.Show("Block name " + blockName + "already exists");
                //    return;
                //

                //// Create a block entity;
                //myTrans.GetObject(myDB.BlockTableId, OpenMode.ForWrite);
                //block = new BlockTableRecord();
                //block.Name = blockName;
                //var blockId = bt.Add(block);
                //myTrans.AddNewlyCreatedDBObject(block, true);

                // Create our CMU list of polylines
                List<Polyline> list1 = AutoCAD_DrawHelp.DrawHelp.DrawCMUTop(new Point2d(insertPoint.X + 100, insertPoint.Y), 2, blockWid, blockLen, blockShellThick, blockWebThick, lineWt);
                foreach (Polyline item in list1)
                { 
                    myBTR.AppendEntity(item);
                    myTrans3.AddNewlyCreatedDBObject(item, true);
                }

                // Create our CMU list of polylines
                List<Polyline> list2 = AutoCAD_DrawHelp.DrawHelp.DrawCMUTop(new Point2d(insertPoint.X + 100, insertPoint.Y + 15), 3, blockWid, blockLen, blockShellThick, blockWebThick, lineWt);
                foreach (Polyline item in list2)
                {
                    myBTR.AppendEntity(item);
                    myTrans3.AddNewlyCreatedDBObject(item, true);
                }

                // Create our CMU list of polylines
                List<Polyline> list3= AutoCAD_DrawHelp.DrawHelp.DrawCMUTop(new Point2d(insertPoint.X + 100, insertPoint.Y + 30), 4, blockWid, blockLen, blockShellThick, blockWebThick, lineWt);
                foreach (Polyline item in list3)
                {
                    myBTR.AppendEntity(item);
                    myTrans3.AddNewlyCreatedDBObject(item, true);
                }

                // Draw mortar joint
                Matrix3d ucs = myEd.CurrentUserCoordinateSystem;
                Polyline myPline8 = AutoCAD_DrawHelp.DrawHelp.DrawUniformMortarJointVertical(new Point2d(insertPoint.X + 10 * blockLen + blockLen, insertPoint.Y), blockWid, blockHt, mortarThick, ucs, lineWt);
                myBTR.AppendEntity(myPline8);
                myTrans3.AddNewlyCreatedDBObject(myPline8, true);

                // Commit the changes
                myTrans3.Commit();
            }
        }
    }
}
