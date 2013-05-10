namespace Sandra.Snow.Barbato
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Nancy;
    using Nancy.Authentication.WorldDomination;
    using Nancy.Responses;
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
            return nancyModule.Response.AsRedirect("/repos/"+githubUser, RedirectResponse.RedirectType.Temporary);

          
        }

        public dynamic OnRedirectToAuthenticationProviderError(NancyModule nancyModule, string errorMessage)
        {
            throw new NotImplementedException();
        }
    }
}