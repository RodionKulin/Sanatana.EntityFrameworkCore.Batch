using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Commands.Tests.Samples.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.EntityFrameworkCore.Commands.Tests.Samples
{
    public class SampleDbContext : DbContext
    {
        public const string SAMPLE_TABLE_SCHEMA = "test";
        public const string SAMPLE_TABLE_NAME = "Sample Entities";
        public const string SAMPLE_ID_COLUMN_NAME = "CustomIntColumn";
        public const string COMPLEX_TYPE_COLUMN_NAME = "BuildingAddress";

        public DbSet<SampleEntity> SampleEntities { get; set; }
        public DbSet<ParentEntity> ParentEntities { get; set; }
        public DbSet<GenericDerivedEntity> GenericDerivedEntities { get; set; }
        public DbSet<OneToManyEntity> OneToManyEntities { get; set; }
        public DbSet<ManyToOneEntity> ManyToOneEntities { get; set; }



        //init
        public SampleDbContext()
            : base()
        {
        }
        public SampleDbContext(DbContextOptions<SampleDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["SampleDbContext"].ConnectionString;
            
            optionsBuilder
                .UseSqlServer(connectionString, providerOptions => providerOptions.CommandTimeout(30))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GenericEntity<Int32>>().HasKey(x => x.EntityId);
            modelBuilder.Entity<GenericEntity<Int32>>().ToTable("GenericEntity");

            modelBuilder.Entity<GenericEntity<Guid>>().HasKey(x => x.EntityId);
            modelBuilder.Entity<GenericEntity<Guid>>().ToTable("GenericEntity2");

            modelBuilder.Entity<GenericDerivedEntity>();

            modelBuilder.Entity<SampleEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<SampleEntity>().Property(x => x.XmlProperty).HasColumnType("xml");
            modelBuilder.Entity<SampleEntity>().Property(x => x.DateProperty).HasColumnType("datetime2");
            modelBuilder.Entity<SampleEntity>().Property(x => x.IntProperty).HasColumnName(SAMPLE_ID_COLUMN_NAME);
            modelBuilder.Entity<SampleEntity>().ToTable(SAMPLE_TABLE_NAME, SAMPLE_TABLE_SCHEMA);
                       
            modelBuilder.Entity<ParentEntity>().OwnsOne(s => s.Embedded, b =>
            {
                b.Property(x => x.Address).HasColumnName(COMPLEX_TYPE_COLUMN_NAME);
            });

            modelBuilder.Entity<OneToManyEntity>();
            modelBuilder.Entity<ManyToOneEntity>()
                .HasOne(x => x.OneToMany)
                .WithMany(x => x.ManyToOnes)
                .HasForeignKey(x => x.OneToManyEntityId)
                .OnDelete(DeleteBehavior.Cascade);            
        }
    } 
}
