using System.ComponentModel.DataAnnotations;

namespace Jarvis.Core.Models.Install
{
    public class InstallTenantModel
    {
        [Required(ErrorMessage = "Tên miền không được để trống")]
        [MaxLength(250, ErrorMessage = "Tên miền tối đa 250 ký tự")]
        public string HostName { get; set; }

        [Required(ErrorMessage = "Mã số thuế không được để trống")]
        [MaxLength(50, ErrorMessage = "Mã số thuế tối đa 50 ký tự")]
        public string TaxCode { get; set; }

        [Required(ErrorMessage = "Tên doanh nghiệp không được để trống")]
        [MaxLength(500, ErrorMessage = "Tên doanh nghiệp tối đa 500 ký tự")]
        public string FullNameVi { get; set; }

        public string FullNameEn { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string District { get; set; }
    }
}
