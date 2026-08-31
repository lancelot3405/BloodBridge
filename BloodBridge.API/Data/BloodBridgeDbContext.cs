using BloodBridge.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Text.Json;

namespace BloodBridge.API.Data;

public class BloodBridgeDbContext : IdentityDbContext<ApplicationUser>
{
    public BloodBridgeDbContext(DbContextOptions<BloodBridgeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Donor> Donors => Set<Donor>();
    public DbSet<Hospital> Hospitals => Set<Hospital>();
    public DbSet<Requester> Requesters => Set<Requester>();
    public DbSet<BloodRequest> BloodRequests => Set<BloodRequest>();
    public DbSet<DonorMatch> DonorMatches => Set<DonorMatch>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<GamificationProfile> GamificationProfiles => Set<GamificationProfile>();
    public DbSet<GamificationActivity> GamificationActivities => Set<GamificationActivity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DonorMatch>()
            .HasOne(match => match.Donor)
            .WithMany(donor => donor.DonorMatches)
            .HasForeignKey(match => match.DonorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DonorMatch>()
            .HasOne(match => match.BloodRequest)
            .WithMany(request => request.DonorMatches)
            .HasForeignKey(match => match.BloodRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DonorMatch>()
            .Property(match => match.MatchScore)
            .HasPrecision(5, 2);

        modelBuilder.Entity<Donation>()
            .HasOne(donation => donation.Donor)
            .WithMany(donor => donor.Donations)
            .HasForeignKey(donation => donation.DonorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Donation>()
            .HasOne(donation => donation.BloodRequest)
            .WithMany(request => request.Donations)
            .HasForeignKey(donation => donation.BloodRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Donation>()
            .HasOne(donation => donation.Hospital)
            .WithMany(hospital => hospital.Donations)
            .HasForeignKey(donation => donation.HospitalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Donor>()
            .HasOne(donor => donor.User)
            .WithOne(user => user.Donor)
            .HasForeignKey<Donor>(donor => donor.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Hospital>()
            .HasOne(hospital => hospital.User)
            .WithOne(user => user.Hospital)
            .HasForeignKey<Hospital>(hospital => hospital.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Requester>()
            .HasOne(requester => requester.User)
            .WithOne(user => user.Requester)
            .HasForeignKey<Requester>(requester => requester.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.User)
            .WithMany()
            .HasForeignKey(notification => notification.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Donor>().Property(donor => donor.UserId).HasMaxLength(450);
        modelBuilder.Entity<Hospital>().Property(hospital => hospital.UserId).HasMaxLength(450);
        modelBuilder.Entity<Requester>().Property(requester => requester.UserId).HasMaxLength(450);

        modelBuilder.Entity<BloodRequest>()
            .HasOne(request => request.Hospital)
            .WithMany(hospital => hospital.BloodRequests)
            .HasForeignKey(request => request.HospitalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BloodRequest>()
            .HasOne(request => request.AcceptedDonor)
            .WithMany()
            .HasForeignKey(request => request.AcceptedDonorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Donation>()
            .HasIndex(donation => new { donation.BloodRequestId, donation.DonorId })
            .IsUnique();

        modelBuilder.Entity<GamificationProfile>()
            .HasOne(profile => profile.Donor)
            .WithOne(donor => donor.GamificationProfile)
            .HasForeignKey<GamificationProfile>(profile => profile.DonorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GamificationProfile>()
            .HasIndex(profile => profile.DonorId)
            .IsUnique();

        var badgesComparer = new ValueComparer<List<string>>(
            (left, right) => left != null && right != null && left.SequenceEqual(right),
            value => value.Aggregate(0, (hash, badge) => HashCode.Combine(hash, badge.GetHashCode(StringComparison.Ordinal))),
            value => value.ToList());

        modelBuilder.Entity<GamificationProfile>()
            .Property(profile => profile.BadgesEarned)
            .HasConversion(
                badges => JsonSerializer.Serialize(badges, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>())
            .Metadata.SetValueComparer(badgesComparer);

        modelBuilder.Entity<GamificationProfile>()
            .Property(profile => profile.ImpactScore)
            .HasDefaultValue(0);

        modelBuilder.Entity<GamificationProfile>()
            .Property(profile => profile.TierRank)
            .HasDefaultValue(GamificationRules.NewDonorTier);

        modelBuilder.Entity<GamificationProfile>()
            .Property(profile => profile.ProfileCompletedXPGranted)
            .HasDefaultValue(false);

        modelBuilder.Entity<GamificationActivity>()
            .HasOne(activity => activity.Donor)
            .WithMany()
            .HasForeignKey(activity => activity.DonorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GamificationActivity>()
            .HasIndex(activity => new { activity.DonorId, activity.ActivityKey })
            .IsUnique();

        modelBuilder.Entity<Donor>().HasData(
            new Donor
            {
                Id = 1,
                UserId = SeedUserIds.Donor1,
                Name = "Harigovind",
                BloodGroup = "A+",
                Phone = "9876543210",
                Location = "23.2599,77.4126",
                IsAvailable = true,
                LastDonationDate = new DateTime(2025, 10, 15)
            },
            new Donor
            {
                Id = 2,
                UserId = SeedUserIds.Donor2,
                Name = "Akshay",
                BloodGroup = "B+",
                Phone = "9876543211",
                Location = "23.2337,77.4340",
                IsAvailable = true,
                LastDonationDate = new DateTime(2025, 11, 20)
            },
            new Donor
            {
                Id = 3,
                UserId = SeedUserIds.Donor3,
                Name = "Adithyan",
                BloodGroup = "B+",
                Phone = "9876543212",
                Location = "23.2156,77.4321",
                IsAvailable = false,
                LastDonationDate = new DateTime(2025, 12, 5)
            });

        modelBuilder.Entity<Hospital>().HasData(
            new Hospital
            {
                Id = 1,
                UserId = SeedUserIds.Hospital1,
                Name = "City Care Hospital",
                Location = "23.2599,77.4126",
                Phone = "0755-4001000"
            },
            new Hospital
            {
                Id = 2,
                UserId = SeedUserIds.Hospital2,
                Name = "Bhopal Medical Center",
                Location = "23.1990,77.3770",
                Phone = "0755-4002000"
            });

        modelBuilder.Entity<BloodRequest>().HasData(
            new BloodRequest
            {
                Id = 1,
                HospitalId = 1,
                BloodGroup = "B+",
                UnitsRequired = 2,
                Urgency = "High",
                RequiredDate = new DateTime(2026, 8, 20),
                Status = "Pending",
                CreatedAt = new DateTime(2026, 8, 18, 8, 0, 0, DateTimeKind.Utc)
            },
            new BloodRequest
            {
                Id = 2,
                HospitalId = 2,
                BloodGroup = "A+",
                UnitsRequired = 1,
                Urgency = "Normal",
                RequiredDate = new DateTime(2026, 8, 25),
                Status = "Pending",
                CreatedAt = new DateTime(2026, 8, 18, 9, 0, 0, DateTimeKind.Utc)
            });

        modelBuilder.Entity<ApplicationUser>().HasData(
            SeedUser(SeedUserIds.Donor1, "seed-donor-1@bloodbridge.local"),
            SeedUser(SeedUserIds.Donor2, "seed-donor-2@bloodbridge.local"),
            SeedUser(SeedUserIds.Donor3, "seed-donor-3@bloodbridge.local"),
            SeedUser(SeedUserIds.Hospital1, "seed-hospital-1@bloodbridge.local"),
            SeedUser(SeedUserIds.Hospital2, "seed-hospital-2@bloodbridge.local"));
    }

    private static ApplicationUser SeedUser(string id, string email) => new()
    {
        Id = id,
        UserName = email,
        NormalizedUserName = email.ToUpperInvariant(),
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        EmailConfirmed = true,
        SecurityStamp = id,
        ConcurrencyStamp = id
    };
}

internal static class SeedUserIds
{
    public const string Donor1 = "00000000-0000-0000-0000-000000000001";
    public const string Donor2 = "00000000-0000-0000-0000-000000000002";
    public const string Donor3 = "00000000-0000-0000-0000-000000000003";
    public const string Hospital1 = "00000000-0000-0000-0000-000000000011";
    public const string Hospital2 = "00000000-0000-0000-0000-000000000012";
}
