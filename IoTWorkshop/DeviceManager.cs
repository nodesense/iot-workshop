using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTWorkshop
{
    public class DeviceManager
    {

        protected RegistryManager registryManager;
        private string connectionString = "HostName=HoneywellIotHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=hiRM8r03Rft7ktHfJwxstcMIzJRhiPYrEEj9LqiPkqE=";

        public void Connect()
        {
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
        }

        public async Task<IEnumerable<Device>> GetDevices()
        {
            IEnumerable<Device> devices;

            devices = await registryManager.GetDevicesAsync(int.MaxValue);
            return devices;
        }

        public async Task PrintDevices()
        {
            IEnumerable<Device> devices = await GetDevices();

            foreach (Device device in devices)
            {
                Console.WriteLine(device.Id);
            }

        }


        public async Task<Device> GetDevice(string deviceId)
        {
            Device device;
            device = await registryManager.GetDeviceAsync(deviceId);
            Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
            return device;
        }

        public async Task PrintDeviceInfo(string deviceId)
        {
            Device device = await GetDevice(deviceId);

            Console.WriteLine(device.Id);

        }

        public async Task<Device> CreateDevice(string deviceId)
        {
            Device device = new Device(deviceId);
            device = await this.registryManager.AddDeviceAsync(device);
            PrintDeviceInfo(deviceId);
            return device;
        }

        public async Task<Device> GetOrCreateDevice(string deviceId)
        {
            Device device;
            try
            {
                device = await this.GetDevice(deviceId);
            }
            catch(Exception e)
            {
                device = await this.CreateDevice(deviceId);
            }

            return device;
        }

    }
}
