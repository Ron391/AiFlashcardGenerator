using AiFlashcardGenerator.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;

namespace AiFlashcardGenerator.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Assign the MainViewModel as the DataContext
            DataContext = new MainViewModel();
        }

        /// <summary>
        /// Allows the borderless window to be dragged when the user presses and holds the header area.
        /// </summary>
        private void OnHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Only allow dragging with the left mouse button
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                System.Diagnostics.Debug.WriteLine("Attempting to drag window...");

                // This is the Avalonia method to start a window drag operation
                BeginMoveDrag(e);

                // IMPORTANT: Mark the event as handled to ensure the drag operation is prioritized 
                // and no other nested controls interfere.
                e.Handled = true;
            }
        }
    }
}