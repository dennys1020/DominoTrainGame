using System;
using System.Threading.Tasks;
using System.Windows.Input;
using log4net.Ext.Trace;

namespace DominoTrainGame.Client.ViewModel.Commands;

public sealed class AsyncParameterRelayCommand<TParameter> : ICommand
{
    private static readonly ITraceLog _logger;

    private readonly Func<TParameter, Task> _execute;
    private bool _isExecuting;

    static AsyncParameterRelayCommand()
    {
        _logger = TraceLogManager.GetLogger(typeof(AsyncParameterRelayCommand<TParameter>));
    }

    public AsyncParameterRelayCommand(Func<TParameter, Task> execute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        bool canExecute = !_isExecuting && parameter is TParameter;
        return canExecute;
    }

    public async void Execute(object parameter)
    {
        if (parameter is TParameter typedParameter)
        {
            _isExecuting = true;
            OnCanExecuteChanged();

            try
            {
                await _execute(typedParameter);
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
    }

    private void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
