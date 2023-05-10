import lexer
from token import *
import ast

import json

if __name__ == '__main__':
    # print('Feel free to write commands:')
    # print()

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

    Execution order: foo3, foo1, foo2
    '''

    parsing_tree = ast.AST()
    parsing_tree.get_tokens_from_input(input)

    print([(t.Literal, t.Type) for t in parsing_tree.stack])
    print()

    # parsing_tree.print_tree(parsing_tree.root)
    
    parsing_tree.populate()
    print('\nSyntax:\n')
    print(json.dumps(parsing_tree.syntax, indent=4))

    # l = lexer.Lexer.new(input)
    # tok = l.NextToken()

    # while tok.Type != EOF:
    #     print(f'{tok.Literal} ({tok.Type})')

    #     tok = l.NextToken()