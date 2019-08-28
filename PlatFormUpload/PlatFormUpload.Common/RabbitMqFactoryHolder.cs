using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlatFormUpload.Common
{
    public static class RabbitMqFactoryHolder
    {
        private static string rabbitmqHost = System.Configuration.ConfigurationManager.AppSettings["RabbitMqHostName"];
        private static string rabbitmqUserName = System.Configuration.ConfigurationManager.AppSettings["RabbitMqUserName"];
        private static string rabbitmqPassword = System.Configuration.ConfigurationManager.AppSettings["RabbitMqPassword"];

        private static readonly Lazy<ConnectionFactory> factoryHolder = new Lazy<ConnectionFactory>(() =>
        {
            var rabbitMqFactory = new ConnectionFactory()
            {
                HostName = rabbitmqHost,
                UserName = rabbitmqUserName,
                Password = rabbitmqPassword
            };
            return rabbitMqFactory;
        });

        public static ConnectionFactory RabbitFactory
        {
            get { return factoryHolder.Value; }
        }
    }
}
