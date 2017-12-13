using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IoTWorkshop
{
    public class CombinerBox : Edge
    {
        //Input from sensors
        protected double string1Voltage; //voltage sensor
        protected double string1Current; //current sensor

        protected double string2Voltage;
        protected double string2Current;

        //output from combiner box
        protected double outputVoltage;
        protected double outputCurrent;

        protected double outputTempeature;

        //derived data    
        protected double outputWatt; // volt * current

        //  1 kwatts per hour is called 1 unit current.
        protected double outputWattsInHour;

        // per hour watts produced so far
        protected double totalWattsHr = 0;


        static int messageId = 1;

        static CombinerBox()
        {
            messageId = new Random().Next(1000, 10000);

        }

        bool string1Status;
        public bool String1Status
        {
            get
            {
                return string1Status;
            }

            set
            {
                string1Status = value;
                updateTwinIfDiff("alerts", "string1Alert", value.ToString());
            }
        }


        bool string2Status;
        public bool String2Status
        {
            get
            {
                return string2Status;
            }

            set
            {
                string2Status = value;
                updateTwinIfDiff("alerts", "string2Alert", value.ToString());
            }
        }


        async Task doSimulate()
        {
            Console.WriteLine("From Second Working");

            double minTemperature = 20;
            double minHumidity = 60;
            Random rand = new Random();

            double currentTemperature = minTemperature + rand.NextDouble() * 15;
            double currentHumidity = minHumidity + rand.NextDouble() * 20;


            string1Voltage = 10 + rand.NextDouble() * 2;
            int r = rand.Next();

            if (r % 6 == 0)
            {
                string1Voltage += 10;
            }


            string2Voltage = 10 + rand.NextDouble() * 2;

            string1Current = 10 + rand.NextDouble() * 20;
            string2Current = 10 + rand.NextDouble() * 20;



            if (r % 5 == 0)
            {
                this.String1Status = false;
            }
            else
            {
                this.String1Status = false;
            }

            if (r % 11 == 0)
            {
                this.String2Status = false;
            }
            else
            {
                this.String1Status = true;
            }

            if (this.String1Status || String2Status)
            {
                //FIX: Set combiner box alarm
            }

            outputVoltage = string1Voltage + string2Voltage;
            outputCurrent = (string1Current + string2Current) / 2;

            int overrange = outputVoltage > 24 ? 1 : 0;


            outputWatt = outputVoltage * outputCurrent;
            totalWattsHr += outputWatt;
            //FIXME: 
            outputWattsInHour += outputWatt;


            var telemetryDataPoint = new
            {
                messageId = messageId++,
                deviceId = DeviceId,
                temperature = currentTemperature,
                humidity = currentHumidity,
                voltage = outputVoltage,
                current = outputCurrent,
                watt = outputWatt,
                overrange = overrange,
                messageType = r % 3 == 0 ? "alert" : "normal"
            };

            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");
            message.Properties.Add("stringAlert", (this.String1Status || this.String2Status) ? "true" : "false");

            message.Properties.Add("messageType", r % 3 == 0 ? "alert" : "normal");


            Console.ForegroundColor = ConsoleColor.Green;

            await deviceClient.SendEventAsync(message);
            Console.WriteLine("Sending message: {0} >  {1}", DateTime.Now, messageString);
            
            //    await Task.Delay(1000);

        }

        protected CancellationToken cancellationToken = new CancellationToken();


        public Task<MethodResponse> onResetTotal(MethodRequest methodRequest, object userContext)
        {
            this.totalWattsHr = 0;

            string result = "'Reset done'";

            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        public Task<MethodResponse> onResetAll(MethodRequest methodRequest, object userContext)
        {

            this.totalWattsHr = 0;
            this.totalWattsHr = 0;

            string result = "'Reset All done'";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        async Task RegisterMethods()
        {
            await this.RegisterMethod("resetTotal", this.onResetTotal);
            await this.RegisterMethod("resetAll", this.onResetAll);
        }


        public async Task Start()
        {
            this.Connect();
            await this.RegisterMethods();

            Action simulateAction = () =>
            {
                doSimulate();
            };

            this.Run(simulateAction, TimeSpan.FromSeconds(10));
        }

        override public async Task Close()
        {
            await base.Close();
        }

    }
}
