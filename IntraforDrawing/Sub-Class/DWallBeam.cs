using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Internal;
using TSM = Tekla.Structures.Model;
using TSD = Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using Tekla.Structures;
using MoreLinq;

namespace IntraforDrawing
{
    public class DWallBeam
    {
        public TSM.Part panelModel { get; set; }
        public TSD.Part panelDrawing { get; set; }
        public string panelCageNo { get; set; }
        public List<string> cageContain { get; set; }
        public List<DWallRebar> listDWallRebar { get; set; }
        public DWallBeam(TSM.Part _panelModel)
        {
            panelModel = _panelModel;
            listDWallRebar = new List<DWallRebar>();
            cageContain = new List<string>();

            string _cageNo = "";
            panelModel.GetReportProperty("CAGE_NO", ref _cageNo);
            panelCageNo = _cageNo;

            ModelObjectEnumerator moe = panelModel.GetReinforcements();
            while (moe.MoveNext())
            {
                if (moe.Current is RebarGroup rebar)
                {
                    DWallRebar dWallRebar = new DWallRebar(rebar);
                    listDWallRebar.Add(dWallRebar);
                    cageContain.Add(dWallRebar.cageName);
                }
            }

            cageContain = cageContain.DistinctBy(x => x).ToList();
        }
        public DWallBeam(Model model, TSD.Part _panelDrawing)
        {
            panelDrawing = _panelDrawing;
            Identifier id = panelDrawing.ModelIdentifier;
            panelModel = model.SelectModelObject(id) as TSM.Part;
            listDWallRebar = new List<DWallRebar>();
            cageContain = new List<string>();

            string _cageNo = "";
            panelModel.GetReportProperty("CAGE_NO", ref _cageNo);
            panelCageNo = _cageNo;

            ModelObjectEnumerator moe = panelModel.GetReinforcements();
            while (moe.MoveNext())
            {
                if (moe.Current is RebarGroup rebar)
                {
                    DWallRebar dWallRebar = new DWallRebar(rebar);
                    listDWallRebar.Add(dWallRebar);
                    cageContain.Add(dWallRebar.cageName);
                }
            }

            cageContain = cageContain.DistinctBy(x => x).ToList();
        }
    }
}
