using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Geometry3d;

namespace IntraforDrawing
{
    public class HorizontalSectionIP : ViewModelBase
    {
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; OnPropertyChanged(); }
        }

        private string _Level;
        public string Level
        {
            get { return _Level; }
            set { _Level = value; OnPropertyChanged(); }
        }
        public bool isFirstShow { get; set; }
        public Point InsertPoint { get; set; }
        public string SentToDrawingName { get; set; }
        public string CageNo { get; set; }
        public double ValueFromTopCage { get; set; }
        public HorizontalSectionIP() { }

        public HorizontalSectionIP(string _name, string _level)
        {
            Name = _name;
            Level = _level;
            isFirstShow = true;
        }
    }
}
