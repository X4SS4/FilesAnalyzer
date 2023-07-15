using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
/// 


public partial class MainWindow : Window, INotifyPropertyChanged
{
    public ObservableCollection<string> Files { get; set; } = new ObservableCollection<string>();
    public bool isAnalyze = false;
    public CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void PropertyChange<T>(out T field, T value, [CallerMemberName] string? propName = "")
    {
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    private int progress;
    public int Progress
    {
        get { return progress; }
        set
        {
            progress = value;
            PropertyChange(out this.progress, value);
        }
    }
    private string? symbolsCount;
    public string? SymbolsCount
    {
        get { return symbolsCount; }
        set
        {
            symbolsCount = value;
            PropertyChange(out this.symbolsCount, value);
        }
    }

    private string? wordsCount;
    public string? WordsCount
    {
        get { return wordsCount; }
        set
        {
            wordsCount = value;
            PropertyChange(out this.wordsCount, value);
        }
    }

    private string? sentencesCount;
    public string? SentencesCount
    {
        get { return sentencesCount; }
        set
        {
            sentencesCount = value;
            PropertyChange(out this.sentencesCount, value);
        }
    }

    public MainWindow()
    {
        Files.Add("Assets/ipsum.txt");
        Files.Add("Assets/loreem.txt");
        Files.Add("Assets/SonO.json");
        InitializeComponent();
        this.DataContext = this;
    }

    private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
    {
        AnalyzeButton.IsEnabled = false;
        string? itemToRead = fileList.SelectedItem as string;
        if (isAnalyze || itemToRead is null) return;
        isAnalyze = true;
        CancellationToken cancellationToken = this.cancellationTokenSource.Token;
        await Task.Run(() => {
            using (var reader = new StreamReader(itemToRead)) {
                /*Ради скорости загрузки прогрессбара, но думаю это бессмысленно было создавать еще один поток*/
                var allContent = reader.ReadToEnd();
                Dispatcher.Invoke(() => LoadingPB.Maximum = allContent.Count());
                StringBuilder content = new StringBuilder();
                int charToWrite;
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                while ((charToWrite = reader.Read()) != -1 && !cancellationToken.IsCancellationRequested)
                {
                    var currentChar = (char)charToWrite;
                    Dispatcher.Invoke(() => Progress++);
                    content.Append(currentChar);
                    //Thread.Sleep(1);
                    /*Лучше было бы так */
                    //Dispatcher.Invoke(()=> LoadingPB.Maximum = LoadingPB.Maximum < Progress * 1.1    
                    //        ? LoadingPB.Maximum * 2
                    //        : LoadingPB.Maximum);                    
                }
                var analyzer = new Analyzer(content.ToString());
                SymbolsCount = $"{analyzer.GetSymbolsCount(content.ToString())}";
                WordsCount = $"{analyzer.GetWordsCount(content.ToString())}";
                SentencesCount = $"{analyzer.GetSentencesCount(content.ToString())}";
                Progress = 0;
                isAnalyze = false;
                cancellationTokenSource = new CancellationTokenSource();
            }
            
        });

        AnalyzeButton.IsEnabled = true;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
       if(isAnalyze) cancellationTokenSource?.Cancel();
    }
}

public class Analyzer
{
    private readonly Regex wordRegex;
    private readonly Regex sentenceRegex;

    public Analyzer(string content)
    {
        wordRegex = new Regex(@"\b\w+\b");
        sentenceRegex = new Regex(@"[.!?]+");
    }

    public int GetSymbolsCount(string line)
    {
        return line.Length;
    }

    public int GetWordsCount(string line)
    {
        return wordRegex.Matches(line).Count;
    }

    public int GetSentencesCount(string line)
    {
        return sentenceRegex.Matches(line).Count;
    }
}