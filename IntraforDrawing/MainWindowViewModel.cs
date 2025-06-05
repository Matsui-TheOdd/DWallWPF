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
using static Tekla.Structures.Drawing.Text;
using static Tekla.Structures.Drawing.LevelMark;
using static Tekla.Structures.Drawing.Line;

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
                                case 0: _HorizontalSectionsIP.Name = cells[j]; break;
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
                            int numberOfFrontDrawing = NumberOfFrontDrawing(listPanel, out listSplit, out containCage);
                            List<HorizontalSectionIP> listHorizontalSection = GetHorizontalSection(listPanel);

                            for (int i = 0; i < listHorizontalSection.Count; i++)
                            {
                                switch (i)
                                {
                                    case 0:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(118, 158, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing));
                                        break;
                                    case 1:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(313, 158, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing));
                                        break;
                                    case 2:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(118, 84, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing));
                                        break;
                                    case 3:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(313, 84, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing));
                                        break;
                                    case 4:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(118, 247, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 1));
                                        break;
                                    case 5:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(313, 247, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 1));
                                        break;
                                    case 6:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(118, 164, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 1));
                                        break;
                                    case 7:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(313, 164, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 1));
                                        break;
                                    case 8:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(118, 81, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 1));
                                        break;
                                    case 9:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(313, 81, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 1));
                                        break;
                                    case 10:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(118, 247, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 2));
                                        break;
                                    case 11:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(313, 247, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 2));
                                        break;
                                    case 12:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(118, 164, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 2));
                                        break;
                                    case 13:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(313, 164, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 2));
                                        break;
                                    case 14:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(118, 81, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 2));
                                        break;
                                    case 15:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(313, 81, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 2));
                                        break;
                                    case 16:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(118, 247, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 3));
                                        break;
                                    case 17:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(313, 247, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 3));
                                        break;
                                    case 18:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(118, 164, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 3));
                                        break;
                                    case 19:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(313, 164, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 3));
                                        break;
                                    case 20:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(118, 81, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 3));
                                        break;
                                    case 21:
                                        listHorizontalSection[i].InsertPoint = new TSG.Point(313, 81, 0);
                                        listHorizontalSection[i].SentToDrawingName = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + 3));
                                        break;
                                }
                            }

                            int numberOfSectionDrawing = 0;
                            switch (listHorizontalSection.Count)
                            {
                                case int n when (n <= 4):
                                    numberOfSectionDrawing = 1;
                                    break;
                                case int n when (n > 4 && n <= 10):
                                    numberOfSectionDrawing = 2;
                                    break;
                                case int n when (n > 10 && n <= 16):
                                    numberOfSectionDrawing = 3;
                                    break;
                                case int n when (n > 16 && n <= 22):
                                    numberOfSectionDrawing = 4;
                                    break;
                            }

                            List<CastUnitDrawing> listSectionDrawing = new List<CastUnitDrawing>();
                            for (int i = 0; i < numberOfSectionDrawing; i++)
                            {
                                CastUnitDrawing sectionView = new CastUnitDrawing(assembly.Identifier, drawingSheetNumber + numberOfFrontDrawing + i, "CIP_Wall");
                                sectionView.Layout.LoadAttributes("DWALL");
                                sectionView.Name = GetNameFromTitle(IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + i));
                                sectionView.Title1 = IncrementLastSegmentByNumber(titel1, numberOfFrontDrawing + i);
                                sectionView.Title2 = assembly.Name;
                                sectionView.Insert();
                                listSectionDrawing.Add(sectionView);
                                ClearViews(ref sectionView);
                            }

                            foreach (string cage in containCage)
                            {
                                if (listSplit.Count == 1)
                                {
                                    // Full in 1 drawing
                                    CastUnitDrawing frontCage = new CastUnitDrawing(assembly.Identifier, drawingSheetNumber, "CIP_Wall");
                                    frontCage.Layout.LoadAttributes("DWALL");
                                    frontCage.Name = GetNameFromTitle(titel1);
                                    frontCage.Title1 = titel1;
                                    frontCage.Title2 = assembly.Name;
                                    frontCage.Insert();

                                    drawingSheetNumber++;
                                    titel1 = IncrementLastSegment(titel1);
                                    FrontViewDrawing(ref frontCage, listPanel, cage, listSplit[0], listHorizontalSection, true, true);
                                }
                                else
                                {
                                    for (int i = 0; i < listSplit.Count; i++)
                                    {
                                        CastUnitDrawing frontCage = new CastUnitDrawing(assembly.Identifier, drawingSheetNumber, "CIP_Wall");
                                        frontCage.Layout.LoadAttributes("DWALL");
                                        frontCage.Name = GetNameFromTitle(titel1);
                                        frontCage.Title1 = titel1;
                                        frontCage.Title2 = assembly.Name;
                                        frontCage.Insert();

                                        drawingSheetNumber++;
                                        titel1 = IncrementLastSegment(titel1);
                                        if (i == 0)
                                        {
                                            FrontViewDrawing(ref frontCage, listPanel, cage, listSplit[i], listHorizontalSection, true, false);
                                            // First drawing
                                        }
                                        else if (i == listSplit.Count - 1)
                                        {
                                            FrontViewDrawing(ref frontCage, listPanel, cage, listSplit[i], listHorizontalSection, false, true);
                                            // Middle drawing
                                        }
                                        else
                                        {
                                            FrontViewDrawing(ref frontCage, listPanel, cage, listSplit[i], listHorizontalSection, false, false);
                                            // Last drawing
                                        }
                                    }
                                }
                            }

                            for (int i = 0; i < listSectionDrawing.Count; i++)
                            {
                                if (i == 0)
                                {

                                }
                                else
                                {

                                }
                                drawingSheetNumber++;
                                titel1 = IncrementLastSegment(titel1);
                            }

                            DrawingIP newIP = new DrawingIP(assembly.Name, drawingSheetNumber - 1);
                            DrawingsIP.Add(newIP);
                        }
                    }
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
        public class PointListComparer : IEqualityComparer<List<TSG.Point>>
        {
            public bool Equals(List<TSG.Point> a, List<TSG.Point> b)
            {
                if (ReferenceEquals(a, b)) return true;
                if (a == null || b == null || a.Count != b.Count) return false;
                return a.Zip(b, (p1, p2) => p1.Equals(p2)).All(equal => equal);
            }

            public int GetHashCode(List<TSG.Point> points)
            {
                if (points == null) return 0;
                int hash = 19;
                foreach (var point in points)
                {
                    hash = hash * 31 + point.GetHashCode();
                }
                return hash;
            }
        }
        private string IncrementLastSegment(string input)
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
        private string IncrementLastSegmentByNumber(string input, int numberIncrease)
        {
            var parts = input.Split('/');
            int lastIndex = parts.Length - 1;
            string lastPart = parts[lastIndex];

            int number;
            if (int.TryParse(lastPart, out number))
            {
                number += numberIncrease;
                parts[lastIndex] = number.ToString("D" + lastPart.Length); // Preserve leading zeros
                return string.Join("/", parts);
            }
            else
            {
                throw new ArgumentException("The last segment is not a valid number.");
            }
        }
        private string GetNameFromTitle(string input)
        {
            var parts = input.Split('/');
            int lastIndex = parts.Length - 1;
            string lastPart = parts[lastIndex];
            string nearLast = parts[lastIndex - 1];
            return nearLast + "/" + lastPart;
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
        private LevelMark CreateLevelMark(TSD.View view, TSG.Point point, TSG.Vector direction)
        {
            LevelMarkAttributes levelMarkAtt = new LevelMarkAttributes("DWall-Level");
            TSG.Point directionPoint = point + direction * 300;
            LevelMark topLevelMark = new LevelMark(view, directionPoint, point, levelMarkAtt);
            topLevelMark.Insert();
            view.Modify();
            Macro.Load_LeverMark(topLevelMark, "DWall-Level");
            return topLevelMark;
        }
        private LevelMark CreateValueLevelMark(TSD.View view, TSG.Point point, TSG.Vector direction, string text)
        {
            LevelMark leverMark = CreateLevelMark(view, point, direction);

            TSG.Point textPoint = point + direction * 675;
            if (text == "TENTATIVE TOE LEVEL")
            {
                textPoint = textPoint - vX * 100;
                Text slabText = new Text(view, textPoint, text, new TextAttributes("CENTER-TEXT-TOE"));
                slabText.Insert();
            }
            else
            {
                Text slabText = new Text(view, textPoint, text, new TextAttributes("CENTER-TEXT"));
                slabText.Insert();
            }
            return leverMark;
        }
        private LevelMark CreateNoValueLevelMark(TSD.View view, TSG.Point point, TSG.Vector direction, string text)
        {
            LevelMarkAttributes levelMarkAtt = new LevelMarkAttributes("DWall-Slab");
            TSG.Point directionPoint = point + direction * 425;
            LevelMark topLevelMark = new LevelMark(view, directionPoint, point, levelMarkAtt);
            topLevelMark.Insert();
            view.Modify();
            Macro.Load_LeverMark(topLevelMark, "DWall-Slab");

            Text slabText = new Text(view, directionPoint, text, new TextAttributes("CENTER-TEXT"));
            slabText.Insert();
            return topLevelMark;
        }
        private List<LevelMark> CreateRebarLevelMark(TSD.View view, List<DWallRebar> listDWallRebars, double xValue, bool isContainTop, bool isContainBot)
        {
            List<LevelMark> listLevelMark = new List<LevelMark>();
            List<TSG.Point> pointList = new List<TSG.Point>();
            foreach (DWallRebar dWallRebar in listDWallRebars)
            {
                TSG.Point topPoint = new TSG.Point(xValue, dWallRebar.topRebarPoint.Y, 0);
                TSG.Point botPoint = new TSG.Point(xValue, dWallRebar.botRebarPoint.Y, 0);

                pointList.Add(topPoint);
                pointList.Add(botPoint);
            }

            pointList = pointList.DistinctBy(x => x).OrderByDescending(x => x.Y).ToList();

            if (isContainTop)
            {
                listLevelMark.Add(CreateLevelMark(view, pointList[0], vY));

                #region Steel Cut-Off
                TSG.Point TopPoint = pointList[0];
                foreach (TSG.Point point in pointList)
                {
                    if (point.Y > TopPoint.Y)
                    {
                        TopPoint = point;
                    }
                }
                TSG.Point steelPoint = new TSG.Point(TopPoint.X, TopPoint.Y + 675, 0);

                Text textSteel = new Text(view, steelPoint, "STEEL CUT-OFF", new TextAttributes("CENTER-TEXT"));
                textSteel.Insert();
                #endregion
            }

            for (int i = 1; i < pointList.Count - 1; i++)
            {
                if (Math.Abs(pointList[i].Y - pointList[i - 1].Y) < 400)
                {
                    listLevelMark.Add(CreateLevelMark(view, pointList[i], new TSG.Vector(0, -1, 0)));
                }
                else
                {
                    listLevelMark.Add(CreateLevelMark(view, pointList[i], vY));
                }
            }

            if (isContainBot)
            {
                if (Math.Abs(pointList[pointList.Count - 1].Y - pointList[pointList.Count - 2].Y) < 400)
                {
                    listLevelMark.Add(CreateLevelMark(view, pointList[pointList.Count - 1], new TSG.Vector(0, -1, 0)));
                }
                else
                {
                    listLevelMark.Add(CreateLevelMark(view, pointList[pointList.Count - 1], vY));
                }
            }

            return listLevelMark;
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
        private List<HorizontalSectionIP> GetHorizontalSection(List<TSM.Part> listPanels)
        {
            List<HorizontalSectionIP> horizontalSections = new List<HorizontalSectionIP>();

            List<DWallBeam> dWallList = new List<DWallBeam>();
            foreach (TSM.Part panel in listPanels)
            {
                DWallBeam dWallPanel = new DWallBeam(panel);
                dWallList.Add(dWallPanel);
            }

            List<List<DWallBeam>> dWallListByCageContain = dWallList.GroupBy(b => b.cageContainNote)
                                                                    .Select(g => g.ToList())
                                                                    .ToList();

            foreach (List<DWallBeam> listDWallBeam in dWallListByCageContain)
            {
                List<DWallBeam> orderListDWallBeam = listDWallBeam.OrderBy(x => x.panelCageNo).ToList();
                foreach (DWallBeam dWallBeam in orderListDWallBeam)
                {
                    dWallBeam.SetMaxMinPointByZ();
                }

                int sectionName = 2;
                for (int i = 0; i < orderListDWallBeam.Count; i++)
                {
                    if (i != 0)
                    {
                        // Create Section on Top
                        TSG.Point topPoint = orderListDWallBeam[i].maxPoint;
                        TSG.Point botPoint = orderListDWallBeam[i - 1].minPoint;
                        double cutLevel = Math.Round((topPoint.Z + botPoint.Z) / 2, 3);
                        HorizontalSectionIP sectionIP = new HorizontalSectionIP(sectionName.ToString(), cutLevel.ToString());
                        sectionIP.CageNo = orderListDWallBeam[i - 1].panelCageNo[0].ToString();
                        sectionIP.ValueFromTopCage = orderListDWallBeam[i - 1].maxPoint.Z - cutLevel;
                        horizontalSections.Add(sectionIP);
                        sectionName++;
                    }
                    // Section at Mid
                    TSG.Point topDWallPoint = orderListDWallBeam[i].maxPoint;
                    TSG.Point botDWallPoint = orderListDWallBeam[i].minPoint;
                    double midCutLevel = Math.Round(topDWallPoint.Z + (botDWallPoint.Z - topDWallPoint.Z) / 3, 3);
                    HorizontalSectionIP midSectionIP = new HorizontalSectionIP(sectionName.ToString(), midCutLevel.ToString());
                    midSectionIP.CageNo = orderListDWallBeam[i].panelCageNo[0].ToString();
                    midSectionIP.ValueFromTopCage = orderListDWallBeam[i].maxPoint.Z - midCutLevel;
                    horizontalSections.Add(midSectionIP);
                    sectionName++;
                }
            }

            return horizontalSections;
        }
        private void ClearViews(ref CastUnitDrawing drawing)
        {
            DrawingHandler dh = new DrawingHandler();
            if (dh.GetConnectionStatus())
            {
                dh.SetActiveDrawing(drawing);

                #region Clear View
                DrawingObjectEnumerator views = drawing.GetSheet().GetAllObjects(typeof(TSD.View));
                while (views.MoveNext())
                {
                    if (views.Current is TSD.View view)
                    {
                        view.Delete();
                    }
                    dh.CloseActiveDrawing(true);
                }
                #endregion
            }
        }
        private void FrontViewDrawing(ref CastUnitDrawing drawing, List<TSM.Part> listPanel, string cageName, string listSplit, List<HorizontalSectionIP> listHorizontalSection, bool isContainTop, bool isContainBot)
        {
            DrawingHandler dh = new DrawingHandler();
            if (dh.GetConnectionStatus())
            {
                dh.SetActiveDrawing(drawing);
                vX = new TSG.Vector(1, 0, 0);
                vY = new TSG.Vector(0, 1, 0);
                vZ = new TSG.Vector(0, 0, 1);
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

                #region Get List Horizontal Section
                List<HorizontalSectionIP> listShowHorizontalSection = new List<HorizontalSectionIP>();
                foreach (HorizontalSectionIP horizontalSection in listHorizontalSection)
                {
                    foreach (string cageNo in cageNos)
                    {
                        if (horizontalSection.CageNo == cageNo)
                        {
                            listShowHorizontalSection.Add(horizontalSection);
                            break;
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
                frontView.Select();
                TSG.Point centerView = frontView.ExtremaCenter;
                double moveDown = centerView.Y - 117;
                frontView.Origin = frontView.Origin - vY * moveDown;
                frontView.Modify();
                #endregion

                EditFrontView(frontView, listPanel, cageName, listSplit, listShowHorizontalSection, isContainTop, isContainBot);
                topView.Delete();
                dh.CloseActiveDrawing(true);
            }
        }
        private void EditFrontView(TSD.View frontView, List<TSM.Part> listPanel, string cageName, string listSplit, List<HorizontalSectionIP> listShowHorizontalSection, bool isContainTop, bool isContainBot)
        {
            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(GetTransformationPlane(frontView));
            frontView.Select();

            StraightDimensionSetHandler sdsh = new StraightDimensionSetHandler();
            StraightDimensionSetAttributes StraightDimAttribute = new StraightDimensionSetAttributes("DWall-Dim");

            double levelLeftXValue = frontView.RestrictionBox.MinPoint.X - 750;
            double levelRightXValue = frontView.RestrictionBox.MaxPoint.X + 550;

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

            double cageStartX = 0;
            double cageEndX = 0;

            List<StraightDimensionSet> list500 = new List<StraightDimensionSet>();
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
                PointList pointList = new PointList()
                    {
                        leftPoint,
                        rightPoint
                    };
                StraightDimensionSet dim = sdsh.CreateDimensionSet(frontView, pointList, vY, -500, StraightCageDimAttribute);
                list500.Add(dim);
                #endregion

                #region Cage Start End X Value
                if (rebarByCage[0].cageName == cageName)
                {
                    cageStartX = leftPoint.X;
                    cageEndX = rightPoint.X;
                }
                #endregion
            }
            cagePointList.Add(new TSG.Point(frontView.RestrictionBox.MaxPoint.X, frontView.RestrictionBox.MinPoint.Y, 0));
            cagePointList = cagePointList.OrderBy(x => x.X).ToList();
            #endregion

            #region Horizontal Dim
            for (int i = 0; i < cagePointList.Count - 1; i += 2)
            {
                PointList pointList = new PointList()
                    {
                        cagePointList[i],
                        cagePointList[i + 1]
                    };
                StraightDimensionSet dim = sdsh.CreateDimensionSet(frontView, pointList, vY, -500, StraightDimAttribute);
                list500.Add(dim);
            }

            StraightDimensionSetAttributes StraightTotalDimAttribute = new StraightDimensionSetAttributes("DWall-DimP");
            PointList _pointList = new PointList()
                    {
                        cagePointList[0],
                        cagePointList[cagePointList.Count - 1]
                    };
            StraightDimensionSet dim1200 = sdsh.CreateDimensionSet(frontView, _pointList, vY, -1200, StraightTotalDimAttribute);
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

            #region Neighbor Connection
            #region Left
            if (leftPartName != "")
            {
                #region Arrow Line
                TSG.Point firstPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X - 550, frontView.RestrictionBox.MinPoint.Y - 1450, 0);
                TSG.Point secondPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X - 2150, frontView.RestrictionBox.MinPoint.Y - 1450, 0);
                TSD.Line line = new TSD.Line(frontView, firstPoint, secondPoint, new TSD.Line.LineAttributes("Panel-Arrow"));
                line.Insert();
                #endregion

                #region Panel Text
                TSG.Point insertPoint = firstPoint - vX * 500 + vY * 15;
                Text text = new Text(frontView, insertPoint, "PANEL " + leftPartName, new TextAttributes("PANEL-CENTER-TEXT"));
                text.Insert();
                #endregion
            }
            #endregion

            #region Right
            if (rightPartName != "")
            {
                #region Arrow Line
                TSG.Point firstPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X + 550, frontView.RestrictionBox.MinPoint.Y - 1450, 0);
                TSG.Point secondPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X + 2150, frontView.RestrictionBox.MinPoint.Y - 1450, 0);
                TSD.Line line = new TSD.Line(frontView, firstPoint, secondPoint, new TSD.Line.LineAttributes("Panel-Arrow"));
                line.Insert();
                #endregion

                #region Panel Text
                TSG.Point insertPoint = firstPoint + vX * 500 + vY * 15;
                Text text = new Text(frontView, insertPoint, "PANEL " + rightPartName, new TextAttributes("PANEL-CENTER-TEXT"));
                text.Insert();
                #endregion
            }
            #endregion

            #region Middle Slab Rectangle
            if (listSlabs.Count() != 0)
            {
                foreach (TSM.Part slab in listSlabs)
                {
                    Solid solid = slab.GetSolid();
                    double YMax = solid.MaximumPoint.Y;
                    double YMin = solid.MinimumPoint.Y;

                    Rectangle rec = new Rectangle(frontView, new TSG.Point(frontView.RestrictionBox.MinPoint.X, YMin, 0), new TSG.Point(frontView.RestrictionBox.MaxPoint.X, YMax, 0), new Rectangle.RectangleAttributes("DWall-SectionRectangle"));
                    rec.Insert();
                }
            }
            #endregion

            #region Break Line
            if (!isContainTop)
            {
                TSG.Point point1 = new TSG.Point(frontView.RestrictionBox.MinPoint.X - 120, frontView.RestrictionBox.MaxPoint.Y, 0);
                TSG.Point point2 = new TSG.Point((frontView.RestrictionBox.MinPoint.X + frontView.RestrictionBox.MaxPoint.X) / 2 - 100, point1.Y, 0);
                TSG.Point point3 = new TSG.Point((frontView.RestrictionBox.MinPoint.X + frontView.RestrictionBox.MaxPoint.X) / 2 - 50, point1.Y - 200, 0);
                TSG.Point point4 = new TSG.Point((frontView.RestrictionBox.MinPoint.X + frontView.RestrictionBox.MaxPoint.X) / 2 + 50, point1.Y + 200, 0);
                TSG.Point point5 = new TSG.Point((frontView.RestrictionBox.MinPoint.X + frontView.RestrictionBox.MaxPoint.X) / 2 + 100, point1.Y, 0);
                TSG.Point point6 = new TSG.Point(frontView.RestrictionBox.MaxPoint.X + 120, frontView.RestrictionBox.MaxPoint.Y, 0);

                new TSD.Line(frontView, point1, point2, new LineAttributes("DWall-BreakLine")).Insert();
                new TSD.Line(frontView, point2, point3, new LineAttributes("DWall-BreakLine")).Insert();
                new TSD.Line(frontView, point3, point4, new LineAttributes("DWall-BreakLine")).Insert();
                new TSD.Line(frontView, point4, point5, new LineAttributes("DWall-BreakLine")).Insert();
                new TSD.Line(frontView, point5, point6, new LineAttributes("DWall-BreakLine")).Insert();
            }

            if (!isContainBot)
            {
                TSG.Point point1 = new TSG.Point(frontView.RestrictionBox.MinPoint.X - 120, frontView.RestrictionBox.MinPoint.Y, 0);
                TSG.Point point2 = new TSG.Point((frontView.RestrictionBox.MinPoint.X + frontView.RestrictionBox.MaxPoint.X) / 2 - 100, point1.Y, 0);
                TSG.Point point3 = new TSG.Point((frontView.RestrictionBox.MinPoint.X + frontView.RestrictionBox.MaxPoint.X) / 2 - 50, point1.Y - 200, 0);
                TSG.Point point4 = new TSG.Point((frontView.RestrictionBox.MinPoint.X + frontView.RestrictionBox.MaxPoint.X) / 2 + 50, point1.Y + 200, 0);
                TSG.Point point5 = new TSG.Point((frontView.RestrictionBox.MinPoint.X + frontView.RestrictionBox.MaxPoint.X) / 2 + 100, point1.Y, 0);
                TSG.Point point6 = new TSG.Point(frontView.RestrictionBox.MaxPoint.X + 120, frontView.RestrictionBox.MinPoint.Y, 0);

                new TSD.Line(frontView, point1, point2, new LineAttributes("DWall-BreakLine")).Insert();
                new TSD.Line(frontView, point2, point3, new LineAttributes("DWall-BreakLine")).Insert();
                new TSD.Line(frontView, point3, point4, new LineAttributes("DWall-BreakLine")).Insert();
                new TSD.Line(frontView, point4, point5, new LineAttributes("DWall-BreakLine")).Insert();
                new TSD.Line(frontView, point5, point6, new LineAttributes("DWall-BreakLine")).Insert();
            }
            #endregion
            #endregion

            #region View Name
            TSG.Point namePoint = new TSG.Point((frontView.RestrictionBox.MinPoint.X + frontView.RestrictionBox.MaxPoint.X) / 2, frontView.RestrictionBox.MinPoint.Y - 2000, 0);
            new Text(frontView, namePoint, "FRONT ELEVATION (" + cageName + ")", new TextAttributes("CENTER-UNDERLINE-TEXT")).Insert();
            TSG.Point note1Point = namePoint - vY * 400;
            new Text(frontView, note1Point, "SHEAR & HORI. REINFORCEMENT NOT SHOWN FOR CLARITY", new TextAttributes("CENTER-NOTE1-TEXT")).Insert();
            TSG.Point note2Point = note1Point - vY * 300;
            new Text(frontView, note2Point, "(VIEWED FROM EXCAVATION FACE)", new TextAttributes("CENTER-NOTE2-TEXT")).Insert();
            TSG.Point scalePoint = note2Point - vY * 300;
            new Text(frontView, scalePoint, "SCALE 1:100", new TextAttributes("CENTER-NOTE1-TEXT")).Insert();
            #endregion

            #region Load After Attribute
            Macro.Load_ViewAttribute_Filter_And_EditSetting(frontView, cageName + " Front");
            frontView.Modify();
            #endregion

            #region Get Reinforcement
            DrawingObjectEnumerator reinforcements = frontView.GetAllObjects(typeof(TSD.ReinforcementBase));
            List<DWallRebar> farFaceRebars = new List<DWallRebar>();
            List<DWallRebar> nearFaceRebars = new List<DWallRebar>();
            List<DWallRebar> cage1stRebars = new List<DWallRebar>();
            List<DWallRebar> cage2ndRebars = new List<DWallRebar>();
            List<DWallRebar> cage3rdRebars = new List<DWallRebar>();
            List<DWallRebar> cage4thRebars = new List<DWallRebar>();
            List<DWallRebar> allNFFFRebars = new List<DWallRebar>();
            while (reinforcements.MoveNext())
            {
                if (reinforcements.Current is TSD.ReinforcementBase rebar)
                {
                    DWallRebar dWallRebar = new DWallRebar(model, rebar);
                    if (dWallRebar.cageName == cageName)
                    {
                        if (dWallRebar.rebarName == "Main Bar FF")
                        {
                            dWallRebar.rebarModel.Select();
                            Solid solid = dWallRebar.rebarModel.GetSolid();
                            dWallRebar.topRebarPoint = new TSG.Point(solid.MaximumPoint.X, solid.MaximumPoint.Y, solid.MaximumPoint.Z);
                            dWallRebar.botRebarPoint = new TSG.Point(solid.MinimumPoint.X, solid.MinimumPoint.Y, solid.MinimumPoint.Z);
                            farFaceRebars.Add(dWallRebar);
                            allNFFFRebars.Add(dWallRebar);
                        }
                        else if (dWallRebar.rebarName == "Main Bar NF")
                        {
                            dWallRebar.rebarModel.Select();
                            Solid solid = dWallRebar.rebarModel.GetSolid();
                            dWallRebar.topRebarPoint = new TSG.Point(solid.MaximumPoint.X, solid.MaximumPoint.Y, solid.MaximumPoint.Z);
                            dWallRebar.botRebarPoint = new TSG.Point(solid.MinimumPoint.X, solid.MinimumPoint.Y, solid.MinimumPoint.Z);
                            nearFaceRebars.Add(dWallRebar);
                            allNFFFRebars.Add(dWallRebar);
                        }

                        if (dWallRebar.cageNo == "1st CAGE")
                        {
                            dWallRebar.rebarModel.Select();
                            Solid solid = dWallRebar.rebarModel.GetSolid();
                            dWallRebar.topRebarPoint = new TSG.Point(solid.MaximumPoint.X, solid.MaximumPoint.Y, 0);
                            dWallRebar.botRebarPoint = new TSG.Point(solid.MinimumPoint.X, solid.MinimumPoint.Y, 0);
                            cage1stRebars.Add(dWallRebar);
                        }
                        else if (dWallRebar.cageNo == "2nd CAGE")
                        {
                            dWallRebar.rebarModel.Select();
                            Solid solid = dWallRebar.rebarModel.GetSolid();
                            dWallRebar.topRebarPoint = new TSG.Point(solid.MaximumPoint.X, solid.MaximumPoint.Y, 0);
                            dWallRebar.botRebarPoint = new TSG.Point(solid.MinimumPoint.X, solid.MinimumPoint.Y, 0);
                            cage2ndRebars.Add(dWallRebar);
                        }
                        else if (dWallRebar.cageNo == "3rd CAGE")
                        {
                            dWallRebar.rebarModel.Select();
                            Solid solid = dWallRebar.rebarModel.GetSolid();
                            dWallRebar.topRebarPoint = new TSG.Point(solid.MaximumPoint.X, solid.MaximumPoint.Y, 0);
                            dWallRebar.botRebarPoint = new TSG.Point(solid.MinimumPoint.X, solid.MinimumPoint.Y, 0);
                            cage3rdRebars.Add(dWallRebar);
                        }
                        else if (dWallRebar.cageNo == "4th CAGE")
                        {
                            dWallRebar.rebarModel.Select();
                            Solid solid = dWallRebar.rebarModel.GetSolid();
                            dWallRebar.topRebarPoint = new TSG.Point(solid.MaximumPoint.X, solid.MaximumPoint.Y, 0);
                            dWallRebar.botRebarPoint = new TSG.Point(solid.MinimumPoint.X, solid.MinimumPoint.Y, 0);
                            cage4thRebars.Add(dWallRebar);
                        }
                    }
                }
            }
            if (farFaceRebars.Count != 0)
            {
                farFaceRebars = farFaceRebars.OrderByDescending(x => x.topRebarPoint.Y).ThenBy(x => x.botRebarPoint.Y).ToList();
            }
            if (nearFaceRebars.Count != 0)
            {
                nearFaceRebars = nearFaceRebars.OrderByDescending(x => x.topRebarPoint.Y).ThenBy(x => x.botRebarPoint.Y).ToList();
            }
            #endregion

            #region Part Lever Mark
            #region Bottom Lever
            if (isContainBot)
            {
                CreateValueLevelMark(frontView, new TSG.Point(levelLeftXValue, frontView.RestrictionBox.MinPoint.Y, 0), new TSG.Vector(0, -1, 0), "TENTATIVE TOE LEVEL");
            }
            #endregion

            #region Create Slab Lever
            List<LevelMark> listLevelSlabs = new List<LevelMark>();
            if (listSlabs.Count != 0)
            {
                foreach (TSM.Part slab in listSlabs)
                {
                    double Yvalue = slab.GetSolid().MaximumPoint.Y;
                    listLevelSlabs.Add(CreateNoValueLevelMark(frontView, new TSG.Point((frontView.RestrictionBox.MinPoint.X + frontView.RestrictionBox.MaxPoint.X) / 2, Yvalue, 0), vY, slab.Name + " SLAB"));
                }
            }
            #endregion

            #region Create Top Lever
            if (isContainTop)
            {
                double maxYValue = double.MinValue;
                foreach (DWallBeam dWallBeam in listShowPanel)
                {
                    Solid solid = dWallBeam.panelModel.GetSolid();
                    if (solid.MaximumPoint.Y > maxYValue)
                    {
                        maxYValue = solid.MaximumPoint.Y;
                    }
                }
                LevelMark levelTopConcrete = CreateValueLevelMark(frontView, new TSG.Point((frontView.RestrictionBox.MinPoint.X + frontView.RestrictionBox.MaxPoint.X) / 2, maxYValue, 0), vY, "CONCRETE CUT-OFF");
            }
            #endregion
            #endregion

            #region Create Horizontal View
            foreach (DWallBeam dWallBeam in listShowPanel)
            {
                dWallBeam.SetMaxMinPointByY();
                TSG.Point topDWallPoint = dWallBeam.maxPoint;
                foreach (HorizontalSectionIP horizontalSection in listShowHorizontalSection)
                {
                    if (dWallBeam.panelCageNo.Contains(horizontalSection.CageNo))
                    {
                        double YSectionValue = topDWallPoint.Y - horizontalSection.ValueFromTopCage;
                        double minXValue = frontView.RestrictionBox.MinPoint.X;
                        double maxXValue = frontView.RestrictionBox.MaxPoint.X;

                        TSD.View.ViewAttributes sectionAttributes = new TSD.View.ViewAttributes("Section View");
                        SectionMarkBase.SectionMarkAttributes sectionMarkAttributes = new SectionMarkBase.SectionMarkAttributes("DWall-Section");
                        if (horizontalSection.isFirstShow)
                        {
                            TSD.View.CreateSectionView(frontView, new TSG.Point(maxXValue, YSectionValue, 0), new TSG.Point(minXValue, YSectionValue, 0), horizontalSection.InsertPoint, 500, 0, sectionAttributes, sectionMarkAttributes, out TSD.View section, out SectionMark sectionMark);
                            Macro.Load_ViewAttribute_Filter_And_EditSetting(section, "Section View");

                            TextElement textName = new TextElement(" " + horizontalSection.Name);
                            textName.Font = new FontAttributes(DrawingColors.Green, 4, "ISOCPEUR", false, false);

                            section.Attributes.TagsAttributes.TagA1.TagContent.Add(textName);
                            section.Name = horizontalSection.Name;
                            section.Modify();

                            sectionMark.Attributes.MarkName = horizontalSection.Name;
                            sectionMark.Modify();


                            Macro.Send_View_To_Other_Drawing(section, horizontalSection.SentToDrawingName, listPanel[0].Name);
                            horizontalSection.isFirstShow = false;
                        }
                        else
                        {
                            TSD.View.CreateSectionView(frontView, new TSG.Point(maxXValue, YSectionValue, 0), new TSG.Point(minXValue, YSectionValue, 0), new TSG.Point(-300, -300, 0), 500, 0, sectionAttributes, sectionMarkAttributes, out TSD.View section, out SectionMark sectionMark);
                            Macro.Load_ViewAttribute_Filter_And_EditSetting(section, "Section View");

                            TextElement textName = new TextElement(" " + horizontalSection.Name);
                            textName.Font = new FontAttributes(DrawingColors.Green, 4, "ISOCPEUR", false, false);

                            section.Attributes.TagsAttributes.TagA1.TagContent.Add(textName);
                            section.Name = horizontalSection.Name;
                            section.Modify();

                            sectionMark.Attributes.MarkName = horizontalSection.Name;
                            sectionMark.Modify();

                            Macro.Send_View_To_Other_Drawing(section, horizontalSection.SentToDrawingName, listPanel[0].Name);
                        }
                    }
                }
            }
            #endregion

            #region Reinforcement Level Mark
            List<LevelMark> farFaceLevelMark = CreateRebarLevelMark(frontView, farFaceRebars, levelLeftXValue, isContainTop, isContainBot);
            List<LevelMark> nearFaceLevelMark = CreateRebarLevelMark(frontView, nearFaceRebars, levelRightXValue, isContainTop, isContainBot);

            foreach (LevelMark levelMark in farFaceLevelMark)
            {
                if (levelMark.BasePoint.Y > frontView.RestrictionBox.MaxPoint.Y || levelMark.BasePoint.Y < frontView.RestrictionBox.MinPoint.Y)
                {
                    levelMark.Delete();
                }
            }
            foreach (LevelMark levelMark in nearFaceLevelMark)
            {
                if (levelMark.BasePoint.Y > frontView.RestrictionBox.MaxPoint.Y || levelMark.BasePoint.Y < frontView.RestrictionBox.MinPoint.Y)
                {
                    levelMark.Delete();
                }
            }
            #endregion

            #region Positioning Reinforcement
            #region Far Face
            if (farFaceRebars.Count != 0)
            {
                #region Insert Rebar Position by Layer
                List<DWallRebar> rebarLayer1st = new List<DWallRebar>();
                List<DWallRebar> rebarLayer2nd = new List<DWallRebar>();
                List<DWallRebar> rebarLayer3rd = new List<DWallRebar>();
                List<DWallRebar> rebarLayer4th = new List<DWallRebar>();
                foreach (DWallRebar dWallRebar in farFaceRebars)
                {
                    if (dWallRebar.rebarLayer == "1st LAYER" || dWallRebar.rebarLayer == "")
                    {
                        rebarLayer1st.Add(dWallRebar);
                    }
                    else if (dWallRebar.rebarLayer == "2nd LAYER")
                    {
                        rebarLayer2nd.Add(dWallRebar);
                    }
                    else if (dWallRebar.rebarLayer == "3rd LAYER")
                    {
                        rebarLayer3rd.Add(dWallRebar);
                    }
                    else if (dWallRebar.rebarLayer == "4th LAYER")
                    {
                        rebarLayer4th.Add(dWallRebar);
                    }
                }

                if (rebarLayer1st.Count != 0)
                {
                    rebarLayer1st = rebarLayer1st.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer1st.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            rebarLayer1st[i].rebarPosition = 1;
                        }
                        else
                        {
                            rebarLayer1st[i].rebarPosition = 2;
                        }
                    }
                }

                if (rebarLayer2nd.Count != 0)
                {
                    rebarLayer2nd = rebarLayer2nd.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer2nd.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            rebarLayer2nd[i].rebarPosition = 3;
                        }
                        else
                        {
                            rebarLayer2nd[i].rebarPosition = 4;
                        }
                    }
                }

                if (rebarLayer3rd.Count != 0)
                {
                    rebarLayer3rd = rebarLayer3rd.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer3rd.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            rebarLayer3rd[i].rebarPosition = 5;
                        }
                        else
                        {
                            rebarLayer3rd[i].rebarPosition = 6;
                        }
                    }
                }

                if (rebarLayer4th.Count != 0)
                {
                    rebarLayer4th = rebarLayer4th.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer4th.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            rebarLayer4th[i].rebarPosition = 7;
                        }
                        else
                        {
                            rebarLayer4th[i].rebarPosition = 8;
                        }
                    }
                }
                #endregion

                #region Positioning
                foreach (DWallRebar dWallRebar in farFaceRebars)
                {
                    ReinforcementGroup rebar = dWallRebar.rebarDrawing as ReinforcementGroup;
                    RebarGroup rebarModel = dWallRebar.rebarModel;
                    rebarModel.Select();
                    if (dWallRebar.rebarPosition == 1)
                    {
                        if (rebarModel.StartPoint.X < rebarModel.EndPoint.X)
                        {
                            rebar.ReinforcementCustomPosition = 0.1;
                        }
                        else
                        {
                            rebar.ReinforcementCustomPosition = 0.9;
                        }
                        rebar.Modify();
                    }
                    else if (dWallRebar.rebarPosition == 2)
                    {
                        if (rebarModel.StartPoint.X < rebarModel.EndPoint.X)
                        {
                            rebar.ReinforcementCustomPosition = 0.14;
                        }
                        else
                        {
                            rebar.ReinforcementCustomPosition = 0.86;
                        }
                        rebar.Modify();
                    }
                    else if (dWallRebar.rebarPosition == 3)
                    {
                        if (rebarModel.StartPoint.X < rebarModel.EndPoint.X)
                        {
                            rebar.ReinforcementCustomPosition = 0.18;
                        }
                        else
                        {
                            rebar.ReinforcementCustomPosition = 0.82;
                        }
                        rebar.Modify();
                    }
                    else if (dWallRebar.rebarPosition == 4)
                    {
                        if (rebarModel.StartPoint.X < rebarModel.EndPoint.X)
                        {
                            rebar.ReinforcementCustomPosition = 0.22;
                        }
                        else
                        {
                            rebar.ReinforcementCustomPosition = 0.78;
                        }
                        ;
                        rebar.Modify();
                    }
                    else if (dWallRebar.rebarPosition == 5)
                    {
                        if (rebarModel.StartPoint.X < rebarModel.EndPoint.X)
                        {
                            rebar.ReinforcementCustomPosition = 0.26;
                        }
                        else
                        {
                            rebar.ReinforcementCustomPosition = 0.74;
                        }
                        rebar.Modify();
                    }
                    else if (dWallRebar.rebarPosition == 6)
                    {
                        if (rebarModel.StartPoint.X < rebarModel.EndPoint.X)
                        {
                            rebar.ReinforcementCustomPosition = 0.3;
                        }
                        else
                        {
                            rebar.ReinforcementCustomPosition = 0.7;
                        }
                        rebar.Modify();
                    }
                    else if (dWallRebar.rebarPosition == 7)
                    {
                        if (rebarModel.StartPoint.X < rebarModel.EndPoint.X)
                        {
                            rebar.ReinforcementCustomPosition = 0.34;
                        }
                        else
                        {
                            rebar.ReinforcementCustomPosition = 0.66;
                        }
                        rebar.Modify();
                    }
                    else if (dWallRebar.rebarPosition == 8)
                    {
                        if (rebarModel.StartPoint.X < rebarModel.EndPoint.X)
                        {
                            rebar.ReinforcementCustomPosition = 0.38;
                        }
                        else
                        {
                            rebar.ReinforcementCustomPosition = 0.62;
                        }
                        rebar.Modify();
                    }
                }
                #endregion
            }
            #endregion

            #region Near Face
            if (nearFaceRebars.Count != 0)
            {
                #region Insert Rebar Position by Layer
                List<DWallRebar> rebarLayer1st = new List<DWallRebar>();
                List<DWallRebar> rebarLayer2nd = new List<DWallRebar>();
                List<DWallRebar> rebarLayer3rd = new List<DWallRebar>();
                List<DWallRebar> rebarLayer4th = new List<DWallRebar>();
                foreach (DWallRebar dWallRebar in nearFaceRebars)
                {
                    if (dWallRebar.rebarLayer == "1st LAYER" || dWallRebar.rebarLayer == "")
                    {
                        rebarLayer1st.Add(dWallRebar);
                    }
                    else if (dWallRebar.rebarLayer == "2nd LAYER")
                    {
                        rebarLayer2nd.Add(dWallRebar);
                    }
                    else if (dWallRebar.rebarLayer == "3rd LAYER")
                    {
                        rebarLayer3rd.Add(dWallRebar);
                    }
                    else if (dWallRebar.rebarLayer == "4th LAYER")
                    {
                        rebarLayer4th.Add(dWallRebar);
                    }
                }

                if (rebarLayer1st.Count != 0)
                {
                    rebarLayer1st = rebarLayer1st.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer1st.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            rebarLayer1st[i].rebarPosition = 1;
                        }
                        else
                        {
                            rebarLayer1st[i].rebarPosition = 2;
                        }
                    }
                }

                if (rebarLayer2nd.Count != 0)
                {
                    rebarLayer2nd = rebarLayer2nd.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer2nd.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            rebarLayer2nd[i].rebarPosition = 3;
                        }
                        else
                        {
                            rebarLayer2nd[i].rebarPosition = 4;
                        }
                    }
                }

                if (rebarLayer3rd.Count != 0)
                {
                    rebarLayer3rd = rebarLayer3rd.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer3rd.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            rebarLayer3rd[i].rebarPosition = 5;
                        }
                        else
                        {
                            rebarLayer3rd[i].rebarPosition = 6;
                        }
                    }
                }

                if (rebarLayer4th.Count != 0)
                {
                    rebarLayer4th = rebarLayer4th.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer4th.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            rebarLayer4th[i].rebarPosition = 7;
                        }
                        else
                        {
                            rebarLayer4th[i].rebarPosition = 8;
                        }
                    }
                }
                #endregion

                #region Positioning
                double startValue = 0.62;
                if (rebarLayer4th.Count == 0)
                {
                    startValue = 0.7;
                }
                if (rebarLayer4th.Count == 0 && rebarLayer3rd.Count == 0)
                {
                    startValue = 0.78;
                }
                if (rebarLayer4th.Count == 0 && rebarLayer3rd.Count == 0 && rebarLayer2nd.Count == 0)
                {
                    startValue = 0.86;
                }

                foreach (DWallRebar dWallRebar in nearFaceRebars)
                {
                    ReinforcementGroup rebar = dWallRebar.rebarDrawing as ReinforcementGroup;
                    RebarGroup rebarModel = dWallRebar.rebarModel;
                    rebarModel.Select();
                    if (dWallRebar.rebarPosition == 1)
                    {
                        if (rebarModel.StartPoint.X < rebarModel.EndPoint.X)
                        {
                            rebar.ReinforcementCustomPosition = startValue;
                        }
                        else
                        {
                            rebar.ReinforcementCustomPosition = 1 - startValue;
                        }
                        rebar.Modify();
                    }
                    else if (dWallRebar.rebarPosition == 2)
                    {
                        if (rebarModel.StartPoint.X < rebarModel.EndPoint.X)
                        {
                            rebar.ReinforcementCustomPosition = startValue + 0.04;
                        }
                        else
                        {
                            rebar.ReinforcementCustomPosition = 0.96 - startValue;
                        }
                        rebar.Modify();
                    }
                    else if (dWallRebar.rebarPosition == 3)
                    {
                        if (rebarModel.StartPoint.X < rebarModel.EndPoint.X)
                        {
                            rebar.ReinforcementCustomPosition = startValue + 0.08;
                        }
                        else
                        {
                            rebar.ReinforcementCustomPosition = 0.92 - startValue;
                        }
                        rebar.Modify();
                    }
                    else if (dWallRebar.rebarPosition == 4)
                    {
                        if (rebarModel.StartPoint.X < rebarModel.EndPoint.X)
                        {
                            rebar.ReinforcementCustomPosition = startValue + 0.12;
                        }
                        else
                        {
                            rebar.ReinforcementCustomPosition = 0.88 - startValue;
                        }
                        rebar.Modify();
                    }
                    else if (dWallRebar.rebarPosition == 5)
                    {
                        if (rebarModel.StartPoint.X < rebarModel.EndPoint.X)
                        {
                            rebar.ReinforcementCustomPosition = startValue + 0.16;
                        }
                        else
                        {
                            rebar.ReinforcementCustomPosition = 0.84 - startValue;
                        }
                        rebar.Modify();
                    }
                    else if (dWallRebar.rebarPosition == 6)
                    {
                        if (rebarModel.StartPoint.X < rebarModel.EndPoint.X)
                        {
                            rebar.ReinforcementCustomPosition = startValue + 0.2;
                        }
                        else
                        {
                            rebar.ReinforcementCustomPosition = 0.8 - startValue;
                        }
                        rebar.Modify();
                    }
                    else if (dWallRebar.rebarPosition == 7)
                    {
                        if (rebarModel.StartPoint.X < rebarModel.EndPoint.X)
                        {
                            rebar.ReinforcementCustomPosition = startValue + 0.24;
                        }
                        else
                        {
                            rebar.ReinforcementCustomPosition = 0.76 - startValue;
                        }
                        rebar.Modify();
                    }
                    else if (dWallRebar.rebarPosition == 8)
                    {
                        if (rebarModel.StartPoint.X < rebarModel.EndPoint.X)
                        {
                            rebar.ReinforcementCustomPosition = startValue + 0.28;
                        }
                        else
                        {
                            rebar.ReinforcementCustomPosition = 0.72 - startValue;
                        }
                        rebar.Modify();
                    }
                }
                #endregion
            }
            #endregion
            #endregion

            #region Vertical Dim
            double leftDimValue = -1700;
            double rightDimValue = 3300;
            #region Dim lap
            #region Far Face
            if (farFaceRebars.Count != 0)
            {
                #region Insert Rebar by Layer
                List<DWallRebar> rebarLayer1st = new List<DWallRebar>();
                List<DWallRebar> rebarLayer2nd = new List<DWallRebar>();
                List<DWallRebar> rebarLayer3rd = new List<DWallRebar>();
                List<DWallRebar> rebarLayer4th = new List<DWallRebar>();
                foreach (DWallRebar dWallRebar in farFaceRebars)
                {
                    if (dWallRebar.rebarLayer == "1st LAYER" || dWallRebar.rebarLayer == "")
                    {
                        rebarLayer1st.Add(dWallRebar);
                    }
                    else if (dWallRebar.rebarLayer == "2nd LAYER")
                    {
                        rebarLayer2nd.Add(dWallRebar);
                    }
                    else if (dWallRebar.rebarLayer == "3rd LAYER")
                    {
                        rebarLayer3rd.Add(dWallRebar);
                    }
                    else if (dWallRebar.rebarLayer == "4th LAYER")
                    {
                        rebarLayer4th.Add(dWallRebar);
                    }
                }
                #endregion

                #region Dim lap
                List<List<TSG.Point>> listLapPoint = new List<List<TSG.Point>>();
                if (rebarLayer1st.Count != 0)
                {
                    rebarLayer1st = rebarLayer1st.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer1st.Count - 1; i++)
                    {
                        TSG.Point firstBotPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer1st[i].botRebarPoint.Y, 0);
                        TSG.Point secondTopPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer1st[i + 1].topRebarPoint.Y, 0);

                        if (secondTopPoint.Y > firstBotPoint.Y)
                        {
                            List<TSG.Point> lapPoint = new List<TSG.Point>
                            {
                                secondTopPoint,
                                firstBotPoint
                            };
                            listLapPoint.Add(lapPoint);
                        }
                    }
                }

                if (rebarLayer2nd.Count != 0)
                {
                    rebarLayer2nd = rebarLayer2nd.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer2nd.Count - 1; i++)
                    {
                        TSG.Point firstBotPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer2nd[i].botRebarPoint.Y, 0);
                        TSG.Point secondTopPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer2nd[i + 1].topRebarPoint.Y, 0);

                        if (secondTopPoint.Y > firstBotPoint.Y)
                        {
                            List<TSG.Point> lapPoint = new List<TSG.Point>
                            {
                                secondTopPoint,
                                firstBotPoint
                            };
                            listLapPoint.Add(lapPoint);
                        }
                    }
                }

                if (rebarLayer3rd.Count != 0)
                {
                    rebarLayer3rd = rebarLayer3rd.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer3rd.Count - 1; i++)
                    {
                        TSG.Point firstBotPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer3rd[i].botRebarPoint.Y, 0);
                        TSG.Point secondTopPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer3rd[i + 1].topRebarPoint.Y, 0);

                        if (secondTopPoint.Y > firstBotPoint.Y)
                        {
                            List<TSG.Point> lapPoint = new List<TSG.Point>
                            {
                                secondTopPoint,
                                firstBotPoint
                            };
                            listLapPoint.Add(lapPoint);
                        }
                    }
                }

                if (rebarLayer4th.Count != 0)
                {
                    rebarLayer4th = rebarLayer4th.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer4th.Count - 1; i++)
                    {
                        TSG.Point firstBotPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer4th[i].botRebarPoint.Y, 0);
                        TSG.Point secondTopPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer4th[i + 1].topRebarPoint.Y, 0);

                        if (secondTopPoint.Y > firstBotPoint.Y)
                        {
                            List<TSG.Point> lapPoint = new List<TSG.Point>
                            {
                                secondTopPoint,
                                firstBotPoint
                            };
                            listLapPoint.Add(lapPoint);
                        }
                    }
                }

                listLapPoint = listLapPoint.Distinct(new PointListComparer()).OrderByDescending(n => n[0].Y).ThenBy(n => n[1].Y).ToList();

                for (int i = 0; i < listLapPoint.Count; i++)
                {
                    if (i == 0)
                    {
                        TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, listLapPoint[i][0].Y, 0);
                        TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, listLapPoint[i][1].Y, 0);
                        PointList pointList = new PointList()
                        {
                            topPoint,
                            botPoint
                        };
                        sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue, StraightDimAttribute);
                    }
                    else
                    {
                        if (listLapPoint[i][0].Y <= listLapPoint[i - 1][1].Y)
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, listLapPoint[i][0].Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, listLapPoint[i][1].Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue, StraightDimAttribute);
                        }
                        else
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, listLapPoint[i][0].Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, listLapPoint[i][1].Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue + 350, StraightDimAttribute);
                        }
                    }
                }
                #endregion
            }
            #endregion

            #region Near Face
            if (nearFaceRebars.Count != 0)
            {
                #region Insert Rebar by Layer
                List<DWallRebar> rebarLayer1st = new List<DWallRebar>();
                List<DWallRebar> rebarLayer2nd = new List<DWallRebar>();
                List<DWallRebar> rebarLayer3rd = new List<DWallRebar>();
                List<DWallRebar> rebarLayer4th = new List<DWallRebar>();
                foreach (DWallRebar dWallRebar in nearFaceRebars)
                {
                    if (dWallRebar.rebarLayer == "1st LAYER" || dWallRebar.rebarLayer == "")
                    {
                        rebarLayer1st.Add(dWallRebar);
                    }
                    else if (dWallRebar.rebarLayer == "2nd LAYER")
                    {
                        rebarLayer2nd.Add(dWallRebar);
                    }
                    else if (dWallRebar.rebarLayer == "3rd LAYER")
                    {
                        rebarLayer3rd.Add(dWallRebar);
                    }
                    else if (dWallRebar.rebarLayer == "4th LAYER")
                    {
                        rebarLayer4th.Add(dWallRebar);
                    }
                }
                #endregion

                #region Dim lap
                List<List<TSG.Point>> listLapPoint = new List<List<TSG.Point>>();
                if (rebarLayer1st.Count != 0)
                {
                    rebarLayer1st = rebarLayer1st.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer1st.Count - 1; i++)
                    {
                        TSG.Point firstBotPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer1st[i].botRebarPoint.Y, 0);
                        TSG.Point secondTopPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer1st[i + 1].topRebarPoint.Y, 0);

                        if (secondTopPoint.Y > firstBotPoint.Y)
                        {
                            List<TSG.Point> lapPoint = new List<TSG.Point>
                            {
                                secondTopPoint,
                                firstBotPoint
                            };
                            listLapPoint.Add(lapPoint);
                        }
                    }
                }

                if (rebarLayer2nd.Count != 0)
                {
                    rebarLayer2nd = rebarLayer2nd.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer2nd.Count - 1; i++)
                    {
                        TSG.Point firstBotPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer2nd[i].botRebarPoint.Y, 0);
                        TSG.Point secondTopPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer2nd[i + 1].topRebarPoint.Y, 0);

                        if (secondTopPoint.Y > firstBotPoint.Y)
                        {
                            List<TSG.Point> lapPoint = new List<TSG.Point>
                            {
                                secondTopPoint,
                                firstBotPoint
                            };
                            listLapPoint.Add(lapPoint);
                        }
                    }
                }

                if (rebarLayer3rd.Count != 0)
                {
                    rebarLayer3rd = rebarLayer3rd.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer3rd.Count - 1; i++)
                    {
                        TSG.Point firstBotPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer3rd[i].botRebarPoint.Y, 0);
                        TSG.Point secondTopPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer3rd[i + 1].topRebarPoint.Y, 0);

                        if (secondTopPoint.Y > firstBotPoint.Y)
                        {
                            List<TSG.Point> lapPoint = new List<TSG.Point>
                            {
                                secondTopPoint,
                                firstBotPoint
                            };
                            listLapPoint.Add(lapPoint);
                        }
                    }
                }

                if (rebarLayer4th.Count != 0)
                {
                    rebarLayer4th = rebarLayer4th.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer4th.Count - 1; i++)
                    {
                        TSG.Point firstBotPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer4th[i].botRebarPoint.Y, 0);
                        TSG.Point secondTopPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer4th[i + 1].topRebarPoint.Y, 0);

                        if (secondTopPoint.Y > firstBotPoint.Y)
                        {
                            List<TSG.Point> lapPoint = new List<TSG.Point>
                            {
                                secondTopPoint,
                                firstBotPoint
                            };
                            listLapPoint.Add(lapPoint);
                        }
                    }
                }

                listLapPoint = listLapPoint.Distinct(new PointListComparer()).OrderByDescending(n => n[0].Y).ThenBy(n => n[1].Y).ToList();

                for (int i = 0; i < listLapPoint.Count; i++)
                {
                    if (i == 0)
                    {
                        TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, listLapPoint[i][0].Y, 0);
                        TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, listLapPoint[i][1].Y, 0);
                        PointList pointList = new PointList()
                        {
                            topPoint,
                            botPoint
                        };
                        sdsh.CreateDimensionSet(frontView, pointList, vX, rightDimValue, StraightDimAttribute);
                    }
                    else
                    {
                        if (listLapPoint[i][0].Y <= listLapPoint[i - 1][1].Y)
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, listLapPoint[i][0].Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, listLapPoint[i][1].Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            sdsh.CreateDimensionSet(frontView, pointList, vX, rightDimValue, StraightDimAttribute);
                        }
                        else
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, listLapPoint[i][0].Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, listLapPoint[i][1].Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            sdsh.CreateDimensionSet(frontView, pointList, vX, rightDimValue - 350, StraightDimAttribute);
                        }
                    }
                }
                #endregion
            }
            #endregion
            #endregion

            #region Dim by Rebar Layer
            StraightDimensionSetAttributes StraightDim1stLayerAttribute = new StraightDimensionSetAttributes("DWall-Dim-1stLayer");
            StraightDimensionSetAttributes StraightDim2ndLayerAttribute = new StraightDimensionSetAttributes("DWall-Dim-2ndLayer");
            StraightDimensionSetAttributes StraightDim3rdLayerAttribute = new StraightDimensionSetAttributes("DWall-Dim-3rdLayer");
            StraightDimensionSetAttributes StraightDim4thLayerAttribute = new StraightDimensionSetAttributes("DWall-Dim-4thLayer");
            #region Far Face
            if (farFaceRebars.Count != 0)
            {
                #region Insert Rebar by Layer
                List<DWallRebar> rebarLayer1st = new List<DWallRebar>();
                List<DWallRebar> rebarLayer2nd = new List<DWallRebar>();
                List<DWallRebar> rebarLayer3rd = new List<DWallRebar>();
                List<DWallRebar> rebarLayer4th = new List<DWallRebar>();
                foreach (DWallRebar dWallRebar in farFaceRebars)
                {
                    if (dWallRebar.topRebarPoint.Y <= frontView.RestrictionBox.MaxPoint.Y && dWallRebar.botRebarPoint.Y >= frontView.RestrictionBox.MinPoint.Y)
                    {
                        if (dWallRebar.rebarLayer == "1st LAYER" || dWallRebar.rebarLayer == "")
                        {
                            rebarLayer1st.Add(dWallRebar);
                        }
                        else if (dWallRebar.rebarLayer == "2nd LAYER")
                        {
                            rebarLayer2nd.Add(dWallRebar);
                        }
                        else if (dWallRebar.rebarLayer == "3rd LAYER")
                        {
                            rebarLayer3rd.Add(dWallRebar);
                        }
                        else if (dWallRebar.rebarLayer == "4th LAYER")
                        {
                            rebarLayer4th.Add(dWallRebar);
                        }
                    }
                }
                #endregion

                #region Dim by Layer
                if (rebarLayer4th.Count != 0)
                {
                    if (rebarLayer4th.Count != 1)
                    {
                        leftDimValue -= 350;
                    }
                    rebarLayer4th = rebarLayer4th.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer4th.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer4th[i].topRebarPoint.Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer4th[i].botRebarPoint.Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue - 350, StraightDim4thLayerAttribute);
                        }
                        else
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer4th[i].topRebarPoint.Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer4th[i].botRebarPoint.Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue, StraightDim4thLayerAttribute);
                        }
                    }
                    leftDimValue -= 350;
                }

                if (rebarLayer3rd.Count != 0)
                {
                    if (rebarLayer3rd.Count != 1)
                    {
                        leftDimValue -= 350;
                    }
                    rebarLayer3rd = rebarLayer3rd.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer3rd.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer3rd[i].topRebarPoint.Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer3rd[i].botRebarPoint.Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue - 350, StraightDim3rdLayerAttribute);
                        }
                        else
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer3rd[i].topRebarPoint.Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer3rd[i].botRebarPoint.Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue, StraightDim3rdLayerAttribute);
                        }
                    }
                    leftDimValue -= 350;
                }

                if (rebarLayer2nd.Count != 0)
                {
                    if (rebarLayer2nd.Count != 1)
                    {
                        leftDimValue -= 350;
                    }
                    rebarLayer2nd = rebarLayer2nd.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer2nd.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer2nd[i].topRebarPoint.Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer2nd[i].botRebarPoint.Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue - 350, StraightDim2ndLayerAttribute);
                        }
                        else
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer2nd[i].topRebarPoint.Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer2nd[i].botRebarPoint.Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue, StraightDim2ndLayerAttribute);
                        }
                    }
                    leftDimValue -= 350;
                }

                if (rebarLayer1st.Count != 0)
                {
                    if (rebarLayer1st.Count != 1)
                    {
                        leftDimValue -= 350;
                    }
                    rebarLayer1st = rebarLayer1st.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer1st.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer1st[i].topRebarPoint.Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer1st[i].botRebarPoint.Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            if (rebarLayer1st[i].rebarLayer == "")
                            {
                                sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue - 350, StraightDimAttribute);
                            }
                            else
                            {
                                sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue - 350, StraightDim1stLayerAttribute);
                            }
                        }
                        else
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer1st[i].topRebarPoint.Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, rebarLayer1st[i].botRebarPoint.Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            if (rebarLayer1st[i].rebarLayer == "")
                            {
                                sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue, StraightDimAttribute);
                            }
                            else
                            {
                                sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue, StraightDim1stLayerAttribute);
                            }
                        }
                    }
                    leftDimValue -= 700;
                }
                #endregion
            }
            #endregion

            #region Near Face
            if (nearFaceRebars.Count != 0)
            {
                #region Insert Rebar by Layer
                List<DWallRebar> rebarLayer1st = new List<DWallRebar>();
                List<DWallRebar> rebarLayer2nd = new List<DWallRebar>();
                List<DWallRebar> rebarLayer3rd = new List<DWallRebar>();
                List<DWallRebar> rebarLayer4th = new List<DWallRebar>();
                foreach (DWallRebar dWallRebar in nearFaceRebars)
                {
                    if (dWallRebar.topRebarPoint.Y <= frontView.RestrictionBox.MaxPoint.Y && dWallRebar.botRebarPoint.Y >= frontView.RestrictionBox.MinPoint.Y)
                    {
                        if (dWallRebar.rebarLayer == "1st LAYER" || dWallRebar.rebarLayer == "")
                        {
                            rebarLayer1st.Add(dWallRebar);
                        }
                        else if (dWallRebar.rebarLayer == "2nd LAYER")
                        {
                            rebarLayer2nd.Add(dWallRebar);
                        }
                        else if (dWallRebar.rebarLayer == "3rd LAYER")
                        {
                            rebarLayer3rd.Add(dWallRebar);
                        }
                        else if (dWallRebar.rebarLayer == "4th LAYER")
                        {
                            rebarLayer4th.Add(dWallRebar);
                        }
                    }
                }
                #endregion

                #region Dim by Layer
                if (rebarLayer4th.Count != 0)
                {
                    if (rebarLayer4th.Count != 1)
                    {
                        rightDimValue += 350;
                    }
                    rebarLayer4th = rebarLayer4th.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer4th.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer4th[i].topRebarPoint.Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer4th[i].botRebarPoint.Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            sdsh.CreateDimensionSet(frontView, pointList, vX, rightDimValue + 350, StraightDim4thLayerAttribute);
                        }
                        else
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer4th[i].topRebarPoint.Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer4th[i].botRebarPoint.Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            sdsh.CreateDimensionSet(frontView, pointList, vX, rightDimValue, StraightDim4thLayerAttribute);
                        }
                    }
                    rightDimValue += 350;
                }

                if (rebarLayer3rd.Count != 0)
                {
                    if (rebarLayer3rd.Count != 1)
                    {
                        rightDimValue += 350;
                    }
                    rebarLayer3rd = rebarLayer3rd.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer3rd.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer3rd[i].topRebarPoint.Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer3rd[i].botRebarPoint.Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            sdsh.CreateDimensionSet(frontView, pointList, vX, rightDimValue + 350, StraightDim3rdLayerAttribute);
                        }
                        else
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer3rd[i].topRebarPoint.Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer3rd[i].botRebarPoint.Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            sdsh.CreateDimensionSet(frontView, pointList, vX, rightDimValue, StraightDim3rdLayerAttribute);
                        }
                    }
                    rightDimValue += 350;
                }

                if (rebarLayer2nd.Count != 0)
                {
                    if (rebarLayer2nd.Count != 1)
                    {
                        rightDimValue += 350;
                    }
                    rebarLayer2nd = rebarLayer2nd.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer2nd.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer2nd[i].topRebarPoint.Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer2nd[i].botRebarPoint.Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            sdsh.CreateDimensionSet(frontView, pointList, vX, rightDimValue + 350, StraightDim2ndLayerAttribute);
                        }
                        else
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer2nd[i].topRebarPoint.Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer2nd[i].botRebarPoint.Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            sdsh.CreateDimensionSet(frontView, pointList, vX, rightDimValue, StraightDim2ndLayerAttribute);
                        }
                    }
                    rightDimValue += 350;
                }

                if (rebarLayer1st.Count != 0)
                {
                    if (rebarLayer1st.Count != 1)
                    {
                        rightDimValue += 350;
                    }
                    rebarLayer1st = rebarLayer1st.OrderByDescending(n => n.topRebarPoint.Y).ToList();
                    for (int i = 0; i < rebarLayer1st.Count; i++)
                    {
                        if (i % 2 == 0)
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer1st[i].topRebarPoint.Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer1st[i].botRebarPoint.Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            if (rebarLayer1st[i].rebarLayer == "")
                            {
                                sdsh.CreateDimensionSet(frontView, pointList, vX, rightDimValue + 350, StraightDimAttribute);
                            }
                            else
                            {
                                sdsh.CreateDimensionSet(frontView, pointList, vX, rightDimValue + 350, StraightDim1stLayerAttribute);
                            }
                        }
                        else
                        {
                            TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer1st[i].topRebarPoint.Y, 0);
                            TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MaxPoint.X, rebarLayer1st[i].botRebarPoint.Y, 0);
                            PointList pointList = new PointList()
                            {
                                topPoint,
                                botPoint
                            };
                            if (rebarLayer1st[i].rebarLayer == "")
                            {
                                sdsh.CreateDimensionSet(frontView, pointList, vX, rightDimValue, StraightDimAttribute);
                            }
                            else
                            {
                                sdsh.CreateDimensionSet(frontView, pointList, vX, rightDimValue, StraightDim1stLayerAttribute);
                            }
                        }
                    }
                    rightDimValue += 350;
                }
                #endregion
            }
            #endregion
            #endregion

            #region Dim by Cage No
            StraightDimensionSetAttributes StraightDim1stCageAttribute = new StraightDimensionSetAttributes("DWall-Dim-1stCage");
            StraightDimensionSetAttributes StraightDim2ndCageAttribute = new StraightDimensionSetAttributes("DWall-Dim-2ndCage");
            StraightDimensionSetAttributes StraightDim3rdCageAttribute = new StraightDimensionSetAttributes("DWall-Dim-3rdCage");
            StraightDimensionSetAttributes StraightDim4thCageAttribute = new StraightDimensionSetAttributes("DWall-Dim-4thCage");
            if (cage1stRebars.Count != 0)
            {
                TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, cage1stRebars[0].topRebarPoint.Y, 0);
                TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, cage1stRebars[0].botRebarPoint.Y, 0);
                foreach (DWallRebar dWallRebar in cage1stRebars)
                {
                    if (dWallRebar.topRebarPoint.Y > topPoint.Y)
                    {
                        topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, dWallRebar.topRebarPoint.Y, 0);
                    }
                    if (dWallRebar.botRebarPoint.Y < botPoint.Y)
                    {
                        botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, dWallRebar.botRebarPoint.Y, 0);
                    }
                }

                if (topPoint.Y <= frontView.RestrictionBox.MaxPoint.Y && botPoint.Y >= frontView.RestrictionBox.MinPoint.Y)
                {
                    PointList pointList = new PointList()
                    {
                        topPoint,
                        botPoint
                    };
                    sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue - 350, StraightDim1stCageAttribute);
                }
            }
            if (cage2ndRebars.Count != 0)
            {
                TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, cage2ndRebars[0].topRebarPoint.Y, 0);
                TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, cage2ndRebars[0].botRebarPoint.Y, 0);
                foreach (DWallRebar dWallRebar in cage2ndRebars)
                {
                    if (dWallRebar.topRebarPoint.Y > topPoint.Y)
                    {
                        topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, dWallRebar.topRebarPoint.Y, 0);
                    }
                    if (dWallRebar.botRebarPoint.Y < botPoint.Y)
                    {
                        botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, dWallRebar.botRebarPoint.Y, 0);
                    }
                }

                if (topPoint.Y <= frontView.RestrictionBox.MaxPoint.Y && botPoint.Y >= frontView.RestrictionBox.MinPoint.Y)
                {
                    PointList pointList = new PointList()
                    {
                        topPoint,
                        botPoint
                    };
                    sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue, StraightDim2ndCageAttribute);
                }
            }
            if (cage3rdRebars.Count != 0)
            {
                TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, cage3rdRebars[0].topRebarPoint.Y, 0);
                TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, cage3rdRebars[0].botRebarPoint.Y, 0);
                foreach (DWallRebar dWallRebar in cage3rdRebars)
                {
                    if (dWallRebar.topRebarPoint.Y > topPoint.Y)
                    {
                        topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, dWallRebar.topRebarPoint.Y, 0);
                    }
                    if (dWallRebar.botRebarPoint.Y < botPoint.Y)
                    {
                        botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, dWallRebar.botRebarPoint.Y, 0);
                    }
                }

                if (topPoint.Y <= frontView.RestrictionBox.MaxPoint.Y && botPoint.Y >= frontView.RestrictionBox.MinPoint.Y)
                {
                    PointList pointList = new PointList()
                    {
                        topPoint,
                        botPoint
                    };
                    sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue - 350, StraightDim3rdCageAttribute);
                }
            }
            if (cage4thRebars.Count != 0)
            {
                TSG.Point topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, cage4thRebars[0].topRebarPoint.Y, 0);
                TSG.Point botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, cage4thRebars[0].botRebarPoint.Y, 0);
                foreach (DWallRebar dWallRebar in cage4thRebars)
                {
                    if (dWallRebar.topRebarPoint.Y > topPoint.Y)
                    {
                        topPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, dWallRebar.topRebarPoint.Y, 0);
                    }
                    if (dWallRebar.botRebarPoint.Y < botPoint.Y)
                    {
                        botPoint = new TSG.Point(frontView.RestrictionBox.MinPoint.X, dWallRebar.botRebarPoint.Y, 0);
                    }
                }

                if (topPoint.Y <= frontView.RestrictionBox.MaxPoint.Y && botPoint.Y >= frontView.RestrictionBox.MinPoint.Y)
                {
                    PointList pointList = new PointList()
                    {
                        topPoint,
                        botPoint
                    };
                    sdsh.CreateDimensionSet(frontView, pointList, vX, leftDimValue, StraightDim4thCageAttribute);
                }
            }
            #endregion
            #endregion

            #region Vertical Section
            if (allNFFFRebars.Count != 0)
            {
                double minY = frontView.RestrictionBox.MinPoint.Y;
                double maxY = frontView.RestrictionBox.MaxPoint.Y;
                TSD.View.ViewAttributes sectionAttributes = new TSD.View.ViewAttributes(cageName + " Front Section");
                SectionMarkBase.SectionMarkAttributes sectionMarkAttributes = new SectionMarkBase.SectionMarkAttributes("DWall-VerticalSection");
                TSD.View.CreateSectionView(frontView, new TSG.Point(cageStartX, maxY, 0), new TSG.Point(cageStartX, minY, 0), new TSG.Point(210, 240, 0), cageEndX - cageStartX + 50, 50, sectionAttributes, sectionMarkAttributes, out TSD.View verticalSection, out SectionMark verticalSectionMark);
                Macro.Load_ViewAttribute_Filter_And_EditSetting(verticalSection, cageName + " Front Section");
                verticalSection.Modify();

                string cageN = cageName.Replace("CAGE ", "");
                verticalSection.Name = "1" + cageN;
                verticalSection.Modify();

                verticalSectionMark.Attributes.MarkName = "1" + cageN;
                verticalSectionMark.Modify();

                EditVerticalSection(verticalSection, listPanel, listSlabs, cageName, listSplit, isContainTop, isContainBot);
            }
            #endregion

            #region Rotate 90 view
            frontView.RotateViewOnDrawingPlane(90);
            frontView.Select();
            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(GetTransformationPlane(frontView));
            frontView.Select();
            #endregion

            #region Modify Cage Dim
            foreach (StraightDimensionSet straightDimension in list500)
            {
                straightDimension.Select();
                straightDimension.Distance = -500;
                straightDimension.Modify();
            }

            dim1200.Select();
            dim1200.Distance = -1200;
            dim1200.Modify();

            //frontView.Modify();
            #endregion
        }
        private void EditVerticalSection(TSD.View sectionView, List<TSM.Part> listPanel, List<TSM.Part> listSlabs, string cageName, string listSplit, bool isContainTop, bool isContainBot)
        {
            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(GetTransformationPlane(sectionView));
            sectionView.Select();

            StraightDimensionSetHandler sdsh = new StraightDimensionSetHandler();
            StraightDimensionSetAttributes StraightDimAttribute = new StraightDimensionSetAttributes("DWall-Dim");
            StraightDimensionSetAttributes StraightDimAttributeP2 = new StraightDimensionSetAttributes("DWall-DimP2");

            double levelLeftXValue = sectionView.RestrictionBox.MinPoint.X - 750;
            double levelRightXValue = sectionView.RestrictionBox.MaxPoint.X + 550;

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

            #region Horizontal Dim
            PointList pointListDimP = new PointList()
                {
                    new TSG.Point(sectionView.RestrictionBox.MinPoint.X, sectionView.RestrictionBox.MinPoint.Y, 0),
                    new TSG.Point(sectionView.RestrictionBox.MaxPoint.X, sectionView.RestrictionBox.MinPoint.Y, 0)
                };
            sdsh.CreateDimensionSet(sectionView, pointListDimP, vY, -800, StraightDimAttributeP2);
            sectionView.Modify();
            #endregion

            #region Neighbor Connect
            #region Left
            string farFaceString = "SOIL FACE";
            if (farFaceString != "")
            {
                #region Arrow Line
                TSG.Point firstPoint = new TSG.Point(sectionView.RestrictionBox.MinPoint.X - 550, sectionView.RestrictionBox.MinPoint.Y - 1450, 0);
                TSG.Point secondPoint = new TSG.Point(sectionView.RestrictionBox.MinPoint.X - 2150, sectionView.RestrictionBox.MinPoint.Y - 1450, 0);
                TSD.Line line = new TSD.Line(sectionView, firstPoint, secondPoint, new TSD.Line.LineAttributes("Panel-Arrow"));
                line.Insert();
                #endregion

                #region Panel Text
                TSG.Point insertPoint = firstPoint - vX * 500 + vY * 20;
                Text text = new Text(sectionView, insertPoint, farFaceString, new TextAttributes("CENTER-TEXT"));
                text.Insert();
                #endregion
            }
            #endregion

            #region Right
            string nearFaceString = "EXCAVATION FACE";
            if (nearFaceString != "")
            {
                #region Arrow Line
                TSG.Point firstPoint = new TSG.Point(sectionView.RestrictionBox.MaxPoint.X + 550, sectionView.RestrictionBox.MinPoint.Y - 1450, 0);
                TSG.Point secondPoint = new TSG.Point(sectionView.RestrictionBox.MaxPoint.X + 2150, sectionView.RestrictionBox.MinPoint.Y - 1450, 0);
                TSD.Line line = new TSD.Line(sectionView, firstPoint, secondPoint, new TSD.Line.LineAttributes("Panel-Arrow"));
                line.Insert();
                #endregion

                #region Panel Text
                TSG.Point insertPoint = firstPoint + vX * 500 + vY * 20;
                Text text = new Text(sectionView, insertPoint, nearFaceString, new TextAttributes("CENTER-TEXT"));
                text.Insert();
                #endregion
            }
            #endregion

            #region Slab Connect
            if (listSlabs.Count() != 0)
            {
                foreach (TSM.Part slab in listSlabs)
                {
                    #region Slab Line
                    Solid solid = slab.GetSolid();
                    double YMax = solid.MaximumPoint.Y;
                    double YMin = solid.MinimumPoint.Y;

                    #region Slab at Right
                    if (solid.MaximumPoint.X > sectionView.RestrictionBox.MaxPoint.X)
                    {
                        #region Slab Line
                        TSG.Point topLineStart = new TSG.Point(sectionView.RestrictionBox.MaxPoint.X, YMax, 0);
                        TSG.Point topLineEnd = topLineStart + vX * 3000;
                        TSG.Point botLineStart = new TSG.Point(sectionView.RestrictionBox.MaxPoint.X, YMin, 0);
                        TSG.Point botLineEnd = botLineStart + vX * 3000;

                        new TSD.Line(sectionView, topLineStart, topLineEnd, new LineAttributes("DWall-SlabLine")).Insert();
                        new TSD.Line(sectionView, botLineStart, botLineEnd, new LineAttributes("DWall-SlabLine")).Insert();
                        #endregion

                        #region Break Line
                        TSG.Point point1 = topLineEnd + vY * 120;
                        TSG.Point point2 = new TSG.Point(point1.X, (YMax + YMin) / 2 + 100, 0);
                        TSG.Point point3 = new TSG.Point(point1.X - 200, (YMax + YMin) / 2 + 50, 0);
                        TSG.Point point4 = new TSG.Point(point1.X + 200, (YMax + YMin) / 2 - 50, 0);
                        TSG.Point point5 = new TSG.Point(point1.X, (YMax + YMin) / 2 - 100, 0);
                        TSG.Point point6 = botLineEnd - vY * 120;

                        new TSD.Line(sectionView, point1, point2, new LineAttributes("DWall-BreakLine")).Insert();
                        new TSD.Line(sectionView, point2, point3, new LineAttributes("DWall-BreakLine")).Insert();
                        new TSD.Line(sectionView, point3, point4, new LineAttributes("DWall-BreakLine")).Insert();
                        new TSD.Line(sectionView, point4, point5, new LineAttributes("DWall-BreakLine")).Insert();
                        new TSD.Line(sectionView, point5, point6, new LineAttributes("DWall-BreakLine")).Insert();
                        #endregion

                        #region Vertical Dim
                        PointList pointList = new PointList()
                        {
                            topLineEnd,
                            botLineEnd
                        };
                        sdsh.CreateDimensionSet(sectionView, pointList, vX, 300, StraightDimAttribute);
                        sectionView.Modify();
                        #endregion

                        #region Lever Mark
                        CreateValueLevelMark(sectionView, new TSG.Point(topLineEnd.X + 400, topLineEnd.Y, 0), vY, slab.Name + " SLAB");
                        #endregion
                    }
                    #endregion

                    #region Slab at Left
                    if (solid.MinimumPoint.X < sectionView.RestrictionBox.MinPoint.X)
                    {
                        #region Slab Line
                        TSG.Point topLineStart = new TSG.Point(sectionView.RestrictionBox.MinPoint.X, YMax, 0);
                        TSG.Point topLineEnd = topLineStart - vX * 1500;
                        TSG.Point botLineStart = new TSG.Point(sectionView.RestrictionBox.MinPoint.X, YMin, 0);
                        TSG.Point botLineEnd = botLineStart - vX * 1500;

                        new TSD.Line(sectionView, topLineStart, topLineEnd, new LineAttributes("DWall-SlabLine")).Insert();
                        new TSD.Line(sectionView, botLineStart, botLineEnd, new LineAttributes("DWall-SlabLine")).Insert();
                        #endregion

                        #region Break Line
                        TSG.Point point1 = topLineEnd + vY * 120;
                        TSG.Point point2 = new TSG.Point(point1.X, (YMax + YMin) / 2 + 100, 0);
                        TSG.Point point3 = new TSG.Point(point1.X - 200, (YMax + YMin) / 2 + 50, 0);
                        TSG.Point point4 = new TSG.Point(point1.X + 200, (YMax + YMin) / 2 - 50, 0);
                        TSG.Point point5 = new TSG.Point(point1.X, (YMax + YMin) / 2 - 100, 0);
                        TSG.Point point6 = botLineEnd - vY * 120;

                        new TSD.Line(sectionView, point1, point2, new LineAttributes("DWall-BreakLine")).Insert();
                        new TSD.Line(sectionView, point2, point3, new LineAttributes("DWall-BreakLine")).Insert();
                        new TSD.Line(sectionView, point3, point4, new LineAttributes("DWall-BreakLine")).Insert();
                        new TSD.Line(sectionView, point4, point5, new LineAttributes("DWall-BreakLine")).Insert();
                        new TSD.Line(sectionView, point5, point6, new LineAttributes("DWall-BreakLine")).Insert();
                        #endregion

                        #region Vertical Dim
                        PointList pointList = new PointList()
                        {
                            topLineEnd,
                            botLineEnd
                        };
                        sdsh.CreateDimensionSet(sectionView, pointList, vX, -500, StraightDimAttribute);
                        sectionView.Modify();
                        #endregion

                        #region Lever Mark
                        CreateValueLevelMark(sectionView, new TSG.Point(topLineEnd.X - 400, topLineEnd.Y, 0), vY, slab.Name + " SLAB");
                        #endregion
                    }
                    #endregion
                    #endregion
                }
            }
            #endregion

            #region Break Line
            if (!isContainTop)
            {
                TSG.Point point1 = new TSG.Point(sectionView.RestrictionBox.MinPoint.X - 120, sectionView.RestrictionBox.MaxPoint.Y, 0);
                TSG.Point point2 = new TSG.Point((sectionView.RestrictionBox.MinPoint.X + sectionView.RestrictionBox.MaxPoint.X) / 2 - 100, point1.Y, 0);
                TSG.Point point3 = new TSG.Point((sectionView.RestrictionBox.MinPoint.X + sectionView.RestrictionBox.MaxPoint.X) / 2 - 50, point1.Y - 200, 0);
                TSG.Point point4 = new TSG.Point((sectionView.RestrictionBox.MinPoint.X + sectionView.RestrictionBox.MaxPoint.X) / 2 + 50, point1.Y + 200, 0);
                TSG.Point point5 = new TSG.Point((sectionView.RestrictionBox.MinPoint.X + sectionView.RestrictionBox.MaxPoint.X) / 2 + 100, point1.Y, 0);
                TSG.Point point6 = new TSG.Point(sectionView.RestrictionBox.MaxPoint.X + 120, sectionView.RestrictionBox.MaxPoint.Y, 0);

                new TSD.Line(sectionView, point1, point2, new LineAttributes("DWall-BreakLine")).Insert();
                new TSD.Line(sectionView, point2, point3, new LineAttributes("DWall-BreakLine")).Insert();
                new TSD.Line(sectionView, point3, point4, new LineAttributes("DWall-BreakLine")).Insert();
                new TSD.Line(sectionView, point4, point5, new LineAttributes("DWall-BreakLine")).Insert();
                new TSD.Line(sectionView, point5, point6, new LineAttributes("DWall-BreakLine")).Insert();
            }

            if (!isContainBot)
            {
                TSG.Point point1 = new TSG.Point(sectionView.RestrictionBox.MinPoint.X - 120, sectionView.RestrictionBox.MinPoint.Y, 0);
                TSG.Point point2 = new TSG.Point((sectionView.RestrictionBox.MinPoint.X + sectionView.RestrictionBox.MaxPoint.X) / 2 - 100, point1.Y, 0);
                TSG.Point point3 = new TSG.Point((sectionView.RestrictionBox.MinPoint.X + sectionView.RestrictionBox.MaxPoint.X) / 2 - 50, point1.Y - 200, 0);
                TSG.Point point4 = new TSG.Point((sectionView.RestrictionBox.MinPoint.X + sectionView.RestrictionBox.MaxPoint.X) / 2 + 50, point1.Y + 200, 0);
                TSG.Point point5 = new TSG.Point((sectionView.RestrictionBox.MinPoint.X + sectionView.RestrictionBox.MaxPoint.X) / 2 + 100, point1.Y, 0);
                TSG.Point point6 = new TSG.Point(sectionView.RestrictionBox.MaxPoint.X + 120, sectionView.RestrictionBox.MinPoint.Y, 0);

                new TSD.Line(sectionView, point1, point2, new LineAttributes("DWall-BreakLine")).Insert();
                new TSD.Line(sectionView, point2, point3, new LineAttributes("DWall-BreakLine")).Insert();
                new TSD.Line(sectionView, point3, point4, new LineAttributes("DWall-BreakLine")).Insert();
                new TSD.Line(sectionView, point4, point5, new LineAttributes("DWall-BreakLine")).Insert();
                new TSD.Line(sectionView, point5, point6, new LineAttributes("DWall-BreakLine")).Insert();
            }
            #endregion
            #endregion

            #region View Name
            TSG.Point namePoint = new TSG.Point((sectionView.RestrictionBox.MinPoint.X + sectionView.RestrictionBox.MaxPoint.X) / 2, sectionView.RestrictionBox.MinPoint.Y - 2000, 0);
            new Text(sectionView, namePoint, "SECTION " + sectionView.Name, new TextAttributes("CENTER-UNDERLINE-TEXT")).Insert();
            TSG.Point scalePoint = namePoint - vY * 400;
            new Text(sectionView, scalePoint, "SCALE 1:100", new TextAttributes("CENTER-NOTE1-TEXT")).Insert();
            #endregion

            #region Part Lever Mark
            #region Bottom Lever
            if (isContainBot)
            {
                CreateValueLevelMark(sectionView, new TSG.Point(levelLeftXValue, sectionView.RestrictionBox.MinPoint.Y, 0), new TSG.Vector(0, -1, 0), "TENTATIVE TOE LEVEL");
            }
            #endregion

            #region Top Lever
            if (isContainTop)
            {
                double maxYValue = double.MinValue;
                foreach (DWallBeam dWallBeam in listShowPanel)
                {
                    Solid solid = dWallBeam.panelModel.GetSolid();
                    if (solid.MaximumPoint.Y > maxYValue)
                    {
                        maxYValue = solid.MaximumPoint.Y;
                    }
                }
                CreateValueLevelMark(sectionView, new TSG.Point((sectionView.RestrictionBox.MinPoint.X + sectionView.RestrictionBox.MaxPoint.X) / 2 - 50, maxYValue, 0), vY, "CONCRETE CUT-OFF");
            }
            #endregion
            #endregion

            #region Get Reinforcement
            DrawingObjectEnumerator reinforcements = sectionView.GetAllObjects(typeof(TSD.ReinforcementBase));
            List<DWallRebar> farFaceRebars = new List<DWallRebar>();
            List<DWallRebar> nearFaceRebars = new List<DWallRebar>();
            while (reinforcements.MoveNext())
            {
                if (reinforcements.Current is TSD.ReinforcementBase rebar)
                {
                    DWallRebar dWallRebar = new DWallRebar(model, rebar);
                    if (dWallRebar.cageName == cageName)
                    {
                        if (dWallRebar.rebarName == "Main Bar FF")
                        {
                            dWallRebar.rebarModel.Select();
                            Solid solid = dWallRebar.rebarModel.GetSolid();
                            dWallRebar.topRebarPoint = new TSG.Point(solid.MaximumPoint.X, solid.MaximumPoint.Y, solid.MaximumPoint.Z);
                            dWallRebar.botRebarPoint = new TSG.Point(solid.MinimumPoint.X, solid.MinimumPoint.Y, solid.MinimumPoint.Z);
                            farFaceRebars.Add(dWallRebar);
                        }
                        else if (dWallRebar.rebarName == "Main Bar NF")
                        {
                            dWallRebar.rebarModel.Select();
                            Solid solid = dWallRebar.rebarModel.GetSolid();
                            dWallRebar.topRebarPoint = new TSG.Point(solid.MaximumPoint.X, solid.MaximumPoint.Y, solid.MaximumPoint.Z);
                            dWallRebar.botRebarPoint = new TSG.Point(solid.MinimumPoint.X, solid.MinimumPoint.Y, solid.MinimumPoint.Z);
                            nearFaceRebars.Add(dWallRebar);
                        }
                    }
                }
            }
            if (farFaceRebars.Count != 0)
            {
                farFaceRebars = farFaceRebars.OrderByDescending(x => x.topRebarPoint.Y).ThenBy(x => x.botRebarPoint.Y).ToList();
            }
            if (nearFaceRebars.Count != 0)
            {
                nearFaceRebars = nearFaceRebars.OrderByDescending(x => x.topRebarPoint.Y).ThenBy(x => x.botRebarPoint.Y).ToList();
            }
            #endregion

            #region Reinforcement Level Mark
            CreateRebarLevelMark(sectionView, farFaceRebars, levelLeftXValue, isContainTop, isContainBot);
            CreateRebarLevelMark(sectionView, nearFaceRebars, levelRightXValue, isContainTop ,isContainBot);
            #endregion

            #region Rotate 90 view
            sectionView.Modify();
            sectionView.RotateViewOnDrawingPlane(90);
            sectionView.Select();
            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(GetTransformationPlane(sectionView));
            sectionView.Select();
            #endregion
        }
    }
}
