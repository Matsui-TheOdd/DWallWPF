using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCEB_D_Wall_Drawing
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

        private string _Tilte_2;
        public string Tilte_2
        {
            get { return _Tilte_2; }
            set { _Tilte_2 = value; OnPropertyChanged(); }
        }

        private string _Tilte_3;
        public string Tilte_3
        {
            get { return _Tilte_3; }
            set { _Tilte_3 = value; OnPropertyChanged(); }
        }

        private string _Comment;
        public string Comment
        {
            get { return _Comment; }
            set { _Comment = value; OnPropertyChanged(); }
        }
    }
}
