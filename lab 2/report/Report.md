# Lab 2: Finite Automata
## Course: Formal Languages & Finite Automata
## Author: Denis Smocvin

## Theory
Definitions:

- **Deterministic finite automaton**, is a finite automaton, where each of its transitions is uniquely determined by its source state and input symbol, and reading an input symbol is required for each state transition.
- **Non-deterministic finite automaton** - a finite automaton, which has many paths for specific input from the current state to the next state.

## Objectives
1. Understand what an automaton is and what it can be used for.

2. Continuing the work in the same repository and the same project, the following need to be added:
    - Provide a function in your grammar type/class that could classify the grammar based on Chomsky hierarchy.

    - For this you can use the variant from the previous lab.

3. According to your variant number (by universal convention it is register ID), get the finite automaton definition and do the following tasks:

    - Implement conversion of a finite automaton to a regular grammar.

    - Determine whether your FA is deterministic or non-deterministic.

    - Implement some functionality that would convert an NDFA to a DFA.

    - Represent the finite automaton graphically (Optional, and can be considered as a bonus point):

4. You can use external libraries, tools or APIs to generate the figures/diagrams.

    Your program needs to gather and send the data about the automaton and the lib/tool/API return the visual representation.



## Implementation
For classifying the grammar by type, the following method is added to the `Grammar` class:

```cs
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
```

Now, to the finite automaton.

First the constructor now receives a `List<char>? finalStates_` argument with `null` as default value. This is to be able to recognize the final state after transformations.

Also, the `printInfo` function has been added to show the results:


```cs
public void printInfo() {
        foreach (State s in states) {
            string type = "a start";

            if (s.type == StateType.intermediate)
                type = "an intermediate";
            else if (s.type == StateType.final)
                type = "a final";

            Console.WriteLine("State " + s.key + " is " + type + " state and has transitions:");
            
            foreach (var transition in s.transitions!)
                Console.WriteLine("\t" + transition);
            
            Console.WriteLine();
        }
    }
```

The method that converts the NFA to a DFA:

```cs
public void toDFA() {
        if (isDeterministic())
            return;

        List<State> newStates = new List<State>();
        List<char> newStateNames = new List<char>();
        List<KeyValuePair<string, List<List<char>>>> table = new List<KeyValuePair<string, List<List<char>>>>();

        int stateIndex = 0;

        // add NFA states to the table
        foreach (State state in states) {
            if (state.key == 'X')
                continue;

            if (newStateNames.Any(name => name == state.key) || !hasKey(table, state.key.ToString())) {                                
                newStateNames.Add(state.key);

                table.Add(new KeyValuePair<string, List<List<char>>>(
                    state.key.ToString(),
                    getStatesFromTransitions(state)
                ));
            }
        }

        stateIndex = newStateNames.Count;

        // extend the table by adding states if necessary
        while (stateIndex > 0) {
            foreach (var row in table.ToList()) {
                foreach (List<char> transitionStates in row.Value) {
                    if (transitionStates.Count == 0) // no transitions for this symbol
                        continue;
                    
                    string name = new string(transitionStates.ToArray());

                    // add the compound state if it doesn't exist in the table
                    if (transitionStates.Count > 1 && !hasKey(table, name)) {
                        table.Add(new KeyValuePair<string, List<List<char>>>(
                            name,
                            getStatesFromTransitions(name, table)
                        ));
                    }
                }
            }

            stateIndex--;
        }

        // create states out of the table
        newStates = createStatesFromTable(ref table, newStates, newStateNames);

        // reassign data fields
        states = newStates;
        stateNames = newStateNames;
        finalStates = new List<char>();

        foreach (State s in states)
            if (s.type == StateType.final)
                finalStates.Add(s.key);
    }
```

The algorithm is as follows:
1. Check if the FA is non-deterministic before continuing.
2. Create the states table. For each state, a list is created. The list contains, for each symbol in the alphabet, a list of states that you can go from the current state. That is, in an NFA there might be multiple states to go from a state with the same symbol.
3. If a state has many states it can go to with a single symbol, create a new state (if it doesn't exist already) with the name composed of that states.Let's name the newly created states compound states for convenience.

    *At this point with have the whole table. But the compound states have a string as a name, while my FA definition expects a char.*

4. Go through the table. If you find a compound state, rename it, determine it's type and rename all the transitions in the table with that name.
5. Create states with the data from the table.
6. Change the data fields of the NFA with the information 
about the newly created states for DFA (states themselves, their names and the list of final states).

A more detailed overview of the methods used can be found in the `FiniteAutomaton.cs` file.

The following program is used to test on my variant (25):
```cs
using FLFA;

List<KeyValuePair<char, string>> productions = new List<KeyValuePair<char, string>> {
    new KeyValuePair<char, string> ('S', "aS"),
    new KeyValuePair<char, string> ('S', "aA"),
    new KeyValuePair<char, string> ('A', "aB"),
    new KeyValuePair<char, string> ('A', "bA"),
    new KeyValuePair<char, string> ('B', "aC"),
    new KeyValuePair<char, string> ('C', "aA"),
};
char [] terminals = new char[] { 'a', 'b' };
char [] nonTerminals = new char[] { 'S', 'A', 'B', 'C' };
List<char> finalStates = new List<char>() {'C'};
Vocabulary vocabulary = new Vocabulary(terminals, nonTerminals);

Grammar g = new Grammar(nonTerminals, terminals, productions);
FiniteAutomaton fa = new FiniteAutomaton(terminals, productions, finalStates);

Console.WriteLine("Grammar type: " + g.classify().ToString());

Console.WriteLine("Created");

Console.WriteLine("Is deterministic: " + fa.isDeterministic().ToString());
fa.toDFA();
Console.WriteLine("Is deterministic: " + fa.isDeterministic().ToString());
fa.printInfo();
```

The output is:
[Image of the output](./output.png "The output")

# Conclusions
Grammar and finite automata are useful concepts in computer science that are based on linguistics and mathematics.

While the Type 3 grammars are the easiest to work with, because they have specific constraints, it is also important to be able to recognize other the other types.

A finite automaton is more useful when it is deterministic. Hence, being able to transform a non-deterministic finite automaton into a deterministic one is a basic skill in finite automata theory.