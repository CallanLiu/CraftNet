namespace CraftNet.Services;

public interface IActor : IDisposable
{
    public ActorId ActorId { get; }

    public object Target { get; set; }

    public void Post(ActorMessage actorMessage);

    public void UnsafePost(ActorMessage actorMessage);
}