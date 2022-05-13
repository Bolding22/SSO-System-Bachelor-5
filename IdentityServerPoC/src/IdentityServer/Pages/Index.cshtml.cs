using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;

namespace IdentityServerAspNetIdentity.Pages.Home;

[AllowAnonymous]
public class Index : PageModel
{
    private readonly IDistributedCache _distributedCache;
    public string Version;

    public Index(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public void OnGet()
    {
        _distributedCache.Set("CONTEST", new byte[]{1});

        Version = typeof(Duende.IdentityServer.Hosting.IdentityServerMiddleware).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+').First();
    }
}