using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmartParking.Model
{
    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _executeAsync;
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Func<Task> executeAsync) => _executeAsync = executeAsync;

        public bool CanExecute(object parameter) => true;

        public async void Execute(object parameter) => await _executeAsync();
    }
}
