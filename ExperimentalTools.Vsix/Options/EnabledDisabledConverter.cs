using System;
using System.ComponentModel;
using System.Globalization;

namespace ExperimentalTools.Vsix.Options
{
    internal class EnabledDisabledConverter : BooleanConverter
    {
        private const string enabled = "Enabled";
        private const string disabled = "Disabled";

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var stringValue = value as string;
            if (stringValue != null)
            {
                if (stringValue.Equals(enabled, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (stringValue.Equals(disabled, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is bool && destinationType == typeof(string))
            {
                return (bool)value ? enabled : disabled;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
