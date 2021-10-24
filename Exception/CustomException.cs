using System;

namespace compiler
{
  class CustomException : Exception
  {
    public CustomException(string text) : base(text)
    {
    }
  }
}