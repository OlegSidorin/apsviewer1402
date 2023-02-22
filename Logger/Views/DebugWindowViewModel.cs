using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Views
{
    public class DebugWindowViewModel : ObservableObject
    {
        string debugInfo;
        public string DebugInfo { get { return debugInfo; } set { debugInfo = value; OnPropertyChanged(); } }
    }
}
