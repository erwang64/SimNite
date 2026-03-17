using System.Windows.Input;

namespace SimNite.ViewModels;

public class RelayCommand : ICommand
{
	private readonly Action<object?>? _execute;
	private readonly Func<object?, Task>? _executeAsync;
	private readonly Predicate<object?>? _canExecute;
	private bool _isExecuting;

	public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
	{
		_execute = execute ?? throw new ArgumentNullException(nameof(execute));
		_canExecute = canExecute;
	}

	public RelayCommand(Func<object?, Task> executeAsync, Predicate<object?>? canExecute = null)
	{
		_executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
		_canExecute = canExecute;
	}

	public event EventHandler? CanExecuteChanged;

	public bool CanExecute(object? parameter)
	{
		if (_isExecuting)
		{
			return false;
		}

		return _canExecute?.Invoke(parameter) ?? true;
	}

	public async void Execute(object? parameter)
	{
		if (!CanExecute(parameter))
		{
			return;
		}

		if (_executeAsync is not null)
		{
			_isExecuting = true;
			RaiseCanExecuteChanged();
			try
			{
				await _executeAsync(parameter);
			}
			finally
			{
				_isExecuting = false;
				RaiseCanExecuteChanged();
			}

			return;
		}

		_execute?.Invoke(parameter);
	}

	public void RaiseCanExecuteChanged()
	{
		CanExecuteChanged?.Invoke(this, EventArgs.Empty);
	}
}
