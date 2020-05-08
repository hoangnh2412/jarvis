using Jarvis.Core.Database.Poco;

namespace Jarvis.Models.Identity.Models.Identity
{
    public class UserInfoModel
    {
        public string FullName { get; set; }
        public string AvatarPath { get; set; }

        public static implicit operator UserInfoModel(UserInfo entity)
        {
            if (entity == null)
                return null;

            var model = new UserInfoModel();
            model.FullName = entity.FullName;
            model.AvatarPath = entity.AvatarPath;
            return model;
        }
    }
}
