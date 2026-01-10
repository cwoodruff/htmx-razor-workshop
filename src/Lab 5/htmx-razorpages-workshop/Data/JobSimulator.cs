namespace RazorPagesHtmxWorkshop.Data;

/// <summary>
/// Simulates a long-running background job for demonstration.
/// In production, you'd use a proper job queue (Hangfire, etc.).
/// </summary>
public static class JobSimulator
{
    private static readonly Dictionary<string, JobStatus> _jobs = new();
    private static readonly object _lock = new();

    public class JobStatus
    {
        public string JobId { get; init; } = "";
        public string State { get; set; } = "pending"; // pending, running, completed, failed
        public int Progress { get; set; } = 0;
        public string? Result { get; set; }
        public string? Error { get; set; }
        public DateTime StartedAt { get; init; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// Starts a new simulated job.
    /// </summary>
    public static JobStatus StartJob()
    {
        var jobId = Guid.NewGuid().ToString("N")[..8];
        var status = new JobStatus
        {
            JobId = jobId,
            State = "running",
            Progress = 0
        };

        lock (_lock)
        {
            _jobs[jobId] = status;
        }

        // Simulate progress in background
        _ = Task.Run(async () =>
        {
            try
            {
                for (var i = 1; i <= 10; i++)
                {
                    await Task.Delay(500); // Simulate work
                    
                    lock (_lock)
                    {
                        if (_jobs.TryGetValue(jobId, out var job))
                        {
                            job.Progress = i * 10;
                        }
                    }
                }

                lock (_lock)
                {
                    if (_jobs.TryGetValue(jobId, out var job))
                    {
                        job.State = "completed";
                        job.Progress = 100;
                        job.Result = $"Report generated successfully at {DateTime.Now:HH:mm:ss}";
                        job.CompletedAt = DateTime.UtcNow;
                    }
                }
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    if (_jobs.TryGetValue(jobId, out var job))
                    {
                        job.State = "failed";
                        job.Error = ex.Message;
                        job.CompletedAt = DateTime.UtcNow;
                    }
                }
            }
        });

        return status;
    }

    /// <summary>
    /// Gets the current status of a job.
    /// </summary>
    public static JobStatus? GetStatus(string jobId)
    {
        lock (_lock)
        {
            return _jobs.TryGetValue(jobId, out var status) ? status : null;
        }
    }

    /// <summary>
    /// Cleans up old jobs (call periodically in production).
    /// </summary>
    public static void Cleanup(TimeSpan maxAge)
    {
        var cutoff = DateTime.UtcNow - maxAge;
        lock (_lock)
        {
            var oldJobs = _jobs
                .Where(kv => kv.Value.StartedAt < cutoff)
                .Select(kv => kv.Key)
                .ToList();
            
            foreach (var id in oldJobs)
            {
                _jobs.Remove(id);
            }
        }
    }
}
