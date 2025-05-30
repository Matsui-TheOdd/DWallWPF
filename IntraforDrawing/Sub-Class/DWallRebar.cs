using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures;

namespace IntraforDrawing
{
    public class DWallRebar
    {
        public RebarGroup rebarModel { get; set; }
        public ReinforcementBase rebarDrawing { get; set; }
        public string rebarLayer { get; set; }
        public string rebarName { get; set; }
        public string cageName { get; set; }
        public string cageNo { get; set; }
        public string relatedFloor { get; set; }
        public string couplerDirection { get; set; }
        public Point topRebarPoint { get; set; }
        public Point botRebarPoint { get; set; }
        public int rebarPosition { get; set; }
        public DWallRebar(RebarGroup _rebarModel)
        {
            rebarModel = _rebarModel;

            rebarName = rebarModel.Name;

            string _rebarLayer = "";
            rebarModel.GetReportProperty("LAYER_REBAR", ref _rebarLayer);
            rebarLayer = _rebarLayer;

            string _cageName = "";
            rebarModel.GetReportProperty("CAGE_NAME", ref _cageName);
            cageName = _cageName;

            string _cageNo = "";
            rebarModel.GetReportProperty("CAGE_NO", ref _cageNo);
            cageNo = _cageNo;

            string _relatedFloor = "";
            rebarModel.GetReportProperty("RELATED_FLOOR", ref _relatedFloor);
            relatedFloor = _relatedFloor;

            string _couplerDirection = "";
            rebarModel.GetReportProperty("COUPLER_DIRECTION", ref _couplerDirection);
            couplerDirection = _couplerDirection;
        }
        public DWallRebar(Model model, ReinforcementBase _rebarDrawing)
        {
            rebarDrawing = _rebarDrawing;
            Identifier id = rebarDrawing.ModelIdentifier;
            rebarModel = model.SelectModelObject(id) as RebarGroup;

            rebarName = rebarModel.Name;

            string _rebarLayer = "";
            rebarModel.GetReportProperty("LAYER_REBAR", ref _rebarLayer);
            rebarLayer = _rebarLayer;

            string _cageName = "";
            rebarModel.GetReportProperty("CAGE_NAME", ref _cageName);
            cageName = _cageName;

            string _cageNo = "";
            rebarModel.GetReportProperty("CAGE_NO", ref _cageNo);
            cageNo = _cageNo;

            string _relatedFloor = "";
            rebarModel.GetReportProperty("RELATED_FLOOR", ref _relatedFloor);
            relatedFloor = _relatedFloor;

            string _couplerDirection = "";
            rebarModel.GetReportProperty("COUPLER_DIRECTION", ref _couplerDirection);
            couplerDirection = _couplerDirection;
        }
    }
}
