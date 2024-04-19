using ReactiveUI;

namespace AirHumidifier.ViewModels
{
    internal class SliderBaseViewModel : ReactiveObject
    {
        public readonly int MaxValue;
        public readonly int MinValue;

        protected double _value;
        public double Value 
        {
            get { return _value; }
            set 
            {
                this.RaiseAndSetIfChanged(ref _value,  value);
            }
        }

        public SliderBaseViewModel(int maxValue, int minValue)
        {
            MaxValue = maxValue;
            MinValue = minValue;
        }
    }
}
