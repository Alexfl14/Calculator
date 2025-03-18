using Calculator.MVVM;
using Calculator.View;
using System;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace Calculator.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        private bool _isBinaryMode;
        private bool _isDecimalMode = true;
        private bool _isHexadecimalMode;
        private bool _isOctalMode;

        private string _displayText = "0";
        private double _currentValue = 0;
        private double _storedValue = 0;
        private string _currentOperation = "";
        private bool _isNewCalculation = true;
        private bool _lastInputWasOperation = false;
        private string _equationText = "";
        private string _currentExpression = "";
        private bool _isStartingNewExpression = true;
        public string EquationText
        {
            get => _equationText;
            set
            {
                if (_equationText != value)
                {
                    _equationText = value;
                    OnPropertyChanged(nameof(EquationText));
                }
            }
        }
        public string DisplayText
        {
            get => _displayText;
            set
            {
                if (_displayText != value)
                {
                    _displayText = value;
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        public bool IsBinaryMode
        {
            get => _isBinaryMode;
            set
            {
                if (_isBinaryMode != value)
                {
                    _isBinaryMode = value;
                    if (value)
                    {
                        _isDecimalMode = false;
                        _isHexadecimalMode = false;
                        _isOctalMode = false;

                        ConvertDisplay();
                    }
                    OnPropertyChanged(nameof(IsBinaryMode));
                    OnPropertyChanged(nameof(IsDecimalMode));
                    OnPropertyChanged(nameof(IsHexadecimalMode));
                    OnPropertyChanged(nameof(IsOctalMode));
                }
            }
        }

        public bool IsDecimalMode
        {
            get => _isDecimalMode;
            set
            {
                if (_isDecimalMode != value)
                {
                    _isDecimalMode = value;
                    if (value)
                    {
                        // Ensure other modes are off
                        _isBinaryMode = false;
                        _isHexadecimalMode = false;
                        _isOctalMode = false;

                        // Convert current value to decimal
                        ConvertDisplay();
                    }
                    OnPropertyChanged(nameof(IsDecimalMode));
                    OnPropertyChanged(nameof(IsBinaryMode));
                    OnPropertyChanged(nameof(IsHexadecimalMode));
                    OnPropertyChanged(nameof(IsOctalMode));
                }
            }
        }

        public bool IsHexadecimalMode
        {
            get => _isHexadecimalMode;
            set
            {
                if (_isHexadecimalMode != value)
                {
                    _isHexadecimalMode = value;
                    if (value)
                    {
                        // Ensure other modes are off
                        _isBinaryMode = false;
                        _isDecimalMode = false;
                        _isOctalMode = false;

                        // Convert current value to hexadecimal
                        ConvertDisplay();
                    }
                    OnPropertyChanged(nameof(IsHexadecimalMode));
                    OnPropertyChanged(nameof(IsBinaryMode));
                    OnPropertyChanged(nameof(IsDecimalMode));
                    OnPropertyChanged(nameof(IsOctalMode));
                }
            }
        }

        public bool IsOctalMode
        {
            get => _isOctalMode;
            set
            {
                if (_isOctalMode != value)
                {
                    _isOctalMode = value;
                    if (value)
                    {
                        _isBinaryMode = false;
                        _isDecimalMode = false;
                        _isHexadecimalMode = false;
                        ConvertDisplay();
                    }
                    OnPropertyChanged(nameof(IsOctalMode));
                    OnPropertyChanged(nameof(IsBinaryMode));
                    OnPropertyChanged(nameof(IsDecimalMode));
                    OnPropertyChanged(nameof(IsHexadecimalMode));
                }
            }
        }

        // Commands
        public ICommand ChangeModeCommand { get; }
        public ICommand ShowAboutCommand { get; }
        public ICommand NumberCommand { get; }
        public ICommand OperationCommand { get; }
        public ICommand EqualsCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ClearEntryCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand DecimalPointCommand { get; }
        public ICommand ToggleSignCommand { get; }
        public ICommand SpecialFunctionCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }


        public MainWindowViewModel()
        {
            LoadSavedSettings();
            ChangeModeCommand = new RelayCommand(ChangeMode);
            ShowAboutCommand = new RelayCommand(ShowAboutDialog);
            NumberCommand = new RelayCommand(AppendNumber);
            OperationCommand = new RelayCommand(SetOperation);
            EqualsCommand = new RelayCommand(PerformCalculation);
            ClearCommand = new RelayCommand(Clear);
            ClearEntryCommand = new RelayCommand(ClearEntry);
            BackspaceCommand = new RelayCommand(Backspace);
            DecimalPointCommand = new RelayCommand(AddDecimalPoint);
            ToggleSignCommand = new RelayCommand(ToggleSign);
            SpecialFunctionCommand = new RelayCommand(PerformSpecialFunction);
            ResetCommand = new RelayCommand(Reset);
            ToggleDigitGroupingCommand = new RelayCommand(ToggleDigitGrouping);
            CutCommand = new RelayCommand(_ => Cut());
            CopyCommand = new RelayCommand(_ => Copy());
            PasteCommand = new RelayCommand(_ => Paste());
        }

        private void ConvertDisplay()
        {
            if (string.IsNullOrEmpty(DisplayText) || DisplayText == "0")
            {
                DisplayText = "0";
                return;
            }

            try
            {
                double value = _currentValue;

                if (IsBinaryMode)
                {
                    DisplayText = Convert.ToString((int)value, 2);
                }
                else if (IsHexadecimalMode)
                {
                    DisplayText = Convert.ToString((int)value, 16).ToUpper();
                }
                else if (IsOctalMode)   
                {
                    DisplayText = Convert.ToString((int)value, 8);
                }
                else 
                {
                    DisplayText = value.ToString();
                }
            }
            catch
            {
                DisplayText = "Error";
            }
        }


        private void AppendNumber(object? parameter)
        {
            if (parameter is string digit)
            {
                
                if (IsBinaryMode && !digit.All(c => c == '0' || c == '1'))
                    return;
                if (IsOctalMode && digit.Any(c => c < '0' || c > '7'))
                    return;
                if (IsHexadecimalMode && !digit.All(c => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
                    return;
                if (IsDecimalMode && !digit.All(c => (c >= '0' && c <= '9')))
                    return;

                if (IsOperatorPrecedenceEnabled)
                {
                    if (_isNewCalculation)
                    {
                        _currentExpression = digit;
                        DisplayText = digit;
                        _isNewCalculation = false;
                        _isStartingNewExpression = false;
                        EquationText = _currentExpression;
                    }
                    else if (_lastInputWasOperation)
                    {
                        _currentExpression += digit;
                        DisplayText = digit;
                        _lastInputWasOperation = false;
                        EquationText = _currentExpression;
                    }
                    else
                    {
                        _currentExpression += digit;
                        DisplayText = DisplayText == "0" ? digit : DisplayText + digit;
                        EquationText = _currentExpression;
                    }

                    UpdateCurrentValue();
                }
                else
                {
                    if (_isNewCalculation || DisplayText == "0" || _lastInputWasOperation)
                    {
                        DisplayText = digit;
                        _isNewCalculation = false;
                        _lastInputWasOperation = false;

                        if (_isNewCalculation)
                            EquationText = "";
                    }
                    else
                    {
                        if (IsDecimalMode)
                        {
                            string currentText = DisplayText.Replace(",", ""); 
                            DisplayText = currentText + digit;
                        }
                        else
                        {
                            DisplayText += digit;
                        }
                    }

                    UpdateCurrentValue();
                }

                if (IsDigitGroupingEnabled)
                {
                    UpdateDisplayText();
                }
            }
        }

        private void UpdateCurrentValue()
        {
            string textToConvert = DisplayText.Replace(",", "");

            if (IsBinaryMode)
                _currentValue = Convert.ToInt32(textToConvert, 2);
            else if (IsHexadecimalMode)
                _currentValue = Convert.ToInt32(textToConvert, 16);
            else if (IsOctalMode)
                _currentValue = Convert.ToInt32(textToConvert, 8);
            else
                _currentValue = Convert.ToDouble(textToConvert);
        }

        private void SetOperation(object? parameter)
        {
            if (parameter is string operation)
            {
                if (IsOperatorPrecedenceEnabled)
                {
                    if (_isStartingNewExpression)
                    {
                        _currentExpression = _currentValue.ToString();
                        _isStartingNewExpression = false;
                    }

                    _currentExpression += GetOperatorSymbol(operation);

                    EquationText = _currentExpression;

                    _lastInputWasOperation = true;
                }
                else
                {
                    if (!_lastInputWasOperation)
                    {
                        if (!string.IsNullOrEmpty(_currentOperation))
                        {
                            PerformCalculation(null);
                        }
                        else
                        {
                            _storedValue = _currentValue;
                        }
                    }

                    _currentOperation = operation;
                    _lastInputWasOperation = true;

                    EquationText = $"{_storedValue} {_currentOperation}";
                }
            }
        }
        private string GetOperatorSymbol(string operation)
        {
            return operation switch
            {
                "×" => "*",
                "÷" => "/",
                "−" => "-",
                _ => operation
            };
        }
        private void PerformCalculation(object? parameter)
        {
            if (IsOperatorPrecedenceEnabled)
            {
                if (string.IsNullOrEmpty(_currentExpression))
                    return;

                try
                {
                    var dataTable = new System.Data.DataTable();

                    double result = Convert.ToDouble(dataTable.Compute(_currentExpression, null));

                    EquationText = $"{_currentExpression} = {result}";

                    _currentValue = result;
                    _storedValue = result;

                    if (IsBinaryMode)
                    {
                        DisplayText = Convert.ToString((int)result, 2);
                    }
                    else if (IsHexadecimalMode)
                    {
                        DisplayText = Convert.ToString((int)result, 16).ToUpper();
                    }
                    else if (IsOctalMode)
                    {
                        DisplayText = Convert.ToString((int)result, 8);
                    }
                    else
                    {
                        DisplayText = result.ToString();
                    }

                    _currentExpression = "";
                    _isNewCalculation = true;
                    _isStartingNewExpression = true;
                }
                catch (Exception)
                {
                    DisplayText = "Error";
                    _currentExpression = "";
                    _isNewCalculation = true;
                    _isStartingNewExpression = true;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_currentOperation))
                    return;

                double result = 0;

                EquationText = $"{_storedValue} {_currentOperation} {_currentValue} =";

                switch (_currentOperation)
                {
                    case "+":
                        result = _storedValue + _currentValue;
                        break;
                    case "−":
                    case "-":
                        result = _storedValue - _currentValue;
                        break;
                    case "×":
                    case "*":
                        result = _storedValue * _currentValue;
                        break;
                    case "÷":
                    case "/":
                        if (_currentValue != 0)
                            result = _storedValue / _currentValue;
                        else
                        {
                            DisplayText = "Error: Div by 0";
                            _isNewCalculation = true;
                            _currentOperation = "";
                            return;
                        }
                        break;
                    case "%":
                        result = _storedValue % _currentValue;
                        break;
                }

                _currentValue = result;
                _storedValue = result;

                if (IsBinaryMode)
                {
                    DisplayText = Convert.ToString((int)result, 2);
                }
                else if (IsHexadecimalMode)
                {
                    DisplayText = Convert.ToString((int)result, 16).ToUpper();
                }
                else if (IsOctalMode)
                {
                    DisplayText = Convert.ToString((int)result, 8);
                }
                else
                {
                    if(IsDigitGroupingEnabled)
                    {
                        System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-US");
                        culture.NumberFormat.NumberGroupSeparator = ",";
                        culture.NumberFormat.NumberDecimalSeparator = ".";
                        DisplayText = _currentValue.ToString("N0", culture);
                    }
                    else { DisplayText = result.ToString(); }
             
                }

                _currentOperation = "";
                _isNewCalculation = true;
            }
        }

        private void Clear(object? parameter)
        {
            DisplayText = "0";
            _currentValue = 0;
            _storedValue = 0;
            _currentOperation = "";
            _currentExpression = "";
            _isNewCalculation = true;
            _isStartingNewExpression = true;
            _lastInputWasOperation = false;
            EquationText = "";
        }

        private void ClearEntry(object? parameter)
        {
            if (IsOperatorPrecedenceEnabled)
            {
                
                DisplayText = "0";
            }
            else
            {
                DisplayText = "0";
                _currentValue = 0;
                _lastInputWasOperation = false;
            }
        }

        private void Backspace(object? parameter)
        {
            if (_isNewCalculation || _lastInputWasOperation || DisplayText.Length <= 1)
            {
                DisplayText = "0";
                _currentValue = 0;
            }
            else
            {
                DisplayText = DisplayText.Substring(0, DisplayText.Length - 1);
                if (string.IsNullOrEmpty(DisplayText))
                {
                    DisplayText = "0";
                    _currentValue = 0;
                }
                else
                {
                    UpdateCurrentValue();
                }
            }
            _lastInputWasOperation = false;
        }

        private void AddDecimalPoint(object? parameter)
        {
            if (!IsDecimalMode)
                return;

            if (_isNewCalculation || _lastInputWasOperation)
            {
                DisplayText = "0.";
                _isNewCalculation = false;
                _lastInputWasOperation = false;
            }
            else if (!DisplayText.Contains("."))
            {
                DisplayText += ".";
            }
        }

        private void ToggleSign(object? parameter)
        {
            if (DisplayText != "0")
            {
                if (IsDecimalMode)
                {
                    _currentValue = -_currentValue;
                    DisplayText = _currentValue.ToString();
                }
                else if (IsBinaryMode)
                {
                    _currentValue = -_currentValue;
                    DisplayText = Convert.ToString((int)_currentValue, 2);
                }
                else if (IsHexadecimalMode)
                {
                    _currentValue = -_currentValue;
                    DisplayText = Convert.ToString((int)_currentValue, 16).ToUpper();
                }
                else if (IsOctalMode)
                {
                    _currentValue = -_currentValue;
                    DisplayText = Convert.ToString((int)_currentValue, 8);
                }
            }
        }

        private void PerformSpecialFunction(object? parameter)
        {
            if (parameter is string function)
            {
                switch (function)
                {
                    case "¹/ₓ":
                        if (_currentValue != 0)
                        {
                            _currentValue = 1 / _currentValue;
                            DisplayText = _currentValue.ToString();
                        }
                        else
                        {
                            DisplayText = "Error: Div by 0";
                        }
                        break;
                    case "x²":
                        _currentValue = _currentValue * _currentValue;
                        DisplayText = _currentValue.ToString();
                        break;
                    case "²√x": 
                        if (_currentValue >= 0)
                        {
                            _currentValue = Math.Sqrt(_currentValue);
                            DisplayText = _currentValue.ToString();
                        }
                        else
                        {
                            DisplayText = "Error";
                        }
                        break;
                }
                _isNewCalculation = true;
            }
        }


        private void Reset(object? parameter)
        {
            Clear(null);
        }

        private void ShowAboutDialog(object? parameter)
        {
            var aboutDialog = new AboutDialog();

            if (parameter is Window owner)
            {
                aboutDialog.Owner = owner;
            }

            aboutDialog.ShowDialog();
        }
        public void HandleKeyPress(Key key)
        {
            if ((key >= Key.D0 && key <= Key.D9) || (key >= Key.NumPad0 && key <= Key.NumPad9))
            {

                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift && key == Key.D8)
                {
                    SetOperation("*");
                }

                else
                { 
                string digit;
                if (key >= Key.D0 && key <= Key.D9)
                    digit = (key - Key.D0).ToString();
                else
                    digit = (key - Key.NumPad0).ToString();

                NumberCommand.Execute(digit);
                }
            }
            else if (key == Key.Add || key == Key.OemPlus)
            {
                OperationCommand.Execute("+");
            }
            else if (key == Key.Subtract || key == Key.OemMinus)
            {
                OperationCommand.Execute("−");
            }
            else if (key == Key.Multiply)
            {
                SetOperation("*");
            }
            else if (key == Key.Divide || key == Key.OemQuestion)
            {
                OperationCommand.Execute("÷");
            }
            else if (key == Key.Oem5 || key == Key.OemBackslash)
            {
                OperationCommand.Execute("%");
            }
            else if (key == Key.Enter || key == Key.OemPlus)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && key == Key.OemPlus)
                {
                    EqualsCommand.Execute(null);
                }
                else if (key == Key.Enter)
                {
                    EqualsCommand.Execute(null);
                }
            }
            else if (key == Key.Back)
            {
                BackspaceCommand.Execute(null);
            }
            else if (key == Key.Escape)
            {
                ClearCommand.Execute(null);
            }
            else if (key == Key.Delete)
            {
                ClearEntryCommand.Execute(null);
            }
            else if (key == Key.OemPeriod || key == Key.Decimal)
            {
                DecimalPointCommand.Execute(null);
            }
            else if (IsHexadecimalMode && key >= Key.A && key <= Key.F)
            {
                string hexDigit = key.ToString().Last().ToString();
                NumberCommand.Execute(hexDigit);
            }
        }
        private bool _isDigitGroupingEnabled;

        public ICommand ToggleDigitGroupingCommand { get; }

       
        private void ToggleDigitGrouping(object? parameter)
        {
            IsDigitGroupingEnabled = !IsDigitGroupingEnabled;
        }

        private void UpdateDisplayText()
        {
            if (IsDecimalMode)
            {
                if (IsDigitGroupingEnabled)
                {
                    System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("en-US");
                    culture.NumberFormat.NumberGroupSeparator = ",";
                    culture.NumberFormat.NumberDecimalSeparator = ".";
                    DisplayText = _currentValue.ToString("N0", culture);
                }
                else
                {
                    DisplayText = _currentValue.ToString();
                }
            }

            else if (IsBinaryMode)
            {
                DisplayText = Convert.ToString((int)_currentValue, 2);
            }
            else if (IsHexadecimalMode)
            {
                DisplayText = Convert.ToString((int)_currentValue, 16).ToUpper();
            }
            else if (IsOctalMode)
            {
                DisplayText = Convert.ToString((int)_currentValue, 8);
            }
        }



        private void Cut()
        {
            Clipboard.SetText(DisplayText);
            DisplayText = "";
        }

        private void Copy()
        {
            Clipboard.SetText(DisplayText);
        }

        private void Paste()
        {
            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();
                if (double.TryParse(clipboardText, out double clipboardValue))
                {
                        DisplayText = clipboardValue.ToString();
                }
                else
                {
                    MessageBox.Show("Clipboard-ul nu conține un număr valid.");
                }
            }
        }
        public bool IsDigitGroupingEnabled
        {
            get => _isDigitGroupingEnabled;
            set
            {
                if (_isDigitGroupingEnabled != value)
                {
                    _isDigitGroupingEnabled = value;
                    OnPropertyChanged(nameof(IsDigitGroupingEnabled));
                    UpdateDisplayText();
                    Properties.Settings.Default.IsDigitGroupingEnabled = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void ChangeMode(object? parameter)
        {
            if (parameter is string mode)
            {
                switch (mode)
                {
                    case "Binary":
                        IsBinaryMode = true;
                        Properties.Settings.Default.CalculatorMode = "Binary";
                        break;
                    case "Decimal":
                        IsDecimalMode = true;
                        Properties.Settings.Default.CalculatorMode = "Decimal";
                        break;
                    case "Hexadecimal":
                        IsHexadecimalMode = true;
                        Properties.Settings.Default.CalculatorMode = "Hexadecimal";
                        break;
                    case "Octal":
                        IsOctalMode = true;
                        Properties.Settings.Default.CalculatorMode = "Octal";
                        break;
                }
                Properties.Settings.Default.Save();
            }
        }

        private void LoadSavedSettings()
        {
            _isDigitGroupingEnabled = Properties.Settings.Default.IsDigitGroupingEnabled;
            OnPropertyChanged(nameof(IsDigitGroupingEnabled));
            _isOperatorPrecedenceEnabled = Properties.Settings.Default.IsOperatorPrecedenceEnabled;
            OnPropertyChanged(nameof(IsOperatorPrecedenceEnabled));
            string savedMode = Properties.Settings.Default.CalculatorMode;
            switch (savedMode)
            {
                case "Binary":
                    _isBinaryMode = true;
                    _isDecimalMode = false;
                    _isHexadecimalMode = false;
                    _isOctalMode = false;
                    break;
                case "Hexadecimal":
                    _isBinaryMode = false;
                    _isDecimalMode = false;
                    _isHexadecimalMode = true;
                    _isOctalMode = false;
                    break;
                case "Octal":
                    _isBinaryMode = false;
                    _isDecimalMode = false;
                    _isHexadecimalMode = false;
                    _isOctalMode = true;
                    break;
                default: 
                    _isBinaryMode = false;
                    _isDecimalMode = true;
                    _isHexadecimalMode = false;
                    _isOctalMode = false;
                    break;
            }

            OnPropertyChanged(nameof(IsBinaryMode));
            OnPropertyChanged(nameof(IsDecimalMode));
            OnPropertyChanged(nameof(IsHexadecimalMode));
            OnPropertyChanged(nameof(IsOctalMode));

            ConvertDisplay();
        }
        private bool _isOperatorPrecedenceEnabled;
        public ICommand ToggleOperatorPrecedenceCommand { get; }
        public bool IsOperatorPrecedenceEnabled
        {
            get => _isOperatorPrecedenceEnabled;
            set
            {
                if (_isOperatorPrecedenceEnabled != value)
                {
                    _isOperatorPrecedenceEnabled = value;
                    OnPropertyChanged(nameof(IsOperatorPrecedenceEnabled));

                    Properties.Settings.Default.IsOperatorPrecedenceEnabled = value;
                    Properties.Settings.Default.Save();
                }
            }
        }
        private void ToggleOperatorPrecedence(object? parameter)
        {
            IsOperatorPrecedenceEnabled = !IsOperatorPrecedenceEnabled;

            Clear(null);
        }

    }
}