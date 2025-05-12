using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend
{
    public class MemoDbContext: DbContext
{
   public DbSet<Planner> Planners { get; set; }
    public DbSet<WeddingStory> Weddings { get; set; }
    public DbSet<Media> Media { get; set; }
    public DbSet<WQRCode> QRCodes { get; set; }
    public DbSet<GuestMessage> GuestMessages { get; set; }
    public DbSet<Proposal> Proposals { get; set; }
        public DbSet<HowWeMet> HowWeMetStories { get; set; }
        public DbSet<HowWeMetMedia> HowWeMetMedias { get; set; } // Add this line to include HowWeMetMedia in the context
        public DbSet<ProposalMedia> ProposalMedias { get; set; } // Add this line to include ProposalMedia in the context
        public DbSet<User> Users { get; set; }
        public DbSet<PlannerProfile> PlannerProfiles { get; set; }
        public DbSet<OurJourney> OurJourneys { get; set; }


        public MemoDbContext(DbContextOptions<MemoDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Planner>().HasKey(p => p.Id);
        modelBuilder.Entity<WeddingStory>().HasKey(w => w.Id);
        modelBuilder.Entity<Media>().HasKey(m => m.Id);
        modelBuilder.Entity<WQRCode>().HasKey(q => q.Id);
        modelBuilder.Entity<GuestMessage>().HasKey(q => q.Id);
            modelBuilder.Entity<Proposal>().HasKey(q => q.Id);
            modelBuilder.Entity<HowWeMet>().HasKey(q => q.Id);
            modelBuilder.Entity<HowWeMetMedia>().HasKey(q => q.Id);
            modelBuilder.Entity<ProposalMedia>().HasKey(q => q.Id);
            modelBuilder.Entity<User>().HasKey(q => q.Id);
            modelBuilder.Entity<PlannerProfile>().HasKey(q => q.Id);



            modelBuilder.Entity<Media>()
            .HasOne(m => m.Wedding)
            .WithMany(w => w.Gallery)
            .HasForeignKey(m => m.WeddingId);

        modelBuilder.Entity<WQRCode>()
            .HasOne(q => q.Wedding)
            .WithOne(w => w.QRCode)
            .HasForeignKey<WQRCode>(q => q.WeddingId);
        modelBuilder.Entity<GuestMessage>()
            .HasOne(q => q.Wedding)
            .WithMany(w => w.GuestMessages)
            .HasForeignKey(q => q.WeddingId);

            modelBuilder.Entity<Proposal>().HasOne(q => q.WeddingStory).WithOne(w => w.Proposals).HasForeignKey<Proposal>(q => q.WeddingStoryId);
            modelBuilder.Entity<HowWeMet>().HasOne(q => q.WeddingStory).WithOne(w => w.HowWeMetStories).HasForeignKey<HowWeMet>(q => q.WeddingStoryId);
            modelBuilder.Entity<HowWeMetMedia>().HasOne(q => q.HowWeMet).WithMany(w => w.Media).HasForeignKey(q => q.HowWeMetId);
            modelBuilder.Entity<ProposalMedia>().HasOne(q => q.Proposal).WithMany(w => w.Media).HasForeignKey(q => q.ProposalId);
            modelBuilder.Entity<Planner>().HasMany(w => w.Weddings).WithOne(q => q.Planner).HasForeignKey(q => q.PlannerId);
            modelBuilder.Entity<PlannerProfile>().HasOne(x=>x.User).WithMany(q=>q.Planners).HasForeignKey(q => q.UserId);
            modelBuilder.Entity<PlannerProfile>().HasOne(x=>x.Planner).WithMany(x=>x.PlannerProfiles).HasForeignKey(q => q.PlannerId);
            modelBuilder.Entity<OurJourney>().HasOne(x => x.WeddingStory).WithMany(x => x.OurJourneys).HasForeignKey(x => x.WeddingId);




        }
}
}