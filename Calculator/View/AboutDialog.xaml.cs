using Calculator.ViewModel;
using System;
using System.Windows;

namespace Calculator.View
{
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();

            var viewModel = new AboutDialogViewModel();
            DataContext = viewModel;

            viewModel.CloseRequested += (sender, e) => Close();
        }
    }
}