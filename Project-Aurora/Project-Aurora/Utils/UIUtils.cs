using Aurora.Devices;
using Aurora.Devices.Layout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Aurora.Utils
{
    public static class UIUtils
    {
        public static void SetSingleKey(TextBlock key_destination, List<DeviceLED> keyslist, int position)
        {
            if (keyslist.Count > position)
                key_destination.Text = keyslist[position].GetName();
            else
                key_destination.Text = "ERROR: OUT OF BOUNDS";
        }

        public static List<DeviceLED> SequenceToList(ItemCollection items)
        {
            List<DeviceLED> newsequence = new List<DeviceLED>();

            foreach (DeviceLED key in items)
            {
                newsequence.Add(key);
            }

            return newsequence;
        }

        public static bool ListBoxMoveSelectedUp(ListBox list)
        {
            if (list.Items.Count > 0 && list.SelectedIndex > 0)
            {
                int selected_index = list.SelectedIndex;
                var saved = list.Items[selected_index];
                list.Items[selected_index] = list.Items[selected_index - 1];
                list.Items[selected_index - 1] = saved;
                list.SelectedIndex = selected_index - 1;

                list.ScrollIntoView(list.Items[selected_index - 1]);
                return true;
            }

            return false;
        }

        public static bool ListBoxMoveSelectedDown(ListBox list)
        {
            if (list.Items.Count > 0 && list.SelectedIndex < (list.Items.Count - 1) && list.SelectedIndex >= 0)
            {
                int selected_index = list.SelectedIndex;
                var saved = list.Items[selected_index];
                list.Items[selected_index] = list.Items[selected_index + 1];
                list.Items[selected_index + 1] = saved;
                list.SelectedIndex = selected_index + 1;

                list.ScrollIntoView(list.Items[selected_index + 1]);
                return true;
            }

            return false;
        }

        public static bool ListBoxRemoveSelected(ListBox list)
        {

            if(list.SelectedItems.Count == 1)
            {
                if (list.Items.Count > 0 && list.SelectedIndex >= 0)
                {
                    int selected = list.SelectedIndex;
                    list.Items.RemoveAt(selected);

                    if (list.Items.Count > selected)
                        list.SelectedIndex = selected;
                    else
                        list.SelectedIndex = (list.Items.Count - 1);

                    if (list.SelectedIndex > -1)
                        list.ScrollIntoView(list.Items[list.SelectedIndex]);

                    return true;
                }

                return false;
            }
            else
            {
                bool isremoved = false;

                while (list.SelectedItems.Count > 0)
                {
                    list.Items.Remove(list.SelectedItems[0]);

                    isremoved = true;
                }

                return isremoved;
            }
        }

        public static bool ListBoxReverseOrder(ListBox keys_keysequence)
        {
            int totalCount = keys_keysequence.Items.Count;
            for (int i = totalCount - 1; i > 0; i--)
            {
                object obj = keys_keysequence.Items.GetItemAt(totalCount-1);
                keys_keysequence.Items.RemoveAt(totalCount-1);
                keys_keysequence.Items.Insert((totalCount - 1) - i, obj);
            }

            return true;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }

    public class TextCharacterLimitConv : IValueConverter
    {
        public int MaxLength { get; set; }

        public string ShortenSignify { get; set; }

        public TextCharacterLimitConv()
        {
            MaxLength = 12;
            ShortenSignify = "...";
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                string val = value as string;
                if ((val.Length + ShortenSignify.Length) > MaxLength)
                {
                    return String.Format("{0}{1}", val.Substring(0, MaxLength - ShortenSignify.Length), ShortenSignify);
                }                    
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class ExceptionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
