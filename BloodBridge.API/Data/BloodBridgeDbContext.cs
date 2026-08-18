using BloodBridge.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Data;

public class BloodBridgeDbContext : DbContext
{
    public BloodBridgeDbContext(DbContextOptions<BloodBridgeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Donor> Donors => Set<Donor>();
    public DbSet<Hospital> Hospitals => Set<Hospital>();
    public DbSet<BloodRequest> BloodRequests => Set<BloodRequest>();
    public DbSet<DonorMatch> DonorMatches => Set<DonorMatch>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<Notification> Notifications => Set<Notification>();

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

        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.Donor)
            .WithMany(donor => donor.Notifications)
            .HasForeignKey(notification => notification.DonorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(notification => notification.BloodRequest)
            .WithMany(request => request.Notifications)
            .HasForeignKey(notification => notification.BloodRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BloodRequest>()
            .HasOne(request => request.Hospital)
            .WithMany(hospital => hospital.BloodRequests)
            .HasForeignKey(request => request.HospitalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Donor>().HasData(
            new Donor
            {
                Id = 1,
                Name = "Harigovind",
                BloodGroup = "A+",
                Phone = "9876543210",
                Location = "Bhopal",
                IsAvailable = true,
                LastDonationDate = new DateTime(2025, 10, 15)
            },
            new Donor
            {
                Id = 2,
                Name = "Akshay",
                BloodGroup = "B+",
                Phone = "9876543211",
                Location = "Bhopal",
                IsAvailable = true,
                LastDonationDate = new DateTime(2025, 11, 20)
            },
            new Donor
            {
                Id = 3,
                Name = "Adithyan",
                BloodGroup = "B+",
                Phone = "9876543212",
                Location = "Bhopal",
                IsAvailable = false,
                LastDonationDate = new DateTime(2025, 12, 5)
            });

        modelBuilder.Entity<Hospital>().HasData(
            new Hospital
            {
                Id = 1,
                Name = "City Care Hospital",
                Address = "MP Nagar, Bhopal",
                Phone = "0755-4001000"
            },
            new Hospital
            {
                Id = 2,
                Name = "Bhopal Medical Center",
                Address = "Arera Colony, Bhopal",
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
    }
}
