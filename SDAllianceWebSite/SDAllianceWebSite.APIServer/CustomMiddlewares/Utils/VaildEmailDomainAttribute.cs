﻿using System.ComponentModel.DataAnnotations;

namespace SDAllianceWebSite.APIServer.CustomMiddlewares.Utils
{
    public class VaildEmailDomainAttribute : ValidationAttribute
    {
        private readonly string allowedDomain;

        public VaildEmailDomainAttribute(string allowedDomain)
        {
            this.allowedDomain = allowedDomain;
        }

        public override bool IsValid(object value)
        {
            string[] strings = value.ToString().Split('@');
            return strings[1].ToUpper() == allowedDomain.ToUpper();
        }
    }
}
