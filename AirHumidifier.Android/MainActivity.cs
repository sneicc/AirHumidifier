using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using static Xamarin.Essentials.Permissions;

namespace AirHumidifier.AndroidApp;

[Activity(
    Label = "AirHumidifier.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        RegisteredServices.RegisteredServices.BluetoothService = new BluetoothService();
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI();
    }

    protected override async void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Platform.Init(this, savedInstanceState);

        await CheckPermissions<BluetoothConnectPermission>();
        await CheckPermissions<Permissions.LocationAlways>();
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }

    public async Task<bool> CheckPermissions<T>() where T : BasePlatformPermission, new()
    {
        var status = await Permissions.CheckStatusAsync<T>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<T>();
        }

        return status == PermissionStatus.Granted;
    }
}

internal class BluetoothConnectPermission : BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new List<(string androidPermission, bool isRuntime)>
      {
          (Android.Manifest.Permission.BluetoothScan, true),
          (Android.Manifest.Permission.BluetoothConnect, true)
      }.ToArray();
}
