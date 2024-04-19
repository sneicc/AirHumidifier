namespace Interfaces
{
    public interface IBluetoothService
    {
        Task<bool> ConnectAsync();
        bool Disconnect();
        string RecievData(string data);
        string GetData();
        void FindDevices();
    }
}
