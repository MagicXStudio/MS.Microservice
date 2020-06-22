﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MS.Microservice.Core.Data;
using MS.Microservice.Database.EntityConfigurations;
using MS.Microservice.Domain;
using System.Threading;
using System.Threading.Tasks;

// refer to https://github.com/dotnet-architecture/eShopOnContainers/blob/dev/src/Services/Ordering/Ordering.Infrastructure
namespace MS.Microservice.Database
{
    public class OrderingContext : DbContext, IUnitOfWork
    {
        public const string DEFAULT_SCHEMA = "ordering";
        public DbSet<Order> Orders { get; set; }

        private readonly IMediator _mediator;
        private IDbContextTransaction _currentTransaction;
        public OrderingContext(DbContextOptions<OrderingContext> options) : base(options)
        {

        }

        public OrderingContext(DbContextOptions<OrderingContext> options, IMediator mediator)
        {
            Check.NotNull(options, nameof(options));
            Check.NotNull(mediator, nameof(mediator));

            _mediator = mediator;

            // 分析
            System.Diagnostics.Debug.WriteLine("OrderingContext::ctor ->" + this.GetHashCode());
        }

        public IDbContextTransaction GetCurrentTransaction => _currentTransaction;
        public bool HasActiveTransaction => _currentTransaction != null;


        // 配置表结构
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OrderingEntityConfiguration());
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            // 调度领域事件
            await _mediator.DispatchDomainEventsAsync(this, cancellationToken);

            var result = await base.SaveChangesAsync(cancellationToken);

            return result > 0;
        }
    }
}