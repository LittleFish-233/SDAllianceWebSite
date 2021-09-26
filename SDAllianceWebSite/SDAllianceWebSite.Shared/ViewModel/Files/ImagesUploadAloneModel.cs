using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.ViewModel.Files
{
    public class ImagesUploadAloneModel
    {
        public string Image { get; set; }

        public string ImagePath { get; set; }
    }

    public class EditImageAloneModel
    {
        [Display(Name = "分类")]
        public string Modifier { get; set; }
        [Display(Name = "备注")]
        public string Note { get; set; }
        [Display(Name = "链接")]
        [Required(ErrorMessage = "请输入链接")]
        public string Url { get; set; }
    }
}
