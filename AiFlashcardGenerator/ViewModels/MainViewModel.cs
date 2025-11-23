using AiFlashcardGenerator.Models;
using AiFlashcardGenerator.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AiFlashcardGenerator.ViewModels
{
    // Simple base class for INotifyPropertyChanged
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    // Simple implementation of ICommand
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object? parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class MainViewModel : ViewModelBase
    {
        private readonly GeminiService _geminiService;

        private string _topicInput = string.Empty;
        public string TopicInput
        {
            get => _topicInput;
            set
            {
                if (Set(ref _topicInput, value))
                {
                    // CRITICAL FIX: Notify the command that the CanExecute state might have changed
                    // because the TopicInput property (which the command depends on) has changed.
                    (GenerateCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isGenerating;
        public bool IsGenerating
        {
            get => _isGenerating;
            set => Set(ref _isGenerating, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (Set(ref _errorMessage, value))
                {
                    // Notify the UI when the ErrorMessage changes, which affects IsErrorMessageVisible
                    OnPropertyChanged(nameof(IsErrorMessageVisible));
                }
            }
        }

        /// <summary>
        /// Computed property used to control the visibility of the error TextBlock in the XAML.
        /// XAML cannot natively use string.IsNullOrWhiteSpace, so we provide a boolean proxy property.
        /// </summary>
        public bool IsErrorMessageVisible => !string.IsNullOrWhiteSpace(ErrorMessage);

        private ObservableCollection<Flashcard> _flashcards = new();
        public ObservableCollection<Flashcard> Flashcards
        {
            get => _flashcards;
            set => Set(ref _flashcards, value);
        }

        public ICommand GenerateCommand { get; }

        public MainViewModel()
        {
            _geminiService = new GeminiService();
            // The CanExecute predicate now correctly resolves string.IsNullOrWhiteSpace
            GenerateCommand = new RelayCommand(async _ => await GenerateFlashcards(), _ => !IsGenerating && !string.IsNullOrWhiteSpace(TopicInput));

            // Initial flashcard for guidance
            Flashcards.Add(new Flashcard
            {
                Front = "Welcome to the AI Flashcard Generator!",
                Back = "Enter a topic in the box above (e.g., 'The Roman Empire' or 'Quantum Computing') and click 'Generate' to create 5 flashcards instantly."
            });
        }

        private async Task GenerateFlashcards()
        {
            IsGenerating = true;
            ErrorMessage = string.Empty;
            Flashcards.Clear();

            // Notify the command manager that TopicInput might change the CanExecute state
            (GenerateCommand as RelayCommand)?.RaiseCanExecuteChanged();

            try
            {
                var newCards = await _geminiService.GenerateFlashcardsAsync(TopicInput);

                if (newCards.Count == 0)
                {
                    ErrorMessage = "The AI did not generate any flashcards. Please try a different topic.";
                }

                foreach (var card in newCards)
                {
                    Flashcards.Add(card);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred during generation: {ex.Message}";
            }
            finally
            {
                IsGenerating = false;
                (GenerateCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }
}
