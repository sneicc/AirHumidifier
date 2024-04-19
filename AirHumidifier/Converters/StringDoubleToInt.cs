using Avalonia.Data.Converters;
using System;
using System.Globalization;


namespace AirHumidifier.Converters
{
    public class StringDoubleToInt : IValueConverter
    {
        //public static readonly StringDoubleToInt Instance = new();
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return 0;

            return (double)value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return 0;

            return (double)value;
        }
    }

    //public static class MyConverters
    //{
    //    /// <summary>
    //    /// Gets a Converter that takes a number as input and converts it into a text representation
    //    /// </summary>
    //    public static FuncValueConverter<decimal?, string> MyConverter { get; } =
    //        new FuncValueConverter<decimal?, string>(num => $"Your number is: '{num}'");

    //    /// <summary>
    //    /// Gets a Converter that takes several numbers as input and converts it into a text representation
    //    /// </summary>
    //    public static FuncMultiValueConverter<decimal?, string> MyMultiConverter { get; } =
    //        new FuncMultiValueConverter<decimal?, string>(num => $"Your numbers are: '{string.Join(", ", num)}'");

    //    public static FuncValueConverter<bool, string> BoolToStringConverter { get; } =
    //    new FuncValueConverter<bool, string>(b =>
    //    {
    //        if (b) return "True";
    //        else return "False";
    //    });
    //}
}
