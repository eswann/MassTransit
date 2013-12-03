using System;

namespace Burrows.PublisherConfirms
{
    [Serializable]
    public class ConfirmableMessage
    {
        public ConfirmableMessage()
        {
        }

        public ConfirmableMessage(object message, Type type)
        {
            Id = Guid.NewGuid().ToString("N");
            Message = message;
            Type = type;
        }

        public string Id { get; set; }

        public object Message { get; set; }

        public Type Type { get; set; }
    }
}