﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.MarketData.Domain.BusinessProcesses;
using Energinet.DataHub.MarketData.Domain.Consumers;
using Energinet.DataHub.MarketData.Domain.EnergySuppliers;
using Energinet.DataHub.MarketData.Domain.MeteringPoints;
using Energinet.DataHub.MarketData.Domain.SeedWork;
using Energinet.DataHub.MarketData.Infrastructure.DatabaseAccess.Write.EnergySuppliers;
using Energinet.DataHub.MarketData.Infrastructure.DatabaseAccess.Write.MeteringPoints;
using Energinet.DataHub.MarketData.Infrastructure.DatabaseAccess.Write.ProcessManagers;
using Energinet.DataHub.MarketData.Infrastructure.InternalCommand;
using Energinet.DataHub.MarketData.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.MarketData.Infrastructure.DatabaseAccess
{
    /// <summary>
    /// EF Core Base Database Context
    /// </summary>
    public interface IBaseDatabaseContext : IDisposable
    {
        /// <summary>
        /// Entity Framework DbSet for Consumers
        /// </summary>
        DbSet<Consumer> Consumers { get; set; }

        /// <summary>
        /// Entity Framework DbSet for Metering Points
        /// </summary>
        DbSet<AccountingPoint> AccountingPoints { get; set; }

        /// <summary>
        /// Entity Framework DbSet for Energy Suppliers
        /// </summary>
        DbSet<EnergySupplier> EnergySuppliers { get; set; }
        // /// <summary>
        // /// Entity Framework DbSet for Internal Commands
        // /// </summary>
        // DbSet<InternalCommandDataModel> InternalCommandDataModels { get; set; }
        //
        // /// <summary>
        // /// Entity Framework DbSet for Outgoing Actor Messages
        // /// </summary>
        // DbSet<OutgoingActorMessageDataModel> OutgoingActorMessageDataModels { get; set; }

        // /// <summary>
        // /// Entity Framework DbSet for Process Managers
        // /// </summary>
        // DbSet<ProcessManagerDataModel> ProcessManagerDataModels { get; set; }

        /// <summary>
        /// Provides access to database related information and operations for this context.
        /// See Entity Framework Core documentation for further information
        /// </summary>
        DatabaseFacade Database { get; }

        /// <summary>
        ///     Saves all changes made in this context to the database.
        ///     See Entity Framework Core documentation for further information
        /// </summary>
        /// <returns> The number of state entries written to the database. </returns>
        int SaveChanges();

        /// <summary>
        ///     Saves all changes made in this context to the database.
        ///     See Entity Framework Core documentation for further information
        /// </summary>
        /// <returns> The number of state entries written to the database. </returns>
        int SaveChanges(bool acceptAllChangesOnSuccess);

        /// <summary>
        ///     Saves all changes made in this context to the database.
        ///     See Entity Framework Core documentation for further information
        /// </summary>
        /// <returns> The number of state entries written to the database. </returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Saves all changes made in this context to the database.
        ///     See Entity Framework Core documentation for further information
        /// </summary>
        /// <returns> The number of state entries written to the database. </returns>
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Creates a <see cref="DbSet{TEntity}" /> that can be used to query and save instances of <typeparamref name="TEntity" />.
        /// See Entity Framework Core documentation for further information
        /// </summary>
        /// <typeparam name="TEntity"> The type of entity for which a set should be returned. </typeparam>
        /// <returns> A set for the given entity type. </returns>
        DbSet<TEntity> Set<TEntity>()
            where TEntity : class;

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        string ToString();
    }

    public class BaseDatabaseContext : DbContext, IBaseDatabaseContext
    {
        public BaseDatabaseContext() { }

        public BaseDatabaseContext(DbContextOptions<BaseDatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<Consumer> Consumers { get; set; } = null!;

        public DbSet<EnergySupplier> EnergySuppliers { get; set; } = null!;

        public DbSet<AccountingPoint> AccountingPoints { get; set; } = null!;
        //public DbSet<InternalCommandDataModel> InternalCommandDataModels { get; set; } = null!;
        //public DbSet<OutgoingActorMessageDataModel> OutgoingActorMessageDataModels { get; set; } = null!;

        //public DbSet<ProcessManagerDataModel> ProcessManagerDataModels { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ConsumerConfiguration());
            modelBuilder.ApplyConfiguration(new EnergySupplierConfiguration());
            modelBuilder.ApplyConfiguration(new AccountingPointConfiguration());
            // modelBuilder.ApplyConfiguration(new InternalCommandQueueDataModelConfiguration());
            // modelBuilder.ApplyConfiguration(new OutgoingActorMessageDataModelConfiguration());
            // modelBuilder.ApplyConfiguration(new ProcessManagerDataModelConfiguration());
            // modelBuilder.ApplyConfiguration(new RelationshipModelConfiguration());
        }

        public class ConsumerConfiguration : IEntityTypeConfiguration<Consumer>
        {
            public void Configure(EntityTypeBuilder<Consumer> builder)
            {
                builder.ToTable("Consumers", "dbo");
                builder.Property<int>("RecordId");
                builder.HasKey("RecordId");
                builder.OwnsOne(x => x.ConsumerId, sp =>
                {
                    sp.Property(x => x.Value)
                        .HasColumnName("RecordId")
                        .HasColumnType("int")
                        .ValueGeneratedOnAdd().UseIdentityColumn();
                });

                builder.Property(x => x.CprNumber)
                    .HasColumnName("CprNumber")
                    .HasConversion(value => value!.Value, value => value == null ? null : new CprNumber(value));
                builder.Property(x => x.CvrNumber)
                    .HasColumnName("CvrNumber")
                    .HasConversion(value => value!.Value, value => value == null ? null : new CvrNumber(value));

                builder.Ignore(x => x.Version);
                builder.Ignore(x => x.DomainEvents);
            }
        }

        public class AccountingPointConfiguration : IEntityTypeConfiguration<AccountingPoint>
        {
            public void Configure(EntityTypeBuilder<AccountingPoint> builder)
            {
                builder.ToTable("AccountingPoints", "dbo");
                builder.Property<int>("RecordId");
                builder.HasKey("RecordId");

                builder.OwnsOne(x => x.AccountingPointId, sp =>
                {
                    sp.Property(x => x.Value)
                        .HasColumnName("RecordId")
                        .HasColumnType("int")
                        .ValueGeneratedOnAdd().UseIdentityColumn();
                });
                builder.Property(x => x.GsrnNumber)
                    .HasColumnName(@"GsrnNumber")
                    .HasColumnType("nvarchar(36)")
                    .HasMaxLength(36)
                    .IsRequired()
                    .HasConversion(value => value.Value, value => GsrnNumber.Create(value));

                builder.Property<bool>("_isProductionObligated")
                    .HasColumnName(@"ProductionObligated")
                    .HasColumnType("bit")
                    .IsRequired();

                builder.Property<PhysicalState>("_physicalState")
                    .HasColumnName(@"PhysicalState")
                    .HasColumnType("int")
                    .IsRequired()
                    .HasConversion(value => value.Id, value => EnumerationType.FromValue<PhysicalState>(value));

                builder.Property<MeteringPointType>("_meteringPointType")
                    .HasColumnName(@"Type")
                    .HasColumnType("int")
                    .IsRequired()
                    .HasConversion(value => value.Id, value => EnumerationType.FromValue<MeteringPointType>(value));

                builder.Property<int>(x => x.Version)
                    .HasColumnName(@"RowVersion")
                    .HasColumnType("int")
                    .IsRequired()
                    .IsConcurrencyToken();

                builder.OwnsMany<Domain.MeteringPoints.BusinessProcess>("_businessProcesses", sp =>
                {
                    sp.ToTable("BusinessProcesses", "dbo");
                    sp.Property<int>("RecordId").ValueGeneratedOnAdd().UseIdentityColumn();
                    sp.HasKey("RecordId");
                    sp.WithOwner()
                        .HasForeignKey("AccountingPointId");
                    sp.Property(x => x.Status)
                        .HasColumnName("Status")
                        .HasConversion(value => value.Id, v => EnumerationType.FromValue<BusinessProcessStatus>(v));
                    sp.Property(x => x.ProcessId)
                        .HasColumnName("ProcessId")
                        .HasConversion<string>(value => value.Value, v => new ProcessId(v));
                    sp.Property(x => x.ProcessType)
                        .HasColumnName("ProcessType")
                        .HasConversion(value => value.Id, value => EnumerationType.FromValue<BusinessProcessType>(value));
                    sp.Property(x => x.EffectiveDate)
                        .HasColumnName("EffectiveDate")
                        .HasColumnType("datetime2")
                        .IsRequired();
                });
            }
        }

        public class EnergySupplierConfiguration : IEntityTypeConfiguration<EnergySupplier>
        {
            public void Configure(EntityTypeBuilder<EnergySupplier> builder)
            {
                builder.ToTable("EnergySuppliers", "dbo");
                builder.Property<int>("RecordId");
                builder.HasKey("RecordId");
                builder.OwnsOne(x => x.EnergySupplierId, sp =>
                {
                    sp.Property(x => x.Value)
                        .HasColumnName("RecordId")
                        .HasColumnType("int")
                        .ValueGeneratedOnAdd().UseIdentityColumn();
                });

                builder.Property(x => x.GlnNumber)
                    .HasColumnName("GlnNumber")
                    .HasConversion(value => value.Value, value => new GlnNumber(value));

                builder.Ignore(x => x.Version);
                builder.Ignore(x => x.DomainEvents);
            }
        }

        public class InternalCommandQueueDataModelConfiguration : IEntityTypeConfiguration<InternalCommandDataModel>
        {
            public void Configure(EntityTypeBuilder<InternalCommandDataModel> builder)
            {
                builder.ToTable("InternalCommandQueue", "dbo");
                builder.HasKey(x => x.Id).HasName("InternalCommandQueue_pk").IsClustered();

                builder.Property(x => x.Id).HasColumnName(@"Id").HasColumnType("uniqueidentifier").IsRequired();
                builder.Property(x => x.Data).HasColumnName(@"Data").HasColumnType("text").IsRequired();
                builder.Property(x => x.Type).HasColumnName(@"Type").HasColumnType("text").IsRequired();
                builder.Property(x => x.ScheduledDate).HasColumnName(@"ScheduledDate").HasColumnType("datetime2");
                builder.Property(x => x.ProcessedDate).HasColumnName(@"ProcessedDate").HasColumnType("datetime2");

                builder.HasIndex(x => x.Id).HasName("UC_InternalCommandQueue_Id").IsUnique();
            }
        }

        public class OutgoingActorMessageDataModelConfiguration : IEntityTypeConfiguration<OutgoingActorMessageDataModel>
        {
            public void Configure(EntityTypeBuilder<OutgoingActorMessageDataModel> builder)
            {
                builder.ToTable("OutgoingActorMessages", "dbo");

                builder.Property(x => x.Id).HasColumnName(@"Id").HasColumnType("uniqueidentifier").IsRequired();
                builder.Property(x => x.Data).HasColumnName(@"Data").HasColumnType("text").IsRequired();
                builder.Property(x => x.Type).HasColumnName(@"Type").HasColumnType("nchar(50)").HasMaxLength(50).IsRequired();
                builder.Property(x => x.OccurredOn).HasColumnName(@"OccurredOn").HasColumnType("datetime2");
                builder.Property(x => x.LastUpdatedOn).HasColumnName(@"LastUpdatedOn").HasColumnType("datetime2");
                builder.Property(x => x.State).HasColumnName(@"State").HasColumnType("int").IsRequired();
                builder.Property(x => x.Recipient).HasColumnName(@"Recipient").HasColumnType("nvarchar(50)").HasMaxLength(50).IsRequired();

                builder.HasIndex(x => x.Id).HasName("UC_OutgoingActorMessages_Id").IsUnique();
            }
        }

        public class ProcessManagerDataModelConfiguration : IEntityTypeConfiguration<ProcessManagerDataModel>
        {
            public void Configure(EntityTypeBuilder<ProcessManagerDataModel> builder)
            {
                builder.ToTable("Relationships", "dbo");

                builder.Property(x => x.Id).HasColumnName(@"Id").HasColumnType("uniqueidentifier").IsRequired();
                builder.Property(x => x.ProcessId).HasColumnName(@"ProcessId").HasColumnType("nvarchar(36)").IsRequired();
                builder.Property(x => x.State).HasColumnName(@"State").HasColumnType("int").IsRequired();
                builder.Property(x => x.EffectiveDate).HasColumnName(@"EffectiveDate").HasColumnType("datetime2").IsRequired();
                builder.Property(x => x.Type).HasColumnName(@"Type").HasColumnType("nvarchar(200)").IsRequired();

                builder.HasIndex(x => x.Id).HasName("UC_ProcessManagers_Id").IsUnique();
            }
        }

        // public class RelationshipDataModelConfiguration : IEntityTypeConfiguration<RelationshipDataModel>
        // {
        //     public void Configure(EntityTypeBuilder<RelationshipDataModel> builder)
        //     {
        //         builder.ToTable("Relationships", "dbo");
        //
        //         builder.Property(x => x.Id).HasColumnName(@"Id").HasColumnType("uniqueidentifier").IsRequired();
        //         builder.Property(x => x.Type).HasColumnName(@"Type").HasColumnType("int").IsRequired();
        //         builder.Property(x => x.State).HasColumnName(@"State").HasColumnType("int").IsRequired();
        //         builder.Property(x => x.MarketParticipantId).HasColumnName(@"MarketParticipant_Id").HasColumnType("uniqueidentifier").IsRequired();
        //         builder.Property(x => x.MarketEvaluationPointId).HasColumnName(@"MarketEvaluationPoint_Id").HasColumnType("uniqueidentifier").IsRequired();
        //         builder.Property(x => x.EffectuationDate).HasColumnName(@"EffectuationDate").HasColumnType("datetime2").IsRequired();
        //
        //         builder.HasOne(x => x.EnergySupplierDataModel).WithMany(x => x!.RelationshipDataModels).HasForeignKey(x => x.MarketParticipantId)
        //             .HasConstraintName("Fk_Relationship_MarketParticipant");
        //         builder.HasOne(x => x.MarketEvaluationPointDataModel).WithMany(x => x!.BusinessProcesses).HasForeignKey(x => x.MarketEvaluationPointId)
        //             .HasConstraintName("Fk_Relationship_MarketEvaluationPoint");
        //
        //         builder.HasIndex(x => x.Id).HasName("UC_Relationships_Id").IsUnique();
        //     }
        // }
    }
}
