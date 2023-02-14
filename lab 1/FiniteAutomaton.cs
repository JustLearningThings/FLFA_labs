using System.Collections.Generic;

namespace FLFA;

public class FiniteAutomaton {
    char[]? alphabet { get; init; }
    List<State> states { get; } = new List<State>();
    List<char> stateNames { get; }


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
}

enum StateType {
        start = 0,
        intermediate = 1,
        final = 2
    }