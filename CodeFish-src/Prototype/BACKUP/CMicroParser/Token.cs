using System;

namespace DDW
{
    [System.Diagnostics.DebuggerDisplay("ID = {ID}, Line = {Line}, Column={Col}")]
	public struct Token
	{
		public TokenID ID;
		public int Data; // index into data table
        public int Line;
        public int Col;
        /// <summary>
        /// while the lexer parse tokens, it makes some check 
        /// to determines if a character '<' may be a generic definition begin
        /// It optimize the generic parsing.
        /// 
        /// if this filed returns false -> this is not a lesser generic start
        /// </summary>
        public bool GenericStart;

        /// <summary>
        /// while the lexer parse tokens, it makes some check 
        /// to determines if a character '?' may be a nullable type declaration
        /// It optimize the conditionals parsing.
        /// 
        /// if this filed returns false -> this is not a nullable type '?'
        /// </summary>
        public bool NullableDeclaration;

		public Token(TokenID id)
		{
			this.ID = id;
			this.Data = -1;
            this.Line = 0;
            this.Col = 0;
            this.GenericStart = true;
            this.NullableDeclaration = true;
		}
		public Token(TokenID id, int data)
		{
			this.ID = id;
			this.Data = data;
            this.Line = 0;
            this.Col = 0;
            this.GenericStart = true;
            this.NullableDeclaration = true;
		}

		public Token(TokenID id, int data, int line, int col)
		{
			this.ID = id;
			this.Data = data;
            this.Line = line;
            this.Col = col;
            this.GenericStart = true;
            this.NullableDeclaration = true;
		}

        public Token(TokenID id, int line, int col)
        {
            this.ID = id;
            this.Data = -1;
            this.Line = line;
            this.Col = col;
            this.GenericStart = true;
            this.NullableDeclaration = true;
        }

		public override string ToString()
		{
			return this.ID.ToString();
		}
	}
}
