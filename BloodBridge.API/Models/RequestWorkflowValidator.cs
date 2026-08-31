namespace BloodBridge.API.Models;

public static class RequestWorkflowValidator
{
    public static void EnsureValidTransition(string currentStatus, string targetStatus)
    {
        if (!BloodRequestStatuses.TryNormalize(currentStatus, out var current)
            || !BloodRequestStatuses.TryNormalize(targetStatus, out var target))
        {
            throw new InvalidOperationException("The current or target request status is invalid.");
        }

        var expected = BloodRequestStatuses.GetNext(current);
        if (!string.Equals(expected, target, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Invalid status transition. The next status after {current} is {expected ?? "none"}.");
        }
    }
}
