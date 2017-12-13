using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IoTWorkshop
{
    public class ConsumerService
    {
        //temp name to know consumer log
        public string Name
        {
            get;
            set;
        }

        public string ConnectionString
        {
            get;
            set;
        }
       //     = "Endpoint=sb://iothub-ns-solarhub-278298-892d07c7e6.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=L+i3XhnX+MRbxN8TobkDHsFb24WDxl4t+hL8EsIs1j8=";

        public string IotHubD2cEndpoint
        {
            get;
            set;
        }
            //= "solarhub";

        public string ConsumerGroupName
        {
            get;set;
        }

        EventHubClient eventHubClient;

        public ConsumerService()
        {
            ConsumerGroupName = "$Default";
        }

        public void Init()
        {
            eventHubClient = EventHubClient.CreateFromConnectionString(ConnectionString, IotHubD2cEndpoint);
        }

        public string Offset
        {
            get;
            set;
        }

        public async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {
            //EventHubConsumerGroup consumerGroup = eventHubClient.GetDefaultConsumerGroup();

            EventHubConsumerGroup consumerGroup = eventHubClient.GetConsumerGroup(ConsumerGroupName);


            //var eventHubReceiver = consumerGroup.CreateReceiver(partition, DateTime.UtcNow);
            EventHubReceiver eventHubReceiver;

            if (string.IsNullOrWhiteSpace(this.Offset))
                eventHubReceiver = consumerGroup.CreateReceiver(partition, DateTime.UtcNow);
            else
            {
                eventHubReceiver = consumerGroup.CreateReceiver(partition, Offset);

            }
            while (true)
            {
                if (ct.IsCancellationRequested) break;
                EventData eventData = await eventHubReceiver.ReceiveAsync();
                if (eventData == null) continue;
                
                Console.ForegroundColor = ConsoleColor.Yellow;

                string data = Encoding.UTF8.GetString(eventData.GetBytes());
                Console.WriteLine("****Message received. Partition: {0} Offset: {1} Seq: {2}   Data: '{3}' Group: {4} Name: {5}", 
                                
                                partition, 
                                eventData.Offset,
                                eventData.SequenceNumber,
                                data,
                                ConsumerGroupName,
                                Name
                                );

                Console.ResetColor();

            }
        }


        public void ReceiveMessages()
        {
            Console.WriteLine("Receive messages. Ctrl-C to exit.\n");

            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;


            CancellationTokenSource cts = new CancellationTokenSource();

            /*
            System.Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };
            */

            var tasks = new List<Task>();
            
            foreach (string partition in d2cPartitions)
            {
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition, cts.Token));
            }

            //tasks.Add(ReceiveMessagesFromDeviceAsync("1", cts.Token));


            //Task.WaitAll(tasks.ToArray());
        }


    }
}
