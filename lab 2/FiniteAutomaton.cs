using System.Collections.Generic;

namespace FLFA;

public class FiniteAutomaton {
    char[]? alphabet { get; init; }
    List<State> states { get; set; } = new List<State>();
    List<char> stateNames { get; set; }
    List<char> finalStates { get; set; }

    public FiniteAutomaton(
        char[] terminals,
        List<KeyValuePair<char, string>> productions,
        List<char>? finalStates_ = null) {

        alphabet = terminals;
        finalStates = finalStates_!;

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
            
            if (finalStates!.Contains(key))
                type = StateType.final;

            this.states.Add(new State(key, transitions, type));            
            itteratedKeys.Add(key);
        }

        stateNames = itteratedKeys;
    }

    private bool hasOnlyTerminals(string s) {
        foreach (var c in s)
            if (!alphabet!.Contains(c))
                return false;
        
        return true;
    }

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

    private State? findState(char key) {
        foreach (var s in states)
            if (s.key == key)
                return s;
        
        return null;
    }

    private State? nextState(char input, State s) {
        foreach (var transition in s.transitions!) {
            if (transition[0] == input) {
                if (transition.Length == 1) // final state
                    return findState('X');
                
                for (int i = 1; i < transition.Length; i++)
                    if (stateNames.Contains(transition[i]))
                        return findState(transition[i]);
            }
        }
        
        return null;
    }

    public bool isDeterministic() {
        foreach (var state in states) {
            List<char> nonTerminals = new List<char>();

            foreach (var transition in state.transitions) {
                if (nonTerminals.Contains(transition[0]))
                    return false;

                nonTerminals.Add(transition[0]);
            }
        }

        return true;
    }

    private char? findTerminal(string production) {
        return production.FirstOrDefault(p => stateNames.Contains(p));
    }

    private char generateStateName(List<char> names) {
        char name = '\0';
        int index = 0;

        while (name == '\0' || name == 'X' || name == 'S' || names.Contains(name)) {
            name = (char)(names.Last() + index);

            index++;
        }

        names.Add(name);
        
        return name;
    }

    private List<char> getStateListForSymbol(State s, char symbol) {
        List<char> returnList = new List<char>();

        foreach (var transition in s.transitions!) {
            if (transition[0] != symbol)
                continue;        

            char? destination = findTerminal(transition);

            if (destination != null && destination != '\0')
                returnList.Add((char)destination);
        }

        return returnList;
    }

    private List<List<char>> getStatesFromTransitions(State s) {
        List<List<char>> stateList = new List<List<char>>();

        foreach (char symbol in alphabet)
            stateList.Add(getStateListForSymbol(s, symbol));

        return stateList;
    }

    // for compound states
    private List<List<char>> getStatesFromTransitions(string name, List<KeyValuePair<string, List<List<char>>>> table) {
        List<List<char>> stateList = new List<List<char>>();
        List<char> nameAsArray = new List<char>(name.ToArray());

        for (int i = 0; i < alphabet.Length; i++) {
            List<char> listToAdd = new List<char>();

            foreach (char stateName in nameAsArray) {
                listToAdd.AddRange(findByKey(table, stateName.ToString())![i]);
            }                
            
            listToAdd = new List<char>(new HashSet<char>(listToAdd));           
            stateList.Add(listToAdd);
        }

        return stateList;
    }

    private bool hasKey(List<KeyValuePair<string, List<List<char>>>> dict, string key) {
        return dict.Any(e => e.Key == key);
    }

    private List<List<char>>? findByKey(List<KeyValuePair<string, List<List<char>>>> dict, string key) {
        return dict.FirstOrDefault(item => item.Key == key).Value;
    }

    private KeyValuePair<char, StateType> renameStateAndReturnNameAndType(ref List<KeyValuePair<string, List<List<char>>>> table, string key, List<char> newStateNames) {
        StateType returnType = StateType.intermediate;
        char returnName = '\0';

        // determine type
        foreach (var name in stateNames) {
            State s = findState(name)!;            

            if (s.type == StateType.final && key.Contains(s.key)) {
                returnType = StateType.final;

                break;
            }
        }
        
        // rename state
        returnName = generateStateName(newStateNames);

        // rename all transitions in the table with this state
        for (int i = 0; i < table.Count; i++) {
            for (int j = 0; j < table[i].Value.Count; j++) {                
                if (table[i].Value[j].SequenceEqual(key.ToArray())) {
                    table[i].Value[j] = new List<char>() {returnName};
                }
            }
        }
        
        return new KeyValuePair<char, StateType>(returnName, returnType);
    }

    private bool isFinalState(List<State> stateList, char key) {
        foreach (State s in stateList)
            if (s.key == key)
                if (s.type == StateType.final)
                    return true;
                else
                    return false;

        return false;
    }

    private List<State> createStatesFromTable(ref List<KeyValuePair<string, List<List<char>>>> table, List<State> newStates, List<char> newStateNames) {
        List<State> returnList = new List<State>();

        // update names of compound states and create them
        foreach (var row in table) {
            if (row.Key.Length > 1) {
                KeyValuePair<char, StateType> newStateInfo = renameStateAndReturnNameAndType(ref table, row.Key.ToString(), newStateNames);
                char name = newStateInfo.Key;
                StateType type = newStateInfo.Value;

                returnList.Add(new State(name, new List<string>(), type));
            }
            else {
                returnList.Add(new State(row.Key[0], new List<string>(), findState(row.Key[0])!.type));
            }
        }

        // update state transitions after renamings
        for (int i = 0; i < table.Count; i++) {
            List<string> transitions = new List<string>();

            for (int j = 0; j < alphabet!.Length; j++) {
                char terminal = alphabet[j];
                
                if (table[i].Value[j].Count == 1)
                    transitions.Add(terminal.ToString() + table[i].Value[j][0]);
            }

            returnList[i].transitions = transitions;        
        }

        return returnList;
    }

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
}

class State {
    public List<string>? transitions;
    public char key;
    public StateType type;

    public State(char key, List<string> transitions, StateType type)
    {
        this.key = key;
        this.transitions = transitions;
        this.type = type;
    }

    public State() {}
}

enum StateType {
        start = 0,
        intermediate = 1,
        final = 2
    }