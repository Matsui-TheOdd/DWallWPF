using MoreLinq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Tekla.Structures;
using Tekla.Structures.Dialog;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using TSD = Tekla.Structures.Drawing;
using TSDui = Tekla.Structures.Drawing.UI;
using TSG = Tekla.Structures.Geometry3d;
using TSM = Tekla.Structures.Model;
using TSMui = Tekla.Structures.Model.UI;
using mainView = iCEB_D_Wall_Drawing;
using static Tekla.Structures.Drawing.Line;
using System.Collections;
using static Tekla.Structures.Drawing.View;
using static Tekla.Structures.Drawing.ReinforcementBase;
using Tekla.Structures.DrawingInternal;
using Tekla.Structures.Geometry3d;
using static Tekla.Structures.Drawing.DimensionSetBaseAttributes;
using System.Windows.Media;
using RenderCommand;
using Tekla.Structures.Datatype;
using Tekla.Structures.ModelInternal;
using System.Drawing;
using static Tekla.Structures.Drawing.StraightDimensionSet;
using System.Windows.Media.Media3D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Input;
using System.Windows.Interop;
using static System.Windows.Forms.LinkLabel;

namespace iCEB_D_Wall_Drawing
{
    public partial class MainWindow : ApplicationWindowBase
    {
        public MainWindowViewModel dataModel = new MainWindowViewModel();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = dataModel;
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            ModelEvents = new TSM.Events();
            ModelEvents.TeklaStructuresExit += this.ModelEvents_TeklaExit;
            ModelEvents.Register();
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
                dataModel.PasteIP_Input(dataGrid, rowIndex, columnIndex, _MainIP, false);
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

        #region Console
        private TSM.Events ModelEvents { get; set; }
        private void ModelEvents_TeklaExit()
        {
            ModelEvents.UnRegister();
            Environment.Exit(0);
        }    
        #endregion
    }
}
