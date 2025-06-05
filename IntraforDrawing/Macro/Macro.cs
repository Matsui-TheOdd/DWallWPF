using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model.Operations;
using TSD = Tekla.Structures.Drawing;
using TSDui = Tekla.Structures.Drawing.UI;

namespace IntraforDrawing
{
    internal static class Macro
    {
        private static TSD.DrawingHandler dh = new TSD.DrawingHandler();
        public static void Load_ViewAttribute_Filter_And_EditSetting(TSD.View _View, string attribute)
        {
            TSDui.DrawingObjectSelector drObjSelector = dh.GetDrawingObjectSelector();
            drObjSelector.UnselectAllObjects();
            drObjSelector.SelectObjects(new ArrayList() { _View }, false);
            string macroName = "Load_View_Filter.cs";
            string macroPath = string.Empty;
            string dir = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref dir);
            bool XS_Enable_Check = false;
            TeklaStructuresSettings.GetAdvancedOption("XS_DIALOG_ENABLE_STATE", ref XS_Enable_Check);
            if (dir != "")
            {
                string[] Macro_folder = dir.Split(';');
                if (Macro_folder.Count() >= 1)
                {
                    macroPath = ((!Macro_folder[0].Contains("\\\\")) ? Macro_folder[0] : Macro_folder[0].Replace("\\\\", "\\"));
                }
            }
            string macros =
                "namespace Tekla.Technology.Akit.UserScript{\r\n" +
                "public class Script{\r\n" +
                "public static void Run(Tekla.Technology.Akit.IScript akit){\r\n" +
                "akit.Callback(\"acmd_display_attr_dialog\", \"view_dial\", \"main_frame\");\r\n" +
                "akit.ValueChange(\"view_dial\", \"gr_view_get_menu\", \"standard\");" +
                "akit.ValueChange(\"view_dial\", \"gr_view_get_menu\", \"" + attribute + "\");" +
                "akit.PushButton(\"view_on_off\", \"view_dial\");" +
                "akit.PushButton(\"gr_view_get\", \"view_dial\");" +
                "akit.PushButton(\"view_on_off\", \"view_dial\");" +
                "akit.PushButton(\"gr_view_get\", \"view_dial\");" +
                "akit.TreeSelect(\"view_dial\", \"gratCastUnitDrawingAttributesMenuTree\", \"Attributes\");" +
                "akit.ValueChange(\"view_dial\", \"radioboxYesNo\", \"1\");" +
                "akit.TreeSelect(\"view_dial\", \"gratCastUnitDrawingAttributesMenuTree\", \"Filter\");" +
                "akit.ValueChange(\"view_dial\", \"ModelSelectionCBox\", \"1\");" +
                "akit.PushButton(\"view_modify\", \"view_dial\");" +
                "}\r\n" +
                "}\r\n" +
                "}";
            string filename = Path.Combine(macroPath, macroName);
            File.WriteAllText(filename, macros);
            Operation.RunMacro("..\\" + macroName);
        }
        public static void Create_Rebar_Mark(ReinforcementBase _rebar, string attribute)
        {
            TSDui.DrawingObjectSelector drObjSelector = dh.GetDrawingObjectSelector();
            drObjSelector.UnselectAllObjects();
            drObjSelector.SelectObjects(new ArrayList() { _rebar }, false);
            string macroName = "Create_Rebar_Mark.cs";
            string macroPath = string.Empty;
            string dir = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref dir);
            bool XS_Enable_Check = false;
            TeklaStructuresSettings.GetAdvancedOption("XS_DIALOG_ENABLE_STATE", ref XS_Enable_Check);
            if (dir != "")
            {
                string[] Macro_folder = dir.Split(';');
                if (Macro_folder.Count() >= 1)
                {
                    macroPath = ((!Macro_folder[0].Contains("\\\\")) ? Macro_folder[0] : Macro_folder[0].Replace("\\\\", "\\"));
                }
            }
            string macros =
                "#pragma reference \"Tekla.Macros.Akit\"\r\n" +
                "#pragma reference \"Tekla.Macros.Runtime\"\r\n" +
                "namespace UserMacros {\r\n" +
                "public class Macro {\r\n" +
                "[Tekla.Macros.Runtime.MacroEntryPointAttribute()]\r\n" +
                "public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime) {\r\n" +
                "Tekla.Macros.Akit.IAkitScriptHost akit = runtime.Get<Tekla.Macros.Akit.IAkitScriptHost>();\r\n" +
                "akit.CommandEnd();\r\n" +
                "akit.Callback(\"acmd_interrupt\", \"\", \"View_10 window_1\");\r\n" +
                "akit.Callback(\"acmd_create_marks_selected\", \"\", \"View_10 window_1\");\r\n" +
                "akit.PushButton(\"gr_rebar_get\", \"rebar_mark_dial\");\r\n" +
                "akit.PushButton(\"rebar_mark_modify\", \"rebar_mark_dial\");\r\n" +
                "}\r\n" +
                "}\r\n" +
                "}";
            string filename = Path.Combine(macroPath, macroName);
            File.WriteAllText(filename, macros);
            Operation.RunMacro("..\\" + macroName);
        }
        public static void Create_FrontView_Rebar_Mark(List<ReinforcementBase> _rebars)
        {
            TSDui.DrawingObjectSelector drObjSelector = dh.GetDrawingObjectSelector();
            drObjSelector.UnselectAllObjects();
            foreach (ReinforcementBase rebar in _rebars)
            {
                drObjSelector.SelectObjects(new ArrayList() { rebar }, true);
            }
            string macroName = "Create_Rebar_Mark_FrontView.cs";
            string macroPath = string.Empty;
            string dir = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref dir);
            bool XS_Enable_Check = false;
            TeklaStructuresSettings.GetAdvancedOption("XS_DIALOG_ENABLE_STATE", ref XS_Enable_Check);
            if (dir != "")
            {
                string[] Macro_folder = dir.Split(';');
                if (Macro_folder.Count() >= 1)
                {
                    macroPath = ((!Macro_folder[0].Contains("\\\\")) ? Macro_folder[0] : Macro_folder[0].Replace("\\\\", "\\"));
                }
            }
            string macros =
                "namespace Tekla.Technology.Akit.UserScript{\r\n" +
                "public class Script{\r\n" +
                "public static void Run(Tekla.Technology.Akit.IScript akit){\r\n" +
                "akit.ValueChange(\"main_frame\", \"gr_sel_drawing_part\", \"1\");\r\n" +
                "akit.Callback(\"acmd_create_marks_selected\", \"RebarNewDimMark\", \"View_10 window_1\");\r\n" +
                "akit.ValueChange(\"main_frame\", \"gr_sel_all\", \"1\");\r\n" +
                "}\r\n" +
                "}\r\n" +
                "}";
            string filename = Path.Combine(macroPath, macroName);
            File.WriteAllText(filename, macros);
            Operation.RunMacro("..\\" + macroName);
        }
        public static void Load_DimMarkAttribute(TSD.StraightDimensionSet dimension, string attribute)
        {
            TSDui.DrawingObjectSelector drObjSelector = dh.GetDrawingObjectSelector();
            drObjSelector.UnselectAllObjects();
            drObjSelector.SelectObjects(new ArrayList() { dimension }, false);
            string macroName = "Load_DimMark_Attribute.cs";
            string macroPath = string.Empty;
            string dir = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref dir);
            bool XS_Enable_Check = false;
            TeklaStructuresSettings.GetAdvancedOption("XS_DIALOG_ENABLE_STATE", ref XS_Enable_Check);
            if (dir != "")
            {
                string[] Macro_folder = dir.Split(';');
                if (Macro_folder.Count() >= 1)
                {
                    macroPath = ((!Macro_folder[0].Contains("\\\\")) ? Macro_folder[0] : Macro_folder[0].Replace("\\\\", "\\"));
                }
            }
            string macros =
                "#pragma reference \"Tekla.Macros.Wpf.Runtime\"\r\n" +
                "#pragma reference \"Tekla.Macros.Akit\"\r\n" +
                "#pragma reference \"Tekla.Macros.Runtime\"\r\n" +
                "namespace UserMacros {\r\n" +
                "public sealed class Macro {\r\n" +
                "[Tekla.Macros.Runtime.MacroEntryPointAttribute()]\r\n" +
                "public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime) {\r\n" +
                "Tekla.Macros.Akit.IAkitScriptHost akit = runtime.Get<Tekla.Macros.Akit.IAkitScriptHost>();\r\n" +
                "Tekla.Macros.Wpf.Runtime.IWpfMacroHost wpf = runtime.Get<Tekla.Macros.Wpf.Runtime.IWpfMacroHost>();\r\n" +
                "wpf.InvokeCommand(\"CommandRepository\", \"Dimensions.RebarDimensionMarkProperties\");\r\n" +
                "akit.ValueChange(\"rebar_dim_dial\", \"gr_dim_get_menu\", \"" + attribute + "\");\r\n" +
                "akit.PushButton(\"gr_dim_get\", \"rebar_dim_dial\");\r\n" +
                "akit.PushButton(\"dim_modify\", \"rebar_dim_dial\");\r\n" +
                "akit.PushButton(\"dim_ok\", \"rebar_dim_dial\");\r\n" +
                "}\r\n" +
                "}\r\n" +
                "}";
            string filename = Path.Combine(macroPath, macroName);
            File.WriteAllText(filename, macros);
            Operation.RunMacro("..\\" + macroName);
        }
        public static void Load_LeverMark(LevelMark leverMark, string attribute)
        {
            TSDui.DrawingObjectSelector drObjSelector = dh.GetDrawingObjectSelector();
            drObjSelector.UnselectAllObjects();
            drObjSelector.SelectObjects(new ArrayList() { leverMark }, false);
            string macroName = "Load_LeverMark.cs";
            string macroPath = string.Empty;
            string dir = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref dir);
            bool XS_Enable_Check = false;
            TeklaStructuresSettings.GetAdvancedOption("XS_DIALOG_ENABLE_STATE", ref XS_Enable_Check);
            if (dir != "")
            {
                string[] Macro_folder = dir.Split(';');
                if (Macro_folder.Count() >= 1)
                {
                    macroPath = ((!Macro_folder[0].Contains("\\\\")) ? Macro_folder[0] : Macro_folder[0].Replace("\\\\", "\\"));
                }
            }
            string macros =
                "#pragma reference \"Tekla.Macros.Wpf.Runtime\"\r\n" +
                "#pragma reference \"Tekla.Macros.Akit\"\r\n" +
                "#pragma reference \"Tekla.Macros.Runtime\"\r\n" +
                "namespace UserMacros {\r\n" +
                "public class Macro {\r\n" +
                "[Tekla.Macros.Runtime.MacroEntryPointAttribute()]\r\n" +
                "public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime) {\r\n" +
                "Tekla.Macros.Akit.IAkitScriptHost akit = runtime.Get<Tekla.Macros.Akit.IAkitScriptHost>();\r\n" +
                "Tekla.Macros.Wpf.Runtime.IWpfMacroHost wpf = runtime.Get<Tekla.Macros.Wpf.Runtime.IWpfMacroHost>();\r\n" +
                "wpf.InvokeCommand(\"CommandRepository\", \"Annotations.LevelMarkProperties\");\r\n" +
                "akit.ValueChange(\"level_dial\", \"gr_text_get_menu\", \"standard\");\r\n" +
                "akit.ValueChange(\"level_dial\", \"gr_text_get_menu\", \"" + attribute + "\");\r\n" +
                "akit.PushButton(\"gr_text_get\", \"level_dial\");\r\n" +
                "akit.PushButton(\"ltext_modify\", \"level_dial\");\r\n" +
                "}\r\n" +
                "}\r\n" +
                "}";
            string filename = Path.Combine(macroPath, macroName);
            File.WriteAllText(filename, macros);
            Operation.RunMacro("..\\" + macroName);
        }
        public static void Send_View_To_Other_Drawing(TSD.View _View, string sentToDrawingName, string panelName)
        {
            TSDui.DrawingObjectSelector drObjSelector = dh.GetDrawingObjectSelector();
            drObjSelector.UnselectAllObjects();
            drObjSelector.SelectObjects(new ArrayList() { _View }, false);

            #region Get Drawing Index
            //DrawingHandler dhh = new DrawingHandler();
            //DrawingEnumerator de = dhh.GetDrawings();
            //int index = -1;

            //while (de.MoveNext())
            //{
            //    index++;
            //    if (de.Current is CastUnitDrawing castDrawing)
            //    {
            //        if (castDrawing.Name == sentToDrawingName)
            //        {
            //            break;
            //        }
            //    }
            //}

            //List<CastUnitDrawing> listPanelDrawing = new List<CastUnitDrawing>();
            //while (de.MoveNext())
            //{
            //    if (de.Current is CastUnitDrawing castDrawing)
            //    {
            //        if (castDrawing.Title2 == panelName)
            //        {
            //            listPanelDrawing.Add(castDrawing);
            //        }
            //    }
            //}

            //listPanelDrawing = listPanelDrawing.OrderBy(x => x.Name).ToList();
            //for (int i = 0; i < listPanelDrawing.Count; i++)
            //{
            //    if (listPanelDrawing[i].Name == sentToDrawingName)
            //    {
            //        index = i;
            //    }
            //}
            #endregion

            Search_Panel_Name(sentToDrawingName);

            string macroName = "Send_View_To_Other_Drawing.cs";
            string macroPath = string.Empty;
            string dir = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref dir);
            bool XS_Enable_Check = false;
            TeklaStructuresSettings.GetAdvancedOption("XS_DIALOG_ENABLE_STATE", ref XS_Enable_Check);
            if (dir != "")
            {
                string[] Macro_folder = dir.Split(';');
                if (Macro_folder.Count() >= 1)
                {
                    macroPath = ((!Macro_folder[0].Contains("\\\\")) ? Macro_folder[0] : Macro_folder[0].Replace("\\\\", "\\"));
                }
            }
            string macros =
                "#pragma reference \"Tekla.Macros.Wpf.Runtime\"\r\n" +
                "#pragma reference \"Tekla.Macros.Akit\"\r\n" +
                "#pragma reference \"Tekla.Macros.Runtime\"\r\n" +
                "namespace UserMacros {\r\n" +
                "public class Macro {\r\n" +
                "[Tekla.Macros.Runtime.MacroEntryPointAttribute()]\r\n" +
                "public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime) {\r\n" +
                "Tekla.Macros.Akit.IAkitScriptHost akit = runtime.Get<Tekla.Macros.Akit.IAkitScriptHost>();\r\n" +
                "Tekla.Macros.Wpf.Runtime.IWpfMacroHost wpf = runtime.Get<Tekla.Macros.Wpf.Runtime.IWpfMacroHost>();\r\n" +
                "wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_DataGridControl\").As.DataGrid.NewSelection.With(0).Invoke();\r\n" +
                "akit.Callback(\"acmdMoveToDrawing\", \"\", \"View_10 window_1\");\r\n" +
                "akit.PushButton(\"butMoveViewMove\", \"gr_MoveViewToDrawing\");\r\n" +
                "}\r\n" +
                "}\r\n" +
                "}";
            string filename = Path.Combine(macroPath, macroName);
            File.WriteAllText(filename, macros);
            Operation.RunMacro("..\\" + macroName);
        }
        public static void Search_Panel_Name(string sentToDrawingName)
        {
            string macroName = "Search_Panel_Name.cs";
            string macroPath = string.Empty;
            string dir = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XS_MACRO_DIRECTORY", ref dir);
            bool XS_Enable_Check = false;
            TeklaStructuresSettings.GetAdvancedOption("XS_DIALOG_ENABLE_STATE", ref XS_Enable_Check);
            if (dir != "")
            {
                string[] Macro_folder = dir.Split(';');
                if (Macro_folder.Count() >= 1)
                {
                    macroPath = ((!Macro_folder[0].Contains("\\\\")) ? Macro_folder[0] : Macro_folder[0].Replace("\\\\", "\\"));
                }
            }
            string macros =
                "#pragma reference \"Tekla.Macros.Wpf.Runtime\"\r\n" +
                "#pragma reference \"Tekla.Macros.Akit\"\r\n" +
                "#pragma reference \"Tekla.Macros.Runtime\"\r\n" +
                "namespace UserMacros {\r\n" +
                "public class Macro {\r\n" +
                "[Tekla.Macros.Runtime.MacroEntryPointAttribute()]\r\n" +
                "public static void Run(Tekla.Macros.Runtime.IMacroRuntime runtime) {\r\n" +
                "Tekla.Macros.Wpf.Runtime.IWpfMacroHost wpf = runtime.Get<Tekla.Macros.Wpf.Runtime.IWpfMacroHost>();\r\n" +
                "wpf.InvokeCommand(\"CommandRepository\", \"Drawing.DrawingList\");\r\n" +
                "wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_SearchBox\").As.TextBox.SetText(\"\");\r\n" +
                "wpf.View(\"DocumentManager.MainWindow\").Find(\"AID_DOCMAN_SearchBox\").As.TextBox.SetText(\"" + sentToDrawingName + "\");\r\n" +
                "}\r\n" +
                "}\r\n" +
                "}";
            string filename = Path.Combine(macroPath, macroName);
            File.WriteAllText(filename, macros);
            Operation.RunMacro("..\\" + macroName);
        }
    }
}
