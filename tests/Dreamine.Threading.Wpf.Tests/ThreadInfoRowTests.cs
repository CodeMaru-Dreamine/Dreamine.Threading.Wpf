using Dreamine.Threading.Models;
using Dreamine.Threading.Wpf.ViewModels;

namespace Dreamine.Threading.Wpf.Tests;

public sealed class ThreadInfoRowTests
{
    [Fact]
    public void ConstructorCopiesSnapshot()
    {
        var started = DateTimeOffset.UtcNow;
        var row = new ThreadInfoRow(CreateInfo(
            DreamineThreadStatus.Running,
            DreamineThreadPriority.High,
            startedAt: started));

        Assert.Equal("worker", row.Name);
        Assert.Equal(DreamineThreadStatus.Running, row.Status);
        Assert.Equal(DreamineThreadPriority.High, row.Priority);
        Assert.Equal(100, row.IntervalMs);
        Assert.Equal(2, row.CoreIndex);
        Assert.True(row.UseAffinity);
        Assert.Equal(3, row.JobCount);
        Assert.Equal(10, row.CycleCount);
        Assert.Equal(started, row.StartedAt);
        Assert.True(row.IsRunning);
        Assert.False(row.IsPaused);
        Assert.False(row.IsFaulted);
    }

    [Fact]
    public void UpdateFromRaisesOnlyChangedProperties()
    {
        var row = new ThreadInfoRow(CreateInfo(DreamineThreadStatus.Running));
        var changed = new List<string?>();
        row.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        var result = row.UpdateFrom(CreateInfo(
            DreamineThreadStatus.Faulted,
            cycleCount: 11,
            lastError: "boom"));

        Assert.True(result);
        Assert.Contains(nameof(ThreadInfoRow.Status), changed);
        Assert.Contains(nameof(ThreadInfoRow.CycleCount), changed);
        Assert.Contains(nameof(ThreadInfoRow.LastErrorMessage), changed);
        Assert.Contains(nameof(ThreadInfoRow.IsRunning), changed);
        Assert.Contains(nameof(ThreadInfoRow.IsFaulted), changed);
        Assert.True(row.IsFaulted);
        Assert.Equal("boom", row.LastErrorMessage);
    }

    [Fact]
    public void UpdateFromWithSameSnapshotDoesNotRaiseChanges()
    {
        var info = CreateInfo(DreamineThreadStatus.Paused);
        var row = new ThreadInfoRow(info);
        var raised = false;
        row.PropertyChanged += (_, _) => raised = true;

        Assert.False(row.UpdateFrom(info));
        Assert.False(raised);
        Assert.True(row.IsPaused);
    }

    private static DreamineThreadInfo CreateInfo(
        DreamineThreadStatus status,
        DreamineThreadPriority priority = DreamineThreadPriority.Normal,
        long cycleCount = 10,
        DateTimeOffset? startedAt = null,
        string? lastError = null) =>
        new(
            "worker",
            status,
            priority,
            100,
            2,
            true,
            3,
            cycleCount,
            startedAt,
            null,
            lastError);
}
