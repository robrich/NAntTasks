﻿using EcmaScript.NET;

namespace Yahoo.Yui.Compressor
{
    public class JavaScriptToken
    {
        public int TokenType { get; private set; }
        public string Value { get; private set; }

        public JavaScriptToken(int type, 
            string value)
        {
            TokenType = type;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("Type:[{0}/{1} Value: [{2}]",
                                 Token.name(TokenType),
                                 TokenType,
                                 Value);
        }
    }
}