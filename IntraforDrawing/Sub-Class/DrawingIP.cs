using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntraforDrawing
{
    public class DrawingIP : ViewModelBase
    {
        private string _Panel;
        public string Panel
        {
            get { return _Panel; }
            set { _Panel = value; OnPropertyChanged(); }
        }

        private int _NumberOfDrawings;
        public int NumberOfDrawings
        {
            get { return _NumberOfDrawings; }
            set { _NumberOfDrawings = value; OnPropertyChanged(); }
        }
        public string sentToDrawing { get; set; }

        public DrawingIP(string _panel, int _numberOfDrawings)
        {
            Panel = _panel;
            NumberOfDrawings = _numberOfDrawings;
        }
    }
}
