using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged;

namespace QualityGrapher.ViewModels
{
    /// <summary>
    /// Base class for all ViewModels, property notification injected automatically by FodyWeavers,
    /// use <see cref="DoNotNotifyAttribute"/> to  avaoid injecting property notification
    /// </summary>
    internal abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
