using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
/// 


public partial class MainWindow : Window
{
    public ObservableCollection<string> Files { get; set; } = new ObservableCollection<string>();
    public bool isAnalyze = false;

    private int progress;
    private string symbolsCount;
    private string wordsCount;
    private string sentencesCount;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void PropertyChange<T>(out T field, T value, [CallerMemberName] string propName = "")
    {
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public int Progress
    {
        get { return progress; }
        set
        {
            progress = value;
            PropertyChange(out this.progress, value);
        }
    }
    public string SymbolsCount
    {
        get { return symbolsCount; }
        set
        {
            symbolsCount = value;
            PropertyChange(out this.symbolsCount, value);
        }
    }

    public string WordsCount
    {
        get { return wordsCount; }
        set
        {
            wordsCount = value;
            PropertyChange(out this.wordsCount, value);
        }
    }

    public string SentencesCount
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
        InitializeComponent();
        this.DataContext = this;
    }

    private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
    {
        if (isAnalyze) return;
        isAnalyze = true;
        var thread = new Thread(() =>
            {
                using (var reader = new StreamReader("Assets/ipsum.txt"))
                {
                    var content = reader.ReadToEnd();
                    var analyzer = new Analyzer(content);


                    int totalSymbols = content.Length;
                    int totalWords = content.Split().Count();
                    int totalSentences = content.Split(separator: new char[] { '.', ',', '?', '!' }).Count();

                    int symbolsProcessed = 0;
                    int wordsProcessed = 0;
                    int sentencesProcessed = 0;

                    while (!reader.EndOfStream && isAnalyze)
                    {
                        var line = reader.ReadLine();

                        symbolsProcessed += analyzer.GetSymbolsCount(line);
                        wordsProcessed += analyzer.GetWordsCount(line);
                        sentencesProcessed += analyzer.GetSentencesCount(line);

                        SymbolsCount = $"{symbolsProcessed}/{totalSymbols}";
                        WordsCount = $"{wordsProcessed}/{totalWords}";
                        SentencesCount = $"{sentencesProcessed}/{totalSentences}";

                        Progress = (int)(((double)symbolsProcessed / totalSymbols) * 100);

                        Thread.Sleep(100);
                    }

                    isAnalyze = false;
                }
            
            });
        thread.Start();

    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {

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