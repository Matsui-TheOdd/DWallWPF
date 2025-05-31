using System.Collections;
using System.ComponentModel;
using System.Linq;
using MoreLinq;
using TD = Tekla.Structures.Datatype;
using TSG = Tekla.Structures.Geometry3d;
using Tekla.Structures.Dialog;
using Tekla.Structures.Model;
using Tekla.Structures.Drawing;
using TSM = Tekla.Structures.Model;
using TSD = Tekla.Structures.Drawing;
using TSMUI = Tekla.Structures.Model.UI;
using Tekla.Structures.Model.Operations;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using System;
using Tekla.Structures.Geometry3d;
using static Tekla.Structures.Drawing.StraightDimensionSet;
using System.Windows.Controls;

namespace IntraforDrawing
{
    /// <summary>
    /// Data logic for MainWindow
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<DrawingIP> _DrawingsIP = new ObservableCollection<DrawingIP>();
        public ObservableCollection<DrawingIP> DrawingsIP
        {
            get { return _DrawingsIP; }
            set { _DrawingsIP = value; OnPropertyChanged(); }
        }

        private ObservableCollection<HorizontalSectionIP> _HorizontalSectionsIP = new ObservableCollection<HorizontalSectionIP>();
        public ObservableCollection<HorizontalSectionIP> HorizontalSectionsIP
        {
            get { return _HorizontalSectionsIP; }
            set { _HorizontalSectionsIP = value; OnPropertyChanged(); }
        }

        public List<HorizontalSectionIP> HorizontalSectionIP_Selected = new List<HorizontalSectionIP>();

        private string _AT_Tilte_1 = "";
        [StructuresDialog("AT_Tilte_1", typeof(TD.String))]
        public string AT_Tilte_1
        {
            get { return _AT_Tilte_1; }
            set
            {
                _AT_Tilte_1 = value;
                OnPropertyChanged();
            }
        }

        private double _Max_Length = 0.0;
        [StructuresDialog("AT_Max_Length", typeof(TD.Double))]
        public double AT_Max_Length
        {
            get { return _Max_Length; }
            set
            {
                _Max_Length = value;
                OnPropertyChanged();
            }
        }

        public ICommand Create_Drawings { get; set; }
        public ICommand Get_Exist_Drawings { get; set; }
        public ICommand Create_Reports { get; set; }
        public ICommand DelRow_HorizontalSectionIP_Command { get; set; }
        public ICommand Suggest_Section { get; set; }
        public ICommand Modify_Drawing { get; set; }

