# Lab 1: Regular Grammar
## Course: Formal Languages & Finite Automata
## Author: Denis Smocvin

## Objectives
---
1. Understand what a language is and what it needs to have in order to be considered a formal one.

2. Provide the initial setup for the evolving project that you will work on during this semester. I said project because usually at lab works, I encourage/impose students to treat all the labs like stages of development of a whole project. Basically you need to do the following:

a. Create a local && remote repository of a VCS hosting service (let us all use Github to avoid unnecessary headaches);

b. Choose a programming language, and my suggestion would be to choose one that supports all the main paradigms;

c. Create a separate folder where you will be keeping the report. This semester I wish I won't see reports alongside source code files, fingers crossed;

3. According to your variant number (by universal convention it is register ID), get the grammar definition and do the following tasks:

a. Implement a type/class for your grammar;

b. Add one function that would generate 5 valid strings from the language expressed by your given grammar;

c. Implement some functionality that would convert and object of type Grammar to one of type Finite Automaton;

d. For the Finite Automaton, please add a method that checks if an input string can be obtained via the state transition from it;

## Implementation
---
Let's start with the `Grammar` class.

First, notice the properties. We have a `Vocabulary`, which is contains the terminal and non-terminal symbols separated from each other (Vt and Vn).

Next, the properties of the `Grammar` class are the vocabulary, the productions, which is defined as a list of key value pairs (the key is char, the value is string), the starting string and the maximal length of the generated words. There is also have a random number generator to choose between different productions of the same non terminal symbol.
```cs
public class Grammar {
    Vocabulary vocabulary { get; init; }
    List<KeyValuePair<char, string>> productions { get; init; }
    char startString { get; init; } = 'S';
    public int maxLength { get; set; } = 20;

    private Random rnd = new Random();

    public Grammar (
        char[] Vn,
        char[] Vt,
        List<KeyValuePair<char, string>> P
    ) { ... }

    // generate a single string in the language
    public string generateString() { ... }

    // generate n words in the language
    public List<string> generateStrings(int n) { ... }

    // check if a string contains non terminal symbols
    private bool containsNonTerminals(string s) { ... }

    // get the value of a production given key (ex: the 'aA' in 'S -> aA')
    private string getProduction (char c) { ... }

    // transform this Grammar into a FiniteAutomaton
    public FiniteAutomaton toFiniteAutomaton() { ... }
}

public record Vocabulary {
    public char[] terminal = {};
    public char[] nonTerminal = {};
}
```

*Note: I did not use Dictionary, because we have non-terminals in productions that often repeat. So I simulated a dictionary that lets me repeat the keys.*

Now, let's check the `FiniteAutomaton` class:

```cs
using System.Collections.Generic;

namespace FLFA;

public class FiniteAutomaton {
    char[]? alphabet { get; init; }
    List<State> states { get; } = new List<State>();
    List<char> stateNames { get; }


    public FiniteAutomaton(
        char[] terminals,
        List<KeyValuePair<char, string>> productions) { ... }

    // check if a string is composed only of terminals
    private bool hasOnlyTerminals(string s) { ... }

    // check if a string is a word in the language defined at automaton creation
    public bool stringBelongToLanguage(string inputString) { ... }

    // find a state given its key
    private State? findState(char key) { ... }

    // find the next state of state s, given an input character
    // (traverse states graph given transition trigger defined by 'input' param)
    private State? nextState(char input, State s) { ... }
}

class State {
    public List<string>? transitions;
    public char key;
    public StateType type;

    public State(char key, List<string> transitions, StateType type) { ... }
}

enum StateType {
        start = 0,
        intermediate = 1,
        final = 2
    }
```

We'll dive into details in a minute. But first, notice that we have a `State` class. It is used as a convenient way to store states. Specifically the key (the non terminal symbol that defines it), the type (start, intermediate or final) and the transitions (basically, the right hand side of the productions for this specific non terminal symbol).

In the `FiniteAutomaton` class, there are some private helper functions and the ones needed for completing this lab. Check the comments above them for a short description. So, I invite you to look into the constructor and `stringBelongToLanguage` function.

The constructor:
```cs
public FiniteAutomaton(
        char[] terminals,
        List<KeyValuePair<char, string>> productions) {

        alphabet = terminals;

        // create the states
        List<char> keys = productions.Select(p => p.Key).ToList();
        List<char> itteratedKeys = new List<char>();        

        foreach (var key in keys) {
            // skip key if already processed it
            if (itteratedKeys.Contains(key))
                continue;

            List<string> transitions = productions.Where(p => p.Key == key).Select(p => p.Value).ToList();

            // determine state type
            StateType type = StateType.intermediate;

            if (key == 'S')
                type = StateType.start;

            this.states.Add(new State(key, transitions, type));            
            itteratedKeys.Add(key);
        }

        this.states.Add(new State('X', new List<string>(){}, StateType.final));
        stateNames = itteratedKeys;
    }
```

First, the alphabet (sigma) is the set of terminals. Then, the constructor creates the states (Q). It iterates over the production key (non-terminals) and saves the productions for each key in a `State`. Along the way it determines the type of the state. At the end it adds the final state (noted as 'X').

Now, the function that checks if a string is a word of the language:
```cs
public bool stringBelongToLanguage(string inputString) {
        State currentState = findState('S')!;
        string currentString = "";

        foreach (var c in inputString) {
            if (currentState!.type == StateType.final && currentString.Length != inputString.Length)
                return false;

            currentState = nextState(c, currentState)!;
            currentString += c;
        }

        if (currentState is null || currentState.type != StateType.final)
            return false;

        return true;
    }
```
We begin at the start state and an empty string. Also, we are given the input word that needs to be checked. At the end the input word and the string mentioned earlier should be equal to have a valid word.

We iterate over the input word and for each character we move along the graph based on that character. So, if we have `S -> aA`, we move to `A` state, we add `a` to the `currentString` and continue the loop.

Now, let's discuss the if statements. They define the cases when a given string is not a word in the language.

The first one checks if we reach the final state (but not the end of the input string), then we don't have any path to move along the graph. In other words, the `X` state has no "arrow" back.
```cs
if (currentState!.type == StateType.final && currentString.Length != inputString.Length)
                return false;
```

The second one is executed after we iterate the word. If at any time we don't have a path to the next state given the input (this is not only the case with the final state, but may happen at intermediate states too), the `nextState` function returns `null`. This is the first condition. The second condition checks if we reached the final state too early.
```cs
if (currentState is null || currentState.type != StateType.final)
            return false;
```

This is an example of output we get:

[Image of a sample output](./output.png "Sample output")

# Conclusions
Grammar and finite automata are useful concepts in computer science that are based on linguistics and mathematics.

Grammar is a convenient way to define a language by its alphabet and all the possible words through the process of their creation. A finite state machine can be used to determine if any finite-length word is part of the language defined by a grammar. This can be used when we define a formal language and need to programmaticaly find the words in an individual language. For example, when creating a programming language during the parsing process.

Grammars and finite automata are both important concepts in computer science and linguistics, and understanding their relationship is essential for understanding how computers process language.