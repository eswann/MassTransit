
namespace Burrows.BackedPublisher
{
    public interface IPublisher
    {
        void Publish<T>(T message, bool force = false);
    }
}