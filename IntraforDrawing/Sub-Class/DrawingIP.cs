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

        private string _Tilte_1;
        public string Tilte_1
        {
            get { return _Tilte_1; }
            set { _Tilte_1 = value; OnPropertyChanged(); }
        }

        private string _Comment;
        public string Comment
        {
            get { return _Comment; }
            set { _Comment = value; OnPropertyChanged(); }
        }
    }
}
