using System.Windows.Input;
using Calculator.MVVM;

namespace Calculator.ViewModel
{
    public class AboutDialogViewModel : ViewModelBase
    {
        public string DeveloperName { get; set; } = "Florea Alexandru-Florentin";
        public string GroupName { get; set; } = "10LF232";
        public string ApplicationName { get; set; } = "WPF Calculator";

        private ICommand _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(
                        param => CloseRequested?.Invoke(this, new EventArgs()),
                        param => true
                    );
                }
                return _closeCommand;
            }
        }

        public event EventHandler CloseRequested;
    }
}