namespace Sandra.Snow.Barbato
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Nancy;
    using Nancy.Authentication.WorldDomination;
    using RestSharp;
    using Model;
    using WorldDomination.Web.Authentication;

    public class AuthenticationProvider : IAuthenticationCallbackProvider
    {
        public dynamic Process(NancyModule nancyModule, AuthenticateCallbackData model)
        {
            if (model.AuthenticatedClient == null)
            {
                model.AuthenticatedClient = new AuthenticatedClient("github")
                    {
                        AccessToken = "123",
                        AccessTokenExpiresOn = DateTime.MinValue,
                        UserInformation =
                            new UserInformation()
                                {
                                    Email = "jonathan.channon@gmail.com",
                                    Gender = GenderType.Unknown,
                                    Id = "123",
                                    Locale = "en-GB",
                                    Name = "Jonathan Channon",
                                    Picture =
                                        "https://secure.gravatar.com/avatar/62e4df82d52221751142c68ee5d2ae0b?d=https://a248.e.akamai.net/assets.github.com%2Fimages%2Fgravatars%2Fgravatar-user-420.png",
                                    UserName = "jchannon"
                                }
                    };
            }
            var githubUser = model.AuthenticatedClient.UserInformation.UserName;

            var client = new RestClient("https://api.github.com");

            var request = new RestRequest("users/" + githubUser + "/repos");
            request.AddHeader("Accept", "application/json");

            var response = client.Execute<List<GithubUserRepos.RootObject>>(request);

            var repoDetail =
                response.Data
                .Where(x => x.fork == false)
                .Select(
                    x =>
                    new RepoDetail
                        {
                            Name = x.name,
                            AvatarUrl = x.owner.avatar_url,
                            Description = x.description,
                            HtmlUrl = x.html_url,
                            UpdatedAt = x.updated_at,
                            CloneUrl = x.clone_url
                        });

            var viewModel = new RepoModel() { Username = model.AuthenticatedClient.UserInformation.UserName };
            viewModel.Repos = repoDetail;

            return nancyModule.Negotiate.WithView("AuthenticateCallback").WithModel(viewModel);
        }

        public dynamic OnRedirectToAuthenticationProviderError(NancyModule nancyModule, string errorMessage)
        {
            throw new NotImplementedException();
        }
    }
}