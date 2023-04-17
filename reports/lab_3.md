# Lab 3 (Lexer Scanner) Report

## Theory
The term lexer comes from lexical analysis, which represents the process of extracting lexical tokens from a string of characters. There are several alternative names for the mechanism called lexer: tokenizer or scanner. The lexical analysis is one of the first stages used in a compiler/interpreter when dealing with programming, markup or other types of languages. The tokens are identified based on some rules of the language and the products that the lexer gives are called lexemes. So basically the lexer is a stream of lexemes. Now in case it is not clear what's the difference between lexemes and tokens, there is a big one. The lexeme is just the byproduct of splitting based on delimiters, for example spaces, but the tokens give names or categories to each lexeme. So the tokens don't retain necessarily the actual value of the lexeme, but rather the type of it and maybe some metadata.

## Objectives
- Understand what lexical analysis is.
- Get familiar with the inner workings of a lexer/scanner/tokenizer.
- Implement a sample lexer and show how it works.

## Implementation
We should tokenize a string, so we need a Token object. Here is its definition:

```py
class Token:
    def __init__(self, type: str = None, literal: str = None):
        self.Type = type
        self.Literal = literal    
    
    # check if an identifier is a keyword or not
    def LookupIdent(self, ident: str):
        if ident in keywords.keys():
            return keywords[ident]
        
        return IDENT
```
And this is the list of keywords:


```py
keywords = {
    'Suite': SUITE,
    'Test': TEST,
    'When': WHEN,
    'Then result should be': RESULT,
    'Execution order:': ORDER,
    'empty': VOID,
    'void': VOID,
    'none': VOID,
    'no parameters': VOID
}
```
Apart from that, you can find the following variables, which describe the tokens recognized by the lexer:
```py
ILLEGAL = 'ILLEGAL'
EOF = 'EOF'

# identifiers + literals
IDENT = 'IDENT'
NUMBER = 'NUMBER' # both int and float
STRING = 'STRING'
OBJECT = 'OBJECT'
VOID = 'VOID'

FLAG = 'FLAG'
QUOTES = 'QUOTES'

# operators
ASSIGN = '='
EQ = "=="
NOT_EQ = "!="
PLUS = '+'
MINUS = '-'
BANG = '!'
ASTERIKS = '*'
SLASH = '/'
MODULO = '%'
LT = '<'
GT = '>'

# keywords
SUITE = 'SUITE'
TEST = 'TEST'
WHEN = 'WHEN'
RESULT = 'RESULT'
ORDER = 'ORDER'
```

Now, to the lexer. It has the following constructor:
```py
def __init__(self, input: str, position: int = 0, readPosition: int = 0, ch: str = ''):
        self.input = input
        self.position = position
        self.readPosition = readPosition
        self.ch = ch
```
`input` is the input string.

`position` is the current reading position through the string.

`readPosition` same as position, but it "peeks" further than `position` making it possible to read a list of characters at the same time if needed. Imagine it as a window that you use to read the `input`. The starting length is 1, meaning one character. But when you read identifiers, for example, its length is greater.

`ch` the current character that is being read.

