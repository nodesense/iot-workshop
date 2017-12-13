using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTWorkshop
{
    public class EdgeDevice
    {

        DeviceClient deviceClient;

        public string ConnectionString
        {
            get;
            set;
        }

        public async Task Init()
        {
            Console.WriteLine("Connecting to hub");
             
            deviceClient = DeviceClient.CreateFromConnectionString(ConnectionString, 
                                  TransportType.Mqtt);

            await deviceClient.SetMethodHandlerAsync("resetTotal", this.onResetTotal, null);
            
        }

        public async void ReceiveAsync()
        {
            Console.WriteLine("\nReceiving cloud to device messages from service");
            while (true)
            {
                Message receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received message: {0}", Encoding.ASCII.GetString(receivedMessage.GetBytes()));
                Console.ResetColor();

                //FIXME:

                //Works with HTTP
                //remove the lock, remove from queue
                //await deviceClient.RejectAsync(receivedMessage);

                //successfully procedded, remove from queue

                //Works with HTTP, MQTT. AMQP QOS level 2
                await deviceClient.CompleteAsync(receivedMessage);


                //abondon
                //remove the lock, keep in the queue
                //Works with HTTP
                // await deviceClient.AbandonAsync(receivedMessage);
            }
        }


        public Task<MethodResponse> onResetTotal(MethodRequest methodRequest, object userContext)
        {
            //this.totalWattsHr = 0;
            Console.WriteLine("onResetTotal called");

            string result = "'Reset done'";

            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

    }
}
