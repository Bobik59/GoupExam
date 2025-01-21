using TopNetwork.Core;
using TopNetwork.Services.MessageBuilder;

public class ProductInfoRequestData : IMsgSourceData
{
    public string ProductName { get; set; }
    public string MessageType => MsgType;
    public static string MsgType => "ProductInfoRequest";
}

public class ProductInfoRequest : IMessageBuilder<ProductInfoRequestData>
{
    private ProductInfoRequestData _data = new();

    public Message BuildMsg()
    {
        return new Message()
        {
            MessageType = _data.MessageType,
        };
    }

    public static ProductInfoRequestData Parse(Message msg)
    {
        if (msg.MessageType != ProductInfoRequestData.MsgType)
            throw new InvalidOperationException("Incorrect message type.");

        return new ProductInfoRequestData()
        {
            // Add properties initialization if needed
        };
    }
}