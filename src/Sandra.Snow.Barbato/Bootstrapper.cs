using WorldDomination.Web.Authentication;

namespace Sandra.Snow.Barbato
{
    using Nancy;
    using Nancy.TinyIoc;
    using WorldDomination.Web.Authentication.ExtraProviders;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private const string GithubConsumerKey = "*key*";
        private const string GithubConsumerSecret = "*secret*";


        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var githubProvider =
                new GitHubProvider(new ProviderParams() {Key = GithubConsumerKey, Secret = GithubConsumerSecret});
        }
    }
}