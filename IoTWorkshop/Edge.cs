using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IoTWorkshop
{
    public class Edge
    {
        protected DeviceClient deviceClient;


        public Device Device
        {
            get;
            set;
        }

        protected Twin twin;

        public Twin Twin
        {
            get;
            set;
        }

        public string HostName
        {
            get;
            set;
        }


        public string DeviceId
        {
            get;
            set;
        }


        private string deviceKey;


        public string DeviceKey //SharedAccessKey
        {
            set
            {
                deviceKey = value;
            }
        }

        public Microsoft.Azure.Devices.Client.TransportType Transport
        {
            get;
            set;
        }


        public Edge()
        {
            this.Transport = Microsoft.Azure.Devices.Client.TransportType.Mqtt;
        }

        public async Task Connect()
        {
            Console.WriteLine("Connecting to hub");
            string deviceConnectionString = "HostName=" + HostName + ";DeviceId=" + DeviceId + ";SharedAccessKey=" + deviceKey;

            deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, this.Transport);
            await InitTwin();
        }

        virtual public async Task Close()
        {
            //await deviceClient.CloseAsync().Wait();
            await deviceClient.CloseAsync();
        }

        //device twin meta information


        public async Task InitTwin()
        {
            this.twin = await deviceClient.GetTwinAsync();
        }

        public async void updateTwin(string section, string property, string value)
        {
            try
            {
                Console.WriteLine("Sending connectivity data as reported property");


                TwinCollection reportedProperties, connectivity;
                reportedProperties = new TwinCollection();
                connectivity = new TwinCollection();
                connectivity[property] = value;
                reportedProperties[section] = connectivity;
                await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
                await InitTwin();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error in sample: {0}", ex.Message);
            }
        }

        //FIXME: converts to string, support all primitives types
        public async void updateTwinIfDiff(string section, string property, string value)
        {
            if (twin != null && twin.Properties.Reported.Contains(section))
            {
                JObject obj = twin.Properties.Reported[section];

                JToken obj2 = obj[property];

                //FIXME: converts to string, support all primitives types

                if (obj2 != null && obj2.Value<String>() != value)
                {
                    updateTwin(section, property, value.ToString());
                }
            }
            else
            {
                updateTwin(section, property, value.ToString());
            }
        }

        public async Task<Twin> getTwin()
        {
            Twin twin = await deviceClient.GetTwinAsync();
            return twin;
        }

        SortedSet<String> clientMethods = new SortedSet<string>();

        public async Task RegisterMethod(string methodName, MethodCallback method)
        {
            // setup callback for "writeLine" method
            deviceClient.SetMethodHandlerAsync(methodName, method, null).Wait();
            clientMethods.Add(methodName);
            Console.WriteLine("Waiting for direct method call\n Press enter to exit.");
        }

        public async Task UnRegisterMethods()
        {
            foreach (string methodName in this.clientMethods)
            {
                deviceClient.SetMethodHandlerAsync(methodName, null, null).Wait();
            }
        }




        public async Task Run(Action action, TimeSpan period, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(period, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                    action();
            }
        }

        public Task Run(Action action, TimeSpan period)
        {
            return Run(action, period, CancellationToken.None);
        }


        public async void UploadFile(string filePath, string cloudFileName)
        {

            string fileName = filePath;
            Console.WriteLine("Uploading file: {0}", fileName);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            using (var sourceData = new FileStream(filePath, FileMode.Open))
            {
                await deviceClient.UploadToBlobAsync(cloudFileName, sourceData);
            }

            watch.Stop();
            Console.WriteLine("Time to upload file: {0}ms\n", watch.ElapsedMilliseconds);
        }

    }
}
