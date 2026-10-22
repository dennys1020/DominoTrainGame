using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DominoTrainGame.Client.ViewModel.Common;

public abstract class ObservableViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

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
}
