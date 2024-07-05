using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace DLMPPData.Models {
    public class FindChild {
        /// <summary>
        /// 找Control
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public T FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T element && element.Name == name) {
                    return element;
                }
                T childOfChild = FindVisualChild<T>(child, name);
                if (childOfChild != null) {
                    return childOfChild;
                }
            }
            return null;
        }
    }
}
