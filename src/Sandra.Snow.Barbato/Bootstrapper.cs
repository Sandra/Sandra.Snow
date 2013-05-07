using WorldDomination.Web.Authentication;

namespace Sandra.Snow.Barbato
{
    using Nancy;
    using Nancy.TinyIoc;
    using WorldDomination.Web.Authentication.ExtraProviders;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private const string GithubConsumerKey = "5ad3b62391672a6cc068";
        private const string GithubConsumerSecret = "75810e6eeb242bb3cfa26c1d10b194fba9dc1075";

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var githubProvider =
                new GitHubProvider(new ProviderParams() {Key = GithubConsumerKey, Secret = GithubConsumerSecret});

            var authenticationService = new AuthenticationService();

            authenticationService.AddProvider(githubProvider);

            container.Register<IAuthenticationService>(authenticationService);

        }
    }
}