# from .lexer import *
import lexer

class Token:
    def __init__(self, type: str = None, literal: str = None):
        self.Type = type
        self.Literal = literal    
    
    def LookupIdent(self, ident: str):
        if ident in keywords.keys():
            return keywords[ident]
        
        return IDENT

# TokenTypes
ILLEGAL = 'ILLEGAL'
EOF = 'EOF'

# identifiers + literals
IDENT = 'IDENT'
# INT = 'INT'
# FLOAT = 'FLOAT'
NUMBER = 'NUMBER'
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