There is also a static method that creates a lexer and sets everything up:
```py
@staticmethod
def new(input: str):
    l = Lexer(input)
    l.readChar()

    return l
```
The most important method is this one:
```py
def NextToken(self):
        tok: Token = Token()

        self.skipWhitespace()
        
        if self.ch == '=':
            if self.peekChar() == '=': # equals sign
                ...
            else: # assignment
                ...
        elif self.ch == '+': # plus operator
            ...
        elif self.ch == '-':
            # check if this is a flag
            if self.isLetter(self.peekChar()):
                ...
            else: # minus operator
                ...
        elif self.ch == '!':
            if self.peekChar() == '=': # not equal comparator
                ...
            else: # bang
                ...
        elif self.ch == '/': # division operator
            ...
        elif self.ch == '*': # multiplication operator
            ...
        elif self.ch == '<': # smaller than
            ...
        elif self.ch == '>': greater than
            ...
        elif self.isLetter(self.ch): # identifiers and keywords
            ...

            if str.lower(tok.Literal) == 'then':
                ...
            elif str.lower(tok.Literal) == 'no':                
                ...

                if tok.Literal != 'no parameters':
                    ...                        
            elif str.lower(tok.Literal) == 'execution':
                ...

                if self.ch != ':':
                    ...          
            
            tok.Type = tok.LookupIdent(tok.Literal)
                
            return tok
        elif self.isDigit(self.ch): # number
            ...
        elif self.ch == '"': # reading strings
            ...
        elif self.ch == None or self.ch == '': # eof
            ...
        else: # illegal token
            tok = self.newToken(ILLEGAL, self.ch) 
        
        self.readChar()

        return tok
```
All the rest of the methods are helpers to read specific tokens or to create a token. To get a feeling how they work take a look at this one:
```py
def readChar(self):
        if self.readPosition >= len(self.input):
            self.ch = None
        else:
            self.ch = self.input[self.readPosition]

        self.position = self.readPosition
        self.readPosition += 1
```
It checks if we didn't reach the end of the `input`. If not, then we read the character based on the position of the peeking index - `readPosition`.
Then it updates the indices of the reading window.

## Presentation
And this is the driver code to test the functionality of the lexer:
```py
import lexer
from token import *

if __name__ == '__main__':
    print('Feel free to write commands:')
    print()

    # repl.start()
    # exit()

    input = '''
    Suite mySuite

    Test foo -skip -repeat 5 times
    When param1=1
    Then result should be 1

    Test foo2
    When param2=True, param3="John"
    Then result should be empty

    Test foo3 -skip
    When no parameters
    Then result should be 1.42

    Execution order: foo1, foo3, foo2
    '''

    l = lexer.Lexer.new(input)
    tok = l.NextToken()

    while tok.Type != EOF:
        print(f'{tok.Literal} ({tok.Type})')

        tok = l.NextToken()
```
Take a look at the input string. It contains all the keywords. We have both ints and floats. We have a string and a boolean. Also we have void: `"empty"`. Also we have two flags for the first test. The `-skip` flag is fairly simple. The `-repeat` flag requires reading first the keyword `-` that tells the lexer it is a flag, then the keyword `"repeat"`. Then it expects an integer and then the keyword `"times"`.

When this code is run, the following is outputed:
```
Suite (SUITE)
mySuite (IDENT)
Test (TEST)
foo (IDENT)
 (FLAG)
skip (IDENT)
 (FLAG)
repeat (IDENT)
5 (NUMBER)
times (IDENT)
When (WHEN)
param1 (IDENT)
= (=)
1 (NUMBER)
Then result should be (RESULT)
1 (NUMBER)
Test (TEST)
foo2 (IDENT)
When (WHEN)
param2 (IDENT)
= (=)
True (IDENT)
param3 (IDENT)
= (=)
"John" (STRING)
Then result should be (RESULT)
empty (VOID)
Test (TEST)
foo3 (IDENT)
 (FLAG)
skip (IDENT)
When (WHEN)
no parameters (VOID)
Then result should be (RESULT)
1.42 (NUMBER)
Execution order: (ORDER)
foo1 (IDENT)
foo3 (IDENT)
foo2 (IDENT)
```
On the left there is the token literal. On the right its type is printed.

*Note that the tokens of type FLAG are empty. This is intended to distinguish it from the equal sign.*

You can try to tweak the input and see that if some string that isn't a subset of the code is inputed, then it will print the illegal token found.

For example, if you forget to write `"parameters"` in the `When` clause that expects no parameters like this:
```
When no empty
```
Then you should get the following message:
```
Syntax error! Expected `no parameters` after `When`, got no empty.
```

## Conclusion
A lexer is a crucial tool in creating a programming language that is to be compiled or interpreted, because you need to have collected the words of the language in a correct way and stored such that you can distinguish one from the other.

Lexer is the first step in creating an interpreter or compiler.