using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;

namespace Lab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private CancellationTokenSource cts;
        private double a = 0.2;
        private double f = 0.2;
        private double m = 10;
        private double x = 0.40764;
        private double y = 0.02079;
        private double z = -0.27351;
        private int numOfIterations = 1000;
        private int iterationNumber = 0;
        private bool isTaskExecuting = false;
        private SeriesCollection seriesCollection = new SeriesCollection();

        public MainWindow()
        {
            InitializeComponent();

            using var db = new CalculationContext();
            Calculations = new ObservableCollection<Calculation>(db.Calculations);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public double A
        {
            get => a;
            set
            {
                if (a != value)
                {
                    a = value;
                    OnPropertyChanged();
                }
            }
        }

        public double F
        {
            get => f;
            set
            {
                if (f != value)
                {
                    f = value;
                    OnPropertyChanged();
                }
            }
        }

        public double M
        {
            get => m;
            set
            {
                if (m != value)
                {
                    m = value;
                    OnPropertyChanged();
                }
            }
        }

        public double X
        {
            get => x;
            set
            {
                if (x != value)
                {
                    x = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Y
        {
            get => y;
            set
            {
                if (y != value)
                {
                    y = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Z
        {
            get => z;
            set
            {
                if (z != value)
                {
                    z = value;
                    OnPropertyChanged();
                }
            }
        }

        public int NumOfIterations
        {
            get => numOfIterations;
            set
            {
                if (numOfIterations != value && value > 0)
                {
                    numOfIterations = value;
                    OnPropertyChanged();
                }
            }
        }

        public int IterationNumber
        {
            get => iterationNumber;
            set
            {
                if (iterationNumber != value)
                {
                    iterationNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Point> Points { get; set; } = new ObservableCollection<Point>();

        public ObservableCollection<Calculation> Calculations { get; set; } = new ObservableCollection<Calculation>();

        public SeriesCollection SeriesCollection
        {
            get => seriesCollection;
            set
            {
                seriesCollection = value;
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            if (isTaskExecuting && (!cts?.IsCancellationRequested ?? false))
            {
                cts?.Cancel();
            }
        }

        private static Point[]? GetCalculationFromDb(Calculation calculation)
        {
            using var db = new CalculationContext();
            var calc = db.Calculations
                .Where(x => x.X0 == calculation.X0 && x.Y0 == calculation.Y0 && x.A == calculation.A && x.F == calculation.F && x.M == calculation.M)
                .Include(x => x.Data)
                .FirstOrDefault();
            if (calc != null)
            {
                var points = new Point[calc.Data.X.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = new Point(calc.Data.X[i], calc.Data.Y[i], calc.Data.Z[i]);
                }

                return points;
            }

            return null;
        }

        private static void SaveCalculationToDb(Point[] points, Calculation calculation)
        {
            using var db = new CalculationContext();
            var calculationFromDb = db.Calculations.Where(c => c.Id == calculation.Id).FirstOrDefault();

            if (calculationFromDb == null)
            {
                var newData = new CalculationData
                {
                    X = points.Select((p) =>p.X).ToArray(),
                    Y = points.Select((p) => p.Y).ToArray(),
                    Z = points.Select((p) => p.Z).ToArray()
                };

                var newCalculaion = new Calculation
                {
                    X0 = calculation.X0,
                    Y0 = calculation.Y0,
                    A = calculation.A,
                    F = calculation.F,
                    M = calculation.M,
                    NumOfIterations = calculation.NumOfIterations,
                    Data = newData
                };

                db.Calculations.Add(newCalculaion);
                db.Data.Add(newData);
            }
            else if (points != null)
            {
                calculation.Data.X = points.Select((p) => p.X).ToArray();
                calculation.Data.Y = points.Select((p) => p.Y).ToArray();
                calculation.Data.Z = points.Select((p) => p.Z).ToArray();
            }

            db.SaveChanges();
        }

        private async void StartButtonClick(object sender, RoutedEventArgs e)
        {
            if (isTaskExecuting)
            {
                return;
            }

            SeriesCollection.Clear();
            SeriesCollection.Add(new LineSeries() { Values = new ChartValues<ObservablePoint>(), PointGeometry = null, Fill = Brushes.Transparent });
            IterationNumber = 1;
            isTaskExecuting = true;
            cts = new CancellationTokenSource();
            Points.Clear();
            var dt = 0.0001;
            double x0, y0, z0, x1, y1, z1;

            x0 = X;
            y0 = Y;
            z0 = Z;

            var point0 = new Point(x0, y0, z0);
            var cal = new Calculation() { X0 = point0.X, Y0 = point0.Y, A = this.A, F = this.F, M = this.M, NumOfIterations = this.NumOfIterations };
            var points = GetCalculationFromDb(cal);

            if (points != null)
            {
                foreach (var point in points)
                {
                    Points.Add(point);
                    IterationNumber++;
                    SeriesCollection[0].Values.Add(new ObservablePoint() { X = point.X, Y = point.Y });
                }

                return;
            }

            try
            {
                
                Points.Add(point0);
                var pointsToDraw = new List<Point>();

                await Task.Factory.StartNew(() =>
                {
                    for (int i = 1; i < NumOfIterations; i++)
                    {
                        x1 = x0 + (-y0 - z0) * dt;
                        y1 = y0 + (x0 + A * y0) * dt;
                        z1 = z0 + (F + z0 * (x0 - M)) * dt;

                        x0 = x1;
                        y0 = y1;
                        z0 = z1;

                        var point = new Point(x0, y0, z0);
                        try
                        {
                            Dispatcher.Invoke(() =>
                            {
                                Points.Add(point);
                                IterationNumber++;
                                SeriesCollection[0].Values.Add(new ObservablePoint() { X = point.X, Y = point.Y });
                            },

                            System.Windows.Threading.DispatcherPriority.Background, cts.Token);

                        }
                        catch
                        {
                            cts?.Dispose();
                            return;
                        }
                    }

                }, cts.Token);

                SaveCalculationToDb(Points.ToArray(), cal);
            }
            catch (TaskCanceledException)
            {
                cts?.Dispose();
            }
            finally
            {
                isTaskExecuting = false;
            }
        }

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
