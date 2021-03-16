using Microsoft.EntityFrameworkCore;
using P03_FootballBetting.Data.Models;

namespace P03_FootballBetting.Data
{
    public class FootballBettingContext : DbContext
    {
        public FootballBettingContext()
        {
        }

        public FootballBettingContext(DbContextOptions<FootballBettingContext> options) :
            base(options)
        {
        }

        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<Color> Colors { get; set; }
        public virtual DbSet<Town> Towns { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Player> Players { get; set; }
        public virtual DbSet<Position> Positions { get; set; }
        public virtual DbSet<PlayerStatistic> PlayerStatistics { get; set; }
        public virtual DbSet<Game> Games { get; set; }
        public virtual DbSet<Bet> Bets { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=T450\SQLEXPRESS;Database=FootballBetting;Integrated Security=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlayerStatistic>().HasKey(x => new { x.GameId, x.PlayerId });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasOne(x => x.PrimaryKitColor)
                .WithMany(x => x.PrimaryKitTeams)
                .HasForeignKey(x => x.PrimaryKitColorId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.SecondaryKitColor)
                .WithMany(x => x.SecondaryKitTeams)
                .HasForeignKey(x => x.SecondaryKitColorId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasOne(x => x.HomeTeam)
                    .WithMany(x => x.HomeGames)
                    .HasForeignKey(x => x.HomeTeamId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.AwayTeam)
                  .WithMany(x => x.AwayGames)
                  .HasForeignKey(x => x.AwayTeamId)
                  .OnDelete(DeleteBehavior.Restrict);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}