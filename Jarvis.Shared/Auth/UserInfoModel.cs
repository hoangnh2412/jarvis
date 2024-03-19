namespace Jarvis.Shared.Auth
{
    public class UserInfoModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{this.FirstName} {this.LastName}";
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool EmailVerified { get; set; }
        public bool PhoneNumberVerified { get; set; }
        public string Organization { get; set; }
        public string AccessToken { get; set; }
        public List<UserCognitoOrgRoleDto> OrgRoleList { get; set; }
        public List<string> SkinEngines { get; set; }

        public class UserCognitoOrgRoleDto
        {
            public string Org { get; set; }
            public List<string> App { get; set; }
            public List<string> ModuleAccess { get; set; }
            public string Role { get; set; }
            public List<UserCognitoSiteDto> Site { get; set; }
        }

        public class UserCognitoSiteDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}