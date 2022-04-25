using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServerAspNetIdentity.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Moq;
using NUnit.Framework;

namespace Test;

public class Tests
{
    private RedirectUriValidator _redirectUriValidator = null!;

    [SetUp]
    public void Setup()
    {
        _redirectUriValidator = new RedirectUriValidator(null);
    }

    [Test]
    public async Task Test1()
    {
        Assert.True(await _redirectUriValidator.IsRedirectUriValidAsync("https://test.ajoursystem.net/signin-oidc", null),
            "This must be supported");
        Assert.True(await _redirectUriValidator.IsRedirectUriValidAsync("https://ajoursystem.net/signin-oidc", null),
            "This must also be supported");
        Assert.False(await _redirectUriValidator.IsRedirectUriValidAsync("http://test.ajoursystem.net/signin-oidc", null), 
            "Must use https");
        Assert.False(await _redirectUriValidator.IsRedirectUriValidAsync("https://test/.ajoursystem.net/signin-oidc", null), 
            "Cannot contain special characters in subdomain");
        Assert.False(await _redirectUriValidator.IsRedirectUriValidAsync("https://test#.ajoursystem.net/signin-oidc", null), 
            "Cannot contain special characters in subdomain");
        Assert.False(await _redirectUriValidator.IsRedirectUriValidAsync("https://test12.ajoursystem.net/signin-oidc", null), 
            "Cannot contain numbers in subdomain");
        Assert.False(await _redirectUriValidator.IsRedirectUriValidAsync("https://test.ajour.net/signin-oidc", null), 
            "The domain must be ajoursystem");
    }
}