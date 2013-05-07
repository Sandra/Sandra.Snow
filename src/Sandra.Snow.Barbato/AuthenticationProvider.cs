using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.Authentication.WorldDomination;
using WorldDomination.Web.Authentication;

namespace Sandra.Snow.Barbato
{
    public class AuthenticationProvider : IAuthenticationCallbackProvider
    {
        public dynamic Process(NancyModule nancyModule, AuthenticateCallbackData model)
        {
            return nancyModule.Negotiate.WithView("AuthenticateCallback").WithModel(model);
        }

        public dynamic OnRedirectToAuthenticationProviderError(NancyModule nancyModule, string errorMessage)
        {
            throw new NotImplementedException();
        }
    }
}