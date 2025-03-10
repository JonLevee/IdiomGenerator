using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace IdiomGenerator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private IdiomParser _idiomParser;
    private PartsOfSpeechData partsOfSpeechData;
    public MainWindow()
    {
        InitializeComponent();
        var searchText = SearchText.Text;
        var loader = new PartsOfSpeechLoader();
        partsOfSpeechData = loader.Load();

        var idiomParser = _idiomParser ??= new IdiomParser();
        var results = idiomParser.Search(searchText);
    }

    private void SearchButtom_Click(object sender, RoutedEventArgs e)
    {
        var searchText = SearchText.Text;
        var idiomParser = _idiomParser ??= new IdiomParser();

    }
}

/// <summary>
/// give me a list of 200 idioms in json format, with each idiom also showing the part of speech of each word
/// </summary>
public class IdiomParser
{
    private IdiomData[] data;
    Dictionary<string, List<string>> partsOfSpeech;
    public IdiomParser()
    {
        var idiomFile = Path.Combine(Directory.GetCurrentDirectory(), @"resources\idioms.json");
        var options = new JsonSerializerOptions
        {
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            PropertyNameCaseInsensitive = true,
        };
        var idiomArray = JsonSerializer.Deserialize<IdiomArray>(File.ReadAllText(idiomFile), options);
        Contract.Assert(idiomArray != null, "idiomArray != null");
        data = idiomArray.Idioms;
        Contract.Assert(data != null, "data != null");
        partsOfSpeech = data
            .SelectMany(idiom => idiom.Words)
            .GroupBy(word => word.PartOfSpeech)
            .ToDictionary(group => group.Key, group => group.Select(word => word.Word).Distinct().Order().ToList());
    }

    internal object Search(string searchText)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// https://github.com/verachell/English-word-lists-parts-of-speech-approximate
/// </summary>
public class PartsOfSpeechLoader
{
    internal PartsOfSpeechData? Load()
    {
        var data = new PartsOfSpeechData();
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\English-word-lists-parts-of-speech-approximate");
        foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
        {
            var name = Path.GetFileName(file);
            switch (name)
            {
                case "mostly-noun-phrases.txt":
                    data.Items.AddRange(File.ReadAllLines(file).Select(line => new PartsOfSpeechItem("noun", "phrase", line)));
                    break;
                case "mostly-nouns-ment.txt":
                    data.Items.AddRange(File.ReadAllLines(file).Select(line => new PartsOfSpeechItem("noun", "ment", line)));
                    break;
                case "mostly-nouns.txt":
                    data.Items.AddRange(File.ReadAllLines(file).Select(line => new PartsOfSpeechItem("noun", line)));
                    break;
                case "mostly-plural-nouns.txt":
                    data.Items.AddRange(File.ReadAllLines(file).Select(line => new PartsOfSpeechItem("noun", "plural", line)));
                    break;
                case "ly-adverbs.txt":
                    data.Items.AddRange(File.ReadAllLines(file).Select(line => new PartsOfSpeechItem("adverb", "ly", line)));
                    break;
                case "mostly-adjectives.txt":
                    data.Items.AddRange(File.ReadAllLines(file).Select(line => new PartsOfSpeechItem("adjective", "ly", line)));
                    break;
                case "mostly-adverbs.txt":
                    data.Items.AddRange(File.ReadAllLines(file).Select(line => new PartsOfSpeechItem("adverb", line)));
                    break;
                case "mostly-conjunctions.txt":
                    data.Items.AddRange(File.ReadAllLines(file).Select(line => new PartsOfSpeechItem("conjunction", line)));
                    break;
                case "mostly-interjections.txt":
                    data.Items.AddRange(File.ReadAllLines(file).Select(line => new PartsOfSpeechItem("interjection", line)));
                    break;
                case "mostly-prepositions.txt":
                    data.Items.AddRange(File.ReadAllLines(file).Select(line => new PartsOfSpeechItem("preposition", line)));
                    break;
                case "safe-american-british-english-all-words.txt":
                    break;
                case "mostly-verbs-infinitive.txt":
                    data.Items.AddRange(File.ReadAllLines(file).Select(line => new PartsOfSpeechItem("verb", "infinitive", line)));
                    break;
                case "mostly-verbs-past-tense.txt":
                    data.Items.AddRange(File.ReadAllLines(file).Select(line => new PartsOfSpeechItem("verb", "past tense", line)));
                    break;
                case "mostly-verbs-present-tense.txt":
                    data.Items.AddRange(File.ReadAllLines(file).Select(line => new PartsOfSpeechItem("verb", "present tense", line)));
                    break;
                case "transitive-past-tense.txt":
                    data.Items.AddRange(File.ReadAllLines(file).Select(line => new PartsOfSpeechItem("verb", "transitive past tense", line)));
                    break;
                case "transitive-present-tense.txt":
                    data.Items.AddRange(File.ReadAllLines(file).Select(line => new PartsOfSpeechItem("verb", "transitive present tense", line)));
                    break;
                default:
                    throw new InvalidOperationException(name);
            }
        }
        return data;
    }
}

public class PartsOfSpeechData
{
    public Dictionary<string, int> SortOrder = new Dictionary<string, int>
    {
        { "noun", 1 },
        { "verb", 2 },
        { "adjective", 3 },
        { "adverb", 4 },
        { "conjunction", 5 },
        { "interjection", 6 },
        { "preposition", 7 },
    };
    public List<PartsOfSpeechItem> Items { get; } = new List<PartsOfSpeechItem>();
    public PartsOfSpeechData()
    {

    }
}

public class PartsOfSpeechItem
{
    public PartsOfSpeechItem(string wordType, string word) : this(wordType, wordType, word)
    {

    }
    public PartsOfSpeechItem(string wordType, string wordSubType, string word)
    {
        WordType = wordType;
        WordSubType = wordSubType;
        Word = word;
    }

    public string WordType { get; }
    public string WordSubType { get; }
    public string Word { get; }
}

public class IdiomArray
{
    public required IdiomData[] Idioms { get; set; }

}

public class IdiomData
{
    public required string Idiom { get; set; }
    public required IdiomWord[] Words { get; set; }
}

public class IdiomWord
{
    public required string Word { get; set; }
    [JsonPropertyName("part_of_speech")]
    public required string PartOfSpeech { get; set; }
}
