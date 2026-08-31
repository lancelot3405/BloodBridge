using BloodBridge.API.Data;
using BloodBridge.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BloodBridge.API.Services;

public sealed class NotificationService
{
    private readonly BloodBridgeDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;

    public NotificationService(
        BloodBridgeDbContext context,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        ISmsSender smsSender)
    {
        _context = context;
        _userManager = userManager;
        _emailSender = emailSender;
        _smsSender = smsSender;
    }

    public async Task NotifyDonorAsync(
        Donor donor,
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(donor.UserId))
        {
            return;
        }

        var recentDuplicate = await _context.Notifications.AnyAsync(
            notification => notification.UserId == donor.UserId
                && notification.Title == title
                && notification.Message == message
                && notification.CreatedAt >= DateTime.UtcNow.AddMinutes(-5),
            cancellationToken);
        if (recentDuplicate)
        {
            return;
        }

        _context.Notifications.Add(new Notification
        {
            UserId = donor.UserId,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        var user = await _userManager.FindByIdAsync(donor.UserId);
        await _emailSender.SendAsync(user?.Email ?? donor.UserId, title, message, cancellationToken);
        await _smsSender.SendAsync(donor.Phone, message, cancellationToken);
    }

    public Task<List<Notification>> GetUnreadAsync(string userId, CancellationToken cancellationToken = default) =>
        _context.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId && !notification.IsRead)
            .OrderByDescending(notification => notification.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<bool> MarkReadAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);
        if (notification is null)
        {
            return false;
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
