namespace Barbato
{
    using System;
    using Model;
    using Nancy;
    using Nancy.Authentication.WorldDomination;
    using Nancy.Responses;
    using WorldDomination.Web.Authentication;

    public class AuthenticationProvider : IAuthenticationCallbackProvider
    {
        private readonly IGithubUserRepository githubUserRepository;

        public AuthenticationProvider(IGithubUserRepository githubUserRepository)
        {
            this.githubUserRepository = githubUserRepository;
        }

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

            if (!githubUserRepository.UserRegistered(model.AuthenticatedClient.AccessToken))
            {
                githubUserRepository.AddOAuthToken(model.AuthenticatedClient.AccessToken, model.AuthenticatedClient.UserInformation.Email, model.AuthenticatedClient.UserInformation.UserName);
            }

            var githubUser = model.AuthenticatedClient.UserInformation.UserName;
            return nancyModule.Response.AsRedirect("/repos/#" + githubUser, RedirectResponse.RedirectType.Temporary);


        }

        public dynamic OnRedirectToAuthenticationProviderError(NancyModule nancyModule, string errorMessage)
        {
            throw new NotImplementedException();
        }
    }
}