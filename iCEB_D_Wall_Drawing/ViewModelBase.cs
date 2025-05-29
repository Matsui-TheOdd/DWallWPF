using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;
using Tekla.Structures;
using Tekla.Structures.Dialog;
using Tekla.Structures.Datatype;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Drawing;
using Tekla.Structures.DrawingInternal;
using TSM = Tekla.Structures.Model;
using TSMui = Tekla.Structures.Model.UI;
using TSD = Tekla.Structures.Drawing;
using TSDi = Tekla.Structures.DrawingInternal;
using TSDa = Tekla.Structures.Datatype;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace iCEB_D_Wall_Drawing
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static event PropertyChangedEventHandler StaticPropertyChanged;

        public static void OnStatisPropertyChanged([CallerMemberName] string propertyName = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        public Window Window { get; set; }

        public virtual void CloseWindow(bool? result = true)
        {
            Window.DialogResult = result;
            Window?.Close();
        }

    }
    class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute1;
        public RelayCommand(Predicate<T> canExecute, Action<T> execute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _canExecute = canExecute;
            _execute = execute;
        }
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute1 = null)
        {
            _execute = execute;
            _canExecute1 = canExecute1;
        }
        public bool CanExecute(object parameter)
        {
            try
            {
                return _canExecute == null ? true : _canExecute((T)parameter);
            }
            catch
            {
                return true;
            }
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
