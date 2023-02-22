using Logger.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public static class Create
    {
        static DebugWindow CurrentWindow { get; set; }
        public static void AndShow(string str)
        {
            if (CurrentWindow != null)
            {
#if DEBUG
                CurrentWindow.Activate();
                if (CurrentWindow.IsActive)
                {
                    string s1 = CurrentWindow.TextBoxDebugInfo.Text + "\n" + str; //:↯\n
                    ((DebugWindowViewModel)CurrentWindow.DataContext).DebugInfo = s1;
                    CurrentWindow.Show();
                }
                else
                {
                    DebugWindow window = new DebugWindow(str);
                    CurrentWindow = window;
                    window.Show();
                }
#endif
            }
            else
            {
#if DEBUG
                DebugWindow window = new DebugWindow(str);
                CurrentWindow = window;
                window.Show();
#endif
            }
        }
    }
}
