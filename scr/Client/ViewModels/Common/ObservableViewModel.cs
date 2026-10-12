using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DominoTrainGame.ViewModels;

public abstract class ObservableViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        Notify(propertyName);
        return true;
    }

    protected void Notify([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
