using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DominoTrainGame.Client.ViewModel.Common;

public abstract class ObservableViewModel : INotifyPropertyChanged
{
    private const int NoActiveOperations = 0;

    private int _activeOperations;
    private bool _isBusy;

    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsBusy
    {
        get
        {
            return _isBusy;
        }
        protected set
        {
            bool hasChanged = SetProperty(ref _isBusy, value);

            if (hasChanged)
            {
                OnPropertyChanged(nameof(IsIdle));
            }
        }
    }

    public bool IsIdle => !IsBusy;

    protected bool SetProperty<TValue>(ref TValue field, TValue value, [CallerMemberName] string propertyName = "")
    {
        bool hasChanged = !EqualityComparer<TValue>.Default.Equals(field, value);

        if (hasChanged)
        {
            field = value;
            OnPropertyChanged(propertyName);
        }

        return hasChanged;
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected Task<TResult> RunWhileBusyAsync<TResult>(Func<Task<TResult>> operation)
    {
        if (operation == null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        Task<TResult> busyOperation = RunWhileBusyCoreAsync(operation);
        return busyOperation;
    }

    private async Task<TResult> RunWhileBusyCoreAsync<TResult>(Func<Task<TResult>> operation)
    {
        TResult result;
        _activeOperations++;
        IsBusy = true;

        try
        {
            result = await operation();
        }
        finally
        {
            _activeOperations--;
            IsBusy = _activeOperations > NoActiveOperations;
        }

        return result;
    }
}
