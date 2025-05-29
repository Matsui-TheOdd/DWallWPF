using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iCEB_D_Wall_Drawing
{
    public class HorizontalSectionIP : ViewModelBase
    {
        private string _Panel;
        public string Panel
        {
            get { return _Panel; }
            set { _Panel = value; OnPropertyChanged(); }
        }

        private string _Level;
        public string Level
        {
            get { return _Level; }
            set { _Level = value; OnPropertyChanged(); }
        }
    }
}
