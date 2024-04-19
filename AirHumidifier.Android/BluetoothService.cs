using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirHumidifier.Services;
using Android.Bluetooth;
using Android.Content;
using Java.Util;
using Console = System.Console;
using Encoding = System.Text.Encoding;
using AirHumidifier.DataModels;
using Java.Lang;
using Exception = System.Exception;

namespace AirHumidifier.AndroidApp
{
    internal class BluetoothService : IBluetoothService
    {
        public event Action<BluetoothDeviceBasicProperties> DeviceFounded;
        public event Action<BluetoothDeviceBasicProperties> DeviceDisconnected;
        public event Action<string> ReceiveMessage;

        private BluetoothAdapter _bluetoothAdapter;
        private List<BluetoothDevice> _foundDevices;
        private BluetoothDeviceReceiver _receiver;
        private BluetoothSocket _socket;
        private BluetoothDevice _connectedDevice;

        public BluetoothService() 
        {
            _bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            _foundDevices = new List<BluetoothDevice>();
            _receiver = new BluetoothDeviceReceiver();
          
            var intentFilter = new IntentFilter();
            intentFilter.AddAction(BluetoothDevice.ActionFound);
            intentFilter.AddAction(BluetoothDevice.ActionAclDisconnected);
            Android.App.Application.Context.RegisterReceiver(_receiver, intentFilter);

            _receiver.DeviceFounded += OnDeviceFounded;
            _receiver.DeviceDisconnected += OnDeviceDisconected;
        }

        public async Task<bool> ConnectAsync(string mac)
        {
            var device = _foundDevices.FirstOrDefault(device => device.Address == mac);
            if (device == null) return false;

            _socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));

            try
            {
                await _socket.ConnectAsync();
            }
            catch (Exception)
            {
                return false;
            }

            _connectedDevice = device;

            return true;
        }

        public void Disconnect()
        {
            _connectedDevice = null;
            _socket.Close();
            _socket.Dispose();
        }

        public void FindDevices()
        {
            if (_bluetoothAdapter.IsDiscovering) _bluetoothAdapter.CancelDiscovery();
            if (_bluetoothAdapter == null || !_bluetoothAdapter.IsEnabled)
            {
#if DEBUG
                Console.WriteLine("Bluetooth is null or disabled");
#endif
                return;
            }

            _foundDevices.Clear();           
            _bluetoothAdapter.StartDiscovery();
        }

        public async void StartListenForMessage()
        {
            var textBuffer = new byte[1024];

            while(true)
            {
                try
                {
                    int bytesReaded = await _socket.InputStream.ReadAsync(textBuffer, 0, textBuffer.Length);
                    string message = Encoding.ASCII.GetString(textBuffer, 0, bytesReaded);
                    ReceiveMessage?.Invoke(message);             
                }
                catch (Exception e)
                {
                    Console.WriteLine("Stop listen for message!");
                    Console.WriteLine("Error: " + e.Message);
                    break;
                }
            }           
        }

        public async Task<bool> SendMessageAsync(string message)
        {
            uint messageLength = (uint)message.Length;
            byte[] countBuffer = BitConverter.GetBytes(messageLength);
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            try
            {
                //await _socket.OutputStream.WriteAsync(countBuffer, 0, countBuffer.Length);
                await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception)
            {
                return false;
            }
            
            return true;
        }

        private void OnDeviceFounded(BluetoothDevice device)
        {
            _foundDevices.Add(device);

            var deviceProperties = new BluetoothDeviceBasicProperties(device.Name, device.Address);
            if (_connectedDevice != null && _socket != null && device.Address == _connectedDevice.Address && _socket.IsConnected)
            {
                deviceProperties.ConnectionStatus = "Connected";
            }
            DeviceFounded?.Invoke(deviceProperties);
#if DEBUG
            Console.WriteLine("Add device with name: " + device.Name);
#endif
        }

        private void OnDeviceDisconected(BluetoothDevice device)
        {
            if(_connectedDevice != null && device.Address == _connectedDevice.Address)
            {
                _connectedDevice = null;
                _socket.Close();
                _socket.Dispose();

                DeviceDisconnected?.Invoke(new BluetoothDeviceBasicProperties(device.Name, device.Address, BluetoothDevice.ActionAclDisconnected));
            }           
        }
    }

    internal class BluetoothDeviceReceiver : BroadcastReceiver
    {
        public event Action<BluetoothDevice> DeviceFounded;
        public event Action<BluetoothDevice> DeviceDisconnected;

        public override void OnReceive(Context? context, Intent? intent)
        {
            var action = intent.Action;
            BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

            if (action == BluetoothDevice.ActionFound)
            {
                
                DeviceFounded?.Invoke(device);
            }
            else if (action == BluetoothDevice.ActionAclDisconnected)
            {
                DeviceDisconnected?.Invoke(device);
            }          
        }
    }
}
