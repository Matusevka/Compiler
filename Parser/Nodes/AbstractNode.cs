namespace compiler
{
  abstract class Node
  {

    abstract public string print(int priority = 0);
    abstract public string getValue();
    
  }
}