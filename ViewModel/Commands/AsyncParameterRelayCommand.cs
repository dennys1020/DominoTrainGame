using System;
using System.Threading.Tasks;

namespace DominoTrainGame.Client.ViewModel.Commands;

public sealed class AsyncParameterRelayCommand<TParameter> : AsyncCommand
{
    private readonly Func<TParameter, Task> _execute;

    public AsyncParameterRelayCommand(Func<TParameter, Task> execute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    public override bool CanExecute(object parameter)
    {
        bool canExecute = base.CanExecute(parameter) && parameter is TParameter;
        return canExecute;
    }

    public override async void Execute(object parameter)
    {
        if (parameter is TParameter typedParameter)
        {
            await RunAsync(() => _execute(typedParameter));
        }
    }
}
