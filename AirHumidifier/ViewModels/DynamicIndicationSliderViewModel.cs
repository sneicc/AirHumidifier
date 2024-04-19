using ReactiveUI;
using System;

namespace AirHumidifier.ViewModels
{
    internal class DynamicIndicationSliderViewModel : SliderBaseViewModel
    {
        public DynamicIndicationSliderViewModel(int minValue, int maxValue) : base(maxValue, minValue) 
        {
            this.WhenAnyValue(x => x.Value)
                .Subscribe(newValue => DynamicIndicationDelay = (int)newValue);
        }

        private int _dynamicIndicationDelay;
        public int DynamicIndicationDelay
        {
            get { return _dynamicIndicationDelay; }
            private set
            {
                int switchDelay = (value * MaxValue) / 100;
                this.RaiseAndSetIfChanged(ref _dynamicIndicationDelay, switchDelay);
            }
        }
    }
}
