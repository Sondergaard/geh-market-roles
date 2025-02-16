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
using Energinet.DataHub.MarketRoles.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Energinet.DataHub.MarketRoles.Infrastructure.Outbox
{
    public class OutboxMessageEntityConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.ToTable("OutboxMessages", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Category)
                .HasColumnName("Category")
                .HasConversion(
                    toDbValue => toDbValue.Name,
                    fromDbValue => EnumerationType.FromName<OutboxMessageCategory>(fromDbValue));
            builder.Property(x => x.Data)
                .HasColumnName("Data");
            builder.Property(x => x.Type)
                .HasColumnName("Type");
            builder.Property(x => x.Correlation)
                .HasColumnName("Correlation");
            builder.Property(x => x.CreationDate)
                .HasColumnName("CreationDate");
            builder.Property(x => x.ProcessedDate)
                .HasColumnName("ProcessedDate");
        }
    }
}
