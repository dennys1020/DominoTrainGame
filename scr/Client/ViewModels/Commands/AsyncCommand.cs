using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DominoTrainGame.ViewModels;

public sealed class AsyncCommand : ICommand
{
    private readonly Func<Task> _execute;
    private bool _isExecuting;

    public AsyncCommand(Func<Task> execute) { _execute = execute; }

    public bool CanExecute(object parameter) => !_isExecuting;
    public event EventHandler CanExecuteChanged;

    public async void Execute(object parameter) => await ExecuteAsync();

    public async Task ExecuteAsync()
    {
        if (_isExecuting) { return; }
        _isExecuting = true;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        try { await _execute(); }
        finally
        {
            _isExecuting = false;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
