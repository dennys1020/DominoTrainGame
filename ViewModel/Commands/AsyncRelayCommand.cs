using System;
using System.Threading.Tasks;

namespace DominoTrainGame.Client.ViewModel.Commands;

public sealed class AsyncRelayCommand : AsyncCommand
{
    private readonly Func<Task> _execute;

    public AsyncRelayCommand(Func<Task> execute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    public override async void Execute(object parameter)
    {
        await RunAsync(_execute);
    }
}
