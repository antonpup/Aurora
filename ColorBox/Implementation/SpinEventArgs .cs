/*****************   NCore Softwares Pvt. Ltd., India   **************************

   ColorBox

   Copyright (C) 2013 NCore Softwares Pvt. Ltd.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://colorbox.codeplex.com/license

***********************************************************************************/

using System.Windows;

namespace ColorBox
{
    class SpinEventArgs : RoutedEventArgs
    {
        public SpinDirection Direction
        {
            get;
            private set;
        }
      
        public SpinEventArgs(SpinDirection direction)
            : base()
        {
            Direction = direction;
        }
    }
}


