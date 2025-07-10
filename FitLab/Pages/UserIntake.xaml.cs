using FitLab.Components;
using FitLab.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Diagnostics;


namespace FitLab.Pages
{
    public partial class UserIntake : Page
    {
        private readonly User _newUser = new();
        private readonly List<Goal> _goals = new();

        public UserIntake()
        {
            InitializeComponent();
        }

        private void NextFirstName(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFirstName.Text))
            {
                MessageBox.Show("Please enter your first name.");
                return;
            }

            _newUser.Name = TxtFirstName.Text.Trim();

            StepFirstName.Visibility = Visibility.Collapsed;
            StepGender.Visibility = Visibility.Visible;
        }

        private void CmbGender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbGender.SelectedItem is ComboBoxItem selected)
            {
                var value = selected.Content?.ToString() ?? "";
                if (value == "Trans-Feminine" || value == "Trans-Masculine")
                {
                    ChkOnHormones.Visibility = Visibility.Visible;
                }
                else
                {
                    ChkOnHormones.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void NextGender(object sender, RoutedEventArgs e)
        {
            if (CmbGender.SelectedItem is ComboBoxItem selected)
            {
                _newUser.Gender = selected.Content?.ToString() ?? "";
                if (_newUser.Gender == "Trans-Feminine" || _newUser.Gender == "Trans-Masculine")
                {
                    _newUser.Gender += ChkOnHormones.IsChecked == true ? " (On Hormones)" : " (Not on Hormones)";
                }
            }
            else
            {
                MessageBox.Show("Please select your gender.");
                return;
            }

            StepGender.Visibility = Visibility.Collapsed;
            StepDOB.Visibility = Visibility.Visible;
        }

        private void NextDOB(object sender, RoutedEventArgs e)
        {
            if (DatePickerDOB.SelectedDate is DateTime dob)
            {
                _newUser.DateOfBirth = dob;
            }
            else
            {
                MessageBox.Show("Please select your date of birth.");
                return;
            }

            StepDOB.Visibility = Visibility.Collapsed;
            StepHeight.Visibility = Visibility.Visible;
        }
        private void CmbHeightUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TxtCmInstruction.Visibility = Visibility.Collapsed;
            TxtInchesInstruction.Visibility = Visibility.Collapsed;
            TxtFeetInchesInstruction.Visibility = Visibility.Collapsed;

            PanelFeetLabel.Visibility = Visibility.Collapsed;
            PanelInchesLabel.Visibility = Visibility.Collapsed;
            PanelSingleInput.Visibility = Visibility.Collapsed;

            if (CmbHeightUnit.SelectedItem is ComboBoxItem selected)
            {
                var value = selected.Content?.ToString() ?? "";

                if (value == "Centimeters")
                {
                    TxtCmInstruction.Visibility = Visibility.Visible;
                    PanelSingleInput.Visibility = Visibility.Visible;
                }
                else if (value == "Inches")
                {
                    TxtInchesInstruction.Visibility = Visibility.Visible;
                    PanelSingleInput.Visibility = Visibility.Visible;
                }
                else if (value == "Feet/Inches")
                {
                    TxtFeetInchesInstruction.Visibility = Visibility.Visible;
                    PanelFeetLabel.Visibility = Visibility.Visible;
                    PanelInchesLabel.Visibility = Visibility.Visible;
                }
            }
        }

        private void NextHeight(object sender, RoutedEventArgs e)
        {
            if (CmbHeightUnit.SelectedItem is ComboBoxItem selected)
            {
                var unit = selected.Content?.ToString() ?? "";
                double heightInches = 0;

                if (unit == "Centimeters")
                {
                    if (double.TryParse(TxtHeightPrimary.Text, out double cm))
                        heightInches = Conversions.CmToInches(cm);
                    else
                    {
                        MessageBox.Show("Enter a valid number.");
                        return;
                    }
                }
                else if (unit == "Inches")
                {
                    if (double.TryParse(TxtHeightPrimary.Text, out double inches))
                        heightInches = inches;
                    else
                    {
                        MessageBox.Show("Enter a valid number.");
                        return;
                    }
                }
                else if (unit == "Feet/Inches")
                {
                    if (int.TryParse(TxtHeightPrimary.Text, out int feet) && double.TryParse(TxtHeightSecondary.Text, out double inches))
                        heightInches = Conversions.FeetInchesToInches(feet, inches);
                    else
                    {
                        MessageBox.Show("Enter valid feet and inches.");
                        return;
                    }
                }

                _newUser.HeightInches = heightInches;
            }
            else
            {
                MessageBox.Show("Select a unit for height.");
                return;
            }

            StepHeight.Visibility = Visibility.Collapsed;
            StepWeight.Visibility = Visibility.Visible;
        }

        private void NextWeight(object sender, RoutedEventArgs e)
        {
            if (CmbWeightUnit.SelectedItem is ComboBoxItem selected)
            {
                var unit = selected.Content?.ToString() ?? "";
                double weightLbs = 0;

                if (double.TryParse(TxtWeight.Text, out double entered))
                {
                    weightLbs = unit == "Kilograms" ? Conversions.KgToLbs(entered) : entered;
                }
                else
                {
                    MessageBox.Show("Enter a valid number.");
                    return;
                }

                _newUser.WeightHistory.Add(new WeightEntry
                {
                    Date = DateTime.Now,
                    WeightLbs = weightLbs
                });
            }
            else
            {
                MessageBox.Show("Select a unit for weight.");
                return;
            }

            StepWeight.Visibility = Visibility.Collapsed;
            StepGoals.Visibility = Visibility.Visible;
        }

        private void AddGoal(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtGoalDescription.Text) && !string.IsNullOrWhiteSpace(TxtGoalTimeframe.Text))
            {
                var goal = new Goal
                {
                    Description = TxtGoalDescription.Text.Trim(),
                    Timeframe = TxtGoalTimeframe.Text.Trim()
                };

                _goals.Add(goal);
                GoalsList.Items.Add($"{goal.Description} ({goal.Timeframe})");

                TxtGoalDescription.Text = "";
                TxtGoalTimeframe.Text = "";
            }
            else
            {
                MessageBox.Show("Enter both description and timeframe.");
            }
        }

        private void NextGoals(object sender, RoutedEventArgs e)
        {
            _newUser.Goals = _goals;

            StepGoals.Visibility = Visibility.Collapsed;
            StepPictures.Visibility = Visibility.Visible;
        }

        private void UploadFrontal(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Select Frontal picture"
            };

            if (dlg.ShowDialog() == true)
            {
                string uploadsFolder = @"A:\DotNetApps\FitLab\FitLab\Uploads";
                string destFileName = System.IO.Path.Combine(
                    uploadsFolder,
                    $"{Guid.NewGuid()}_{System.IO.Path.GetFileName(dlg.FileName)}");

                System.IO.File.Copy(dlg.FileName, destFileName, overwrite: true);

                _newUser.BeforePictures.Add(new BeforePicture
                {
                    FilePath = destFileName,
                    DateTaken = DateTime.Now,
                    Type = "Frontal"
                });

                ImgFrontalPreview.Source = new BitmapImage(new Uri(destFileName));
                TxtFrontalFileName.Text = System.IO.Path.GetFileName(destFileName);
            }
        }

        private void UploadLeft(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Select Left picture"
            };

            if (dlg.ShowDialog() == true)
            {
                string uploadsFolder = @"A:\DotNetApps\FitLab\FitLab\Uploads";
                string destFileName = System.IO.Path.Combine(
                    uploadsFolder,
                    $"{Guid.NewGuid()}_{System.IO.Path.GetFileName(dlg.FileName)}");

                System.IO.File.Copy(dlg.FileName, destFileName, overwrite: true);

                _newUser.BeforePictures.Add(new BeforePicture
                {
                    FilePath = destFileName,
                    DateTaken = DateTime.Now,
                    Type = "Left"
                });

                ImgLeftPreview.Source = new BitmapImage(new Uri(destFileName));
                TxtLeftFileName.Text = System.IO.Path.GetFileName(destFileName);
            }
        }

        private void UploadRight(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Select Right picture"
            };

            if (dlg.ShowDialog() == true)
            {
                string uploadsFolder = @"A:\DotNetApps\FitLab\FitLab\Uploads";
                string destFileName = System.IO.Path.Combine(
                    uploadsFolder,
                    $"{Guid.NewGuid()}_{System.IO.Path.GetFileName(dlg.FileName)}");

                System.IO.File.Copy(dlg.FileName, destFileName, overwrite: true);

                _newUser.BeforePictures.Add(new BeforePicture
                {
                    FilePath = destFileName,
                    DateTaken = DateTime.Now,
                    Type = "Right"
                });

                ImgRightPreview.Source = new BitmapImage(new Uri(destFileName));
                TxtRightFileName.Text = System.IO.Path.GetFileName(destFileName);
            }
        }

        private void UploadBack(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Select Back picture"
            };

            if (dlg.ShowDialog() == true)
            {
                string uploadsFolder = @"A:\DotNetApps\FitLab\FitLab\Uploads";
                string destFileName = System.IO.Path.Combine(
                    uploadsFolder,
                    $"{Guid.NewGuid()}_{System.IO.Path.GetFileName(dlg.FileName)}");

                System.IO.File.Copy(dlg.FileName, destFileName, overwrite: true);

                _newUser.BeforePictures.Add(new BeforePicture
                {
                    FilePath = destFileName,
                    DateTaken = DateTime.Now,
                    Type = "Back"
                });

                ImgBackPreview.Source = new BitmapImage(new Uri(destFileName));
                TxtBackFileName.Text = System.IO.Path.GetFileName(destFileName);
            }
        }

        private void FinishIntake(object sender, RoutedEventArgs e)
        {
            _newUser.CompletedIntake = true;

            var db = new LocalDatabaseService();
            db.SaveUser(_newUser);
            db.SaveCurrentUserId(_newUser.Id);

            Debug.WriteLine($"[FitLab] New user created with Id: {_newUser.Id}");
            Debug.WriteLine($"[FitLab] CurrentUserId saved to LiteDB: {_newUser.Id}");

            ((MainWindow)Application.Current.MainWindow).Header.Visibility = Visibility.Visible;
            NavigationService.Navigate(new HomePage());
        }

    }
}
