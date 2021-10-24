namespace compiler
{
  class BinaryOpNode : Node
  {
    Token operation;
    Node left;
    Node right;

    public BinaryOpNode(Token operation, Node left, Node right)
    {
      this.operation = operation;
      this.left = left;
      this.right = right;
    }

    public override string getValue()
    {
      return operation.value;
    }

    override public string print(int priority)
    {
      string val = operation.value;
      string rightOp = right.print(priority + 1);
      string leftOp = left.print(priority + 1);

      return $"{val}\n{new string (' ', priority * 6)}{leftOp}\n{new string (' ', priority * 6)}{rightOp}";
    }
  }
}