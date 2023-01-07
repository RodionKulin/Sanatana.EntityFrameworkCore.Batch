using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.EntityFrameworkCore.Batch.PostgreSql.DbContextExtentions;
using Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples.Entities.Constraints;

namespace Sanatana.EntityFrameworkCore.Batch.PostgreSqlSpecs.Samples
{
    public class SampleDbContext : DbContext
    {
        public const string SAMPLE_TABLE_SCHEMA = "test";
        public const string SAMPLE_TABLE_NAME = "Sample Entities";
        public const string SAMPLE_ID_COLUMN_NAME = "CustomIntColumn";
        public const string COMPLEX_TYPE_COLUMN_NAME = "BuildingAddress";
        public const string RENAMED_DB_GENERATED_COLUMN_NAME = "IWasRenamed";

        public DbSet<SampleEntity> SampleEntities { get; set; }
        public DbSet<ParentEntity> ParentEntities { get; set; }
        public DbSet<GenericDerivedEntity> GenericDerivedEntities { get; set; }
        public DbSet<OneToManyEntity> OneToManyEntities { get; set; }
        public DbSet<ManyToOneEntity> ManyToOneEntities { get; set; }
        public DbSet<UniqueConstrantEntity> UniqueConstrantEntities { get; set; }


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
            IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string connectionString = config.GetConnectionString("DefaultConnection");

            optionsBuilder
                .UseNpgsql(connectionString, opt => opt.CommandTimeout(30))
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
            modelBuilder.Entity<SampleEntity>().Property(x => x.DateProperty).HasColumnType("TimestampTz");
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

            modelBuilder.Entity<CustomKeyName>()
                .HasKey(x => x.CustomKey);
            modelBuilder.Entity<CompoundKeyEntity>()
                .HasKey(x => new { x.CompoundKeyNumber, x.CompoundKeyString });

            modelBuilder.Entity<AttributedIdDbGenerated>();
            modelBuilder.Entity<CompoundKeyDbGenerated>()
                .HasKey(x => new { x.CompoundNumberGenerated, x.CompoundStringGenerated });
            modelBuilder.Entity<ConventionKeyDbGenerated>();
            modelBuilder.Entity<RenamedColumnDbGenerated>().HasKey(x => x.CustomId);
            modelBuilder.Entity<RenamedColumnDbGenerated>().Property(x => x.HelloIAmAProp).HasColumnName(RENAMED_DB_GENERATED_COLUMN_NAME);

            modelBuilder.Entity<WithSomePropsUnmapped>().Ignore(x => x.NotMappedProp2);

            modelBuilder.Entity<UniqueConstrantEntity>().HasIndex(x => x.Name).IsUnique();

            //convert DateTime to Utc before insert to database.
            //should go in the end of OnModelCreating method
            modelBuilder.ApplyUtcDateTimeConverter();
        }
    } 
}
