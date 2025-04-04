//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Windows;
using TP.ConcurrentProgramming.Presentation.ViewModel;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.PresentationView
{
  /// <summary>
  /// View implementation
  /// </summary>
  public partial class MainWindow : Window
  {
        private DataAbstractAPI _dataLayer;
        public MainWindow()
        {
            InitializeComponent();
            _dataLayer = DataAbstractAPI.GetDataLayer();
            MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            SpeedSlider.ValueChanged += SpeedSlider_ValueChanged;
        }
        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _dataLayer.UpdateSpeed(SpeedSlider.Value);

            SpeedLabel.Content = $"Speed: {SpeedSlider.Value}";
        }
        /// <summary>
        /// Obsluguje klikniecie przycisku Start Game.
        /// </summary>
        private void OnStartGameClick(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;

            if (int.TryParse(BallsCountTextBox.Text, out int numberOfBalls) && numberOfBalls > 0)
            {
                
                viewModel.Start(numberOfBalls);
            }
            else
            {
                
                MessageBox.Show("Prosze wprowadzic poprawne liczbe kul", "Blad", MessageBoxButton.OK, MessageBoxImage.Error);
                viewModel.Start(5); 
            }
        }

        /// <summary>
        /// Raises the <seealso cref="System.Windows.Window.Closed"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
    {
      if (DataContext is MainWindowViewModel viewModel)
        viewModel.Dispose();
      base.OnClosed(e);
    }
  }
}