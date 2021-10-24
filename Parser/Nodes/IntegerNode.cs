using System;

namespace compiler
{
  class IntegerNode : Node
  {
    Token token;

    public IntegerNode(Token token)
    {
      this.token = token;
    }

    public override string getValue()
    {
      return Convert.ToString(token.value);
    }

    public string getCoords()
    {
      return $"{token.str}:{token.col}";
    }

    override public string print(int priority=0)
    {
      return getValue();
    }
  }
}