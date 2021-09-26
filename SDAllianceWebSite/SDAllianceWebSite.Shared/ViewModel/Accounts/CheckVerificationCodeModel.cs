using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Accounts
{
    public class CheckVerificationCodeModel
    {
        [Display(Name = "验证码")]
        public string Num { get; set; }
    }
}
