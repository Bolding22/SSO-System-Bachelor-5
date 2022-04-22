using System.Text.RegularExpressions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;

namespace IdentityServerAspNetIdentity.Services;

public class RedirectUriValidator : StrictRedirectUriValidator
{
    private readonly IHostEnvironment _hostEnvironment;

    public RedirectUriValidator(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }
    public override Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
    {
        // Only letter are allowed in subdomain
        var pattern = _hostEnvironment.IsDevelopment()
            ? "^(https:\\/\\/)[a-zA-Z]+(\\.localhost:5002/signin-oidc)$"
            : "^(https:\\/\\/)[a-zA-Z]+(\\.ajoursystem\\.net\\/signin-oidc)$";
        
        
        var regex = new Regex(pattern);
        
        return Task.FromResult(regex.IsMatch(requestedUri));
    }
    
}