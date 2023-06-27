using XGFramework.Services;

namespace Demo;

public class Agent : IDisposable
{
    private bool _isDispose;

    private readonly IAsyncDisposable              _asyncDisposable;
    private readonly Action<ushort, uint?, object> _sendToClient;
    
    public ActorId ActorId { get; set; }

    public Agent(IAsyncDisposable asyncDisposable, Action<ushort, uint?, object> sendToClient)
    {
        _asyncDisposable   = asyncDisposable;
        this._sendToClient = sendToClient;
    }

    public void Send<T>(T msg) where T : IMessage, IMessageMeta => Send(T.Opcode, msg);

    public void Send(ushort opcode, IMessage message) => _sendToClient?.Invoke(opcode, null, message);

    public void Reply(ushort opcode, uint rpcId, IResponse response) => _sendToClient?.Invoke(opcode, rpcId, response);

    public void Dispose()
    {
        if (_isDispose)
            return;
        _isDispose = true;
        _          = _asyncDisposable.DisposeAsync();
    }
}