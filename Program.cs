using FLFA;

char[] Vn = new char[] {'S', 'A', 'C', 'D'};
char[] Vt = new char[] {'a', 'b'};
List<KeyValuePair<char, string>> P = new List<KeyValuePair<char, string>> {
    new KeyValuePair<char, string>('S', "aA"),
    new KeyValuePair<char, string>('A', "bS"),
    new KeyValuePair<char, string>('A', "dD"),
    new KeyValuePair<char, string>('D', "bC"),
    new KeyValuePair<char, string>('C', "a"),
    new KeyValuePair<char, string>('C', "bA"),
    new KeyValuePair<char, string>('D', "aD"),
};

Grammar grammar = new Grammar(Vn, Vt, P);
    
Console.WriteLine("Generate 5 words in the language:");

List<string> generatedWords = grammar.generateStrings(5);

foreach(string word in generatedWords)
    Console.WriteLine(word);
Console.WriteLine();

Console.WriteLine("Checking if words are part of the language:");

FiniteAutomaton automaton = grammar.toFiniteAutomaton();
foreach(var w in generatedWords)
    checkWord(w);

void checkWord(string word) {
    // Console.WriteLine($"The word \"{word}\" is part of the language: {automaton.stringBelongToLanguage(word)}");
    bool isWord = automaton.stringBelongToLanguage(word);

    Console.Write("The word \"");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write(word);
    Console.ResetColor();
    Console.Write("\"");

    if (isWord) {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.Write(" is ");
    }
    else {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write(" is not ");
    }
    
    Console.ResetColor();
    Console.WriteLine("part of the language");
}

// these should be false:
checkWord("abd"); // case when there is no path from intermediate node to another done given input
checkWord("abad"); // case when didn't finish at final states