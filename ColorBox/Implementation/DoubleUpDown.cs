/*****************   NCore Softwares Pvt. Ltd., India   **************************

   ColorBox

   Copyright (C) 2013 NCore Softwares Pvt. Ltd.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://colorbox.codeplex.com/license

***********************************************************************************/

using System;
using System.Windows;
using System.Globalization;
using System.IO;

namespace ColorBox
{
    class DoubleUpDown : UpDownBase
    {
        protected delegate double FromText(string s, NumberStyles style, IFormatProvider provider);
        protected delegate double FromDecimal(decimal d);

        private FromText _fromText;
        private FromDecimal _fromDecimal;
        private Func<double, double, bool> _fromLowerThan;
        private Func<double, double, bool> _fromGreaterThan;

        protected DoubleUpDown(FromText fromText, FromDecimal fromDecimal, Func<double, double, bool> fromLowerThan, Func<double, double, bool> fromGreaterThan)
        {
            if (fromText == null)
                throw new ArgumentNullException("parseMethod");

            if (fromDecimal == null)
                throw new ArgumentNullException("fromDecimal");

            if (fromLowerThan == null)
                throw new ArgumentNullException("fromLowerThan");

            if (fromGreaterThan == null)
                throw new ArgumentNullException("fromGreaterThan");

            _fromText = fromText;
            _fromDecimal = fromDecimal;
            _fromLowerThan = fromLowerThan;
            _fromGreaterThan = fromGreaterThan;
        }

        protected static void UpdateMetadata(Type type, double? increment, double? minValue, double? maxValue)
        {
            DefaultStyleKeyProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(type));
            IncrementProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(increment));
            MaximumProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(maxValue));
            MinimumProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(minValue));
        }

        private bool IsLowerThan(double? value1, double? value2)
        {
            if (value1 == null || value2 == null)
                return false;

            return _fromLowerThan(value1.Value, value2.Value);
        }

        private bool IsGreaterThan(double? value1, double? value2)
        {
            if (value1 == null || value2 == null)
                return false;

            return _fromGreaterThan(value1.Value, value2.Value);
        }

        private bool HandleNullSpin()
        {
            if (!Value.HasValue)
            {
                double forcedValue = (DefaultValue.HasValue)
                  ? DefaultValue.Value
                  : default(double);

                Value = CoerceValueMinMax(forcedValue);

                return true;
            }
            else if (!Increment.HasValue)
            {
                return true;
            }

            return false;
        }

        private double CoerceValueMinMax(double value)
        {
            if (IsLowerThan(value, Minimum))
                return Minimum;
            else if (IsGreaterThan(value, Maximum))
                return Maximum;
            else
                return value;
        }

        private void ValidateDefaultMinMax(double? value)
        {           
            if (object.Equals(value, DefaultValue))
                return;

            if (IsLowerThan(value, Minimum))
                throw new ArgumentOutOfRangeException("Minimum", String.Format("Value must be greater than MinValue of {0}", Minimum));
            else if (IsGreaterThan(value, Maximum))
                throw new ArgumentOutOfRangeException("Maximum", String.Format("Value must be less than MaxValue of {0}", Maximum));
        }

        #region Base Class Overrides

        protected override void OnIncrement()
        {
            if (!HandleNullSpin())
            {
                double? result = Value.Value + Increment.Value;
                Value = CoerceValueMinMax(result.Value);
            }
        }

        protected override void OnDecrement()
        {
            if (!HandleNullSpin())
            {
                double? result = Value.Value - Increment.Value;
                Value = CoerceValueMinMax(result.Value);
            }
        }

        protected override string ConvertValueToText()
        {
            if (Value == null)
                return string.Empty;

            return Value.Value.ToString(FormatString, CultureInfo);
        }

        protected override double? ConvertTextToValue(string text)
        {
            double? result = 0;

            if (String.IsNullOrEmpty(text))
                return result;

            string currentValueText = ConvertValueToText();
            if (object.Equals(currentValueText, text))
                return this.Value;
    
            result = FormatString.Contains("P")
              ? _fromDecimal(ParsePercent(text, CultureInfo))
              : _fromText(text, this.ParsingNumberStyle, CultureInfo);

            ValidateDefaultMinMax(result);

            return result;
        }

        protected override void SetValidSpinDirection()
        {
            ValidSpinDirections validDirections = ValidSpinDirections.None;
            
            if ((this.Increment != null) && !IsReadOnly)
            {
                if (IsLowerThan(Value, Maximum) || !Value.HasValue)
                    validDirections = validDirections | ValidSpinDirections.Increase;

                if (IsGreaterThan(Value, Minimum) || !Value.HasValue)
                    validDirections = validDirections | ValidSpinDirections.Decrease;
            }

            if (Spinner != null)
                Spinner.ValidSpinDirection = validDirections;
        }

        #endregion

        #region Constructors

        static DoubleUpDown()
        {
            UpdateMetadata(typeof(DoubleUpDown), 1d, double.NegativeInfinity, double.PositiveInfinity);
        }

        public DoubleUpDown()
            : this(Double.Parse, Decimal.ToDouble, (v1, v2) => v1 < v2, (v1, v2) => v1 > v2)
        {

        }

        #endregion       
    }
}
