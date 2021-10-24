using System;

namespace compiler
{
  class UnaryOpNode : Node
  {
    Token operation;
    Node operand;

    public UnaryOpNode(Node operand, Token operation)
    {
      this.operand = operand;
      this.operation = operation;
    }

    public override string getValue()
    {
      return Convert.ToString(operation.value);
    }

    public override string print(int priority)
    {
      return getValue() + operand.getValue();
    }
  }
}