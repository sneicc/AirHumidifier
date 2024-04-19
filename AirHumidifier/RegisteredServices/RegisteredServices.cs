using AirHumidifier.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirHumidifier.RegisteredServices
{
    public static class RegisteredServices
    {
        public static IBluetoothService BluetoothService { get; set; }
    }
}
