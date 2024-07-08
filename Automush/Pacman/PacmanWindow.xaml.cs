using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Automush.Pacman;

/// <summary>
/// Interaction logic for PacmanWindow.xaml
/// </summary>
public partial class PacmanWindow : Window
{
    internal Game game;
    private readonly DispatcherTimer timer;

    public PacmanWindow()
    {
        InitializeComponent();

        game = new Game();

        timer = new DispatcherTimer();

        timer.Tick += new EventHandler(PacmanTimerTick);

        timer.Interval = new TimeSpan(0, 0, 0, 0, 500);

        timer.Start();

        KeyDown += new KeyEventHandler(PacmanKeyDown);
    }

    void PacmanKeyDown(object sender, KeyEventArgs e) => game.KeyPressed(e);

    private void PacmanTimerTick(object? sender, EventArgs e)
    {
        game.PlayRound();
        if (!game.active)
            timer.Stop();

        Paint();
        CommandManager.InvalidateRequerySuggested();
    }

    internal void Paint()
    {
        canvas.Children.Clear();

        var map = game.Map;

        int BLOCK_WIDTH = 15;

        Rectangle background = new();
        SolidColorBrush brush1 = new()
        {
            Color = Color.FromRgb(0, 0, 0)
        };
        background.Fill = brush1;
        background.StrokeThickness = 2;
        background.Stroke = Brushes.Black;
        background.Width = BLOCK_WIDTH * 28;
        background.Height = BLOCK_WIDTH * 29;
        Canvas.SetLeft(background, 0);
        Canvas.SetTop(background, 0);
        canvas.Children.Add(background);

        for (int i = 0; i < game.sizeX; i++)
            for (int j = 0; j < game.sizeY; j++)
            {
                Rectangle r = new();
                SolidColorBrush brush2 = new();
                if (map[i, j].Entity != null)
                {
                    brush2.Color = map[i, j].Entity!.type switch
                    {
                        EntityType.Pacman => ColorDictionary.Pacman,
                        _ => ColorDictionary.Ghost
                    };
                }
                else
                {
                    brush2.Color = map[i, j].Type switch
                    {
                        BlockType.Wall => ColorDictionary.Wall,
                        BlockType.Point => ColorDictionary.Point,
                        _ => ColorDictionary.Empty,
                    };
                }
                r.Fill = brush2;
                r.StrokeThickness = 1;
                r.Stroke = Brushes.Black;
                r.Width = BLOCK_WIDTH;
                r.Height = BLOCK_WIDTH;
                Canvas.SetLeft(r, j * BLOCK_WIDTH);
                Canvas.SetTop(r, i * BLOCK_WIDTH);
                canvas.Children.Add(r);
            }
        if (!game.active)
        {
            TextBlock textBlock = new()
            {
                Text = "Game Over",
                FontSize = 24,
                Foreground = Brushes.Red
            };
            Canvas.SetLeft(textBlock, 150);
            Canvas.SetTop(textBlock, 500);
            canvas.Children.Add(textBlock);
        }
    }
}

internal class ColorDictionary
{
    internal static Color Pacman { get { return Color.FromRgb(255, 255, 0); } } // Yellow
    internal static Color Ghost { get { return Color.FromRgb(0, 0, 255); } } // Blue
    internal static Color Wall { get { return Color.FromRgb(255, 255, 255); } } // White
    internal static Color Point { get { return Color.FromRgb(255, 0, 0); } } // Red
    internal static Color Empty { get { return Color.FromRgb(0, 255, 0); } } // Green
}