using Interpreter.Expr;
using Interpreter.Expr.LogicExpr;
using Interpreter.Expr.MathExpr;
using Interpreter.Statement;

namespace Interpreter;

internal class Program {
    static void Main (string[] args) 
    {
        // Реализация программы:
        // int n = input();
        // int i = 0;
        // while(i < n) { Console.WriteLine(n); n--; }

        new BlockStatement
        (
            // int n = input();
            new ExprStatement
            (
                new AssignExpr(new VarExpr("n"), new InputExpr())
            ),
            // int i = 0;
            new ExprStatement
            (
                new AssignExpr(new VarExpr("i"), new ConstExpr(0))
            ),
            // while (i < n)
            new WhileStatement
            (
                new LessExpr(new VarExpr("i"), new VarExpr("n")),
                new BlockStatement
                (
                    // Console.WriteLine(n);
                    new ExprStatement
                    (
                        new PrintExpr(new VarExpr("n"))
                    ),
                    // n--;
                    new ExprStatement
                    (
                        new DecrementExpr(new VarExpr("n"))
                    ),
                    // i++;
                    new ExprStatement
                    (
                        new IncrementExpr(new VarExpr("i"))
                    )
                )
            )
        ).Execute();
    }
}
