using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Infrastructure
{
    /// <summary>
    /// Mẫu khởi tạo module
    /// </summary>
    public interface IModuleInitializer
    {
        int Piority { get; }

        /// <summary>
        /// Cấu hình khởi tạo, đăng ký service lúc runtime
        /// </summary>
        /// <param name="services"></param>
        void ConfigureServices(IServiceCollection services);

        /// <summary>
        /// Cấu hình builder lúc runtime
        /// </summary>
        /// <param name="app"></param>
        void Configure(IApplicationBuilder app);
    }
}
