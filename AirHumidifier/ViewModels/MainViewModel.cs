using ReactiveUI;
using AirHumidifier.Services;
namespace AirHumidifier.ViewModels;

using AirHumidifier.DataModels;
using AirHumidifier.StaticClasses;
using RegisteredServices;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
public class MainViewModel : ViewModelBase
{
    private bool _isIndicatorSlaiderWaitBeforeNextSend;

    private StringBuilder _messageBuffer;

    private BluetoothDeviceBasicProperties _connectedDevice;
    private IBluetoothService _bluetoothService;

    internal DynamicIndicationSliderViewModel DynamicIndicationSliderViewModel { get; }
    internal DisplaySettingsViewModel DisplaySettingsViewModel { get; }
    public int MaxMicrosecondsDelay => 10_000;//600;
    public ReactiveCommand<Unit, Unit> FindDevicesCommand { get; }
    public ReactiveCommand<string, Unit> ConnectToDeviceCommand { get; }

    public MainViewModel()
    {
        _bluetoothService = RegisteredServices.BluetoothService;

        FindDevicesCommand = ReactiveCommand.Create(FindBluetoothDevices);
        ConnectToDeviceCommand = ReactiveCommand.CreateFromTask<string>(ConnectToBluetoothDevice);
        DynamicIndicationSliderViewModel = new DynamicIndicationSliderViewModel(0, MaxMicrosecondsDelay);
        DisplaySettingsViewModel = new DisplaySettingsViewModel(true, true, true); // change to load from save file or get from bluetooth request

        _bluetoothDeviceBasicPropertiesCollection = new ObservableCollection<BluetoothDeviceBasicProperties>();

        _messageBuffer = new StringBuilder();
    }


    /// <summary>
    /// Must be called after instantiation and passing to DataContext. 
    /// Allows to avoid "Object reference not set to an instance of an object" in the preview
    /// </summary>
    public void RuntimeInit()
    {
        _bluetoothService.DeviceFounded += OnBluetoothDeviceFounded;
        _bluetoothService.ReceiveMessage += OnReceiveMessage;

        this.WhenAnyValue(x => x.DynamicIndicationSliderViewModel.DynamicIndicationDelay)
            .Subscribe(OnDynamicIndicationDelayChanged);

        this.WhenAnyValue(x => x.DisplaySettingsViewModel.IsTimeDisplayed,
            x => x.DisplaySettingsViewModel.IsTemperatureDisplayed,
            x => x.DisplaySettingsViewModel.IsHumidityDisplayed).Subscribe(OnDisplayedElementChanged);

        this.WhenAnyValue(x => x.DisplaySettingsViewModel.TimeDisplayTime).Subscribe(OnTimeDisplayTimeChanged);
        this.WhenAnyValue(x => x.DisplaySettingsViewModel.TemperatureDisplayTime).Subscribe(OnTemperatureDisplayTimeChanged);
        this.WhenAnyValue(x => x.DisplaySettingsViewModel.HumidityDisplayTime).Subscribe(OnHumidityDisplayTimeChanged);
    }


    private bool _isConnecting;
    public bool IsConnecting
    {
        get { return _isConnecting; }
        private set { this.RaiseAndSetIfChanged(ref _isConnecting, value); }
    }


    private ObservableCollection<BluetoothDeviceBasicProperties> _bluetoothDeviceBasicPropertiesCollection;
    public ObservableCollection<BluetoothDeviceBasicProperties> BluetoothDeviceBasicPropertiesCollection
    {
        get { return _bluetoothDeviceBasicPropertiesCollection; }
        private set { this.RaiseAndSetIfChanged(ref _bluetoothDeviceBasicPropertiesCollection, value); }
    }


    private int _userHumidityLevel;
    public string UserHumidityLevel
    {
        get { return _userHumidityLevel.ToString(); }
        set
        {
            if (!double.TryParse(value, out double parsedValue)) return;
            this.RaiseAndSetIfChanged(ref _userHumidityLevel, (int)parsedValue);
            ChangeSetHumidity((uint)_userHumidityLevel);
        }
    }


    private uint _currentHumidity;
    public string CurrentHumidity
    {
        get { return _currentHumidity.ToString(); }
        private set
        {
            if (!uint.TryParse(value, out uint parsedValue)) return;
            this.RaiseAndSetIfChanged(ref _currentHumidity, parsedValue);
        }
    }


    private uint _currentTemperature;
    public string CurrentTemperature
    {
        get { return _currentTemperature.ToString(); }
        private set
        {
            if (!uint.TryParse(value, out uint parsedValue)) return;
            this.RaiseAndSetIfChanged(ref _currentTemperature, parsedValue);
        }
    }


