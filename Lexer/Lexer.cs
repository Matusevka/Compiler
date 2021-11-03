using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace compiler
{
    class Lexer
    {
      States state;
      private string[] reservedWords = 
      {
        "array", "asm", "begin", "case", "const", "constructor", "destructor", "do",
        "downto", "else", "end", "exports", "file", "for", "function", "goto", "if", "implementation",
        "in", "inherited", "inline", "interface", "label", "library", "nil", "object",
        "of", "packed", "procedure", "program", "record", "repeat", "set", "shl", "shr",
        "string", "then", "to", "type", "unit", "until", "uses", "var", "while", "with", "xor",
        "abs", "arctan", "boolean", "char", "cos", "dispose", "eof", "eoln", "exp",
        "false", "get", "input", "integer", "ln", "maxint", "new", "output",
        "pack", "page", "pred", "put", "read", "readln", "real", "reset", "rewrite",
        "sin", "sqr", "sqrt", "succ", "text", "true", "unpack", "write", "writeln"
      };

      private string[] separators = {".", ",", ";", ":", "..", "(", ")", "[", "]"};
      private char[] spaceSymbols = {'\0', ' ', '\n', '\t', '\0', '\r'};
      private string[] operators = 
      { 
        "+", "-", "*", "/", "%", "&",
        "|", "^", "!", "~", "=", "<",
        ">", "++", "--", "&&", "||", 
        "!=", "<=", ">=", "=>", "mod", "div",
        "and", "or", "not"
      };

      private string[] assignment = 
      {
        ":=", "+=", "-=", "*=", "/="
      };

      private string buf = "";
      private char symbol = new char();
      private char saveSymbol = new char();

      private int currentStr = 1;        
      private int currentCol = 0;

      private int[] coordinates = new int[2];

      private bool isLast = false;
      private bool isEnd = false;

      private StreamReader streamReader; 
      TokenType TokenType = new TokenType();

      public Lexer(string path)
      {
        streamReader = new StreamReader(path);
        getNextSymbol();
      }

      public Token getLexem()
      {
        clearBuffer();

        if (saveSymbol != '\0')
        {
          buf += saveSymbol;
          saveSymbol = '\0';
        }

        while (!isEnd)
        {
          switch (state)
          {
            case States.Start:
              coordinates[0] = currentStr;
              coordinates[1] = currentCol;

              if (isSpaceSymbols(symbol))
              {
                if (symbol == '\n') newLine();
                getNextSymbol();
              }
              else if (Char.IsLetter(symbol))
              {
                addSymbol(symbol, States.Identifier);
              }
              else if (Char.IsDigit(symbol))
              {
                addSymbol(symbol, States.Int);
              }
              else if (symbol == '\'')
              {
                addSymbol(symbol, States.String);
              }
              else if(isOperator(buf + symbol))
              {
                addSymbol(symbol, States.Operator);
              }
              else if(isSeparator(buf + symbol))
              {
                addSymbol(symbol, States.Sep);
              }
              else if(symbol == '{')
              {
                addSymbol(symbol, States.Comment);
              }
              else
              {
                addSymbol(symbol, States.Error);
              }
              break;
            
            case States.Int:
              if (Char.IsDigit(symbol))
              {
                addSymbol(symbol, States.Int);
              }
              else if (symbol == '.')
              {
                addSymbol(symbol, States.Real);
              }
              else if (Char.ToLower(symbol) == 'e')
              {
                addSymbol(symbol, States.RealExp);
              }
              else if(Char.IsLetter(symbol))
              {
                addSymbol(symbol, States.Error);
              }
              else
              {
                state = States.Start;

                if (Int32.TryParse(buf, out int res))
                {
                  return new Token(coordinates, TokenType.integer, buf, res);
                }
                errorHadler($"Range check error: {buf}");
              }
              break;

            case States.Real:
              if (Char.IsDigit(symbol))
              {
                addSymbol(symbol, States.Real);
              }
              else if (Char.ToLower(symbol) == 'e')
              {
                if (buf[buf.Length - 1] != '.')
                {
                  addSymbol(symbol, States.RealExp);
                }
                else
                {
                  addSymbol(symbol, States.Error);
                }
              }
              else if (Char.IsLetter(symbol))
              {
                addSymbol(symbol, States.Error);
              }
              else if(symbol == '.' && buf[buf.Length - 1] == '.')
              {
                state = States.Start;
                saveSymbol = '.';
                buf  = buf.Substring(0, buf.Length - 1);

                if (Int32.TryParse(buf, out int res)) 
                {
                  return new Token(coordinates, TokenType.integer, buf, res);
                }
                errorHadler($"Range check error: {buf}");
              }
              else
              {
                state = States.Start;
                if (buf[buf.Length - 1] != '.')
                {
                  if (float.TryParse(buf, out float res))
                  {
                    return new Token(coordinates, TokenType.real, buf, buf);
                  }
                  errorHadler($"Range check error: {buf}");
                }
                else if (isEnd)
                {
                  errorHadler($"Syntax error: \"{buf}\"");
                }
              }
              break;

            case States.RealExp:
              if ((symbol == '-' || symbol == '+') && buf[buf.Length - 1] == 'e')
              {
                addSymbol(symbol, States.RealExp);
              }
              else if (Char.IsDigit(symbol))                                                                                          //пример 2.е5
              {
                addSymbol(symbol, States.RealExp);
              }
              else
              {
                if (new HashSet<char>(){'e', '+', '-'}.Contains(buf[buf.Length - 1]))                                                                                                                         //в случае если пришла ахинея. прмер 2.e+
                {
                  errorHadler($"Syntax error: \"{buf}\"");
                }
                if (float.TryParse(buf, out float res))
                {
                    state = States.Start;
                    return new Token(coordinates, TokenType.real, buf, res);
                }
                errorHadler($"Range check error: {buf}");
                
              }
              break;

            case States.Identifier:
              if (Char.IsLetterOrDigit(symbol) || symbol == '_')
              {
                addSymbol(symbol, States.Identifier);
              }
              else
              {
                state = States.Start;
                string tokenType = TokenType.identifier;                                                                                            //указываем изначальный тип

                if(isReservedWord(buf.ToLower())) tokenType = TokenType.reserved;
                if(isOperator(buf.ToLower())) tokenType = TokenType.lexOperator;

                return new Token(coordinates, tokenType, buf, buf );
              }
              break;
            
            case States.String:
              if ((symbol != '\'' && isLast) || symbol == '\n' || symbol=='\r')
              {
                errorHadler("End of line encountered");
              }
              else if (symbol == '\'')
              {
                addSymbol(symbol, States.Start);
                return new Token(coordinates, TokenType.lexString, buf, buf);
              }
              else
              {
                addSymbol(symbol, States.String);
              }
              break;

            case States.Operator:
              if (isOperator(buf + symbol) || isAssignment(buf + symbol))
              {
                addSymbol(symbol, States.Operator);
              }
              else if (buf + symbol == "//")
              {
                addSymbol(symbol, States.Comment);
              }
              else
              {
                state = States.Start;
                string type = TokenType.lexOperator;
                if (isAssignment(buf)) type = TokenType.assignment;

                return new Token(coordinates, type, buf, buf);
              }
              break;

            case States.Sep:
              if (isAssignment(buf + symbol))
              {
                state = States.Operator;
              }
              else if (isSeparator(buf + symbol))
              {
                addSymbol(symbol, States.Sep);
              }
              else if(buf + symbol == "(*")
              {
                addSymbol(symbol, States.Comment);
              }
              else
              {
                  if (buf == "." && Char.IsLetterOrDigit(symbol))                                               //.a
                  {
                    addSymbol(symbol, States.Error);
                  }
                  else
                  {
                    state = States.Start;
                    return new Token(coordinates, TokenType.separator, buf, buf);
                  }
              }
              break;
            
            case States.Error:
              if (isSpaceSymbols(symbol) || isLast || isSeparator(symbol.ToString()))
              {
                isEnd = true; 
                errorHadler($"Syntax error: \"{buf}\"");
              }
              addSymbol(symbol, States.Error);
              break;
            
            case States.Comment:
              if ( symbol == '}' && buf[0] == '{' || buf[buf.Length - 1].ToString() + symbol == "*)" && buf.Substring(0, 2) == "(*")                //если комент, очищаем буфер и идём дальше
              {
                state = States.Start;
                clearBuffer();
                getNextSymbol();
              }
              else if (symbol == '\n')
              {
                 if (buf.Length >= 2 && buf.Substring(0, 2) == "//")
                 {
                   state = States.Start;
                   clearBuffer();
                 }
                 else addToBuffer(symbol);

                 newLine();
                 getNextSymbol();
              }
              else if(isLast)
              {
                coordinates[0] = currentStr;
                coordinates[1] = currentCol - 1;
                errorHadler("End of file encountered");
              }
              else
              {
                addSymbol(symbol, States.Comment);
              }

              break;
          }
        }
        return new Token(currentStr, currentCol, "EOF", "End of file", "End of file");
      }

      public void errorHadler(string text)
      {
        isLast = true;
        isEnd = true;
        string output = $"{coordinates[0]}:{coordinates[1]} {text}";
        throw new CustomException(output);
      }

      public void clearBuffer()
      {
        buf = "";
      }

      public void addToBuffer(char ch)
      {
        buf += ch;
      }

      public bool isSpaceSymbols(char ch)
      {
        foreach (char symbol in spaceSymbols)
        {
          if (symbol == ch) return true;
        }

        return false;
      }

      public bool isReservedWord(string word)
      {
        foreach (string str in reservedWords)
        {
          if (str == word) return true;
        }

        return false;
      }

      public bool isSeparator(string str)
      {
        foreach (string sep in separators)
        {
          if (str == sep) return true;
        }

        return false;
      }

      public bool isOperator(string str)
      {
        foreach (string op in operators)
        {
          if (str == op) return true;
        }

        return false;
      }

      public bool isAssignment(string str)
      {
        foreach (string item in assignment)
        {
          if (str == item) return true;
        }

        return false;
      }

      public void newLine()
      {
        currentCol = 0;
        currentStr += 1;
      }

      public void addSymbol(char ch, States state)
      {
        addToBuffer(ch);
        this.state = state;
        getNextSymbol();
      }

      public void getNextSymbol()
      {
        int input = streamReader.Read();
        symbol = (char) input;

        isEnd = isLast;

        if (!isEnd) currentCol += 1;

        if (input == -1)
        {
          isLast = true;
        }
      }
    }
}