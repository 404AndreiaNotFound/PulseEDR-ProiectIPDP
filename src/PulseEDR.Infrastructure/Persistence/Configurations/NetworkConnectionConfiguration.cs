using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PulseEDR.Domain.Entities;

namespace PulseEDR.Infrastructure.Persistence.Configurations;

public class NetworkConnectionConfiguration : IEntityTypeConfiguration<NetworkConnection>
{
    public void Configure(EntityTypeBuilder<NetworkConnection> builder)
    {
        builder.ToTable("connections");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.LocalAddress).HasMaxLength(64).IsRequired();
        builder.Property(c => c.RemoteAddress).HasMaxLength(64).IsRequired();
        builder.Property(c => c.Protocol).HasMaxLength(8).IsRequired();
        builder.Property(c => c.State).HasMaxLength(32).IsRequired();

        builder.HasIndex(c => c.ScanResultId);
        builder.HasIndex(c => c.RemoteAddress);
    }
}