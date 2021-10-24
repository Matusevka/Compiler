using System;

namespace compiler
{
    class Token
    {
        // token string number
        public int str;

        // token column number
        public int col;

        // type of token
        public string type;

        // this token in input code
        public string code;

        // value of token
        public dynamic value;
        public Token(int[] coordinates, string type, string code, dynamic value)
        {
          this.str = coordinates[0];
          this.col = coordinates[1];
          this.type = type;
          this.code = code;
          this.value = value;
        }

        public Token(int str, int col, string type, string code, dynamic value)
        {
          this.str = str;
          this.col = col;
          this.type = type;
          this.code = code;
          this.value = value;
        }

        public bool isEOF()
        {
          return type == "EOF";
        }

        public string toString()
        {
          return $"{str}:{col} {type}, \"{code}\", {value}";
        }
    }
}