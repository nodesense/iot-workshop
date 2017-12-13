using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTWorkshop
{
    public class ServiceManager
    {
        protected ServiceClient serviceClient;

        public string ConnectionString
        {
            get;
            set;
        }


        public void Init()
        {
            serviceClient = ServiceClient.CreateFromConnectionString(ConnectionString);
        }

        public async Task SendMessageToDevice(string deviceId, string message)
        {
            Message commandMessage = new Message(Encoding.ASCII.GetBytes(message));

            commandMessage.MessageId = Guid.NewGuid().ToString();

            //Full: to receive feedback on fail or success both the cases
            //None: No ack
            //Negative: on failure only
            //Positive: on Success
            commandMessage.Ack = DeliveryAcknowledgement.Full;
             
            await serviceClient.SendAsync(deviceId, commandMessage);
        }

        public async Task ReceiveFeedbacks()
        {
            //To Receive feedback

            var feedbackReceiver = serviceClient.GetFeedbackReceiver();

            Console.WriteLine("\nReceiving c2d feedback from service");
            while (true)
            {
                var feedbackBatch = await feedbackReceiver.ReceiveAsync();
                if (feedbackBatch == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;

                foreach (FeedbackRecord feedbackRecord in feedbackBatch.Records)
                {
                    Console.WriteLine("ID: {0}, Status {1}", feedbackRecord.OriginalMessageId, feedbackRecord.StatusCode);
                }

                Console.ResetColor();


                await feedbackReceiver.CompleteAsync(feedbackBatch);
            }
        }

        public async Task ReceiveFileUploadNotificationAsync()
        {
            Console.WriteLine("waiting for file notification");

            var notificationReceiver = serviceClient.GetFileNotificationReceiver();

            Console.WriteLine("\nReceiving file upload notification from service");
            while (true)
            {
                var fileUploadNotification = await notificationReceiver.ReceiveAsync();
                if (fileUploadNotification == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received file upload noticiation: {0}", string.Join(", ", fileUploadNotification.BlobName));
                Console.ResetColor();

                await notificationReceiver.CompleteAsync(fileUploadNotification);
            }
        }


        public async Task StartFirmwareUpdate(string deviceId)
        {
            CloudToDeviceMethod method = new CloudToDeviceMethod("firmwareUpdate");
            method.ResponseTimeout = TimeSpan.FromSeconds(90);
            method.ConnectionTimeout = TimeSpan.FromSeconds(60);
            method.SetPayloadJson(
                @"{
                         firmwareUrl : 'https://example.com'
                }");

            CloudToDeviceMethodResult result = await serviceClient.InvokeDeviceMethodAsync(deviceId, method);

            Console.WriteLine("Invoked firmware update on device.");
        }

        public async Task resetTotal(string deviceId)
        {
            CloudToDeviceMethod method = new CloudToDeviceMethod("resetTotal");
            method.ResponseTimeout = TimeSpan.FromSeconds(90);
            method.ConnectionTimeout = TimeSpan.FromSeconds(60);
            method.SetPayloadJson(
                @"{
                         reset :  'true'
                }");

            CloudToDeviceMethodResult result = await serviceClient.InvokeDeviceMethodAsync(deviceId, method);

            Console.WriteLine("Invoked resetTotal   on device.");
        }

        public async Task factoryReset(string deviceId)
        {
            CloudToDeviceMethod method = new CloudToDeviceMethod("factoryReset");
            method.ResponseTimeout = TimeSpan.FromSeconds(90);
            method.ConnectionTimeout = TimeSpan.FromSeconds(60);
            method.SetPayloadJson(
                @"{
                         reset :  'true'
                }");

            CloudToDeviceMethodResult result = await serviceClient.InvokeDeviceMethodAsync(deviceId, method);

            Console.WriteLine("Invoked factoryReset on device.");
        }

    }
}
