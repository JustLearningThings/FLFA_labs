# Lab 5 (Parser) Report
## Theory
A parser is a program that is part of the compiler, and parsing is part of the 
compiling process. Parsing happens during the analysis stage of compilation.
In parsing, code is taken from the preprocessor, broken into smaller pieces and 
analyzed so other software can understand it.

Making a parser involves creating an abstract syntax tree out of the tokens resulted
from the lexer.

## Objectives
 1. Get familiar with parsing, what it is and how it can be programmed [1].
 2. Get familiar with the concept of AST [2].
 3. In addition to what has been done in the 3rd lab work do the following:
     - In case you didn't have a type that denotes the possible types of tokens you need to:
        -  Have a type TokenType (like an enum) that can be used in the lexical analysis to categorize the tokens.
         - Please use regular expressions to identify the type of the token.
     - Implement the necessary data structures for an AST that could be used for the text you have processed in the 3rd lab work.
     - Implement a simple parser program that could extract the syntactic information from the input text.


## Implementation
A class, `AST` was created with the following properties:
```pycon
class AST:
    def __init__(self) -> None:
        self.syntax = dict()
        self.stack = list()
        self.lexer = None
```
The `syntax` represents the abstract syntax tree. In this lab it is represented in the
form of a dictionary in order to have an easier processing for the PBL language we build.

The `stack` is the stack of tokens resulted from the `lexer`.

The most important setup method is this one:
```pycon
def get_tokens_from_input(self, input_string: str) -> None:
        self.lexer = lexer.Lexer.new(input_string)

        tok = self.lexer.NextToken()
        self.__add_tok(tok)

        while tok.Type != EOF:
            tok = self.lexer.NextToken()
            self.__add_tok(tok)
```
It extracts tokens from an input string and saves them in the stack.

Now, the method that creates the AST is this one:
```pycon
def populate(self) -> None:
        for i, tok in enumerate(self.stack):
            if tok.Type == SUITE and self.stack[i + 1].Type == IDENT:
                self.__create_suite(self.stack[i + 1].Literal)            
            elif tok.Type == TEST and self.stack[i + 1].Type == IDENT:
                self.syntax['suite']['tests'].append(self.__create_tests(self.stack[i + 1].Literal, i))
            elif tok.Type == ORDER:
                self.syntax['suite']['executionOrder'] = self.__create_order(i)
```
As the language has 3 major parts: `Suite` (the "header" of the script), `Test` (the defined functions that require tests in the script),
and `Order` (specifies the execution order of the tests).

For each case, there is a different behavior that should be applied for the AST to be correct.
For that reason there are the following private methods: `__create_suite`, `__create_tests` (which depends on `__create_params`), and `__create_order`.

Additionaly there is the `__convert_constant_to_type`, method to convert a specific token that is a
constant, to the required type for the interpreter.

For the following input:
```
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

Execution order: foo3, foo1, foo2
```
this is the result of the parser:
```
{
    "suite": {
        "name": "mySuite",
        "tests": [
            {
                "name": "foo",
                "params": [
                    {
                        "name": "param1",
                        "type": "NUMBER",
                        "value": 1
                    }
                ],
                "result": 1,
                "skip": true,
                "num_repetitions": 5
            },
            {
                "name": "foo2",
                "params": [
                    {
                        "name": "param2",
                        "type": "BOOLEAN",        
                        "value": true
                    },
                    {
                        "name": "param3",
                        "type": "STRING",
                        "value": "John"
                    }
                ],
                "result": null,
                "skip": false,
                "num_repetitions": 1
            },
            {
                "name": "foo3",
                "params": [],
                "result": 1.42,
                "skip": true,
                "num_repetitions": 1
            }
        ],
        "executionOrder": [
            "foo3",
            "foo1",
            "foo2"
        ]
    }
}
```

## Conclusion
Creating an AST from a parse tree is a common task in software development,
particularly in compilers and interpreters. An AST is a simplified version
of the parse tree that removes redundant information and focuses on the
essential structure of the input. ASTs are useful for further processing
of the input, such as code optimization, code generation, or program analysis.

The process of creating an AST typically involves traversing the parse tree and
constructing a new tree that represents the essential structure of the input,
discarding the irrelevant information. The resulting AST can be represented as
a tree data structure in memory, and can be further processed or transformed
as needed.