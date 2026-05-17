using {Product}.Domain.Repositories;
using Jarvis.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;

namespace {Product}.Infrastructure.Persistence;

public sealed class AppUnitOfWork(
  IServiceProvider services,
  IDbContextFactory<AppDbContext> factory)
  : BaseUnitOfWork<AppDbContext>(services, factory), IAppUnitOfWork;
