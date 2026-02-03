using Microsoft.EntityFrameworkCore;

namespace InitWorker.Entity;

public partial class SampleContext : DbContext
{
    public SampleContext(DbContextOptions<SampleContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
