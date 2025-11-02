using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Projektverwaltung.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action _exec;
        private readonly Func<bool> _can;
        public RelayCommand(Action exec, Func<bool> can = null)
        { _exec = exec; _can = can; }

        public bool CanExecute(object parameter) => _can?.Invoke() ?? true;
        public void Execute(object parameter) => _exec();
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
