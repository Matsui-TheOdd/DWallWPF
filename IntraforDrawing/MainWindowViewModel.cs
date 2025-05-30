using System.Collections;
using System.ComponentModel;
using System.Linq;
using MoreLinq;
using TD = Tekla.Structures.Datatype;
using TSG = Tekla.Structures.Geometry3d;
using Tekla.Structures.Dialog;
using Tekla.Structures.Model;
using TSM = Tekla.Structures.Model;
using TSMUI = Tekla.Structures.Model.UI;
using Tekla.Structures.Model.Operations;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using System;
using Tekla.Structures.Drawing;

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

                            foreach(string cage in containCage)
                            {
                                if (listSplit.Count == 1)
                                {

                                }
                                CastUnitDrawing frontCage = new CastUnitDrawing(assembly.Identifier, drawingSheetNumber, "CIP_Wall");
                                frontCage.Layout.LoadAttributes("DWALL");
                                frontCage.Name = assembly.Name;
                                frontCage.Title1 = titel1;
                                frontCage.Insert();
                                drawingSheetNumber++;
                                titel1 = IncrementLastSegment(titel1);
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

                dWallBeam.SetMaxMinPoint();
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

                foreach(DWallBeam dWallBeam in panel1stCage)
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
    }
}
