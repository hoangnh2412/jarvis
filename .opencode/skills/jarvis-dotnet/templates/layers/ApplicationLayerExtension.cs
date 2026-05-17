using Jarvis.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace {Product}.Application.DependencyInjection;

public static class ApplicationLayerExtension
{
  public static IHostApplicationBuilder AddApplicationLayer(this IHostApplicationBuilder builder)
  {
    builder.AddCoreApplication();
    // Đăng ký ICommandHandler<>, IQueryHandler<> của product tại đây
    return builder;
  }
}
