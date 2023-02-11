using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DataAccessLayer.WMDbContext
{
    public partial class WMDbContext : DbContext
    {
        public WMDbContext()
        {
        }

        public WMDbContext(DbContextOptions<WMDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Admin> Admin { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseNpgsql("Host=46.101.146.148;Database=digit_case;Username=postgreroot;Password=Ali.1453.Arge");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("admin");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AccountConfirm)
                    .HasColumnName("account_confirm")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.ConfirmCode)
                    .HasColumnName("confirm_code")
                    .HasMaxLength(255);

                entity.Property(e => e.CreateAt)
                    .HasColumnName("create_at")
                    .HasColumnType("timestamp(0) without time zone");

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(255);

                entity.Property(e => e.IsOnline)
                    .HasColumnName("is_online")
                    .HasDefaultValueSql("false");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(255);

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasMaxLength(255);

                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");

                entity.Property(e => e.PasswordSalt).HasColumnName("password_salt");

                entity.Property(e => e.Surname)
                    .HasColumnName("surname")
                    .HasMaxLength(255);

                entity.Property(e => e.TmpPassword)
                    .HasColumnName("tmp_password")
                    .HasMaxLength(255);

                entity.Property(e => e.Token).HasColumnName("token");
            });
        }
    }
}
