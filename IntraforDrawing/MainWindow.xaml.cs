using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tekla.Structures.Dialog;
using Tekla.Structures.Model;

namespace IntraforDrawing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ApplicationWindowBase
    {
        MainWindowViewModel dataModel = new MainWindowViewModel();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = dataModel;
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            this.InitializeDataStorage(dataModel);
            if (this.GetConnectionStatus())
            {
                InitializeDistanceUnitDecimals();
            }
        }

        private void DataGrid_No_Sorting(object sender, System.Windows.Controls.DataGridSortingEventArgs e)
        {
            e.Handled = true; // Ngăn WPF thực hiện sắp xếp
        }
        private void DtG_HorizontalSection_KeyDown(object sender, KeyEventArgs e)
        {
            Datagrid_KeyDown(sender, e, dataModel.HorizontalSectionsIP);
        }
        private void Datagrid_KeyDown(object sender, KeyEventArgs e, object _MainIP)
        {
            var dataGrid = sender as DataGrid;
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (dataGrid == null || dataGrid.SelectedCells.Count == 0) return;
                var selectedCell = dataGrid.SelectedCells[0];
                var rowIndex = dataGrid.Items.IndexOf(selectedCell.Item);
                var columnIndex = selectedCell.Column.DisplayIndex;
                dataModel.PasteIP_Input(dataGrid, rowIndex, columnIndex, _MainIP);
                e.Handled = true; // Ngăn xử lý phím mặc định
            }
        }
        private void DtG_HorizontalSection_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (sender is DataGrid element)
            {
                var uniqueRows = new HashSet<HorizontalSectionIP>();
                foreach (var cell in element.SelectedCells)
                {
                    if (cell.Item is HorizontalSectionIP row)
                    {
                        uniqueRows.Add(row);
                    }
                }

                if (DataContext is MainWindowViewModel viewModel)
                {
                    viewModel.HorizontalSectionIP_Selected = new List<HorizontalSectionIP>(uniqueRows);
                }
            }
        }
    }
}
