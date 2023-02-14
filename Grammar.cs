using System.Collections.Generic;

namespace FLFA;
public class Grammar {
    Vocabulary vocabulary { get; init; }
    // Dictionary<char, string> productions { get; init; }
    List<KeyValuePair<char, string>> productions { get; init; }
    char startString { get; init; } = 'S';
    public int maxLength { get; set; } = 20;

    private Random rnd = new Random();

    public Grammar (
        char[] Vn,
        char[] Vt,
        List<KeyValuePair<char, string>> P
    ) {
        vocabulary = new Vocabulary();
        vocabulary.nonTerminal = Vn;
        vocabulary.terminal = Vt.Where(c => c != startString).ToArray();

        productions = P;
    }

    public string generateString() {
        string result = startString.ToString();
        bool breakFlag = false;

        while (containsNonTerminals(result) && !breakFlag) {
            for (int i = 0; i < result.Length; i++) {
                if (result.Length >= maxLength) {
                    breakFlag = true;
                    break;
                }

                if (vocabulary.nonTerminal.Contains(result[i])) {
                    result = result.Replace(result[i].ToString(), getProduction(result[i]));
                }
            }
        }

        return result;
    }

    public List<string> generateStrings(int n) {
        List<string> res = new List<string>(){};

        for (int i = 0; i < n; i++)
            res.Add(generateString());
        
        return res;
    }

    private bool containsNonTerminals(string s) {
        return s.Intersect(vocabulary.nonTerminal).Any();
    }

    private string getProduction (char c) {
        List<string> matches = productions.Where(p => p.Key == c).Select(p => p.Value).ToList();
        
        return matches[rnd.Next(0, matches.Count)].ToString();
    }

    public FiniteAutomaton toFiniteAutomaton() {
        char[] alphabet = vocabulary.terminal!;

        return new FiniteAutomaton(alphabet, productions);
    }
}

public record Vocabulary {
    public char[] terminal = {};
    public char[] nonTerminal = {};
}