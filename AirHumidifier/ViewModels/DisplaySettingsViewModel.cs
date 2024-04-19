using ReactiveUI;

namespace AirHumidifier.ViewModels
{
    internal class DisplaySettingsViewModel : ReactiveObject
    {
        public DisplaySettingsViewModel(bool isTimeDisplayed, bool isTemperatureDisplayed, bool isHumidityDisplayed)
        {
            IsTimeDisplayed = isTimeDisplayed;
            IsTemperatureDisplayed = isTemperatureDisplayed;
            IsHumidityDisplayed = isHumidityDisplayed;

            TimeDisplayTime = string.Empty;
            TemperatureDisplayTime = string.Empty;
            HumidityDisplayTime = string.Empty;
        }

        private bool _isTimeDisplayed;
		public bool IsTimeDisplayed
        {
			get { return _isTimeDisplayed; }
			set 
			{
				this.RaiseAndSetIfChanged(ref _isTimeDisplayed, value); 
			}
		}

        private bool _isTemperatureDisplayed;
        public bool IsTemperatureDisplayed
        {
            get { return _isTemperatureDisplayed; }
            set
            {
                this.RaiseAndSetIfChanged(ref _isTemperatureDisplayed, value);
            }
        }

        private bool _isHumidityDisplayed;
        public bool IsHumidityDisplayed
        {
            get { return _isHumidityDisplayed; }
            set
            {
                this.RaiseAndSetIfChanged(ref _isHumidityDisplayed, value);
            }
        }

        private string _timeDisplayTime;
        public string TimeDisplayTime
        {
            get { return _timeDisplayTime; }
            set
            {
                if (!TryConvertInputValue(value, out string result)) return;
                this.RaiseAndSetIfChanged(ref _timeDisplayTime, result);
            }
        }

        private string _temperatureDisplayTime;
        public string TemperatureDisplayTime
        {
            get { return _temperatureDisplayTime; }
            set
            {
                if (!TryConvertInputValue(value, out string result)) return;
                this.RaiseAndSetIfChanged(ref _temperatureDisplayTime, result);
            }
        }

        private string _humidityDisplayTime;
        public string HumidityDisplayTime
        {
            get { return _humidityDisplayTime; }
            set 
            {
                if (!TryConvertInputValue(value, out string result)) return;
                this.RaiseAndSetIfChanged(ref _humidityDisplayTime, result);
            }
        }

        private bool TryConvertInputValue(string value, out string result) 
        {
            if (value == "" || (value.Length == 1 && value[0] == '0'))
            {
                result = "AUTO";
                return true;
            }

            result = "";
            if (!int.TryParse(value, out _)) return false;

            result = value;
            return true;
        }
    }
}
