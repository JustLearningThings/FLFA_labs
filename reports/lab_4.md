# Lab 4 Report
### Variant 25

## Theory
In formal language theory, a context-free grammar, G, is said to be in Chomsky normal form (first described by Noam Chomsky) if all of its production rules are of the form:

    A → BC,   or
    A → a,   or
    S → ε,

where A, B, and C are nonterminal symbols, the letter a is a terminal symbol (a symbol that represents a constant value), S is the start symbol, and ε denotes the empty string. Also, neither B nor C may be the start symbol, and the third production rule can only appear if ε is in L(G), the language produced by the context-free grammar G.

Every grammar in Chomsky normal form is context-free, and conversely, every context-free grammar can be transformed into an equivalent one which is in Chomsky normal form and has a size no larger than the square of the original grammar's size.

## Objectives
1. Get familiar with parsing, what it is and how it can be programmed.
2. Get familiar with the concept of AST.
3. In addition to what has been done in the 3rd lab work do the following:
    - In case you didn't have a type that denotes the possible types of tokens you need to:
        -   Have a type TokenType (like an enum) that can be used in the lexical analysis to categorize the tokens.
        - Please use regular expressions to identify the type of the token.
    - Implement the necessary data structures for an AST that could be used for the text you have processed in the 3rd lab work.
    - Implement a simple parser program that could extract the syntactic information from the input text.

## Implementation
A special class for CNF was created that behaves as a Grammar, similar to the previous labs. The fields are the following:
```c#
public const string Epsilon = "ε";
private ProductionRules Rules { get; init; }
private int _nameIndex = 1; // used in helper functions to give proper naming to keys
```
It depends on two other classes. One of them `Vocabulary` is a class that stores information about terminal and non-terminal symbols.
The second class is the `Rules` class. It stores the production rules of the grammar and it implements methods that process them.

The order of the main operations in the `CNF` class is the one specified by the `Normalize` method:
```c#
public void Normalize()
    {
        ResolveStart();
        EliminateEpsilonRules();
        EliminateNonSolitaryTerminals();
        ResolveNonTerminalCoupling();
        EliminateUnitRules();
        
        SortRules();
    }
```
Each of the methods do their own part of normalization and modify the grammar in-place accordingly.

The way to use `CNF` is given by the following test:
```c#
using FLFA;

Console.OutputEncoding = System.Text.Encoding.UTF8;

List<KeyValuePair<string, string>> productions = new List<KeyValuePair<string, string>> {
    new KeyValuePair<string, string> ("S", "bA"),
    new KeyValuePair<string, string> ("S", "BC"),
    new KeyValuePair<string, string> ("A", "a"),
    new KeyValuePair<string, string> ("A", "aS"),
    new KeyValuePair<string, string> ("A", "bCaCa"),
    new KeyValuePair<string, string> ("B", "A"),
    new KeyValuePair<string, string> ("B", "bS"),
    new KeyValuePair<string, string> ("B", "bCAa"),
    new KeyValuePair<string, string> ("C", CNF.Epsilon),
    new KeyValuePair<string, string> ("C", "AB"),
    new KeyValuePair<string, string> ("D", "AB"),
};
char [] terminals = new[] { 'a', 'b' };
string [] nonTerminals = new[] { "S", "A", "B", "C" };

CNF grammar = new CNF(nonTerminals, terminals, productions);

grammar.PrintGrammar();
grammar.Normalize();
grammar.PrintGrammar();
```
It outputs the grammar before and after the normalization:
```txt
P = {
S --> bA
S --> BC
A --> a
A --> aS
A --> bCaCa
B --> A
B --> bS
B --> bCAa
C --> ε
C --> AB
D --> AB
}

P = {
A --> a
A --> N1S
A --> N2A1
A --> a
A1 --> N2A1
A2 --> N1A1
B --> N2S
B --> N2B1
B --> a
B --> N1S
B --> N2A1
B --> N1
B --> a
B1 --> N2B1
B2 --> AB1
C --> AB
D --> AB
N1 --> a
N2 --> b
S --> BC
S --> N2A
S' --> S
}
```
As you can observe, it added the S' rule, it eliminated the epsilon rules, it eliminated the rules with more than one terminal,
it processed the rules with more than one non-terminal, created transitive rules and eliminated unit rules. Finally, it sorts the
production rules by keys to be able to look at them easier.

## Conclusion
In conclusion, Chomsky Normal Form (CNF) is a type of context-free grammar that has been proven to have several useful properties in
formal language theory and computational linguistics. By converting a context-free grammar to CNF, it is possible to efficiently
parse strings and to determine whether a given language is context-free. While the conversion process may introduce additional
non-terminals and productions, it ultimately produces a more structured and easier to manipulate grammar that can be useful in a variety of applications.
