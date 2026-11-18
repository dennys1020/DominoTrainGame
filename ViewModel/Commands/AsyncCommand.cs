using System;
using System.Threading.Tasks;
using System.Windows.Input;
using log4net.Ext.Trace;

namespace DominoTrainGame.Client.ViewModel.Commands;

public abstract class AsyncCommand : ICommand
{
    private static readonly ITraceLog _logger;

    private bool _isExecuting;

    static AsyncCommand()
    {
        _logger = TraceLogManager.GetLogger(typeof(AsyncCommand));
    }

    public event EventHandler CanExecuteChanged;

    public virtual bool CanExecute(object parameter)
    {
        bool canExecute = !_isExecuting;
        return canExecute;
    }

    public abstract void Execute(object parameter);

    protected async Task RunAsync(Func<Task> operation)
    {
        if (operation == null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        _isExecuting = true;
        OnCanExecuteChanged();

        try
        {
            await operation();
        }
        catch (OperationCanceledException exception)
        {
            _logger.Debug("A command was canceled before it finished.", exception);
        }
        catch (Exception exception)
        {
            _logger.Error("A command failed with an unexpected error.", exception);
        }
        finally
        {
            _isExecuting = false;
            OnCanExecuteChanged();
        }
    }

    private void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
