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

                        foreach(Assembly assembly in ListAssembly)
                        {
                            model.GetWorkPlaneHandler().SetCurrentTransformationPlane(new TransformationPlane());
                            assembly.Select();
                            List<TSM.Part> listPanel = new List<Part> { assembly.GetMainPart() as TSM.Part };
                            ArrayList moe = assembly.GetSecondaries();
                            foreach (TSM.Part beam in moe)
                            {
                                if (beam.Name != "JOIN")
                                {
                                    listPanel.Add(beam);
                                }
                            }

                            int drawingSheetNumber = 1;

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
        private int NumberOfFrontDrawing(List<TSM.Part> listPanels)
        {
            int numberOfDrawing = 0;

            List<string> containCage = new List<string>();
            foreach (TSM.Part panel in listPanels)
            {
                DWallBeam dWallBeam = new DWallBeam(panel);
                foreach (string cageName in dWallBeam.cageContain)
                {
                    containCage.Add(cageName);
                }
            }

            #region Get number of Drawing per Panel

            #endregion

            return numberOfDrawing;
        }
    }
}
