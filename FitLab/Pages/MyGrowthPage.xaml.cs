#nullable enable
using FitLab.Components;
using FitLab.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FitLab.Pages
{
    // This page displays the user's growth tracking data, right now only weight over time is displayed
    public partial class MyGrowthPage : Page
    {
        private readonly LocalDatabaseService _db = new LocalDatabaseService(); // database service to load user data
        private User? _user; // current user data, loaded on page load
        public MyGrowthPage()
        {
            InitializeComponent();
            Loaded += OnLoaded; // subscribe to the Loaded event to load user data
            CmbWeightUnit.SelectionChanged += (_, __) => RefreshWeightChart(); // refresh chart when unit changes
            CmbWeightBucket.SelectionChanged += (_, __) => RefreshWeightChart(); // refresh chart when bucket changes
            WeightChartCanvas.SizeChanged += (_, __) => RefreshWeightChart(); // refresh chart when canvas size changes
        }
        // This method is called when the page is loaded, it loads the first user and refreshes the weight chart
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _user = _db.LoadFirstUser(); // load the first user from the database
            if (_user == null)
            {
                TxtWeightEmpty.Text = "No user found."; // display message if no user is found
                TxtWeightEmpty.Visibility = Visibility.Visible; // show the empty message
                return;
            }
            RefreshWeightChart(); // refresh the weight chart with the loaded user data
        }
        // This method refreshes the weight chart based on the selected bucket and unit
        private void RefreshWeightChart()
        {
            if (_user == null) return; // if no user is loaded, do nothing
            var bucket = GetSelectedBucket(); // get the selected bucket (weekly or monthly)
            var points = GrowthTracking.Weight(_user, bucket); // get the weight tracking data for the user based on the selected bucket
            bool useKg = GetSelectedUnitIsKg(); // check if the selected unit is kilograms
            List<TimePoint> series = points.Select(p => new TimePoint // convert each point to a TimePoint with the selected unit
            {
                T = p.T, // keep the date as is
                V = useKg ? Conversions.LbsToKg(p.V) : p.V // convert the value to kg if the selected unit is kg, otherwise keep it in lbs
            }).ToList(); // create a list of TimePoints for the chart
            TxtWeightBucketLabel.Text = $"Bucket: {bucket}"; // update the bucket label with the selected bucket
            TxtWeightUnitLabel.Text = $"Unit: {(useKg ? "Kg" : "Lbs")}"; // update the unit label with the selected unit
            DrawLineSeries(WeightChartCanvas, series, yLabelFmt: useKg ? "0.0 kg" : "0.0 lb"); // draw the line series on the canvas with the selected unit format
        }
        // This method gets the selected bucket from the combo box, returning either weekly or monthly
        private Bucket GetSelectedBucket()
        {
            return CmbWeightBucket.SelectedIndex == 1 ? Bucket.Monthly : Bucket.Weekly; // return monthly bucket if the second item is selected, otherwise return weekly
        }
        private bool GetSelectedUnitIsKg() => CmbWeightUnit.SelectedIndex == 1; // returns true if the second item (Kg) is selected, otherwise false (Lbs)
        // This method draws the line series on the canvas using the provided data points
        private void DrawLineSeries(Canvas canvas, List<TimePoint> data, string yLabelFmt = "0.0")
        {
            canvas.Children.Clear(); // clear any existing children in the canvas before drawing
            if (data == null || data.Count == 0) // if no data points are provided, show an empty message
            {
                TxtWeightEmpty.Visibility = Visibility.Visible; // show the empty message
                return; 
            }
            TxtWeightEmpty.Visibility = Visibility.Collapsed; // hide the empty message if data is available
            double w = canvas.ActualWidth > 0 ? canvas.ActualWidth : (canvas.Width > 0 ? canvas.Width : 640); // get the width of the canvas, default to 640 if not set
            double h = canvas.ActualHeight > 0 ? canvas.ActualHeight : 240; // get the height of the canvas, default to 240 if not set
            const double padL = 56, padR = 12, padT = 12, padB = 30; // padding values for the chart
            double plotW = Math.Max(w - (padL + padR), 1); // calculate the width of the plot area, ensuring it's at least 1 pixel wide
            double plotH = Math.Max(h - (padT + padB), 1); // calculate the height of the plot area, ensuring it's at least 1 pixel high
            int n = data.Count; // number of data points
            double dx = plotW / Math.Max(n - 1, 1); // calculate the horizontal spacing between points, ensuring it's at least 1 pixel wide
            double minY = data.Min(p => p.V); // find the minimum value in the data points
            double maxY = data.Max(p => p.V); // find the maximum value in the data points
            if (Math.Abs(maxY - minY) < 1e-6) { maxY += 1; minY -= 1; } // if the range is too small, adjust it to ensure visibility
            double yPad = (maxY - minY) * 0.08; // calculate padding for the Y-axis to ensure visibility of the line
            minY -= yPad; maxY += yPad; // adjust the min and max Y values with padding
            Func<int, double> mapX = i => padL + i * dx; // function to map X index to pixel position
            Func<double, double> mapY = v => // function to map Y value to pixel position
            {
                double t = (v - minY) / (maxY - minY); // normalize the value to a range between 0 and 1
                return padT + (1 - t) * plotH; // calculate the pixel position based on the normalized value
            };
            var axis = new SolidColorBrush(Color.FromRgb(90, 90, 90)); // color for the axis lines
            canvas.Children.Add(new Line { X1 = padL, Y1 = padT + plotH, X2 = padL + plotW, Y2 = padT + plotH, Stroke = axis, StrokeThickness = 1 }); // X axis
            canvas.Children.Add(new Line { X1 = padL, Y1 = padT, X2 = padL, Y2 = padT + plotH, Stroke = axis, StrokeThickness = 1 }); // Y axis
            for (int i = 0; i <= 4; i++) // draw horizontal grid lines and labels
            {
                double vy = minY + (maxY - minY) * i / 4.0; // calculate the Y value for the grid line
                double y = mapY(vy); // map the Y value to pixel position
                canvas.Children.Add(new Line // draw the horizontal grid line
                {
                    X1 = padL,
                    Y1 = y,
                    X2 = padL + plotW,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromRgb(45, 45, 45)),
                    StrokeDashArray = new DoubleCollection { 2, 4 },
                    StrokeThickness = 1
                });
                var lbl = new TextBlock // create a label for the Y value
                {
                    Text = FormatY(vy, yLabelFmt),
                    Foreground = new SolidColorBrush(Color.FromRgb(185, 185, 185)),
                    FontSize = 11
                };
                Canvas.SetLeft(lbl, 6); // position the label to the left of the Y axis
                Canvas.SetTop(lbl, y - 8); // position the label vertically centered on the grid line
                canvas.Children.Add(lbl); // add the label to the canvas
            }
            var line = new Polyline //create a polyline to draw the line series
            {
                Stroke = new SolidColorBrush(Color.FromRgb(154, 66, 255)),
                StrokeThickness = 2
            };
            int labelStride = Math.Max(1, n / 8); // determine the stride for X labels, ensuring at least one label is shown
            for (int i = 0; i < n; i++) // iterate through each data point
            {
                var p = data[i]; // get the current data point
                var x = mapX(i); // map the index to pixel position on the X axis
                var y = mapY(p.V); // map the value to pixel position on the Y axis
                line.Points.Add(new System.Windows.Point(x, y)); // add the point to the polyline
                var dot = new Ellipse { Width = 4, Height = 4, Fill = new SolidColorBrush(Color.FromRgb(154, 66, 255)) }; // create a dot for the data point
                Canvas.SetLeft(dot, x - 2); // position the dot horizontally centered on the X position
                Canvas.SetTop(dot, y - 2); // position the dot vertically centered on the Y position
                canvas.Children.Add(dot); // add the dot to the canvas
                if (i % labelStride == 0) // check if this point should have a label
                {
                    var xlbl = new TextBlock // create a label for the X value
                    {
                        Text = p.T.ToString("MM/dd"),
                        Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170)),
                        FontSize = 11
                    };
                    Canvas.SetLeft(xlbl, x - 16); // position the label horizontally centered on the X position
                    Canvas.SetTop(xlbl, padT + plotH + 6); // position the label below the X axis
                    canvas.Children.Add(xlbl); // add the label to the canvas
                }
            }
            canvas.Children.Add(line); // add the polyline to the canvas
        }
        // This method formats the Y value for display, rounding it if necessary
        private static string FormatY(double v, string fmt)
        {
            if (Math.Abs(v) >= 100) return Math.Round(v).ToString(); // if the value is large, round it to the nearest integer
            return v.ToString(fmt); // otherwise, format it using the provided format string
        }
    }
}
