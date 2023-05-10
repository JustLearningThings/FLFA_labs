from token import *
from token import Token
import lexer

class AST:
    def __init__(self) -> None:
        self.syntax = dict()
        self.stack = list()
        self.lexer = None
    
    def __add_tok(self, tok: Token) -> None:
        self.stack.append(tok)
    
    def get_tokens_from_input(self, input_string: str) -> None:
        self.lexer = lexer.Lexer.new(input_string)

        tok = self.lexer.NextToken()
        self.__add_tok(tok)

        while tok.Type != EOF:
            tok = self.lexer.NextToken()
            self.__add_tok(tok)
        
    def populate(self) -> None:
        for i, tok in enumerate(self.stack):
            if tok.Type == SUITE and self.stack[i + 1].Type == IDENT:
                self.__create_suite(self.stack[i + 1].Literal)            
            elif tok.Type == TEST and self.stack[i + 1].Type == IDENT:
                self.syntax['suite']['tests'].append(self.__create_tests(self.stack[i + 1].Literal, i))
            elif tok.Type == ORDER:
                self.syntax['suite']['executionOrder'] = self.__create_order(i)
            
    
    def __create_suite(self, name) -> None:
        self.syntax['suite'] = dict({
            'name': name,
            'tests': list(),
            'executionOrder': list()
        })

    def __create_tests(self, name, i) -> dict:
        test = dict({
            'name': name,
            'params': list(),
            'result': None,
            'skip': False,
            'num_repetitions': 1
        })

        while self.stack[i + 1].Type not in [ORDER, TEST, EOF]:
            if self.stack[i + 1].Type == FLAG:
                if self.stack[i + 2].Literal.lower() == 'skip':
                    test['skip'] = True

                    i += 2
                    continue
                elif self.stack[i + 2].Literal.lower() == 'repeat' and self.stack[i + 3].Type == NUMBER and self.stack[i + 4].Literal.lower() == 'times':
                    test['num_repetitions'] = int(self.stack[i + 3].Literal)

                    i += 4
                    continue
            elif self.stack[i + 1].Type == WHEN:
                self.__create_params(test, i)
            elif self.stack[i + 1].Type == RESULT and self.stack[i + 2].Type in [NUMBER, STRING, OBJECT, VOID] or self.stack[i + 2].Type == IDENT and self.stack[i + 2].Literal.lower() in ['true', 'false']:
                test['result'] = self.__convert_constant_to_type(self.stack[i + 2])
            
            i += 1
        
        return test

    def __create_params(self, test: dict, i: int) -> None:
        i += 1

        while self.stack[i + 1].Type != RESULT:
            print(self.stack[i].Literal)
            if self.stack[i + 1].Type == IDENT and self.stack[i + 2].Type == ASSIGN and (self.stack[i + 3].Type in [NUMBER, STRING, OBJECT, VOID] or self.stack[i + 3].Type == IDENT and self.stack[i + 3].Literal.lower() in ['true', 'false']):
                const_type = BOOLEAN if self.stack[i + 3].Type == IDENT else self.stack[i + 3].Type

                test['params'].append(dict({
                    'name': self.stack[i + 1].Literal,
                    'type': const_type,
                    'value': self.__convert_constant_to_type(self.stack[i + 3])
                }))
            
                i += 2
            
            i += 1

    def __convert_constant_to_type(self, tok: Token):
        if tok.Type == NUMBER:
            if '.' in tok.Literal:
                return float(tok.Literal)
            
            return int(tok.Literal)
        elif tok.Type == STRING:
            return str(tok.Literal[1:-1])
        elif tok.Type == IDENT:
            return bool(tok.Literal)
        elif tok.Type == OBJECT:
            return object(tok.Literal)
        elif tok.Type == VOID:
            return None
    
    def __create_order(self, i: int) -> list:
        order = list()
        i += 1

        while self.stack[i].Type != EOF:
            if self.stack[i].Type == IDENT:
                order.append(self.stack[i].Literal)
            
            i += 1
        
        return order