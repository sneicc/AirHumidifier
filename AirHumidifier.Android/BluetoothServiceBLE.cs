using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Exceptions;
using AirHumidifier.Services;
using AirHumidifier.DataModels;


namespace AirHumidifier.AndroidApp
{
    internal class BluetoothServiceBLE //: IBluetoothService // not used
    {
        private IBluetoothLE _ble;
        private IAdapter _adapter;
        private List<IDevice> _devices;

        public event Action<BluetoothDeviceBasicProperties> DeviceFounded;

        public BluetoothServiceBLE()
        {
            _ble = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;
            _devices = new List<IDevice>();
            _adapter.DeviceDiscovered += (s, a) => _devices.Add(a.Device);
        }

        public async Task<bool> ConnectAsync(string mac)
        {
            _devices.Clear();

            if (!_adapter.IsScanning)
            {
                await _adapter.StartScanningForDevicesAsync();
            }

            IDevice myDevice = null;
            foreach (var device in _devices)
            {
#if DEBUG
                Console.WriteLine(device.Name);
                Console.WriteLine(device.Id);
                Console.WriteLine();
#endif

                if (device.Name == "HC-06")
                {
#if DEBUG
                    Console.WriteLine();
                    Console.WriteLine("!!!Find!!!");
                    Console.WriteLine();
#endif
                    myDevice = device;
                    break;
                }
            }

            try
            {
                await _adapter.ConnectToDeviceAsync(myDevice);
            }
            catch (DeviceConnectionException e)
            {
                return false;
                // ... could not connect to device
            }

            //await _adapter.StopScanningForDevicesAsync();

            return true;
        }

        public async void FindDevices()
        {
            //await _adapter.StartScanningForDevicesAsync();
        }

        public bool Disconnect()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetDataAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendMessageAsync(string message)
        {
            throw new NotImplementedException();
        }
    }
}
