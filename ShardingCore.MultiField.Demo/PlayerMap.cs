using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShardingCore.MultiField.Demo;

public class PlayerMap : IEntityTypeConfiguration<Player>
{

    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.AppCode).HasMaxLength(64).HasComment("应用代码");
        builder.Property(x => x.GroupCode).HasMaxLength(64).HasComment("组代码");
        builder.Property(x => x.Email).HasMaxLength(64).HasComment("邮箱");
        builder.Property(x => x.Name).HasMaxLength(64).HasComment("名称");
        builder.Ignore(x => x.SplitTableKey);
        builder.ToTable(nameof(Player));
    }
}