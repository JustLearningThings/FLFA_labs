using System.Data;

namespace FLFA;

public class CNF
{
    public const string Epsilon = "ε";
    private ProductionRules Rules { get; init; }
    private int _nameIndex = 1; // used in helper functions to give proper naming to keys

    public CNF(
        string[] vn,
        char[] vt,
        List<KeyValuePair<string, string>> p
    )
    {
        Vocabulary v = new Vocabulary
        {
            NonTerminal = vn,
            Terminal = vt
        };

        Rules = new ProductionRules(p, v);
    }

    public void Normalize()
    {
        ResolveStart();
        EliminateEpsilonRules();
        EliminateNonSolitaryTerminals();
        ResolveNonTerminalCoupling();
        EliminateUnitRules();
        
        SortRules();
    }

    public void PrintGrammar()
    {
        Console.WriteLine("P = {");
        foreach (var production in Rules.Productions)
        {
            Console.WriteLine(production.Key + " --> " + production.Value);
        }

        Console.WriteLine("}");
    }

    // --|| FUNCTIONS CORRESPONDING TO STEPS IN THE ALGORITHM ||--
    private void ResolveStart()
    {
        if (Rules.IsStartOnRightHandSide())
        {
            Rules.Productions.Add(new KeyValuePair<string, string>("S'", "S"));
        }
    }

    private void EliminateEpsilonRules()
    {
        List<KeyValuePair<string, string>> productionsToRemove = new();
        
        foreach (var production in Rules.Productions)
        {
            if (Rules.HasEpsilon(production.Value))
            {
                productionsToRemove.Add(production);
            }
        }

        foreach (var production in productionsToRemove)
        {
            Rules.Productions.Remove(production);
        }
    }

    // method to ensure we have only 2 non-terminals, no more, no less
    private void ResolveNonTerminalCoupling()
    {
        // temporary lists to deal with the immutability of our dictionary-like list elements
        List<KeyValuePair<string, string>> productionsToRemove = new();
        List<KeyValuePair<string, string>> productionsToAdd = new();

        foreach (var production in Rules.Productions)
        {
            if (Rules.IsMoreThanTwoNonTerminals(production.Value))
            {
                productionsToRemove.Add(production);

                productionsToAdd.Add(new KeyValuePair<string, string>(
                    production.Key,
                    GetComposeProductionKey(production.Value) + production.Key + _nameIndex));
                
                Console.WriteLine("------- " + GetComposeProductionKey(production.Value) + production.Key + _nameIndex);
            }
        }

        foreach (var production in productionsToAdd)
        {
            Rules.Productions.Add(production);
        }

        foreach (var production in productionsToRemove)
        {
            _nameIndex = 1;

            CreateTransitiveRules(production.Key, production.Value);
            Rules.Productions.Remove(production);
        }
    }

    private string GetComposeProductionKey(string key) => Char.IsDigit(key[1]) ? key[0].ToString() + key[1] : key[0].ToString();
    private string TrimCompositeProductionRule(string s) => Char.IsDigit(s[1]) ? s[2..^0] : s;

    private void EliminateNonSolitaryTerminals()
    {
        foreach (char c in Rules.Vocabulary.Terminal)
        {
            ReplaceNonSolitaryTerminal(c);
        }

        _nameIndex = 1;
    }

    private void ReplaceNonSolitaryTerminal(char terminal)
    {
        if (!Rules.KeyExists("N" + _nameIndex.ToString()))
        {
            // temporary variables to deal with the immutability of our Dictionary-like list elements
            List<KeyValuePair<string, string>> productionsToRemove = new();
            List<KeyValuePair<string, string>> productionsToAdd = new();

            foreach (var production in Rules.Productions)
            {
                if (production.Value.Length > 1 && Rules.HasTerminal(production.Value, terminal))
                {
                    productionsToRemove.Add(production);
                    productionsToAdd.Add(new KeyValuePair<string, string>(
                        production.Key,
                        production.Value.Replace(terminal.ToString(), "N" + _nameIndex.ToString())
                    ));
                }
            }

            foreach (var production in productionsToRemove)
            {
                Rules.Productions.Remove(production);
            }
        
            foreach (var production in productionsToAdd)
            {
                Rules.Productions.Add(production);
            }
        
            // add new rule for the termimal
            Rules.Productions.Add(new("N" + _nameIndex.ToString(), terminal.ToString()));

            _nameIndex++;
        }
    }

    private void CreateTransitiveRules(string key, string s)
    {
        if (s.Length == 2)
        {
            Rules.Productions.Add(new KeyValuePair<string, string>(key, s));
            
            return;
        }
        
        Rules.Productions.Add(new KeyValuePair<string, string>(key + _nameIndex, GetComposeProductionKey(s) + (key + 1)));

        _nameIndex++;
        
        CreateTransitiveRules(key, TrimCompositeProductionRule(s)[1..^0]);
    }

