namespace Lang.Interpreter
{
    /// <summary>
    /// Defines the available types of <see cref="Token"/>.
    /// </summary>
    public enum TokenType
    {
        // symbol tokens
        LeftParen,
        RightParen,
        LeftCurlyBrace,
        RightCurlyBrace,
        Comma,
        Dot,
        Plus,
        Minus,
        SemiColon,
        Slash,
        Star,
        Bang,
        BangEqual,
        Equal,
        DoubleEqual,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,

        // literals
        Identifier,
        String,
        Number,

        // keywords
        Class,
        And,
        Or,
        If,
        Else,
        True,
        False,
        This,
        Super,
        Var,
        Fun,
        Return,
        For,
        While,
        Print,
        Null,

        EndOfFile
    }
}
