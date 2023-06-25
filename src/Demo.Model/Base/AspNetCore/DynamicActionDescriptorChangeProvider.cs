using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace Demo;

public class DynamicActionDescriptorChangeProvider : IActionDescriptorChangeProvider
{
    public CancellationTokenSource TokenSource { get; private set; }

    public IChangeToken GetChangeToken()
    {
        TokenSource = new CancellationTokenSource();
        return new CancellationChangeToken(TokenSource.Token);
    }

    public void NotifyChange()
    {
        TokenSource?.Cancel();
    }
}