    private async void ChangeSetHumidity(uint humidity)
    {
        string command = CommandGenerator.CreateChangeSetHumidityCommand(humidity);
        bool result = await _bluetoothService.SendMessageAsync(command);

#if DEBUG
        Console.WriteLine("Sended value:" + humidity);
        Console.WriteLine("Humidity changed: " + result);
#endif
    }


    private async void OnTimeDisplayTimeChanged(string time)
    {
        bool result = await ChangeDisplayTime('T', time);

#if DEBUG
        Console.WriteLine("Sended value:" + time);
        Console.WriteLine("Time changed: " + result);
#endif
    }


    private async void OnTemperatureDisplayTimeChanged(string time)
    {
        bool result = await ChangeDisplayTime('C', time);

#if DEBUG
        Console.WriteLine("Sended value:" + time);
        Console.WriteLine("Temperature changed: " + result);
#endif
    }


    private async void OnHumidityDisplayTimeChanged(string time)
    {
        bool result = await ChangeDisplayTime('H', time);

#if DEBUG
        Console.WriteLine("Sended value:" + time);
        Console.WriteLine("Humidity changed: " + result);
#endif
    }


    private async Task<bool> ChangeDisplayTime(char type, string time)
    {
        string command = CommandGenerator.CreateDisplayTimeChangeCommand(type, time);
        return await _bluetoothService.SendMessageAsync(command);
    }


    private async void OnDynamicIndicationDelayChanged(int dynamicIndicationDelay)
    {
        if (_isIndicatorSlaiderWaitBeforeNextSend) return;

        _isIndicatorSlaiderWaitBeforeNextSend = true;

        bool result = await _bluetoothService.SendMessageAsync(CommandGenerator.CreateSetIndicationDelayCommand(dynamicIndicationDelay));
        await Task.Delay(200);

        _isIndicatorSlaiderWaitBeforeNextSend = false;

#if DEBUG
        Console.WriteLine($"Change indication delay to: {dynamicIndicationDelay}");
        Console.WriteLine($"Change indication speed result: {result}");
#endif
    }


    private async void OnDisplayedElementChanged((bool isTimeDisplayed, bool isTemperatureDisplayed, bool isHumidityDisplayed) elements)
    {
        string command = CommandGenerator.CreateChangeDisplayedInformationCommand(elements.isTimeDisplayed, elements.isTemperatureDisplayed, elements.isHumidityDisplayed);
        bool result = await _bluetoothService.SendMessageAsync(command);

#if DEBUG
        Console.WriteLine($"Send to display: {elements.isTimeDisplayed} {elements.isTemperatureDisplayed} {elements.isHumidityDisplayed}");
        Console.WriteLine($"Send to display result: {result}");
#endif
    }


    private void FindBluetoothDevices()
    {
        BluetoothDeviceBasicPropertiesCollection.Clear();
        _bluetoothService.FindDevices();
    }


    private void OnBluetoothDeviceFounded(BluetoothDeviceBasicProperties properties)
    {
        BluetoothDeviceBasicPropertiesCollection.Add(properties);
        this.RaisePropertyChanged(nameof(BluetoothDeviceBasicPropertiesCollection));
    }


    private void OnReceiveMessage(string message)
    {
        if (message == null || message == string.Empty) return;
      
        while (true) 
        {
            int separatorIndex = message.IndexOf('\n');
            if (separatorIndex == -1)
            {
                _messageBuffer.Append(message);
                return;
            }
            else
            {
                _messageBuffer.Append(message[..separatorIndex]);
                HandleMessage(_messageBuffer.ToString());
                _messageBuffer.Clear();

                if (separatorIndex < message.Length - 1) message = message[(separatorIndex + 1)..];
                else return;
            }
        }    
    }


    private void HandleMessage(string message)
    {
        char messageType = message[0];
        string data = message[1..];

        switch (messageType)
        {
            case 'T':
                Console.WriteLine("Enter T, msg: " + message);
                CurrentTemperature = data;
                break;

            case 'H':
                Console.WriteLine("Enter H, msg: " + message);
                CurrentHumidity = data;
                break;
            default:
                Console.WriteLine("Received message: " + message);
                Console.WriteLine("Unknown command: " + messageType);
                break;
        }
    }

    private async Task ConnectToBluetoothDevice(string mac)
    {
        IsConnecting = true;

        bool result = await _bluetoothService.ConnectAsync(mac);
        BluetoothDeviceBasicProperties properties = BluetoothDeviceBasicPropertiesCollection.FirstOrDefault(p => (p.Address == mac));       
        if (result)
        {
            _bluetoothService.StartListenForMessage();
            properties.ConnectionStatus = "Connected";
        }
        else
        {
            properties.ConnectionStatus = "Connection failed! Try again.";
        }

        IsConnecting = false;

#if DEBUG
        Console.WriteLine($"Connection result: {result}");
#endif
    }
}

