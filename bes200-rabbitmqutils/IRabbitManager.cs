using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMqUtils
{
    public interface IRabbitManager
    {
        void Publish<T>(T message, string exchangeName, string exchangeType, string routeKey)
       where T : class;
    }
}
