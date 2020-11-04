using Infrastructure.Abstractions;
using Jarvis.Core.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Jarvis.Core.Permissions
{
    [Display(Name = "Quản lý hệ thống", Order = 999)]
    public class CorePolicy
    {
        [Display(Name = "Quản lý người dùng")]
        public class UserPolicy : IPolicy
        {
            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> User_Read = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Danh sách người dùng",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> User_Create = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Tạo người dùng",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> User_Update = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Sửa người dùng",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> User_Delete = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Xóa người dùng",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> User_Lock = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Khóa người dùng",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> User_Reset_Password = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Đổi mật khẩu tài khoản",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });
        }

        [Display(Name = "Quản lý quyền")]
        public class RolePolicy : IPolicy
        {
            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Role_Read = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Danh sách quyền",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Role_Create = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Tạo quyền",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Role_Update = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Sửa quyền",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Role_Delete = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Xóa quyền",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });
        }

        [Display(Name = "Quản lý site")]
        public class TenantPolicy : IPolicy
        {
            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Tenant_Read = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Danh sách chi nhánh",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Tenant_Create = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Tạo chi nhánh",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Tenant_Update = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Sửa chi nhánh",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Tenant_Delete = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Xóa chi nhánh",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });
        }

        [Display(Name = "Quản lý nhãn")]
        public class LabelPolicy : IPolicy
        {
            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Label_Read = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Danh sách nhãn",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Label_Create = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Tạo nhãn",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Label_Update = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Sửa nhãn",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Label_Delete = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Xóa nhãn",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });
        }

        [Display(Name = "Quản lý tham số")]
        public class SettingPolicy : IPolicy
        {
            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Setting_Read = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Danh sách tham số",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Setting_Update = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Sửa tham số",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });
        }

        [Display(Name = "Quản lý phòng ban")]
        public class OrganizationPolicy : IPolicy
        {
            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Organization_Read = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Danh sách phòng ban",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Organization_Create = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Tạo phòng ban",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Organization_Update = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Sửa phòng ban",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });

            public static Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>> Organization_Delete = new Tuple<string, List<ClaimOfResource>, List<ClaimOfChildResource>>(
                "Xóa phòng ban",
                new List<ClaimOfResource> {
                    ClaimOfResource.Tenant,
                    ClaimOfResource.Owner
                },
                new List<ClaimOfChildResource> {
                    ClaimOfChildResource.None,
                    ClaimOfChildResource.Tenant,
                    ClaimOfChildResource.Owner
                });
        }
    }
}
