using System;
using System.ComponentModel.DataAnnotations;

namespace Jarvis.Core.Models.Organizations
{
    public class CreateOrganizationUnitRequestModel
    {
        [Required(ErrorMessage="Mã đơn vị không được để trống")]
        [MaxLength(50, ErrorMessage = "Mã đơn vị không thể quá 50 ký tự")]
        public string Name { get; set; }
        
        [Required(ErrorMessage="Tên đơn vị không được để trống")]
        [MaxLength(50, ErrorMessage = "Tên đơn vị không thể quá 250 ký tự")]
        public string FullName { get; set; }

        public string Description { get; set; }
        
        public Guid? IdParent { get; set; }
    }
}