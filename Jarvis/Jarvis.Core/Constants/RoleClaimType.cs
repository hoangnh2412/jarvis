using System.ComponentModel.DataAnnotations;

namespace Jarvis.Core.Constants
{
    public enum RoleClaimType
    {
        [Display(Name = "Quyền chức năng")]
        Function = 1,

        [Display(Name = "Quyền dữ liệu")]
        Data = 2
    }
}