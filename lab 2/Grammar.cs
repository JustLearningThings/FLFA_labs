using System.Collections.Generic;

namespace FLFA;
public class Grammar {
    Vocabulary vocabulary { get; init; }
    List<KeyValuePair<char, string>> productions { get; init; }
    char startString { get; init; } = 'S';
    public int maxLength { get; set; } = 20;

    private Random rnd = new Random();

    public ChomskyType type { get; init; } = ChomskyType.Type3;

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

    public ChomskyType classify() {
        ChomskyType returnType = ChomskyType.Type3;

        // type 3 conditions
        foreach (var production in productions)
            if (
                production.Value.Length > 2 ||
                (production.Value.Length == 1 && !(vocabulary.terminal.Contains(production.Value[0]))) || // terminal ending
                (production.Value.Length == 2 && 
                    !((vocabulary.nonTerminal.Contains(production.Value[0]) && vocabulary.terminal.Contains(production.Value[1])) ||
                     (vocabulary.terminal.Contains(production.Value[0]) && vocabulary.nonTerminal.Contains(production.Value[1])))
                )
            ) {
                    
                returnType = ChomskyType.Type2;

                break;
            }        

        if (returnType == ChomskyType.Type3)
            return returnType;

        // type 2 conditions
        foreach (var production in productions)
            if (
                !production.Value.Intersect(vocabulary.nonTerminal).Any() ||
                !production.Value.Intersect(vocabulary.terminal).Any()
                || production.Value.Any(s => !(vocabulary.nonTerminal.Contains(s) || vocabulary.terminal.Contains(s)))
            ) {
                returnType = ChomskyType.Type1;

                break;
            }
        
        if (returnType == ChomskyType.Type2)
            return returnType;

        // type 1 conditions
        foreach (var production in productions)
            if (
                production.Value.Count(s => vocabulary.nonTerminal.Contains(s)) == 0 || // there is a non-terminal
                !production.Key.ToString().Intersect(vocabulary.terminal).Any() // if there are terminal on left hand side
            ) {
                returnType = ChomskyType.Type0;

                break;
            }        

        return returnType;
    }
}

public record Vocabulary {
    public char[] terminal = {};
    public char[] nonTerminal = {};

    public Vocabulary(char[] term, char[] nonTerm) {
        terminal = term;
        nonTerminal = nonTerm;
    }

    public Vocabulary() {}
}

public enum ChomskyType {
    Type0 = 1,
    Type1 = 2,
    Type2 = 3,
    Type3 = 4,
}