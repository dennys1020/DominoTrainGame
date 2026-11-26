using System;
using System.Windows.Input;

namespace DominoTrainGame.Client.ViewModel.Commands;

public sealed class RelayCommand : ICommand
{
    private readonly Action _execute;

    public RelayCommand(Action execute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    public event EventHandler CanExecuteChanged
    {
        add
        {
        }
        remove
        {
        }
    }

    public bool CanExecute(object parameter)
    {
        bool canExecute = true;
        return canExecute;
    }

    public void Execute(object parameter)
    {
        _execute();
    }
}
