using System;
using System.Threading.Tasks;
using AirHumidifier.DataModels;

namespace AirHumidifier.Services
{
    public interface IBluetoothService
    {
        public event Action<BluetoothDeviceBasicProperties> DeviceFounded;
        public event Action<string> ReceiveMessage;

        public Task<bool> ConnectAsync(string mac);

        public void FindDevices();

        public void Disconnect();

        public void StartListenForMessage();

        public Task<bool> SendMessageAsync(string message);
    }
}