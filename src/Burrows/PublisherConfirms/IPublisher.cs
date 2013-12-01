
namespace Burrows.PublisherConfirms
{
    public interface IPublisher
    {
        void Publish<T>(T message, bool force = false);
    }
}