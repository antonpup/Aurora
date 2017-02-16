/*****************   NCore Softwares Pvt. Ltd., India   **************************

   ColorBox

   Copyright (C) 2013 NCore Softwares Pvt. Ltd.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://colorbox.codeplex.com/license

***********************************************************************************/

using System;

namespace ColorBox
{    
    internal enum SpinDirection
    {
        Increase,
        Decrease
    }


    internal enum ValidSpinDirections
    {
        None,
        Increase,
        Decrease
    }


    internal enum AllowedSpecialValues
    {
        None = 0,
        NaN = 1,
        PositiveInfinity = 2,
        NegativeInfinity = 4,
        AnyInfinity = PositiveInfinity | NegativeInfinity,
        Any = NaN | AnyInfinity
    }


    internal enum BrushTypes
    {
        //None,
        Solid,
        Linear,
        Radial
    }
}
