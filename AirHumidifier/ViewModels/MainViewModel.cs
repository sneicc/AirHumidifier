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
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml.Linq;

public class MainViewModel : ViewModelBase
{
    private bool _isIndicatorSlaiderWaitBeforeNextSend;

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
    }

    /// <summary>
    /// Must be called after instantiation and passing to DataContext. 
    /// Allows to avoid "Object reference not set to an instance of an object" in the preview
    /// </summary>
    public void RuntimeInit()
    {
        _bluetoothService.DeviceFounded += OnBluetoothDeviceFounded;

        this.WhenAnyValue(x => x.DynamicIndicationSliderViewModel.DynamicIndicationDelay)
            .Subscribe(OnDynamicIndicationDelayChanged);

        this.WhenAnyValue(x => x.DisplaySettingsViewModel.IsTimeDisplayed,
            x => x.DisplaySettingsViewModel.IsTemperatureDisplayed,
            x => x.DisplaySettingsViewModel.IsHumidityDisplayed).Subscribe(OnDisplayedElementChanged);

        this.WhenAnyValue(x => x.DisplaySettingsViewModel.TimeDisplayTime).Subscribe(OnTimeDisplayTimeChanged);
        this.WhenAnyValue(x => x.DisplaySettingsViewModel.TemperatureDisplayTime).Subscribe(OnTemperatureDisplayTimeChanged);
        this.WhenAnyValue(x => x.DisplaySettingsViewModel.HumidityDisplayTime).Subscribe(OnHumidityDisplayTimeChanged);
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
        return await _bluetoothService.SendDataAsync(command);       
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


    private async void OnDynamicIndicationDelayChanged(int dynamicIndicationDelay)
    {
        if (_isIndicatorSlaiderWaitBeforeNextSend) return;

        _isIndicatorSlaiderWaitBeforeNextSend = true;

        bool result = await _bluetoothService.SendDataAsync(CommandGenerator.CreateSetIndicationDelayCommand(dynamicIndicationDelay));
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
        bool result = await _bluetoothService.SendDataAsync(command);

#if DEBUG
        Console.WriteLine($"Send to display: {elements.isTimeDisplayed} {elements.isTemperatureDisplayed} {elements.isHumidityDisplayed}");
        Console.WriteLine($"Send to display result: {result}");
#endif
    }


    private int _userHumidityLevel;
    public string UserHumidityLevel
    {
        get { return _userHumidityLevel.ToString(); }
        set
        {
            if (!double.TryParse(value, out double parsedValue)) return;
            this.RaiseAndSetIfChanged(ref _userHumidityLevel, (int)parsedValue);
        }
    }


    private uint _currentHumidity;
    public string CurrentHumidity
    {
        get { return _currentHumidity.ToString(); }
        private set 
        {
            throw new NotImplementedException();
        }
    }


    private uint _currentTemperature;
    public string CurrentTemperature
    {
        get { return _currentTemperature.ToString(); }
        private set
        {
            throw new NotImplementedException();
        }
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

    private async Task ConnectToBluetoothDevice(string mac)
    {
        IsConnecting = true;

        bool result = await _bluetoothService.ConnectAsync(mac);
        BluetoothDeviceBasicProperties properties = BluetoothDeviceBasicPropertiesCollection.FirstOrDefault(p => (p.Address == mac));       
        if (result)
        {
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

