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
        PlusEqual,
        Minus,
        MinusEqual,
        Colon,
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
        Amp,
        DoubleAmp,
        Pipe,
        DoublePipe,
        Caret,

        // literals
        Identifier,
        String,
        Number,

        // keywords
        Class,
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
        Break,
        Print,
        Null,

        EndOfFile
    }
}
