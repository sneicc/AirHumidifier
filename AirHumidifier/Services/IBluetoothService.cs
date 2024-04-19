using System;
using System.Threading.Tasks;
using AirHumidifier.DataModels;

namespace AirHumidifier.Services
{
    public interface IBluetoothService
    {
        public event Action<BluetoothDeviceBasicProperties> DeviceFounded;

        public Task<bool> ConnectAsync(string mac);

        public void FindDevices();

        public bool Disconnect();

        public Task<string> GetDataAsync();

        public Task<bool> SendDataAsync(string message);
    }
}