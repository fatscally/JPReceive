using System;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;
using System.Xml.Serialization;
using System.IO;
using Jp.Models;
using System.Collections.Generic;
using System.Linq;

namespace JPR
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ReceivedItems = new();

            StartListeningForMessages();

        }


        static List<SaleItem> ReceivedItems;


        static void StartListeningForMessages()
        {

            var factory = new ConnectionFactory() { HostName = "localhost" };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "qSalesItems", durable: false,
                  exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                SaleItem sim = XmlDeserializeFromString<SaleItem>(message);
                ReceivedItems.Add(sim);
                Console.WriteLine(" [x] Received {0}", sim.ToString());


                //Spec : 
                //After 50 messages your application should log that it is pausing, stop accepting new
                //messages and log a report of the adjustments that have been made to each sale type while
                //the application was running.
                if (ReceivedItems.Count > 50)
                {
                    Console.WriteLine("*****   PAUSED  ******");
                }
                //Spec: After every 10th message received your application 
                //should log a report detailing the number
                //of sales of each product and their total value.
                if (ReceivedItems.Count % 10 == 0)
                    PrintSalesSummary();

            };


            channel.BasicConsume(queue: "qSalesItems",
                                 autoAck: true, consumer: consumer);


            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();


            PrintObjects();
            PrintSalesSummary();

        }


        public static T XmlDeserializeFromString<T>(string objectData)
        {
            return (T)XmlDeserializeFromString(objectData, typeof(T));
        }

        public static object XmlDeserializeFromString(string objectData, Type type)
        {
            var serializer = new XmlSerializer(type);
            object result;

            using (TextReader reader = new StringReader(objectData))
            {
                result = serializer.Deserialize(reader);
            }

            return result;
        }


        static void PrintObjects()
        {

            foreach (SaleItem item in ReceivedItems)
            {
                string strOutput = string.Concat(item.ProductType, " ", item.ProductValue.ToString());
                Console.WriteLine(strOutput);
            }

            Console.WriteLine(string.Concat("Number of sale items: ",ReceivedItems.Count.ToString()));
        }


        static void PrintSalesSummary()
        {
            Console.WriteLine("***** Sales Summary *****");

            List<SalesSummary> summaries = ReceivedItems
                    .GroupBy(p => p.ProductType)
                    .Select(x => new SalesSummary 
                        { 
                            ProductType = x.Key,
                            TotalAmountSales = x.Sum(y => y.ProductValue),
                            TotalUnitSales = x.Count()
                        }).ToList();

            foreach (SalesSummary item in summaries)
            {
                Console.WriteLine(item.ProductType + ":   Units " + item.TotalUnitSales + ":   $" + item.TotalAmountSales  );
            }

            Console.WriteLine("*****   **********   *****");

        }

    }
}
