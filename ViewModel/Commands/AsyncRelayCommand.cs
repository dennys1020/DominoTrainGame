using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DominoTrainGame.ViewModels.Commands;

public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<Task> execute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        bool canExecute = !_isExecuting;
        return canExecute;
    }

    public async void Execute(object parameter)
    {
        _isExecuting = true;
        NotifyCanExecuteChanged();

        try
        {
            await _execute();
        }
        finally
        {
            _isExecuting = false;
            NotifyCanExecuteChanged();
        }
    }

    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
