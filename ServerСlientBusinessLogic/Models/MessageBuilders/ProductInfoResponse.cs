using System.Text.Json.Nodes;
using Newtonsoft.Json;
using TopNetwork.Core;
using TopNetwork.Services.MessageBuilder;
using Server.DataBase;

public class ProductInfoResponseData : IMsgSourceData
{
    public Product product = new Product();
    public string MessageType => MsgType;
    public static string MsgType => "ProductInfoResponse";
}

public class ProductInfoResponse : IMessageBuilder<ProductInfoResponseData>
{
    private ProductInfoResponseData _data = new();


    public ProductInfoResponse SetProduct(Product product)
    {
        _data.product = product;
        return this;
    }

    public Message BuildMsg()
    {
        return new Message()
        {
            MessageType = _data.MessageType,
            Payload = JsonConvert.SerializeObject(_data.product),
        };
    }

    public static ProductInfoResponseData Parse(Message msg)
    {
        if (msg.MessageType != ProductInfoResponseData.MsgType)
            throw new InvalidOperationException("Incorrect message type.");

        return new ProductInfoResponseData()
        {
            product = JsonConvert.DeserializeObject<Product>(msg.Payload)
        };
    }
}