        public void PasteIP_Input(System.Windows.Controls.DataGrid dataGrid, int selectedRow, int selectedColumn, object ListUpdate)
        {
            try
            {
                // Lấy dữ liệu từ Clipboard
                string clipboardText = Clipboard.GetText();

                if (string.IsNullOrWhiteSpace(clipboardText))
                {
                    return;
                }

                // Phân tích dữ liệu Clipboard
                var rows = clipboardText.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < rows.Length; i++)
                {
                    var cells = rows[i].Split('\t');
                    // Tìm hàng đích trong DataGrid
                    int targetRow = selectedRow + i;

                    if (targetRow >= dataGrid.Items.Count - 1)
                    {
                        if (ListUpdate is ObservableCollection<HorizontalSectionIP> _HorizontalSectionsIP)
                        {
                            _HorizontalSectionsIP.Add(new HorizontalSectionIP());
                        }
                    }
                }
                for (int i = 0; i < rows.Length; i++)
                {
                    var cells = rows[i].Split('\t');
                    // Tìm hàng đích trong DataGrid
                    int targetRow = selectedRow + i;
                    // Lấy đối tượng từ DataGrid.Items
                    var dataGridItem = dataGrid.Items[targetRow];

                    if (dataGridItem is HorizontalSectionIP _HorizontalSectionsIP)
                    {
                        // Cập nhật từng ô trong hàng
                        for (int j = 0; j < cells.Length; j++)
                        {
                            int targetColumn = selectedColumn + j;

                            // Ánh xạ cột với thuộc tính trong đối tượng
                            switch (targetColumn)
                            {
                                case 0: _HorizontalSectionsIP.Panel = cells[j]; break;
                                case 1: _HorizontalSectionsIP.Level = cells[j]; break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        Model model = new Model();
        TSG.Vector vX = new TSG.Vector(1, 0, 0);
        TSG.Vector vY = new TSG.Vector(0, 1, 0);
        TSG.Vector vZ = new TSG.Vector(0, 0, 1);
        public MainWindowViewModel()
        {
            Create_Drawings = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {
                    if (model.GetConnectionStatus())
                    {
                        string titel1 = AT_Tilte_1;
                        List<Assembly> ListAssembly = GetAssemblyList();

                        foreach (Assembly assembly in ListAssembly)
                        {
                            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
                            assembly.Select();
                            List<TSM.Part> listPanel = new List<TSM.Part> { assembly.GetMainPart() as TSM.Part };
                            ArrayList moe = assembly.GetSecondaries();
                            foreach (TSM.Part beam in moe)
                            {
                                if (beam.Name != "JOIN")
                                {
                                    listPanel.Add(beam);
                                }
                            }

                            int drawingSheetNumber = 1;
                            List<string> listSplit = new List<string>();
                            List<string> containCage = new List<string>();
                            int numberOfFronDrawing = NumberOfFrontDrawing(listPanel, out listSplit, out containCage);

                            foreach (string cage in containCage)
                            {
                                if (listSplit.Count == 1)
                                {
                                    // Full in 1 drawing
                                    CastUnitDrawing frontCage = new CastUnitDrawing(assembly.Identifier, drawingSheetNumber, "CIP_Wall");
                                    frontCage.Layout.LoadAttributes("DWALL");
                                    frontCage.Name = assembly.Name;
                                    frontCage.Title1 = titel1;
                                    frontCage.SetUserProperty("COMMENT", "Front View");
                                    frontCage.Insert();
                                    DrawingIP newIP = new DrawingIP(assembly.Name, titel1, "Front View");
                                    DrawingsIP.Add(newIP);

                                    drawingSheetNumber++;
                                    titel1 = IncrementLastSegment(titel1);
                                    FrontViewDrawing(ref frontCage, listPanel, cage, listSplit[0], true, true);
                                }
                                else
                                {
                                    for (int i = 0; i < listSplit.Count; i++)
                                    {
                                        CastUnitDrawing frontCage = new CastUnitDrawing(assembly.Identifier, drawingSheetNumber, "CIP_Wall");
                                        frontCage.Layout.LoadAttributes("DWALL");
                                        frontCage.Name = assembly.Name;
                                        frontCage.Title1 = titel1;
                                        frontCage.SetUserProperty("COMMENT", "Front View");
                                        frontCage.Insert();
                                        DrawingIP newIP = new DrawingIP(assembly.Name, titel1, "Front View");
                                        DrawingsIP.Add(newIP);

                                        drawingSheetNumber++;
                                        titel1 = IncrementLastSegment(titel1);
                                        if (i == 0)
                                        {
                                            FrontViewDrawing(ref frontCage, listPanel, cage, listSplit[i], true, false);
                                            // First drawing
                                        }
                                        else if (i == listSplit.Count - 1)
                                        {
                                            FrontViewDrawing(ref frontCage, listPanel, cage, listSplit[i], false, true);
                                            // Middle drawing
                                        }
                                        else
                                        {
                                            FrontViewDrawing(ref frontCage, listPanel, cage, listSplit[i], false, false);
                                            // Last drawing
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            );

            Get_Exist_Drawings = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {

                }
            );

            Create_Reports = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {

                }
            );

            DelRow_HorizontalSectionIP_Command = new RelayCommand<object>((p) => { return HorizontalSectionIP_Selected.Count > 0 ? true : false; }, (p) =>
            {
                foreach (var item in HorizontalSectionIP_Selected)
                {
                    HorizontalSectionsIP.Remove(item);
                    HorizontalSectionIP_Selected.Remove(item);
                }
            });

            Suggest_Section = new RelayCommand<object>(
               (p) => true,
               (p) =>
               {

               }
            );

            Modify_Drawing = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {

                }
            );
        }
        string IncrementLastSegment(string input)
        {
            var parts = input.Split('/');
            int lastIndex = parts.Length - 1;
            string lastPart = parts[lastIndex];

            int number;
            if (int.TryParse(lastPart, out number))
            {
                number++;
                parts[lastIndex] = number.ToString("D" + lastPart.Length); // Preserve leading zeros
                return string.Join("/", parts);
            }
            else
            {
                throw new ArgumentException("The last segment is not a valid number.");
            }
        }
        private TransformationPlane GetTransformationPlane(TSD.View view)
        {
            CoordinateSystem viewcoo = view.ViewCoordinateSystem;
            CoordinateSystem Displaycoo = view.DisplayCoordinateSystem;
            TransformationPlane transformationPlane = new TransformationPlane(viewcoo.Origin, Displaycoo.AxisX, Displaycoo.AxisY);
            return transformationPlane;
        }
        private bool RangesOverlap(double start1, double end1, double start2, double end2)
        {
            return Math.Round(start1) < Math.Round(end2) && Math.Round(start2) < Math.Round(end1);
        }
        private List<Assembly> GetAssemblyList()
        {
            TSMUI.ModelObjectSelector mos = new TSMUI.ModelObjectSelector();
            ModelObjectEnumerator objsele = mos.GetSelectedObjects();
            List<Assembly> ListAss = new List<Assembly>();
            while (objsele.MoveNext())
            {
                if (objsele.Current is TSM.Part)
                {
                    TSM.Part part = objsele.Current as TSM.Part;
                    Assembly ass = part.GetAssembly();
                    ListAss.Add(ass);
                }
                if (objsele.Current is TSM.Assembly)
                {
                    Assembly assembly = objsele.Current as Assembly;
                    ListAss.Add(assembly);
                }
            }
            ListAss = ListAss.DistinctBy(p => p.Identifier.GUID).ToList();
            ListAss = ListAss.OrderBy(p => p.Name).ToList();
            return ListAss;
        }
        private int NumberOfFrontDrawing(List<TSM.Part> listPanels, out List<string> listSplit, out List<string> containCage)
        {
            listSplit = new List<string>();
            int numberOfDrawing = 0;

            containCage = new List<string>();
            List<DWallBeam> panel1stCage = new List<DWallBeam>();
            List<DWallBeam> panel2ndCage = new List<DWallBeam>();
            List<DWallBeam> panel3rdCage = new List<DWallBeam>();
            List<DWallBeam> panel4thCage = new List<DWallBeam>();
            foreach (TSM.Part panel in listPanels)
            {
                DWallBeam dWallBeam = new DWallBeam(panel);
                foreach (string cageName in dWallBeam.cageContain)
                {
                    containCage.Add(cageName);
                }

                dWallBeam.SetMaxMinPointByZ();
                if (dWallBeam.panelCageNo == "1st CAGE")
                {
                    panel1stCage.Add(dWallBeam);
                }
                else if (dWallBeam.panelCageNo == "2nd CAGE")
                {
                    panel2ndCage.Add(dWallBeam);
                }
                else if (dWallBeam.panelCageNo == "3rd CAGE")
                {
                    panel3rdCage.Add(dWallBeam);
                }
                else if (dWallBeam.panelCageNo == "4th CAGE")
                {
                    panel4thCage.Add(dWallBeam);
                }
            }

            containCage = containCage.DistinctBy(x => x).OrderBy(x => x).ToList();

            #region Get number of Drawing per Panel
            if (panel4thCage.Count != 0)
            {
                TSG.Point cage1stMaxPoint = panel1stCage[0].maxPoint;
                TSG.Point cage1stMinPoint = panel1stCage[0].minPoint;
                TSG.Point cage2ndMaxPoint = panel2ndCage[0].maxPoint;
                TSG.Point cage2ndMinPoint = panel2ndCage[0].minPoint;
                TSG.Point cage3rdMaxPoint = panel3rdCage[0].maxPoint;
                TSG.Point cage3rdMinPoint = panel3rdCage[0].minPoint;
                TSG.Point cage4thMaxPoint = panel4thCage[0].maxPoint;
                TSG.Point cage4thMinPoint = panel4thCage[0].minPoint;

                foreach (DWallBeam dWallBeam in panel1stCage)
                {
                    if (cage1stMaxPoint.Z < dWallBeam.maxPoint.Z)
                    {
                        cage1stMaxPoint = dWallBeam.maxPoint;
                    }
                    if (cage1stMinPoint.Z > dWallBeam.minPoint.Z)
                    {
                        cage1stMinPoint = dWallBeam.minPoint;
                    }
                }
                foreach (DWallBeam dWallBeam in panel2ndCage)
                {
                    if (cage2ndMaxPoint.Z < dWallBeam.maxPoint.Z)
                    {
                        cage2ndMaxPoint = dWallBeam.maxPoint;
                    }
                    if (cage2ndMinPoint.Z > dWallBeam.minPoint.Z)
                    {
                        cage2ndMinPoint = dWallBeam.minPoint;
                    }
                }
                foreach (DWallBeam dWallBeam in panel3rdCage)
                {
                    if (cage3rdMaxPoint.Z < dWallBeam.maxPoint.Z)
                    {
                        cage3rdMaxPoint = dWallBeam.maxPoint;
                    }
                    if (cage3rdMinPoint.Z > dWallBeam.minPoint.Z)
                    {
                        cage3rdMinPoint = dWallBeam.minPoint;
                    }
                }
                foreach (DWallBeam dWallBeam in panel4thCage)
                {
                    if (cage4thMaxPoint.Z < dWallBeam.maxPoint.Z)
                    {
                        cage4thMaxPoint = dWallBeam.maxPoint;
                    }
                    if (cage4thMinPoint.Z > dWallBeam.minPoint.Z)
                    {
                        cage4thMinPoint = dWallBeam.minPoint;
                    }
                }

                if (cage1stMaxPoint.Z - cage2ndMinPoint.Z < AT_Max_Length)
                {
                    if (cage1stMaxPoint.Z - cage3rdMinPoint.Z < AT_Max_Length)
                    {
                        if (cage1stMaxPoint.Z - cage4thMinPoint.Z < AT_Max_Length)
                        {
                            // Drawing contain cage 1234
                            listSplit.Add("1234");
                        }
                        else
                        {
                            // Drawing contain cage 123, 4
                            listSplit.Add("123");
                            listSplit.Add("4");
                        }
                    }
                    else
                    {
                        if (cage3rdMaxPoint.Z - cage4thMinPoint.Z < AT_Max_Length)
                        {
                            // Drawing contain cage 12, 34
                            listSplit.Add("12");
                            listSplit.Add("34");
                        }
                        else
                        {
                            // Drawing contain cage 12, 3, 4
                            listSplit.Add("12");
                            listSplit.Add("3");
                            listSplit.Add("4");
                        }
                    }
                }
                else
                {
                    if (cage2ndMaxPoint.Z - cage3rdMinPoint.Z < AT_Max_Length)
                    {
                        if (cage2ndMaxPoint.Z - cage4thMinPoint.Z < AT_Max_Length)
                        {
                            // Drawing contain cage 1, 234
                            listSplit.Add("1");
                            listSplit.Add("234");
                        }
                        else
                        {
                            // Drawing contain cage 1, 23, 4
                            listSplit.Add("1");
                            listSplit.Add("23");
                            listSplit.Add("4");
                        }
                    }
                    else
                    {
                        if (cage3rdMaxPoint.Z - cage4thMinPoint.Z < AT_Max_Length)
                        {
                            // Drawing contain cage 1, 2, 34
                            listSplit.Add("1");
                            listSplit.Add("2");
                            listSplit.Add("34");
                        }
                        else
                        {
                            // Drawing contain cage 1, 2, 3, 4
                            listSplit.Add("1");
                            listSplit.Add("2");
                            listSplit.Add("3");
                            listSplit.Add("4");
                        }
                    }
                }
            }
            else if (panel3rdCage.Count != 0)
            {
                TSG.Point cage1stMaxPoint = panel1stCage[0].maxPoint;
                TSG.Point cage1stMinPoint = panel1stCage[0].minPoint;
                TSG.Point cage2ndMaxPoint = panel2ndCage[0].maxPoint;
                TSG.Point cage2ndMinPoint = panel2ndCage[0].minPoint;
                TSG.Point cage3rdMaxPoint = panel3rdCage[0].maxPoint;
                TSG.Point cage3rdMinPoint = panel3rdCage[0].minPoint;

                foreach (DWallBeam dWallBeam in panel1stCage)
                {
                    if (cage1stMaxPoint.Z < dWallBeam.maxPoint.Z)
                    {
                        cage1stMaxPoint = dWallBeam.maxPoint;
                    }
                    if (cage1stMinPoint.Z > dWallBeam.minPoint.Z)
                    {
                        cage1stMinPoint = dWallBeam.minPoint;
                    }
                }
                foreach (DWallBeam dWallBeam in panel2ndCage)
                {
                    if (cage2ndMaxPoint.Z < dWallBeam.maxPoint.Z)
                    {
                        cage2ndMaxPoint = dWallBeam.maxPoint;
                    }
                    if (cage2ndMinPoint.Z > dWallBeam.minPoint.Z)
                    {
                        cage2ndMinPoint = dWallBeam.minPoint;
                    }
                }
                foreach (DWallBeam dWallBeam in panel3rdCage)
                {
                    if (cage3rdMaxPoint.Z < dWallBeam.maxPoint.Z)
                    {
                        cage3rdMaxPoint = dWallBeam.maxPoint;
                    }
                    if (cage3rdMinPoint.Z > dWallBeam.minPoint.Z)
                    {
                        cage3rdMinPoint = dWallBeam.minPoint;
                    }
                }

                if (cage1stMaxPoint.Z - cage2ndMinPoint.Z < AT_Max_Length)
                {
                    if (cage1stMaxPoint.Z - cage3rdMinPoint.Z < AT_Max_Length)
                    {
                        // Drawing contain cage 123
                        listSplit.Add("123");
                    }
                    else
                    {
                        // Drawing contain cage 12, 3
                        listSplit.Add("12");
                        listSplit.Add("3");
                    }
                }
                else
                {
                    if (cage2ndMaxPoint.Z - cage3rdMinPoint.Z < AT_Max_Length)
                    {
                        // Drawing contain cage 1, 23
                        listSplit.Add("1");
                        listSplit.Add("23");
                    }
                    else
                    {
                        // Drawing contain cage 1, 2, 3
                        listSplit.Add("1");
                        listSplit.Add("2");
                        listSplit.Add("3");
                    }
                }
            }
            else if (panel2ndCage.Count != 0)
            {
                TSG.Point cage1stMaxPoint = panel1stCage[0].maxPoint;
                TSG.Point cage1stMinPoint = panel1stCage[0].minPoint;
                TSG.Point cage2ndMaxPoint = panel2ndCage[0].maxPoint;
                TSG.Point cage2ndMinPoint = panel2ndCage[0].minPoint;

                foreach (DWallBeam dWallBeam in panel1stCage)
                {
                    if (cage1stMaxPoint.Z < dWallBeam.maxPoint.Z)
                    {
                        cage1stMaxPoint = dWallBeam.maxPoint;
                    }
                    if (cage1stMinPoint.Z > dWallBeam.minPoint.Z)
                    {
                        cage1stMinPoint = dWallBeam.minPoint;
                    }
                }
                foreach (DWallBeam dWallBeam in panel2ndCage)
                {
                    if (cage2ndMaxPoint.Z < dWallBeam.maxPoint.Z)
                    {
                        cage2ndMaxPoint = dWallBeam.maxPoint;
                    }
                    if (cage2ndMinPoint.Z > dWallBeam.minPoint.Z)
                    {
                        cage2ndMinPoint = dWallBeam.minPoint;
                    }
                }

                if (cage1stMaxPoint.Z - cage2ndMinPoint.Z < AT_Max_Length)
                {
                    // Drawing contain cage 12
                    listSplit.Add("12");
                }
                else
                {
                    // Drawing contain cage 1, 2
                    listSplit.Add("1");
                    listSplit.Add("2");
                }
            }
            else
            {
                listSplit.Add("1");
            }

            numberOfDrawing = listSplit.Count * containCage.Count();
            #endregion

            return numberOfDrawing;
        }
        private void FrontViewDrawing(ref CastUnitDrawing drawing, List<TSM.Part> listPanel, string cageName, string listSplit, bool isContainTop, bool isContainBot)
        {
            DrawingHandler dh = new DrawingHandler();
            if (dh.GetConnectionStatus())
            {
                dh.SetActiveDrawing(drawing);

                #region Get Showing Panel
                List<string> cageNos = listSplit.ToCharArray()
                                   .Select(c => c.ToString())
                                   .ToList();

                List<DWallBeam> listShowPanel = new List<DWallBeam>();
                List<DWallBeam> listFullPanel = new List<DWallBeam>();
                foreach (TSM.Part _panel in listPanel)
                {
                    DWallBeam dWallBeam = new DWallBeam(_panel);
                    if (dWallBeam.cageContain.Contains(cageName))
                    {
                        listFullPanel.Add(dWallBeam);
                        foreach (string cageNo in cageNos)
                        {
                            if (dWallBeam.panelCageNo.Contains(cageNo))
                            {
                                listShowPanel.Add(dWallBeam);
                                break;
                            }
                        }
                    }
                }
                #endregion

                #region Clear View
                DrawingObjectEnumerator views = drawing.GetSheet().GetAllObjects(typeof(TSD.View));
                while (views.MoveNext())
                {
                    if (views.Current is TSD.View view)
                    {
                        view.Delete();
                    }
                }
                #endregion

                #region Create Top View
                TSD.View.ViewAttributes viewAttributes = new TSD.View.ViewAttributes("Top View");
                TSD.View.CreateTopView(drawing, new TSG.Point(0, 0, 0), viewAttributes, out TSD.View topView);
                Macro.Load_ViewAttribute_Filter_And_EditSetting(topView, "Top View");

                model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
                model.GetWorkPlaneHandler().SetCurrentTransformationPlane(GetTransformationPlane(topView));
                #endregion

                #region Create Front View
                Beam panel = listShowPanel[0].panelModel as Beam;
                panel.Select();
                TSG.Point _startPoint = new TSG.Point(panel.StartPoint.X, panel.StartPoint.Y, 0);
                TSG.Point _endPoint = new TSG.Point(panel.EndPoint.X, panel.EndPoint.Y, 0);
                TSG.Vector beamStartEnd = new TSG.Vector(_endPoint - _startPoint).GetNormal();
                TSG.Vector pendicularBeam = beamStartEnd.Cross(vZ);

                TransformationPlane tPlane = new TransformationPlane(_startPoint, beamStartEnd, pendicularBeam);
                model.GetWorkPlaneHandler().SetCurrentTransformationPlane(tPlane);

                panel.Select();

                TSG.Point startPoint = new TSG.Point(panel.StartPoint.X, panel.StartPoint.Y, 0);
                TSG.Point endPoint = new TSG.Point(panel.EndPoint.X, panel.EndPoint.Y, 0);

                Solid beamSolid = panel.GetSolid();

                TSG.Point topLeftPoint = new TSG.Point(panel.StartPoint.X, beamSolid.MaximumPoint.Y, 0);
                TSG.Point topRightPoint = new TSG.Point(panel.EndPoint.X, beamSolid.MaximumPoint.Y, 0);
                TSG.Point botLeftPoint = new TSG.Point(panel.StartPoint.X, beamSolid.MinimumPoint.Y, 0);
                TSG.Point botRightPoint = new TSG.Point(panel.EndPoint.X, beamSolid.MinimumPoint.Y, 0);

                TSD.View.ViewAttributes frontViewAttributes = new TSD.View.ViewAttributes("Front View");
                SectionMarkBase.SectionMarkAttributes sectionMarkAttributes = new SectionMarkBase.SectionMarkAttributes("DWall-NoMark");
                TSD.View.CreateSectionView(topView, botLeftPoint, botRightPoint, new TSG.Point(210, 117, 0), beamSolid.MaximumPoint.Y - beamSolid.MinimumPoint.Y, 0, frontViewAttributes, sectionMarkAttributes, out TSD.View frontView, out SectionMark checkSectionMark);
                Macro.Load_ViewAttribute_Filter_And_EditSetting(frontView, "Front View");
                frontView.Modify();
                frontView.Select();
                #endregion

                #region Modify Front View Range
                model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
                model.GetWorkPlaneHandler().SetCurrentTransformationPlane(GetTransformationPlane(frontView));

                TSG.Point topPoint = new TSG.Point(0, double.MinValue, 0);
                TSG.Point botPoint = new TSG.Point(0, double.MaxValue, 0);
                foreach (DWallBeam dWall in listShowPanel)
                {
                    dWall.SetMaxMinPointByY();
                    if (topPoint.Y < dWall.maxPoint.Y)
                    {
                        topPoint = dWall.maxPoint;
                    }
                    if (botPoint.Y > dWall.minPoint.Y)
                    {
                        botPoint = dWall.minPoint;
                    }
                }

                TSG.Point topPanelPoint = new TSG.Point(0, double.MinValue, 0);
                TSG.Point botPanelPoint = new TSG.Point(0, double.MaxValue, 0);
                foreach (DWallBeam dWall in listFullPanel)
                {
                    dWall.SetMaxMinPointByY();
                    if (topPanelPoint.Y < dWall.maxPoint.Y)
                    {
                        topPanelPoint = dWall.maxPoint;
                    }
                    if (botPanelPoint.Y > dWall.minPoint.Y)
                    {
                        botPanelPoint = dWall.minPoint;
                    }
                } 

                if (!isContainTop)
                {
                    frontView.RestrictionBox.MaxPoint.Y = frontView.RestrictionBox.MaxPoint.Y - (topPanelPoint.Y - topPoint.Y) + 500;
                }
                if (!isContainBot)
                {
                    frontView.RestrictionBox.MinPoint.Y = frontView.RestrictionBox.MinPoint.Y + (botPoint.Y - botPanelPoint.Y) - 500;
                }
                frontView.Modify();

                model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
                TSG.Point centerView = frontView.ExtremaCenter;
                double moveDown = centerView.Y - 117;
                frontView.Origin = frontView.Origin - vY * moveDown;
                frontView.Modify();
                #endregion
                EditFrontView(frontView, listPanel, cageName, listSplit, isContainTop, isContainBot);
                topView.Delete();
                dh.CloseActiveDrawing(true);
            }
        }
        private void EditFrontView(TSD.View frontView, List<TSM.Part> listPanel, string cageName, string listSplit, bool isContainTop, bool isContainBot)
        {
            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(GetTransformationPlane(frontView));
            frontView.Select();

            StraightDimensionSetHandler sdsh = new StraightDimensionSetHandler();
            StraightDimensionSetAttributes StraightDimAttribute = new StraightDimensionSetAttributes("DWall-Dim");

            #region Get Panel
            List<string> cageNos = listSplit.ToCharArray()
                               .Select(c => c.ToString())
                               .ToList();

            List<DWallBeam> listShowPanel = new List<DWallBeam>();
            List<DWallBeam> listFullPanel = new List<DWallBeam>();
            foreach (TSM.Part _panel in listPanel)
            {
                DWallBeam dWallBeam = new DWallBeam(_panel);
                if (dWallBeam.cageContain.Contains(cageName))
                {
                    listFullPanel.Add(dWallBeam);
                    foreach (string cageNo in cageNos)
                    {
                        if (dWallBeam.panelCageNo.Contains(cageNo))
                        {
                            listShowPanel.Add(dWallBeam);
                            break;
                        }
                    }
                }
            }
            #endregion

            #region Indicate Cage
            DrawingObjectEnumerator fullReinforcements = frontView.GetAllObjects(typeof(TSD.ReinforcementBase));
            List<DWallRebar> fullRebars = new List<DWallRebar>();
            while (fullReinforcements.MoveNext())
            {
                if (fullReinforcements.Current is ReinforcementBase rebar)
                {
                    DWallRebar dWallRebar = new DWallRebar(model, rebar);
                    fullRebars.Add(dWallRebar);
                }
            }

            List<List<DWallRebar>> splitedByCageRebars = fullRebars
            .GroupBy(r => r.cageName)
            .Select(group => group.ToList())
            .ToList();

            List<TSG.Point> cagePointList = new List<TSG.Point>() { new TSG.Point(frontView.RestrictionBox.MinPoint.X, frontView.RestrictionBox.MinPoint.Y, 0) };
            foreach (List<DWallRebar> rebarByCage in splitedByCageRebars)
            {
                double minXValue = double.MaxValue;
                double maxXValue = double.MinValue;
                foreach (DWallRebar dWallRebar in rebarByCage)
                {
                    dWallRebar.rebarModel.Select();
                    Solid rebarSolid = dWallRebar.rebarModel.GetSolid();
                    if (minXValue > rebarSolid.MinimumPoint.X)
                    {
                        minXValue = rebarSolid.MinimumPoint.X;
                    }
                    if (maxXValue < rebarSolid.MaximumPoint.X)
                    {
                        maxXValue = rebarSolid.MaximumPoint.X;
                    }
                }

                TSG.Point leftPoint = new TSG.Point(minXValue, frontView.RestrictionBox.MinPoint.Y, 0);
                TSG.Point rightPoint = new TSG.Point(maxXValue, frontView.RestrictionBox.MinPoint.Y, 0);
                cagePointList.Add(leftPoint);
                cagePointList.Add(rightPoint);
                #region Create Cage Line
                TSG.Point leftTop = new TSG.Point(minXValue, frontView.RestrictionBox.MaxPoint.Y, 0);
                TSG.Point rightTop = new TSG.Point(maxXValue, frontView.RestrictionBox.MaxPoint.Y, 0);

                TSD.Line leftLine = new TSD.Line(frontView, leftTop, leftPoint);
                leftLine.Attributes.Line.Type = LineTypes.DashDot;
                leftLine.Attributes.Line.Color = DrawingColors.Black;
                leftLine.Insert();

                TSD.Line rightLine = new TSD.Line(frontView, rightTop, rightPoint);
                rightLine.Attributes.Line.Type = LineTypes.DashDot;
                rightLine.Attributes.Line.Color = DrawingColors.Black;
                rightLine.Insert();
                #endregion

                #region Create Cage Dim
                string cageN = rebarByCage[0].cageName.Replace("CAGE ", "");

                StraightDimensionSetAttributes StraightCageDimAttribute = new StraightDimensionSetAttributes("DWall-Dim" + cageN);
                new StraightDimension(frontView, leftPoint, rightPoint, vY, -500, StraightCageDimAttribute).Insert();
                #endregion
            }
            cagePointList.Add(new TSG.Point(frontView.RestrictionBox.MaxPoint.X, frontView.RestrictionBox.MinPoint.Y, 0));
            #endregion

            #region Horizontal Dim
            for (int i = 0; i < cagePointList.Count - 1; i += 2)
            {
                new StraightDimension(frontView, cagePointList[i], cagePointList[i + 1], vY, -500, StraightDimAttribute).Insert();
            }

            StraightDimensionSetAttributes StraightTotalDimAttribute = new StraightDimensionSetAttributes("DWall-DimP");
            new StraightDimension(frontView, cagePointList[0], cagePointList[cagePointList.Count - 1], vY, -1200, StraightDimAttribute).Insert();
            #endregion

            #region Get Neighbor
            DrawingObjectEnumerator parts = frontView.GetAllObjects(typeof(TSD.Part));
            string leftPartName = "";
            string rightPartName = "";
            List<TSM.Part> listSlabs = new List<TSM.Part>();
            string panelName = listPanel[0].Name;
            while (parts.MoveNext())
            {
                if (parts.Current is TSD.Part _part)
                {
                    DWallBeam dWallPart = new DWallBeam(model, _part);
                    TSM.Part part = dWallPart.panelModel;
                    if (part.Name != panelName && part.Name != "TREMIE" && part.Name != "SHEAR PIN" && part.Name != "PIPE" && part.Name != "INCLINOMETER" && part.Name != "SONIC" && part.Name != "JOIN")
                    {
                        if (part.Class == listPanel[0].Class)
                        {
                            Solid solid = part.GetSolid();
                            if (Math.Round(solid.MinimumPoint.X) <= Math.Round(frontView.RestrictionBox.MinPoint.X))
                            {
                                leftPartName = part.Name;
                            }
                            else
                            {
                                rightPartName = part.Name;
                            }
                        }
                        else
                        {
                            Solid solid = part.GetSolid();
                            if (RangesOverlap(solid.MinimumPoint.X, solid.MaximumPoint.X, frontView.RestrictionBox.MinPoint.X, frontView.RestrictionBox.MaxPoint.X))
                            {
                                listSlabs.Add(part);
                            }
                        }
                    }
                }
            }
            #endregion
        }
    }
}
