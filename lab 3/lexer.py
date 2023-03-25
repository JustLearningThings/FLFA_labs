# from .token import *
from token import *

class Lexer:
    def __init__(self, input: str, position: int = 0, readPosition: int = 0, ch: str = ''):
        self.input = input
        self.position = position
        self.readPosition = readPosition
        self.ch = ch
    
    @staticmethod
    def new(input: str):
        l = Lexer(input)
        l.readChar()

        return l
    
    def readChar(self):
        if self.readPosition >= len(self.input):
            self.ch = None
        else:
            self.ch = self.input[self.readPosition]

        self.position = self.readPosition
        self.readPosition += 1

    def NextToken(self):
        tok: Token = Token()

        self.skipWhitespace()

        if self.ch == '=':
            if self.peekChar() == '=':
                ch = self.ch
                self.readChar()

                tok = Token(EQ, ch + self.ch)
            else:
                tok = self.newToken(ASSIGN, self.ch)
        elif self.ch == '+':
            tok = self.newToken(PLUS, self.ch)
        elif self.ch == '-':
            # check if this is a flag
            if self.isLetter(self.peekChar()):
                tok = self.newToken(FLAG, self.readFlag())
            else:
                tok = self.newToken(MINUS, self.ch)
        elif self.ch == '!':
            if self.peekChar() == '=':
                ch = self.ch
                self.readChar()

                tok = Token(NOT_EQ, ch + self.ch)
            else:
                tok = self.newToken(BANG, self.ch)
        elif self.ch == '/':
            tok = self.newToken(SLASH, self.ch)
        elif self.ch == '*':
            tok = self.newToken(ASTERIKS, self.ch)
        elif self.ch == '<':
            tok = self.newToken(LT, self.ch)
        elif self.ch == '>':
            tok = self.newToken(GT, self.ch)
        # elif self.ch == '"':
        #     tok = self.newToken(QUOTES, self.ch)
        elif self.isLetter(self.ch):
            tok.Literal = self.readIdentifier()
            # tok.Type = tok.LookupIdent(tok.Literal)

            if str.lower(tok.Literal) == 'then':
                tok_list = [tok.Literal]

                for _, expected in enumerate(['result', 'should', 'be']):
                    self.skipWhitespace()
                    read_ident = self.readIdentifier()

                    if read_ident != expected:
                        print(f'Syntax error! Expected {expected} after {tok.Literal}.')
                        exit(1)

                    tok_list.append(read_ident)
                
                tok.Literal = ' '.join(tok_list)
            elif str.lower(tok.Literal) == 'no':                
                tok.Literal = 'no '
                self.skipWhitespace()
                tok.Literal += self.readIdentifier()

                if tok.Literal != 'no parameters':
                    print(f'Syntax error! Expected `no parameters` after `When`, got {tok.Literal}.')
                    exit(1)                            
            elif str.lower(tok.Literal) == 'execution':
                tok_list = [tok.Literal]

                for _, expected in enumerate(['order']):
                    self.skipWhitespace()
                    read_ident = self.readIdentifier()

                    if read_ident != expected:
                        print(f'Syntax error! Expected {expected} after {tok.Literal}.')
                        exit(1)
                    
                    tok_list.append(read_ident)

                tok.Literal = ' '.join(tok_list)

                if self.ch != ':':
                    print(f'Syntax error! Expected `:` after `{tok_list[-1]}`.')
                    exit(1)
                
                tok.Literal += self.ch
                self.readChar()            
            
            tok.Type = tok.LookupIdent(tok.Literal)
                
            return tok
        elif self.isDigit(self.ch):
            # make case for float (0.)
            tok.Type = NUMBER
            tok.Literal = self.readNumber()

            return tok
        elif self.ch == '"': # reading strings
            tok.Type = STRING
            tok.Literal = '"' + self.readString() + '"'
            
            return tok
        elif self.ch == None or self.ch == '':
            tok = self.newToken(EOF, self.ch)

            return tok
        else:
            tok = self.newToken(ILLEGAL, self.ch)
        
        self.readChar()

        return tok
    
    def newToken(self, tokenType, ch):
        return Token(tokenType, ch)
    
    def peekChar(self):
        if self.readPosition >= len(self.input):
            return 0
        else: 
            return self.input[self.readPosition]    

    def readIdentifier(self):
        position = self.position

        while self.isLetter(self.ch) or self.isDigit(self.ch):
            self.readChar()

        return self.input[position:self.position]
    
    def isLetter(self, ch):
        return ch and (('a' <= ch and ch <= 'z') or ('A' <= ch and ch <= 'Z') or ch == '_')

    def skipWhitespace(self):
        while self.ch in [' ', '\t', '\n', '\r', ',']:
            self.readChar()
    
    def readNumber(self):
        position = self.position

        while self.isDigit(self.ch):
            self.readChar()
        
        return self.input[position:self.position]

    def isDigit(self, ch):
        return ch and '0' <= ch <= '9' or ch == '.'  

    def readString(self):
        self.readChar()
        position = self.position

        while self.ch != '"':
            self.readChar()
        
        string = self.input[position:self.position]
        self.readChar()

        return string

    def readFlag(self):
        tok = self.readIdentifier()

        if tok.lower() == 'repeat':
            if self.isDigit(self.peekChar()):
                tok += ' ' + self.readNumber()

                if self.isLetter(self.peekChar()):
                    ident += self.readIdentifier()                    

                    if ident.lower() != 'times':
                        print(f'Syntax error! Expected `times`, got {self.peekChar()}')
                        exit(1)

                    tok += ident                
                else:
                    print(f'Syntax error! Expected `times`, got {self.peekChar()}')
                    exit(1)
            else:
                print(f'Syntax error! Expected an integer, got {self.peekChar()}.')
                exit(1)

        return tok