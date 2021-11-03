using System;
using System.IO;
using System.Collections.Generic;

namespace compiler
{
    class Program
    {
        private static int allTests = 0;
        private static int passedTests = 0;
        private static List<string> resultRows = new List<string>();
        private static List<string> innerRows = new List<string>();
        private static bool isPassed = true;

        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            if(args[0] == "-l")
            {
                if (args.Length >= 2)
                {
                    if (args[1] == "-d")
                    {
                        IEnumerable<string> codeFiles = Directory.EnumerateFiles("./Tests/Lexer/");

                        foreach (string innerFile in codeFiles)
                        {
                            string fileName = innerFile.Substring(innerFile.LastIndexOf('/') + 1);
                            StreamReader resultReader = new StreamReader($"./Tests/Lexer/Result/{fileName}");
                            Lexer lexer = new Lexer(innerFile);
                            Token token;

                            try 
                            {
                                do
                                {
                                    token = lexer.getLexem();
                                    if(!token.isEOF())
                                        isPassed = checkResult(token.toString(), resultReader);
                                }
                                while (!token.isEOF());
                            } 
                            catch (CustomException ex)
                            {
                                isPassed = checkResult(ex.Message, resultReader);
                            }
                            finally
                            {
                                Console.WriteLine($"{fileName} - {isPassed}");

                                if (isPassed) passedTests += 1;
                                else
                                {
                                    Console.WriteLine("Для этого файла:");
                                    for(int i =0; i < resultRows.Count; i++)
                                    {
                                        Console.WriteLine($"Ожидалось: {resultRows[i]}");
                                        Console.WriteLine($"Вывело: {innerRows[i]}");
                                    }
                                }
                            }

                            allTests += 1;
                        }

                        Console.WriteLine();
                        Console.WriteLine($"Всего тестов: {allTests}");
                        Console.WriteLine($"Пройдено: {passedTests}");
                    }
                    else
                    {
                        Lexer lexer = new Lexer($"./Tests/Lexer/{args[1]}.txt");
                        Token token;
                        try
                        {
                            do
                            {
                                token = lexer.getLexem();
                                if(!token.isEOF())
                                    Console.WriteLine(token.toString());
                            }
                            while (!token.isEOF());
                        }
                        catch (CustomException ex)
                        {
                            
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }

            if (args[0] == "-ps")
            {
                if (args.Length >= 2)
                {
                    if (args[1] == "-d")
                    {
                        IEnumerable<string> codeFiles = Directory.EnumerateFiles("./Tests/Parser/");

                        foreach (string innerFile in codeFiles)
                        {
                            string fileName = innerFile.Substring(innerFile.LastIndexOf('/') + 1);
                            StreamReader resultReader = new StreamReader($"./Tests/Parser/Result/{fileName}");

                            try
                            {
                                Lexer lexer = new Lexer(innerFile);
                                Node node = new Parser(lexer).parseExpr();

                                string res = node.print(1);
                                isPassed = checkResultParse(res, resultReader);
                            }
                            catch (CustomException ex)
                            {
                                isPassed = checkResultParse(ex.Message, resultReader);
                            }
                            finally
                            {
                                Console.WriteLine($"{fileName} - {isPassed}");
                                if (isPassed) passedTests+=1;
                            }

                            allTests += 1;
                        }

                        Console.WriteLine();
                        Console.WriteLine($"Всего тестов: {allTests}");
                        Console.WriteLine($"Пройдено: {passedTests}");

                    }
                    else
                    {
                        try
                        {
                            Lexer lexer = new Lexer($"./Tests/Parser/{args[1]}.txt");
                            Node node = new Parser(lexer).parseExpr();  
                            string res = node.print(1);
                            Console.WriteLine(res);   
                        }
                        catch (CustomException ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                    }
                }
            }
        }

        private static bool checkResult(string lexem, StreamReader resultFile)
        {
            string trueAnswer = resultFile.ReadLine();
            bool isPassed =  trueAnswer == lexem;

            if(!isPassed)
            {
                resultRows.Add(trueAnswer);
                innerRows.Add(lexem); 
            }

            return isPassed;
        }

        private static bool checkResultParse(string res, StreamReader resultFile)
        {
            string trueAnswer = resultFile.ReadToEnd();

            return res == trueAnswer;
        }
    }
}