    private void EliminateUnitRules()
    {
        // temporary lists to deal with the immutability of our dictionary-like list elements
        List<KeyValuePair<string, string>> productionsToRemove = new();
        List<KeyValuePair<string, string>> productionsToAdd = new();

        foreach (var production in Rules.Productions)
        {
            if (production.Value == "S")
                continue;

            bool needsRemoval = false;
            
            foreach (var prod in Rules.Productions)
            {
                if (prod.Equals(production))
                    continue;

                if (prod.Key == production.Value)
                {
                    Console.WriteLine("$$$$$ " + prod.Key);
                    productionsToAdd.Add(new KeyValuePair<string, string>(
                        production.Key,
                        prod.Value
                        ));

                    needsRemoval = true;
                }
            }
            
            if (needsRemoval)
                productionsToRemove.Add(production);
        }

        // add productions
        foreach (var production in productionsToAdd)
        {
            Console.WriteLine("Add: " + production.Value);
            Rules.Productions.Add(production);
        }
        
        // remove productions
        foreach (var production in productionsToRemove)
        {
            Console.WriteLine("Remove: " + production.Value);
            Rules.Productions.Remove(production);
        }
    }
    
    private void SortRules()
    {
        Rules.Productions = Rules.Productions.OrderBy(rule => rule.Key).ToList();
    }
}

public record Vocabulary {
    public char[] Terminal = {};
    public string[] NonTerminal = {};

    public Vocabulary(char[] term, string[] nonTerm) {
        Terminal = term;
        NonTerminal = nonTerm;
    }

    public Vocabulary() {}
}

class ProductionRules
{
    public List<KeyValuePair<string, string>> Productions;
    public Vocabulary Vocabulary;

    public ProductionRules(List<KeyValuePair<string, string>> productions, Vocabulary vocabulary)
    {
        Productions = productions;
        Vocabulary = vocabulary;
    }

    public bool IsStartOnRightHandSide()
    {
        foreach (var production in Productions)
        {
            if (production.Value.Contains('S'))
            {
                return true;
            }
        }

        return false;
    }
    
    public bool IsOneTerminal(string s) => s.Length == 1 && Vocabulary.Terminal.Contains(s[0]);

    public bool IsMoreThanTwoNonTerminals(string s)
    {
        int count = 0;
        
        if (s.Length < 3)
        {
            return false;
        }

        foreach (char c in s)
        {
            count++;

            if (Char.IsDigit(c))
            {
                count--;
            }

                if (c == 'N' || Char.IsDigit(c))
            {
                continue;
            }
            
            if (Vocabulary.Terminal.Contains(c))
            {
                return false;
            }
        }

        return count > 2;
    }

    public bool IsNonTerminal(char c) => Vocabulary.NonTerminal.Contains(c.ToString());
    
    public bool IsTwoNonTerminals(string s) =>
        s.Length == 2 &&
        Vocabulary.NonTerminal.Contains(s[0].ToString()) &&
        Vocabulary.NonTerminal.Contains(s[1].ToString());

    public bool HasEpsilon(string s) => s.Contains(CNF.Epsilon);
    
    public bool IsMixOfTerminalsAndNonTerminals(string s)
    {
        if (s.Length < 3)
        {
            return false;
        }
        
        bool hasTerminal = false,
            hasNonTerminal = false;

        foreach (char c in Vocabulary.Terminal)
        {
            if (s.Contains(c))
            {
                hasTerminal = true;
                
                break;
            }
        }
        
        foreach (string s_ in Vocabulary.NonTerminal)
        {
            if (s.Contains(s_))
            {
                hasNonTerminal = true;
                
                break;
            }
        }

        return hasTerminal && hasNonTerminal;
    }

    public bool KeyExists(string key)
    {
        foreach (var production in Productions)
        {
            if (production.Key == key)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsTerminal(char s) => Vocabulary.Terminal.Contains(s);

    public bool HasTerminal(string s, char c) => s.Contains(c);

    public void RemoveByProduction(KeyValuePair<string, string> production)
    {
        Productions.Remove(production);
    }

    public void AddProduction(KeyValuePair<string, string> production)
    {
        Productions.Add(production);
    }
    
    // used to differentiate non-terminals of multiple chars
    public string GetNonTerminalFromString(string s)
    {
        if (s[1] is >= '0' and <= '9')
        {
            return (s[0] + s[1]).ToString();
        }

        return s[0].ToString();
    }
}