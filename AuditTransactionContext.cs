using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Newtonsoft.Json;
using TMS.DataContracts;
using TMS.DataContracts.HistoricalModels;
using Version = TMS.DataContracts.HistoricalModels.Version;

namespace TMS.DataStores
{

    public class Repository
    {
        private readonly AuditTransactionContext _context;

        public Repository(AuditTransactionContext context)
        {
            _context = context;
        }

        public bool InsertFact<T>(T fact)
        {

        }


        private static string CalculateSignature(MessageContent content)
        {
            var json =System.Text.Json.JsonSerializer.Serialize(content);
            using var sha = SHA512.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
            var stringBuilder = new StringBuilder();

            foreach (var b in bytes)
            {
                stringBuilder.Append(b.ToString("x2"));
            }

            return stringBuilder.ToString();
        }
    }

    public class AuditTransactionContextFactory : IDesignTimeDbContextFactory<AuditTransactionContext>
    {
        public AuditTransactionContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AuditTransactionContext>();
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=TMS");

            return new AuditTransactionContext(optionsBuilder.Options);
        }
    }

    public class AuditTransactionContext : DbContext
    {
        public DbSet<Fact> Facts { get; set; }
        public DbSet<FactType> FactTypes { get; set; }
        public DbSet<Version> Versions { get; set; }
        public DbSet<Edge> Edges { get; set; }
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Creates a new instance using the provided options.
        /// </summary>
        /// <param name="options"></param>
        public AuditTransactionContext(DbContextOptions<AuditTransactionContext> options)
            : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           // optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.ConfigureWarnings(warnings =>
            {
                warnings.Default(WarningBehavior.Throw);
            });
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Facts
            modelBuilder.Entity<Fact>().ToTable("Facts");
            modelBuilder.Entity<Fact>().HasKey(a => a.Id);
            modelBuilder.Entity<Fact>().Property(a => a.Id).UseIdentityColumn();
            modelBuilder.Entity<Fact>().Property(a => a.Fields)
                .HasColumnType("jsonb")
                .IsRequired();
            modelBuilder.Entity<Fact>().Property(a => a.Hash).HasMaxLength(88).IsRequired();
            modelBuilder.Entity<Fact>().HasOne<Version>().WithMany().HasForeignKey(a=>a.VersionId);

            modelBuilder.Entity<Fact>().HasIndex(a => new {a.VersionId, a.Hash}).IsUnique();
            modelBuilder.Entity<Fact>().HasIndex(a => a.Hash).IsUnique();

            // Version
            modelBuilder.Entity<Version>().ToTable("Versions");
            modelBuilder.Entity<Version>().HasKey(a => a.Id);
            modelBuilder.Entity<Version>().Property(a => a.Hash).HasMaxLength(88).IsRequired();
            modelBuilder.Entity<Version>().HasIndex(a => a.Hash).IsUnique();

            // Type
            modelBuilder.Entity<FactType>().ToTable("Types");
            modelBuilder.Entity<FactType>().HasKey(a => a.Id);
            modelBuilder.Entity<FactType>().Property(a => a.Name).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<FactType>().HasIndex(a => a.Name).IsUnique();

            // Role
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<Role>().HasKey(a => a.Id);
            modelBuilder.Entity<Role>().Property(a => a.DeclaringTypeId).IsRequired();
            modelBuilder.Entity<Role>().Property(a => a.TargetTypeId).IsRequired();
            modelBuilder.Entity<Role>().Property(a => a.Name)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<Role>().HasIndex(a => new { a.DeclaringTypeId, a.Name }).IsUnique();

            modelBuilder.Entity<Role>().HasOne<FactType>().WithMany().HasForeignKey(a => a.DeclaringTypeId).HasConstraintName("DeclaringType");
            modelBuilder.Entity<Role>().HasOne<FactType>().WithMany().HasForeignKey(a => a.TargetTypeId).HasConstraintName("TargetType");

            // Edge
            modelBuilder.Entity<Edge>().ToTable("Edges");
            modelBuilder.Entity<Edge>().HasKey(a =>new { a.PredecessorId, a.SuccessorId,a.RoleId});
            modelBuilder.Entity<Edge>().HasIndex(a => new {a.PredecessorId, a.RoleId})
                .HasDatabaseName("Edges_Predecessor");



        }
    }
}
