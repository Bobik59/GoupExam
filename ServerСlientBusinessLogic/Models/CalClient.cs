using Server.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopNetwork.Services.MessageBuilder;
using WPFTasks.Core.Models.Core;

namespace GoupExam.Models
{
    public class CalClient : BaseClient
    {
        public event Action<ProductInfoResponseData>? OnProductInfoResponse;

        protected override void RegisterMessageBuilders() // регистрация отправляемых сообщений
        {
            MessageBuilderService
                .Register(() => new ProductInfoRequest());
        }
        //чтобы отправить запрос нужен метод который у него отправляет ход здесь написать setInfoReq зачем?

        protected override void RegisterMessageHandlers() // регистрация обработчиков принимаемых сообщени
        {
            Handlers.AddHandlerForMessageType(ProductInfoResponseData.MsgType, async msg =>
            {
                try { OnProductInfoResponse?.Invoke(ProductInfoResponse.Parse(msg)); }
                catch { InvokeOnErroreOnClient($"Error parsing response: {msg}"); }
                return null;
            });
            
        }

        public async Task SendFindProductInfo(Product product)
        {
            await SendMessageAsync<ProductInfoResponse, ProductInfoResponseData>(
                builder => builder.SetProduct(product)
            );
        }
    }
}
