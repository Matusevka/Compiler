using System;

namespace compiler
{
  class RealNode : Node
  {
    Token token;

    public RealNode(Token token)
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

    override public string print(int priority)
    {
      return  getValue();
    }
  }
}