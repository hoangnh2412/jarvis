using System.Collections.Generic;
using Infrastructure.Extensions;
using Jarvis.Core.Abstractions;
using Jarvis.Core.Constants;
using Jarvis.Core.Database.Poco;

namespace Jarvis.Core
{
    public class JarvisDefaultData : IDefaultData
    {
        public List<Setting> GetSettings()
        {
            var settings = new List<Setting>();

            settings.AddRange(TenantSettings);
            settings.AddRange(SmtpSettings);

            return settings;
        }

        private List<Setting> TenantSettings = new List<Setting>
        {

        };

        private static List<Setting> SmtpSettings = new List<Setting> {
            new Setting {
                Group = SettingGroupKey.Smtp.ToString(),
                Code = SettingKey.Smtp_From.ToString(),
                Name = EnumExtension.GetName(SettingKey.Smtp_From),
                Type = SettingType.Text.GetHashCode(),
                Value = "#",
            },
            new Setting {
                Group = SettingGroupKey.Smtp.ToString(),
                Code = SettingKey.Smtp_FromName.ToString(),
                Name = EnumExtension.GetName(SettingKey.Smtp_FromName),
                Type = SettingType.Text.GetHashCode(),
                Value = "#",
            },
            new Setting {
                Group = SettingGroupKey.Smtp.ToString(),
                Code = SettingKey.Smtp_Authentication.ToString(),
                Name = EnumExtension.GetName(SettingKey.Smtp_Authentication),
                Type = SettingType.Combobox.GetHashCode(),
                Options = "1:Yes|0:No",
                Value = "1",
            },
            new Setting {
                Group = SettingGroupKey.Smtp.ToString(),
                Code = SettingKey.Smtp_Host.ToString(),
                Name = EnumExtension.GetName(SettingKey.Smtp_Host),
                Type = SettingType.Text.GetHashCode(),
                Value = "#",
            },
            new Setting {
                Group = SettingGroupKey.Smtp.ToString(),
                Code = SettingKey.Smtp_Port.ToString(),
                Name = EnumExtension.GetName(SettingKey.Smtp_Port),
                Type = SettingType.Text.GetHashCode(),
                Value = "#",
                Description = "TLS/STARTTLS = 587, SSL = 465"
            },
            new Setting {
                Group = SettingGroupKey.Smtp.ToString(),
                Code = SettingKey.Smtp_UserName.ToString(),
                Name = EnumExtension.GetName(SettingKey.Smtp_UserName),
                Type = SettingType.Text.GetHashCode(),
                Value = "#",
            },
            new Setting {
                Group = SettingGroupKey.Smtp.ToString(),
                Code = SettingKey.Smtp_Password.ToString(),
                Name = EnumExtension.GetName(SettingKey.Smtp_Password),
                Type = SettingType.Password.GetHashCode(),
                Value = "#",
            },
            new Setting {
                Group = SettingGroupKey.Smtp.ToString(),
                Code = SettingKey.Smtp_Socket.ToString(),
                Name = EnumExtension.GetName(SettingKey.Smtp_Socket),
                Type = SettingType.Combobox.GetHashCode(),
                Options = "0:None|1:Auto|2:SslOnConnect|3:StartTls|4:StartTlsWhenAvailable",
                Value = "1"
            },
        };
    }
}
