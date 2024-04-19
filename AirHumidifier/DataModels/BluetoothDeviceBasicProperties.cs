using System.ComponentModel;

namespace AirHumidifier.DataModels
{
    public class BluetoothDeviceBasicProperties : INotifyPropertyChanged
    {
        public string? Name { get; init; }
        public string? Address { get; init; }

        private string? _connectionStatus;

        public string? ConnectionStatus
        {
            get { return _connectionStatus; }
            set
            {
                _connectionStatus = value;
                OnPropertyChanged(nameof(ConnectionStatus));
            }
        }

        public BluetoothDeviceBasicProperties(string name, string address, string connectionStatus = "")
        {
            Name = name;
            Address = address;
            ConnectionStatus = connectionStatus;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
