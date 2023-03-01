using Logger.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
                    //CurrentWindow.TextBoxDebugInfo.Select(CurrentWindow.TextBoxDebugInfo.Text.Length, 0);
                    CurrentWindow.TextBoxDebugInfo.CaretIndex = CurrentWindow.TextBoxDebugInfo.Text.Length;
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
