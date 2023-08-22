namespace WinService;

//This code tries to address the issue of loading offline data during startup,
//which can take a long time if the data is large, resulting in Windows Service Manager timing out.
//By using the BackgroundService class, we can load the offline data in a none blocking manner.
//This will allow the service to start and be available to the service manager.
//Once the service has started, we can then process the offline data in the background.
//I have also demonstrated the deep rooting of the cancellation token,
//which will allow us to gracefully shutdown the service and exit other long running operations.
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private Task? _noneBlockingTask;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _noneBlockingTask = NoneBlockingOperation(cancellationToken).ContinueWith(task =>
        {
            if (task.IsFaulted) return;
            
            //We should not continue if the task is faulted.
            //There could be configuration issues that need to be resolved.
            CallbackOperation(cancellationToken);
        }, cancellationToken);

        //Calling this will raise the ExecuteAsync method once the StartAsync method has completed.
        await base.StartAsync(cancellationToken);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //This will not be called until the StartAsync method calls the base method.
        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_noneBlockingTask != null)
        {
            //Checkpoint to make sure all processors are gracefully shutdown.
            await _noneBlockingTask;

            _noneBlockingTask?.Dispose();
        }
        
        await base.StopAsync(cancellationToken);
    }

    private async Task NoneBlockingOperation(CancellationToken cancellationToken)
    {
        //Code to process offline data
        await Task.Delay(8000, cancellationToken);
    }
    
    private void CallbackOperation(CancellationToken cancellationToken)
    {
        //All long running operations needs to be initialized within this block.
        if (!cancellationToken.IsCancellationRequested)
        {
            //Example of a long running operation would be our sql server service broker or
            //initializing a timer to run a task every 5 minutes.
        }
    }
}