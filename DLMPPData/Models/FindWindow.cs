using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace DLMPPData.Models {
    public class FindWindow {
        /// <summary>
        /// 找Window
        /// </summary>
        /// <param name="userControl"></param>
        /// <returns></returns>
        public Window GetParentWindow(UserControl userControl) {
            DependencyObject parent = VisualTreeHelper.GetParent(userControl);

            while (parent != null && !(parent is Window)) {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as Window;
        }
    }
}
