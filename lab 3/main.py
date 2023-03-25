import repl
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