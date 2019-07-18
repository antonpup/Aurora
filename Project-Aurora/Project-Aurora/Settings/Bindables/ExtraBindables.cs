using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings.Bindables
{
    public class BindableDouble : BindableNumber<double>
    {
        public override bool IsDefault => Math.Abs(Value - Default) < Precision;

        protected override double DefaultMinValue => double.MinValue;
        protected override double DefaultMaxValue => double.MaxValue;
        protected override double DefaultPrecision => double.Epsilon;

        public BindableDouble(double value = 0)
            : base(value)
        {
        }

        public override string ToString() => Value.ToString("0.0###", NumberFormatInfo.InvariantInfo);
    }

    public class BindableFloat : BindableNumber<float>
    {
        public override bool IsDefault => Math.Abs(Value - Default) < Precision;

        protected override float DefaultMinValue => float.MinValue;
        protected override float DefaultMaxValue => float.MaxValue;
        protected override float DefaultPrecision => float.Epsilon;

        public BindableFloat(float value = 0)
            : base(value)
        {
        }

        public override string ToString() => Value.ToString("0.0###", NumberFormatInfo.InvariantInfo);
    }

    public class BindableInt : BindableNumber<int>
    {
        protected override int DefaultMinValue => int.MinValue;
        protected override int DefaultMaxValue => int.MaxValue;
        protected override int DefaultPrecision => 1;

        public BindableInt(int value = 0)
            : base(value)
        {
        }

        public override string ToString() => Value.ToString(NumberFormatInfo.InvariantInfo);
    }

    public class BindableLong : BindableNumber<long>
    {
        protected override long DefaultMinValue => long.MinValue;
        protected override long DefaultMaxValue => long.MaxValue;
        protected override long DefaultPrecision => 1;

        public BindableLong(long value = 0)
            : base(value)
        {
        }

        public override string ToString() => Value.ToString(NumberFormatInfo.InvariantInfo);
    }

    public class BindableBool : Bindable<bool>
    {
        public BindableBool(bool value = false)
            : base(value)
        {
        }

        public override string ToString() => Value.ToString();

        public override void Parse(object input)
        {
            if (input.Equals("1"))
                Value = true;
            else if (input.Equals("0"))
                Value = false;
            else
                base.Parse(input);
        }

        public void Toggle() => Value = !Value;
    }
}
