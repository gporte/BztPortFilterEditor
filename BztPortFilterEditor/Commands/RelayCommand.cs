using System;
using System.Windows.Input;

namespace BztPortFilterEditor.Commands
{
	public class RelayCommand<T> : ICommand
	{
		#region private fields
		private readonly Action<T> execute;
		private readonly Predicate<T> canExecute;
		#endregion

		public event EventHandler CanExecuteChanged {
			add {
				if (this.canExecute != null)
					CommandManager.RequerySuggested += value;
			}
			remove {
				if (this.canExecute != null)
					CommandManager.RequerySuggested -= value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the RelayCommand class
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		public RelayCommand(Action<T> execute)
			: this(execute, null) {
		}

		/// <summary>
		/// Initializes a new instance of the RelayCommand class
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		/// <param name="canExecute">The execution status logic.</param>
		public RelayCommand(Action<T> execute, Predicate<T> canExecute) {
			if (execute == null)
				throw new ArgumentNullException("execute");

			this.execute = execute;
			this.canExecute = canExecute;
		}

		public void Execute(object parameter) {
			this.execute((T)parameter);
		}

		public bool CanExecute(object parameter) {
			return this.canExecute == null ? true : this.canExecute((T)parameter);
		}
	}
}
