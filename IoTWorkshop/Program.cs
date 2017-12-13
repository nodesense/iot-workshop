using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IoTWorkshop
{
    class Program
    {

        static async Task SimulateDevices()
        {
            EdgeDevice edgeDevice1 = new EdgeDevice();
            edgeDevice1.ConnectionString = "HostName=HoneywellIotHub.azure-devices.net;DeviceId=cb1;SharedAccessKey=z5fWMsg2u5+r5hdL6MhW3svuhr90IArRmhjNHX8RSyM=";
            await edgeDevice1.Init();

            edgeDevice1.ReceiveAsync();

        }

        static async Task StartConsumers()
        {
            ConsumerService consumerService = new ConsumerService();
            consumerService.ConnectionString = "Endpoint=sb://ihsuprodsgres009dednamespace.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=hiRM8r03Rft7ktHfJwxstcMIzJRhiPYrEEj9LqiPkqE=";
            consumerService.IotHubD2cEndpoint = "iothub-ehub-honeywelli-290394-7d2e12a167";
            consumerService.Name = "Consumer1";
            consumerService.Offset = "10";
            consumerService.Init();

            consumerService.ReceiveMessages();
            
            ConsumerService consumerService5 = new ConsumerService();
            consumerService5.ConnectionString = "Endpoint=sb://ihsuprodsgres009dednamespace.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=hiRM8r03Rft7ktHfJwxstcMIzJRhiPYrEEj9LqiPkqE=";
            consumerService5.IotHubD2cEndpoint = "iothub-ehub-honeywelli-290394-7d2e12a167";
            consumerService5.Name = "Consumer5";
            consumerService5.Init();

            consumerService5.ReceiveMessages();


            Action DynConsumer = () =>
            {
                ConsumerService dynConsumer = new ConsumerService();
                dynConsumer.ConnectionString = "Endpoint=sb://ihsuprodsgres009dednamespace.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=hiRM8r03Rft7ktHfJwxstcMIzJRhiPYrEEj9LqiPkqE=";
                dynConsumer.IotHubD2cEndpoint = "iothub-ehub-honeywelli-290394-7d2e12a167";
                dynConsumer.Name = "Dynamic";
                dynConsumer.Init();

                dynConsumer.ReceiveMessages();

                
            };


            CancellationToken cancellationToken = new CancellationToken();

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                    DynConsumer();
            }


            ConsumerService consumerService2 = new ConsumerService();
            consumerService2.ConnectionString = "Endpoint=sb://ihsuprodsgres009dednamespace.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=hiRM8r03Rft7ktHfJwxstcMIzJRhiPYrEEj9LqiPkqE=";
            consumerService2.IotHubD2cEndpoint = "iothub-ehub-honeywelli-290394-7d2e12a167";
            consumerService2.ConsumerGroupName = "test";
            consumerService.Name = "Consumer2";
            consumerService2.Init();
            consumerService2.ReceiveMessages();
            
            ConsumerService consumerService3 = new ConsumerService();
            consumerService3.ConnectionString = "Endpoint=sb://iotworkshopalert.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=/3TLY+16tnvKbmMsne4qLqNpaEps5mxKwZJ2oYjmxC0=";
            consumerService3.IotHubD2cEndpoint = "alerts";
            //consumerService2.ConsumerGroupName = "test";
            consumerService3.Name = "Alert";

            consumerService3.Init();
            consumerService3.ReceiveMessages();

        }

        static void Main(string[] args)
        {
                     
            DeviceManager dm = new DeviceManager();
            dm.Connect();

            dm.PrintDevices();

            dm.PrintDeviceInfo("cb1");
            //dm.CreateDevice("cb2");
            
            
            //SimulateDevices();


            ServiceManager serviceManager = new ServiceManager();
            //use IoTHub connection string, NOT device one
            serviceManager.ConnectionString = "HostName=HoneywellIotHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=hiRM8r03Rft7ktHfJwxstcMIzJRhiPYrEEj9LqiPkqE=";
            serviceManager.Init();
            serviceManager.SendMessageToDevice("cb1", "from service");

            serviceManager.ReceiveFeedbacks();

            //Thread.Sleep(5000);

            serviceManager.resetTotal("cb1");


            CombinerBox cb = new CombinerBox();
            cb.HostName = "HoneywellIotHub.azure-devices.net";
            cb.DeviceId = "cb1";
            cb.DeviceKey = "z5fWMsg2u5+r5hdL6MhW3svuhr90IArRmhjNHX8RSyM=";


            cb.Start();


            StartConsumers();

            ///TODO: get items count in queue
            ///

            Console.ReadLine();

        }
    }
}
