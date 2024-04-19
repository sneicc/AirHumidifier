namespace AirHumidifier.StaticClasses
{
    internal static class CommandGenerator
    {
        internal static string CreateSetIndicationDelayCommand(int delay)
        {
            return $"M3{delay}\n";
        }

        internal static string CreateChangeDisplayedInformationCommand(bool isTimeDisplayed, bool isTemperatureDisplayed, bool isHumidityDisplayed)
        {
            return $"M4{(isTimeDisplayed ? 1 : 0)}{(isTemperatureDisplayed ? 1 : 0)}{(isHumidityDisplayed ? 1 : 0)}\n";
        }

        internal static string CreateDisplayTimeChangeCommand(char type, string time)
        {
            time = time == "AUTO" ? "A" : time;
            return $"M5{type}{time}\n";
        }
    }
}
