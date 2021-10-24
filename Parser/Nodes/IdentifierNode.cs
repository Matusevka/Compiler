namespace compiler
{
  class IdentifierNode : Node
  {
    Token token;

    public IdentifierNode(Token token)
    {
      this.token = token;
    }

    public override string getValue()
    {
      return token.value;
    }

    public string getCoords()
    {
      return $"{token.str}:{token.col}";
    }

    public override string print(int priority)
    {
      return getValue();
    }
  }
}