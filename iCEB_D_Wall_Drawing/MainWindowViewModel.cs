using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Dialog;
using System.Windows;
using TD = Tekla.Structures.Datatype;
using System.Windows.Input;
using static System.Windows.Forms.LinkLabel;

namespace iCEB_D_Wall_Drawing
{
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

        private string _AT_Tilte_2 = "";
        [StructuresDialog("AT_Tilte_2", typeof(TD.String))]
        public string AT_Tilte_2
        {
            get { return _AT_Tilte_2; }
            set
            {
                _AT_Tilte_2 = value;
                OnPropertyChanged();
            }
        }

        private string _AT_Tilte_3 = "";
        [StructuresDialog("AT_Tilte_3", typeof(TD.String))]
        public string AT_Tilte_3
        {
            get { return _AT_Tilte_3; }
            set
            {
                _AT_Tilte_3 = value;
                OnPropertyChanged();
            }
        }

        public ICommand Create_Drawings { get; set; }
        public ICommand Get_Exist_Drawings { get; set; }
        public ICommand Create_Reports { get; set; }
        public ICommand DelRow_HorizontalSectionIP_Command { get; set; }
        public ICommand Suggest_Section { get; set; }
        public ICommand Modify_Drawing { get; set; }

        public void PasteIP_Input(System.Windows.Controls.DataGrid dataGrid, int selectedRow, int selectedColumn, object ListUpdate, bool isDowelStarter)
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
        public MainWindowViewModel()
        {
            Create_Drawings = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {
                    
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
    }
}
