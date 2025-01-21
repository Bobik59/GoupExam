using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TopNetwork.Core;
using TopNetwork.RequestResponse;
using TopNetwork.Services.MessageBuilder;

namespace Server.Models
{
    public class CalServer
    {
        // Регистрация всех фабрик для типов сообщений отправляемых сервером 
        private static readonly MessageBuilderService _msgService = new MessageBuilderService()
                    .Register(() => new ErroreMessageBuilder())
                    .Register(() => new EndSessionNotificationMessageBuilder())
                    .Register(() => new ServerOverloadedNotificationMessageBuilder());

        private readonly RrServerHandlerBase _handlers;
        private RrServer _server = new();


        public EndPoint? EndPoint => _server.CurrentEndPoint;
        public bool IsRunning => _server.IsRunning;
        public int CountOpenSessions => _server.CountOpenSessions;


        private async Task<ClientSession?> SessionFactory(TopClient client, ServiceRegistry context, LogString? logger)
        {
            //if (_server.CountOpenSessions >= MaxConnections)
            //{
            //    try
            //    {
            //        client.SendMessageAsync(_msgService.BuildMessage<ServerOverloadedNotificationMessageBuilder, ServerOverloadedNotificationData>(null)).Wait();
            //        logger?.Invoke($"[SessionFactory]: Отвергнуто подключение с [{client.RemoteEndPoint}], из-за перегрузки сервера...");
            //        return null;
            //    }
            //    catch (Exception ex)
            //    {
            //        logger?.Invoke($"[SessionFactory]: {ex.Message}.");
            //        return null;
            //    }
            //}

            ClientSession session = new(client, _handlers, context);

            return session;
        }

        public CalServer()
        {
            _server
                .RegisterService(_msgService);


            _handlers = new RrServerHandlerBase()
                .AddHandlerForMessageType(ProductInfoRequestData.MsgType, async (client, msg, context) =>
                {
                    return await SafeWrapperForHandler(client, msg, context, async (client, msg, context) =>
                    {
                        return _msgService.BuildMessage<ProductInfoResponse, ProductInfoResponseData>(
                        b => b.SetProduct(new DataBase.Product(){Name = "fff", CaloriesPer100g = 11, CarbsPer100g = 5m, Category ="fruct", FatPer100g = 9m, ProteinPer100g = 100m })
                        );
                    });
                });

            _server.SetSessionFactory(SessionFactory);
        }
        private async Task<Message> SafeWrapperForHandler(TopClient client, Message msg, ServiceRegistry context, Func<TopClient, Message, ServiceRegistry, Task<Message?>> handler)
        {
            try
            {
                return await handler?.Invoke(client, msg, context);
            }
            catch (Exception ex)
            {
                return _msgService.BuildMessage<ErroreMessageBuilder, ErroreData>(builder => builder
                    .SetPayload($"Невозможно обработать {msg.MessageType}.\n{ex.Message}")
                );
            }
        }

    }
}
