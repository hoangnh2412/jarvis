// using System.ComponentModel.DataAnnotations;

// namespace Jarvis.Core.Constants
// {
//     public enum ClaimOfChildResource
//     {
//         [Display(Name = "Không sử dụng")]
//         None = 0,
        
//         //Ko sử dụng phân quyền dữ liệu => Xem dữ liệu của cả công ty
//         //Query theo field TeantCode
//         [Display(Name = "Chi nhánh")]
//         Tenant = 4,

//         //Xem được dữ liệu trong phòng ban hiện tại
//         //Query theo Organization
//         [Display(Name = "Quản lý 1 cấp")]
//         ManageGroup = 2,

//         //Xem dc dữ liệu các phòng ban hiện tại và các phòng ban cấp dưới
//         //Query theo nhiều Organization
//         [Display(Name = "Quản lý nhiều cấp")]
//         ManageMultiGroup = 3,

//         //Xem được dữ liệu do mình tạo ra
//         //Query theo field CreatedBy và TenantCode
//         [Display(Name = "Cá nhân")]
//         Owner = 1,
//     }
// }
