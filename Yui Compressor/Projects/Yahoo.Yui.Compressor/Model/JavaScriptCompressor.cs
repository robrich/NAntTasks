﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using EcmaScript.NET;

namespace Yahoo.Yui.Compressor
{
    public class JavaScriptCompressor
    {
        #region Fields

        private const int BUILDING_SYMBOL_TREE = 1;
        private static readonly object _synLock = new object();

        private static readonly Regex SIMPLE_IDENTIFIER_NAME_PATTERN = new Regex("^[a-zA-Z_][a-zA-Z0-9_]*$",
                                                                                 RegexOptions.Compiled);

        private static HashSet<string> _builtin;

        public static int CHECKING_SYMBOL_TREE = 2;

        private readonly ScriptOrFunctionScope _globalScope = new ScriptOrFunctionScope(-1, null);
        private readonly Hashtable _indexedScopes = new Hashtable();
        private readonly bool _isEvalIgnored;
        private readonly ErrorReporter _logger;
        private readonly Stack _scopes = new Stack();
        private readonly ArrayList _tokens;
        private readonly bool _verbose;
        private int _braceNesting;
        private int _mode;
        private bool _munge;
        private int _offset;

        #endregion

        #region Properties

        internal static List<string> Ones;
        internal static List<string> Threes;
        internal static List<string> Twos;
        private static Hashtable Literals { get; set; }
        private static HashSet<string> Reserved { get; set; }

        public ErrorReporter ErrorReporter { get; private set; }

        #endregion

        #region Constructors

        public JavaScriptCompressor(string javaScript,
									string fileName) : this(javaScript,
															  fileName,
                                                              true)
        {
        }

        public JavaScriptCompressor(string javaScript,
									string fileName,
                                    bool isVerboseLogging)
            : this(javaScript,
				   fileName,
                   isVerboseLogging,
                   Encoding.Default,
                   CultureInfo.CreateSpecificCulture("en-GB"))
        {
        }

        public JavaScriptCompressor(string javaScript,
									string fileName,
                                    bool isVerboseLogging,
                                    Encoding encoding,
                                    CultureInfo threadCulture) : this(javaScript,
																	  fileName,
                                                                      isVerboseLogging,
                                                                      encoding,
                                                                      threadCulture,
                                                                      false,
                                                                      null)
        {
        }

        public JavaScriptCompressor(string javaScript,
									string fileName,
                                    bool isVerboseLogging,
                                    Encoding encoding,
                                    CultureInfo threadCulture,
                                    bool isEvalIgnored,
                                    ErrorReporter errorReporter)
        {
            if (string.IsNullOrEmpty(javaScript))
            {
                throw new ArgumentNullException("javaScript");
            }

            // Lets make sure the current thread is in english. This is because most javascript (yes, this also does css..)
            // must be in english in case a developer runs this on a non-english OS.
            // Reference: http://www.codeplex.com/YUICompressor/WorkItem/View.aspx?WorkItemId=3219
            Thread.CurrentThread.CurrentCulture = threadCulture;
            Thread.CurrentThread.CurrentUICulture = threadCulture;

            Initialise();

            _verbose = isVerboseLogging;

            var memoryStream = new MemoryStream(encoding.GetBytes(javaScript));
            ErrorReporter = errorReporter ?? new CustomErrorReporter(isVerboseLogging, fileName);
            _logger = ErrorReporter;
            _tokens = Parse(new StreamReader(memoryStream),
                            ErrorReporter);
            _isEvalIgnored = isEvalIgnored;
        }

        #endregion

        #region Methods

        #region Private Methods

        private static void InitialiseBuiltIn()
        {
            if (_builtin == null)
            {
                lock (_synLock)
                {
                    if (_builtin == null)
                    {
                        var builtin = new HashSet<string> {"NaN", "top"};
                        _builtin = builtin;
                    }
                }
            }
        }

        private static void InitialiseOnesList()
        {
            List<string> onesList;


            if (Ones == null)
            {
                lock (_synLock)
                {
                    if (Ones == null)
                    {
                        onesList = new List<string>();
                        for (char c = 'a'; c <= 'z'; c++)
                        {
                            onesList.Add(Convert.ToString(c, CultureInfo.InvariantCulture));
                        }

                        for (char c = 'A'; c <= 'Z'; c++)
                        {
                            onesList.Add(Convert.ToString(c, CultureInfo.InvariantCulture));
                        }

                        Ones = onesList;
                    }
                }
            }
        }

        private static void InitialiseTwosList()
        {
            List<string> twosList;


            if (Twos == null)
            {
                lock (_synLock)
                {
                    if (Twos == null)
                    {
                        twosList = new List<string>();

                        for (int i = 0; i < Ones.Count; i++)
                        {
                            string one = Ones[i];

                            for (char c = 'a'; c <= 'z'; c++)
                            {
                                twosList.Add(one + Convert.ToString(c, CultureInfo.InvariantCulture));
                            }

                            for (char c = 'A'; c <= 'Z'; c++)
                            {
                                twosList.Add(one + Convert.ToString(c, CultureInfo.InvariantCulture));
                            }

                            for (char c = '0'; c <= '9'; c++)
                            {
                                twosList.Add(one + Convert.ToString(c, CultureInfo.InvariantCulture));
                            }
                        }

                        // Remove two-letter JavaScript reserved words and built-in globals...
                        twosList.Remove("as");
                        twosList.Remove("is");
                        twosList.Remove("do");
                        twosList.Remove("if");
                        twosList.Remove("in");

                        foreach (string word in _builtin)
                        {
                            twosList.Remove(word);
                        }

                        Twos = twosList;
                    }
                }
            }
        }

        private static void InitialiseThreesList()
        {
            List<string> threesList;


            if (Threes == null)
            {
                lock (_synLock)
                {
                    if (Threes == null)
                    {
                        threesList = new List<string>();

                        for (int i = 0; i < Twos.Count; i++)
                        {
                            string two = Twos[i];

                            for (char c = 'a'; c <= 'z'; c++)
                            {
                                threesList.Add(two + Convert.ToString(c, CultureInfo.InvariantCulture));
                            }

                            for (char c = 'A'; c <= 'Z'; c++)
                            {
                                threesList.Add(two + Convert.ToString(c, CultureInfo.InvariantCulture));
                            }

                            for (char c = '0'; c <= '9'; c++)
                            {
                                threesList.Add(two + Convert.ToString(c, CultureInfo.InvariantCulture));
                            }
                        }

                        // Remove three-letter JavaScript reserved words and built-in globals...
                        threesList.Remove("for");
                        threesList.Remove("int");
                        threesList.Remove("new");
                        threesList.Remove("try");
                        threesList.Remove("use");
                        threesList.Remove("var");

                        foreach (string word in _builtin)
                        {
                            threesList.Remove(word);
                        }

                        Threes = threesList;
                    }
                }
            }
        }

        private static void InitialiseLiterals()
        {
            if (Literals == null)
            {
                lock (_synLock)
                {
                    if (Literals == null)
                    {
                        var literals = new Hashtable
                                           {
                                               {Token.GET, "get "},
                                               {Token.SET, "set "},
                                               {Token.TRUE, "true"},
                                               {Token.FALSE, "false"},
                                               {Token.NULL, "null"},
                                               {Token.THIS, "this"},
                                               {Token.FUNCTION, "function"},
                                               {Token.COMMA, ","},
                                               {Token.LC, "{"},
                                               {Token.RC, "}"},
                                               {Token.LP, "("},
                                               {Token.RP, ")"},
                                               {Token.LB, "["},
                                               {Token.RB, "]"},
                                               {Token.DOT, "."},
                                               {Token.NEW, "new "},
                                               {Token.DELPROP, "delete "},
                                               {Token.IF, "if"},
                                               {Token.ELSE, "else"},
                                               {Token.FOR, "for"},
                                               {Token.IN, " in "},
                                               {Token.WITH, "with"},
                                               {Token.WHILE, "while"},
                                               {Token.DO, "do"},
                                               {Token.TRY, "try"},
                                               {Token.CATCH, "catch"},
                                               {Token.FINALLY, "finally"},
                                               {Token.THROW, "throw"},
                                               {Token.SWITCH, "switch"},
                                               {Token.BREAK, "break"},
                                               {Token.CONTINUE, "continue"},
                                               {Token.CASE, "case"},
                                               {Token.DEFAULT, "default"},
                                               {Token.RETURN, "return"},
                                               {Token.VAR, "var "},
                                               {Token.SEMI, ";"},
                                               {Token.ASSIGN, "="},
                                               {Token.ASSIGN_ADD, "+="},
                                               {Token.ASSIGN_SUB, "-="},
                                               {Token.ASSIGN_MUL, "*="},
                                               {Token.ASSIGN_DIV, "/="},
                                               {Token.ASSIGN_MOD, "%="},
                                               {Token.ASSIGN_BITOR, "|="},
                                               {Token.ASSIGN_BITXOR, "^="},
                                               {Token.ASSIGN_BITAND, "&="},
                                               {Token.ASSIGN_LSH, "<<="},
                                               {Token.ASSIGN_RSH, ">>="},
                                               {Token.ASSIGN_URSH, ">>>="},
                                               {Token.HOOK, "?"},
                                               {Token.OBJECTLIT, ":"},
                                               {Token.COLON, ":"},
                                               {Token.OR, "||"},
                                               {Token.AND, "&&"},
                                               {Token.BITOR, "|"},
                                               {Token.BITXOR, "^"},
                                               {Token.BITAND, "&"},
                                               {Token.SHEQ, "==="},
                                               {Token.SHNE, "!=="},
                                               {Token.EQ, "=="},
                                               {Token.NE, "!="},
                                               {Token.LE, "<="},
                                               {Token.LT, "<"},
                                               {Token.GE, ">="},
                                               {Token.GT, ">"},
                                               {Token.INSTANCEOF, " instanceof "},
                                               {Token.LSH, "<<"},
                                               {Token.RSH, ">>"},
                                               {Token.URSH, ">>>"},
                                               {Token.TYPEOF, "typeof"},
                                               {Token.VOID, "void "},
                                               {Token.CONST, "const "},
                                               {Token.NOT, "!"},
                                               {Token.BITNOT, "~"},
                                               {Token.POS, "+"},
                                               {Token.NEG, "-"},
                                               {Token.INC, "++"},
                                               {Token.DEC, "--"},
                                               {Token.ADD, "+"},
                                               {Token.SUB, "-"},
                                               {Token.MUL, "*"},
                                               {Token.DIV, "/"},
                                               {Token.MOD, "%"},
                                               {Token.COLONCOLON, "::"},
                                               {Token.DOTDOT, ".."},
                                               {Token.DOTQUERY, ".("},
                                               {Token.XMLATTR, "@"}
                                           };

                        Literals = literals;
                    }
                }
            }
        }

        private static void InitialiseReserved()
        {
            if (Reserved == null)
            {
                lock (_synLock)
                {
                    if (Reserved == null)
                    {
                        // See http://developer.mozilla.org/en/docs/Core_JavaScript_1.5_Reference:Reserved_Words

                        // JavaScript 1.5 reserved words
                        // Words reserved for future use
                        // These are not reserved, but should be taken into account
                        // in isValidIdentifier (See jslint source code)

                        var reserved = new HashSet<string>
                                           {
                                               "break",
                                               "case",
                                               "catch",
                                               "continue",
                                               "default",
                                               "delete",
                                               "do",
                                               "else",
                                               "finally",
                                               "for",
                                               "function",
                                               "if",
                                               "in",
                                               "instanceof",
                                               "new",
                                               "return",
                                               "switch",
                                               "this",
                                               "throw",
                                               "try",
                                               "typeof",
                                               "var",
                                               "void",
                                               "while",
                                               "with",
                                               "abstract",
                                               "boolean",
                                               "byte",
                                               "char",
                                               "class",
                                               "const",
                                               "debugger",
                                               "double",
                                               "enum",
                                               "export",
                                               "extends",
                                               "final",
                                               "float",
                                               "goto",
                                               "implements",
                                               "import",
                                               "int",
                                               "interface",
                                               "long",
                                               "native",
                                               "package",
                                               "private",
                                               "protected",
                                               "public",
                                               "short",
                                               "static",
                                               "super",
                                               "synchronized",
                                               "throws",
                                               "transient",
                                               "volatile",
                                               "arguments",
                                               "eval",
                                               "true",
                                               "false",
                                               "Infinity",
                                               "NaN",
                                               "null",
                                               "undefined"
                                           };

                        Reserved = reserved;
                    }
                }
            }
        }

        private static void Initialise()
        {
            InitialiseBuiltIn();
            InitialiseOnesList();
            InitialiseTwosList();
            InitialiseThreesList();
            InitialiseLiterals();
            InitialiseReserved();
        }

        private static int CountChar(string haystack,
                                     char needle)
        {
            int index = 0;
            int count = 0;
            int length = haystack.Length;


            while (index < length)
            {
                char c = haystack[index++];
                if (c == needle)
                {
                    count++;
                }
            }

            return count;
        }

        private static int PrintSourceString(string source,
                                             int offset,
                                             StringBuilder stringBuilder)
        {
            int length = source[offset];
            ++offset;
            if ((0x8000 & length) != 0)
            {
                length = ((0x7FFF & length) << 16) | source[offset];
                ++offset;
            }
            if (stringBuilder != null)
            {
                //string word = source.Substring(offset, offset + length);
                string word = source.Substring(offset, length);
                stringBuilder.Append(word);
            }

            return offset + length;
        }

        private static int PrintSourceNumber(string source,
                                             int offset,
                                             StringBuilder stringBuilder)
        {
            double number = 0.0;
            char type = source[offset];
            ++offset;
            if (type == 'S')
            {
                if (stringBuilder != null)
                {
                    number = source[offset];
                }
                ++offset;
            }
            else if (type == 'J' || type == 'D')
            {
                if (stringBuilder != null)
                {
                    long lbits = (long) source[offset] << 48;
                    lbits |= (long) source[offset + 1] << 32;
                    lbits |= (long) source[offset + 2] << 16;
                    lbits |= source[offset + 3];
                    number = type == 'J' ? lbits : BitConverter.Int64BitsToDouble(lbits);
                }
                offset += 4;
            }
            else
            {
                // Bad source
                throw new InvalidOperationException();
            }

            if (stringBuilder != null)
            {
                stringBuilder.Append(ScriptConvert.ToString(number, 10));
            }

            return offset;
        }

        private static ArrayList Parse(StreamReader stream,
                                       ErrorReporter reporter)
        {
            var compilerEnvirons = new CompilerEnvirons();
            var parser = new Parser(compilerEnvirons, reporter);
            parser.Parse(stream, null, 1);
            string source = parser.EncodedSource;

            int offset = 0;
            int length = source.Length;
            var tokens = new ArrayList();
            var stringBuilder = new StringBuilder();

            while (offset < length)
            {
                int tt = source[offset++];
                switch (tt)
                {
                    case Token.CONDCOMMENT:
                    case Token.KEEPCOMMENT:
                    case Token.NAME:
                    case Token.REGEXP:
                    case Token.STRING:
                        stringBuilder.Length = 0;
                        offset = PrintSourceString(source,
                                                   offset,
                                                   stringBuilder);
                        tokens.Add(new JavaScriptToken(tt, stringBuilder.ToString()));
                        break;

                    case Token.NUMBER:
                        stringBuilder.Length = 0;
                        offset = PrintSourceNumber(source, offset, stringBuilder);
                        tokens.Add(new JavaScriptToken(tt, stringBuilder.ToString()));
                        break;

                    default:
                        var literal = (string) Literals[tt];
                        if (literal != null)
                        {
                            tokens.Add(new JavaScriptToken(tt, literal));
                        }
                        break;
                }
            }

            return tokens;
        }

        private static void ProcessStringLiterals(IList tokens, bool merge)
        {
            string tv;
            int i, length = tokens.Count;
            JavaScriptToken token, prevToken, nextToken;

            if (merge)
            {
                // Concatenate string literals that are being appended wherever
                // it is safe to do so. Note that we take care of the case:
                //     "a" + "b".toUpperCase()

                for (i = 0; i < length; i++)
                {
                    token = (JavaScriptToken) tokens[i];
                    switch (token.TokenType)
                    {
                        case Token.ADD:
                            if (i > 0 && i < length)
                            {
                                prevToken = (JavaScriptToken) tokens[i - 1];
                                nextToken = (JavaScriptToken) tokens[i + 1];
                                if (prevToken.TokenType == Token.STRING && nextToken.TokenType == Token.STRING &&
                                    (i == length - 1 || ((JavaScriptToken) tokens[i + 2]).TokenType != Token.DOT))
                                {
                                    tokens[i - 1] = new JavaScriptToken(Token.STRING,
                                                                        prevToken.Value + nextToken.Value);
                                    tokens.RemoveAt(i + 1);
                                    tokens.RemoveAt(i);
                                    i = i - 1;
                                    length = length - 2;
                                    break;
                                }
                            }
                            break;
                    }
                }
            }

            // Second pass...
            for (i = 0; i < length; i++)
            {
                token = (JavaScriptToken) tokens[i];
                if (token.TokenType == Token.STRING)
                {
                    tv = token.Value;

                    // Finally, add the quoting characters and escape the string. We use
                    // the quoting character that minimizes the amount of escaping to save
                    // a few additional bytes.

                    int singleQuoteCount = CountChar(tv, '\'');
                    int doubleQuoteCount = CountChar(tv, '"');
                    char quotechar = doubleQuoteCount <= singleQuoteCount ? '"' : '\'';

                    tv = quotechar + EscapeString(tv, quotechar) + quotechar;

                    // String concatenation transforms the old script scheme:
                    //     '<scr'+'ipt ...><'+'/script>'
                    // into the following:
                    //     '<script ...></script>'
                    // which breaks if this code is embedded inside an HTML document.
                    // Since this is not the right way to do this, let's fix the code by
                    // transforming all "</script" into "<\/script"

                    if (tv.IndexOf("</script", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        tv = tv.Replace("<\\/script", "<\\\\/script");
                    }

                    tokens[i] = new JavaScriptToken(Token.STRING, tv);
                }
            }
        }

        // Add necessary escaping that was removed in Rhino's tokenizer.
        private static string EscapeString(string s,
                                           char quotechar)
        {
            if (quotechar != '"' &&
                quotechar != '\'')
            {
                throw new ArgumentException("quotechar argument has to be a \" or a \\ character only.",
                                            "quotechar");
            }
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            var stringBuilder = new StringBuilder();
            for (int i = 0, length = s.Length; i < length; i++)
            {
                int c = s[i];
                if (c == quotechar)
                {
                    stringBuilder.Append("\\");
                }

                stringBuilder.Append((char) c);
            }

            return stringBuilder.ToString();
        }

        /*
         * Simple check to see whether a string is a valid identifier name.
         * If a string matches this pattern, it means it IS a valid
         * identifier name. If a string doesn't match it, it does not
         * necessarily mean it is not a valid identifier name.
         */

        private static bool IsValidIdentifier(string s)
        {
            Match match = SIMPLE_IDENTIFIER_NAME_PATTERN.Match(s);
            return (match.Success && !Reserved.Contains(s));
        }

        /*
        * Transforms obj["foo"] into obj.foo whenever possible, saving 3 bytes.
        */

        private static void OptimizeObjectMemberAccess(IList tokens)
        {
            string tv;
            int i, length;
            JavaScriptToken token;


            for (i = 0, length = tokens.Count; i < length; i++)
            {
                if (((JavaScriptToken) tokens[i]).TokenType == Token.LB &&
                    i > 0 && i < length - 2 &&
                    ((JavaScriptToken) tokens[i - 1]).TokenType == Token.NAME &&
                    ((JavaScriptToken) tokens[i + 1]).TokenType == Token.STRING &&
                    ((JavaScriptToken) tokens[i + 2]).TokenType == Token.RB)
                {
                    token = (JavaScriptToken) tokens[i + 1];
                    tv = token.Value;
                    tv = tv.Substring(1, tv.Length - 2);
                    if (IsValidIdentifier(tv))
                    {
                        tokens[i] = new JavaScriptToken(Token.DOT, ".");
                        tokens[i + 1] = new JavaScriptToken(Token.NAME, tv);
                        tokens.RemoveAt(i + 2);
                        i = i + 2;
                        length = length - 1;
                    }
                }
            }
        }

        /*
         * Transforms 'foo': ... into foo: ... whenever possible, saving 2 bytes.
        */

        private static void OptimizeObjLitMemberDecl(IList tokens)
        {
            string tv;
            int i, length;
            JavaScriptToken token;


            for (i = 0, length = tokens.Count; i < length; i++)
            {
                if (((JavaScriptToken) tokens[i]).TokenType == Token.OBJECTLIT &&
                    i > 0 && ((JavaScriptToken) tokens[i - 1]).TokenType == Token.STRING)
                {
                    token = (JavaScriptToken) tokens[i - 1];
                    tv = token.Value;
                    tv = tv.Substring(1, tv.Length - 2);
                    if (IsValidIdentifier(tv))
                    {
                        tokens[i - 1] = new JavaScriptToken(Token.NAME, tv);
                    }
                }
            }
        }

        private ScriptOrFunctionScope GetCurrentScope()
        {
            return (ScriptOrFunctionScope) _scopes.Peek();
        }

        private void EnterScope(ScriptOrFunctionScope scope)
        {
            _scopes.Push(scope);
        }

        private void LeaveCurrentScope()
        {
            _scopes.Pop();
        }

        private JavaScriptToken ConsumeToken()
        {
            return (JavaScriptToken) _tokens[_offset++];
        }

        private JavaScriptToken GetToken(int delta)
        {
            return (JavaScriptToken) _tokens[_offset + delta];
        }

        /*
         * Returns the identifier for the specified symbol defined in
         * the specified scope or in any scope above it. Returns null
         * if this symbol does not have a corresponding identifier.
         */

        private static JavaScriptIdentifier GetIdentifier(string symbol,
                                                          ScriptOrFunctionScope scope)
        {
            while (scope != null)
            {
                JavaScriptIdentifier identifier = scope.GetIdentifier(symbol);
                if (identifier != null)
                {
                    return identifier;
                }

                scope = scope.ParentScope;
            }

            return null;
        }

        /*
         * If either 'eval' or 'with' is used in a local scope, we must make
         * sure that all containing local scopes don't get munged. Otherwise,
         * the obfuscation would potentially introduce bugs.
         */

        private void ProtectScopeFromObfuscation(ScriptOrFunctionScope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }

            if (scope == _globalScope)
            {
                // The global scope does not get obfuscated,
                // so we don't need to worry about it...
                return;
            }

            // Find the highest local scope containing the specified scope.
            while (scope.ParentScope != _globalScope)
            {
                scope = scope.ParentScope;
            }

            if (scope.ParentScope != _globalScope)
            {
                throw new InvalidOperationException();
            }

            scope.PreventMunging();
        }

        private String GetDebugString(int max)
        {
            if (max <= 0)
            {
                throw new ArgumentOutOfRangeException("max");
            }

            var result = new StringBuilder();
            int start = Math.Max(_offset - max, 0);
            int end = Math.Min(_offset + max, _tokens.Count);

            for (int i = start; i < end; i++)
            {
                var token = (JavaScriptToken) _tokens[i];
                if (i == _offset - 1)
                {
                    result.Append(" ---> ");
                }

                result.Append(token.Value);

                if (i == _offset - 1)
                {
                    result.Append(" <--- ");
                }
            }

            return result.ToString();
        }

        private void Warn(string message,
                          bool showDebugString)
        {
            if (_verbose)
            {
                if (showDebugString)
                {
                    message = message + Environment.NewLine + GetDebugString(10);
                }

                _logger.Warning(message, null, -1, null, -1);
            }
        }

        private void ParseFunctionDeclaration()
        {
            string symbol;
            ScriptOrFunctionScope functionScope;
            JavaScriptIdentifier identifier;


            ScriptOrFunctionScope currentScope = GetCurrentScope();

            JavaScriptToken token = ConsumeToken();

            if (token.TokenType == Token.NAME)
            {
                if (_mode == BUILDING_SYMBOL_TREE)
                {
                    // Get the name of the function and declare it in the current scope.
                    symbol = token.Value;
                    if (currentScope.GetIdentifier(symbol) != null)
                    {
                        Warn("The function " + symbol + " has already been declared in the same scope...", true);
                    }

                    currentScope.DeclareIdentifier(symbol);
                }

                token = ConsumeToken();
            }

            if (token.TokenType != Token.LP)
            {
                throw new InvalidOperationException();
            }

            if (_mode == BUILDING_SYMBOL_TREE)
            {
                functionScope = new ScriptOrFunctionScope(_braceNesting, currentScope);
                _indexedScopes.Add(_offset, functionScope);
            }
            else
            {
                functionScope = (ScriptOrFunctionScope) _indexedScopes[_offset];
            }

            // Parse function arguments.
            int argpos = 0;
            while ((token = ConsumeToken()).TokenType != Token.RP)
            {
                if (token.TokenType != Token.NAME &&
                    token.TokenType != Token.COMMA)
                {
                    throw new InvalidOperationException();
                }

                if (token.TokenType == Token.NAME &&
                    _mode == BUILDING_SYMBOL_TREE)
                {
                    symbol = token.Value;
                    identifier = functionScope.DeclareIdentifier(symbol);

                    if (symbol.Equals("$super", StringComparison.OrdinalIgnoreCase) && argpos == 0)
                    {
                        // Exception for Prototype 1.6...
                        identifier.MarkedForMunging = false;
                    }

                    argpos++;
                }
            }

            token = ConsumeToken();

            if (token.TokenType != Token.LC)
            {
                throw new InvalidOperationException();
            }

            _braceNesting++;

            token = GetToken(0);
            if (token.TokenType == Token.STRING &&
                GetToken(1).TokenType == Token.SEMI)
            {
                // This is a hint. Hints are empty statements that look like
                // "localvar1:nomunge, localvar2:nomunge"; They allow developers
                // to prevent specific symbols from getting obfuscated (some heretic
                // implementations, such as Prototype 1.6, require specific variable
                // names, such as $super for example, in order to work appropriately.
                // Note: right now, only "nomunge" is supported in the right hand side
                // of a hint. However, in the future, the right hand side may contain
                // other values.
                ConsumeToken();
                string hints = token.Value;

                // Remove the leading and trailing quotes...
                hints = hints.Substring(1, hints.Length - 1).Trim();

                foreach (string hint in hints.Split(','))
                {
                    int idx = hint.IndexOf(':');
                    if (idx <= 0 || idx >= hint.Length - 1)
                    {
                        if (_mode == BUILDING_SYMBOL_TREE)
                        {
                            // No need to report the error twice, hence the test...
                            Warn("Invalid hint syntax: " + hint, true);
                        }

                        break;
                    }

                    string variableName = hint.Substring(0, idx).Trim();
                    string variableType = hint.Substring(idx + 1).Trim();
                    if (_mode == BUILDING_SYMBOL_TREE)
                    {
                        functionScope.AddHint(variableName, variableType);
                    }
                    else if (_mode == CHECKING_SYMBOL_TREE)
                    {
                        identifier = functionScope.GetIdentifier(variableName);
                        if (identifier != null)
                        {
                            if (variableType.Equals("nomunge", StringComparison.OrdinalIgnoreCase))
                            {
                                identifier.MarkedForMunging = false;
                            }
                            else
                            {
                                Warn("Unsupported hint value: " + hint, true);
                            }
                        }
                        else
                        {
                            Warn("Hint refers to an unknown identifier: " + hint, true);
                        }
                    }
                }
            }

            ParseScope(functionScope);
        }

        private void ParseCatch()
        {
            JavaScriptIdentifier identifier;

            JavaScriptToken token = GetToken(-1);
            if (token.TokenType != Token.CATCH)
            {
                throw new InvalidOperationException();
            }

            token = ConsumeToken();
            if (token.TokenType != Token.LP)
            {
                throw new InvalidOperationException();
            }

            token = ConsumeToken();
            if (token.TokenType != Token.NAME)
            {
                throw new InvalidOperationException();
            }

            string symbol = token.Value;
            ScriptOrFunctionScope currentScope = GetCurrentScope();

            if (_mode == BUILDING_SYMBOL_TREE)
            {
                // We must declare the exception identifier in the containing function
                // scope to avoid errors related to the obfuscation process. No need to
                // display a warning if the symbol was already declared here...
                currentScope.DeclareIdentifier(symbol);
            }
            else
            {
                identifier = GetIdentifier(symbol, currentScope);
                identifier.RefCount++;
            }

            token = ConsumeToken();
            if (token.TokenType != Token.RP)
            {
                throw new InvalidOperationException();
            }
        }

        private void ParseExpression()
        {
            // Parse the expression until we encounter a comma or a semi-colon
            // in the same brace nesting, bracket nesting and paren nesting.
            // Parse functions if any...

            string symbol;
            JavaScriptIdentifier identifier;


            int expressionBraceNesting = _braceNesting;
            int bracketNesting = 0;
            int parensNesting = 0;
            int length = _tokens.Count;

            while (_offset < length)
            {
                JavaScriptToken token = ConsumeToken();
                ScriptOrFunctionScope currentScope = GetCurrentScope();

                switch (token.TokenType)
                {
                    case Token.SEMI:
                    case Token.COMMA:
                        if (_braceNesting == expressionBraceNesting &&
                            bracketNesting == 0 &&
                            parensNesting == 0)
                        {
                            return;
                        }
                        break;

                    case Token.FUNCTION:
                        ParseFunctionDeclaration();
                        break;

                    case Token.LC:
                        _braceNesting++;
                        break;

                    case Token.RC:
                        _braceNesting--;
                        if (_braceNesting < expressionBraceNesting)
                        {
                            throw new InvalidOperationException();
                        }
                        break;

                    case Token.LB:
                        bracketNesting++;
                        break;

                    case Token.RB:
                        bracketNesting--;
                        break;

                    case Token.LP:
                        parensNesting++;
                        break;

                    case Token.RP:
                        parensNesting--;
                        break;

                    case Token.CONDCOMMENT:
                        if (_mode == BUILDING_SYMBOL_TREE)
                        {
                            ProtectScopeFromObfuscation(currentScope);
                            Warn(
                                "Using JScript conditional comments is not recommended." +
                                (_munge
                                     ? " Moreover, using JScript conditional comments reduces the level of compression!"
                                     : ""), true);
                        }
                        break;

                    case Token.NAME:
                        symbol = token.Value;

                        if (_mode == BUILDING_SYMBOL_TREE)
                        {
                            if (!_isEvalIgnored &&
                                symbol.Equals("eval", StringComparison.OrdinalIgnoreCase))
                            {
                                ProtectScopeFromObfuscation(currentScope);
                                Warn(
                                    "Using 'eval' is not recommended." +
                                    (_munge ? " Moreover, using 'eval' reduces the level of compression!" : ""),
                                    true);
                            }
                        }
                        else if (_mode == CHECKING_SYMBOL_TREE)
                        {
                            if ((_offset < 2 ||
                                 (GetToken(-2).TokenType != Token.DOT &&
                                  GetToken(-2).TokenType != Token.GET &&
                                  GetToken(-2).TokenType != Token.SET)) &&
                                GetToken(0).TokenType != Token.OBJECTLIT)
                            {
                                identifier = GetIdentifier(symbol, currentScope);

                                if (identifier == null)
                                {
                                    if (symbol.Length <= 3 && !_builtin.Contains(symbol))
                                    {
                                        // Here, we found an undeclared and un-namespaced symbol that is
                                        // 3 characters or less in length. Declare it in the global scope.
                                        // We don't need to declare longer symbols since they won't cause
                                        // any conflict with other munged symbols.
                                        _globalScope.DeclareIdentifier(symbol);
                                        Warn("Found an undeclared symbol: " + symbol, true);
                                    }
                                }
                                else
                                {
                                    identifier.RefCount++;
                                }
                            }
                        }

                        break;
                }
            }
        }

        private void ParseScope(ScriptOrFunctionScope scope)
        {
            string symbol;
            JavaScriptToken token;
            JavaScriptIdentifier identifier;


            int length = _tokens.Count;

            EnterScope(scope);

            while (_offset < length)
            {
                token = ConsumeToken();

                switch (token.TokenType)
                {
                    case Token.VAR:
                    case Token.CONST:
                        if (token.TokenType == Token.VAR)
                        {
                            if (_mode == BUILDING_SYMBOL_TREE &&
                                scope.VarCount++ > 1)
                            {
                                Warn("Try to use a single 'var' statement per scope.", true);
                            }
                        }

                        // The var keyword is followed by at least one symbol name.
                        // If several symbols follow, they are comma separated.
                        //for (; ;)
                        while (true)
                        {
                            token = ConsumeToken();
                            if (token.TokenType != Token.NAME)
                            {
                                throw new InvalidOperationException();
                            }

                            if (_mode == BUILDING_SYMBOL_TREE)
                            {
                                symbol = token.Value;
                                if (scope.GetIdentifier(symbol) == null)
                                {
                                    scope.DeclareIdentifier(symbol);
                                }
                                else
                                {
                                    Warn(
                                        "The variable " + symbol + " has already been declared in the same scope...",
                                        true);
                                }
                            }

                            token = GetToken(0);
                            if (token.TokenType != Token.SEMI &&
                                token.TokenType != Token.ASSIGN &&
                                token.TokenType != Token.COMMA &&
                                token.TokenType != Token.IN)
                            {
                                throw new InvalidOperationException();
                            }

                            if (token.TokenType == Token.IN)
                            {
                                break;
                            }

                            ParseExpression();
                            token = GetToken(-1);
                            if (token.TokenType == Token.SEMI)
                            {
                                break;
                            }
                        }

                        break;

                    case Token.FUNCTION:
                        ParseFunctionDeclaration();
                        break;

                    case Token.LC:
                        _braceNesting++;
                        break;

                    case Token.RC:
                        _braceNesting--;
                        if (_braceNesting < scope.BraceNesting)
                        {
                            throw new InvalidOperationException();
                        }

                        if (_braceNesting == scope.BraceNesting)
                        {
                            LeaveCurrentScope();
                            return;
                        }

                        break;

                    case Token.WITH:
                        if (_mode == BUILDING_SYMBOL_TREE)
                        {
                            // Inside a 'with' block, it is impossible to figure out
                            // statically whether a symbol is a local variable or an
                            // object member. As a consequence, the only thing we can
                            // do is turn the obfuscation off for the highest scope
                            // containing the 'with' block.
                            ProtectScopeFromObfuscation(scope);
                            Warn(
                                "Using 'with' is not recommended." +
                                (_munge ? " Moreover, using 'with' reduces the level of compression!" : ""), true);
                        }
                        break;

                    case Token.CATCH:
                        ParseCatch();
                        break;

                    case Token.CONDCOMMENT:
                        if (_mode == BUILDING_SYMBOL_TREE)
                        {
                            ProtectScopeFromObfuscation(scope);
                            Warn(
                                "Using JScript conditional comments is not recommended." +
                                (_munge
                                     ? " Moreover, using JScript conditional comments reduces the level of compression."
                                     : ""), true);
                        }
                        break;

                    case Token.NAME:
                        symbol = token.Value;

                        if (_mode == BUILDING_SYMBOL_TREE)
                        {
                            if (!_isEvalIgnored &&
                                symbol.Equals("eval", StringComparison.OrdinalIgnoreCase))
                            {
                                ProtectScopeFromObfuscation(scope);
                                Warn(
                                    "Using 'eval' is not recommended." +
                                    (_munge ? " Moreover, using 'eval' reduces the level of compression!" : ""),
                                    true);
                            }
                        }
                        else if (_mode == CHECKING_SYMBOL_TREE)
                        {
                            if ((_offset < 2 ||
                                 GetToken(-2).TokenType != Token.DOT) &&
                                GetToken(0).TokenType != Token.OBJECTLIT)
                            {
                                identifier = GetIdentifier(symbol, scope);

                                if (identifier == null)
                                {
                                    if (symbol.Length <= 3 &&
                                        !_builtin.Contains(symbol))
                                    {
                                        // Here, we found an undeclared and un-namespaced symbol that is
                                        // 3 characters or less in length. Declare it in the global scope.
                                        // We don't need to declare longer symbols since they won't cause
                                        // any conflict with other munged symbols.
                                        _globalScope.DeclareIdentifier(symbol);
                                        Warn("Found an undeclared symbol: " + symbol, true);
                                    }
                                }
                                else
                                {
                                    identifier.RefCount++;
                                }
                            }
                        }

                        break;
                }
            }
        }

        private void BuildSymbolTree()
        {
            _offset = 0;
            _braceNesting = 0;
            _scopes.Clear();
            _indexedScopes.Clear();
            _indexedScopes.Add(0, _globalScope);
            _mode = BUILDING_SYMBOL_TREE;
            ParseScope(_globalScope);
        }

        private void MungeSymboltree()
        {
            if (!_munge)
            {
                return;
            }

            // One problem with obfuscation resides in the use of undeclared
            // and un-namespaced global symbols that are 3 characters or less
            // in length. Here is an example:
            //
            //     var declaredGlobalVar;
            //
            //     function declaredGlobalFn() {
            //         var localvar;
            //         localvar = abc; // abc is an undeclared global symbol
            //     }
            //
            // In the example above, there is a slim chance that localvar may be
            // munged to 'abc', conflicting with the undeclared global symbol
            // abc, creating a potential bug. The following code detects such
            // global symbols. This must be done AFTER the entire file has been
            // parsed, and BEFORE munging the symbol tree. Note that declaring
            // extra symbols in the global scope won't hurt.
            //
            // Note: Since we go through all the tokens to do this, we also use
            // the opportunity to count how many times each identifier is used.

            _offset = 0;
            _braceNesting = 0;
            _scopes.Clear();
            _mode = CHECKING_SYMBOL_TREE;
            ParseScope(_globalScope);
            _globalScope.Munge();
        }

        private StringBuilder PrintSymbolTree(int linebreakpos,
                                              bool preserveAllSemiColons)
        {
            _offset = 0;
            _braceNesting = 0;
            _scopes.Clear();

            JavaScriptToken token;
            ScriptOrFunctionScope currentScope;
            JavaScriptIdentifier identifier;

            int length = _tokens.Count;
            var result = new StringBuilder();

            int linestartpos = 0;

            EnterScope(_globalScope);

            while (_offset < length)
            {
                token = ConsumeToken();
                string symbol = token.Value;
                currentScope = GetCurrentScope();

                switch (token.TokenType)
                {
                    case Token.NAME:
                        if (_offset >= 2 &&
                            GetToken(-2).TokenType == Token.DOT ||
                            GetToken(0).TokenType == Token.OBJECTLIT)
                        {
                            result.Append(symbol);
                        }
                        else
                        {
                            identifier = GetIdentifier(symbol, currentScope);

                            if (identifier != null)
                            {
                                if (identifier.MungedValue != null)
                                {
                                    result.Append(identifier.MungedValue);
                                }
                                else
                                {
                                    result.Append(symbol);
                                }

                                if (currentScope != _globalScope &&
                                    identifier.RefCount == 0)
                                {
                                    Warn(
                                        "The symbol " + symbol + " is declared but is apparently never used." +
                                        Environment.NewLine + "This code can probably be written in a more compact way.",
                                        true);
                                }
                            }
                            else
                            {
                                result.Append(symbol);
                            }
                        }
                        break;

                    case Token.REGEXP:
                    case Token.NUMBER:
                    case Token.STRING:
                        result.Append(symbol);
                        break;

                    case Token.ADD:
                    case Token.SUB:
                        result.Append((string) Literals[token.TokenType]);

                        if (_offset < length)
                        {
                            token = GetToken(0);
                            if (token.TokenType == Token.INC ||
                                token.TokenType == Token.DEC ||
                                token.TokenType == Token.ADD ||
                                token.TokenType == Token.DEC)
                            {
                                // Handle the case x +/- ++/-- y
                                // We must keep a white space here. Otherwise, x +++ y would be
                                // interpreted as x ++ + y by the compiler, which is a bug (due
                                // to the implicit assignment being done on the wrong variable)
                                result.Append(' ');
                            }
                            else if (token.TokenType == Token.POS &&
                                     GetToken(-1).TokenType == Token.ADD ||
                                     token.TokenType == Token.NEG &&
                                     GetToken(-1).TokenType == Token.SUB)
                            {
                                // Handle the case x + + y and x - - y
                                result.Append(' ');
                            }
                        }
                        break;

                    case Token.FUNCTION:
                        result.Append("function");
                        token = ConsumeToken();

                        if (token.TokenType == Token.NAME)
                        {
                            result.Append(' ');
                            symbol = token.Value;
                            identifier = GetIdentifier(symbol, currentScope);

                            if (identifier == null)
                            {
                                throw new InvalidOperationException();
                            }

                            if (identifier.MungedValue != null)
                            {
                                result.Append(identifier.MungedValue);
                            }
                            else
                            {
                                result.Append(symbol);
                            }

                            if (currentScope != _globalScope &&
                                identifier.RefCount == 0)
                            {
                                Warn(
                                    "The symbol " + symbol + " is declared but is apparently never used." +
                                    Environment.NewLine + "This code can probably be written in a more compact way.",
                                    true);
                            }

                            token = ConsumeToken();
                        }

                        if (token.TokenType != Token.LP)
                        {
                            throw new InvalidOperationException();
                        }

                        result.Append('(');
                        currentScope = (ScriptOrFunctionScope) _indexedScopes[_offset];
                        EnterScope(currentScope);

                        while ((token = ConsumeToken()).TokenType != Token.RP)
                        {
                            if (token.TokenType != Token.NAME &&
                                token.TokenType != Token.COMMA)
                            {
                                throw new InvalidOperationException();
                            }

                            if (token.TokenType == Token.NAME)
                            {
                                symbol = token.Value;
                                identifier = GetIdentifier(symbol, currentScope);

                                if (identifier == null)
                                {
                                    throw new InvalidOperationException();
                                }

                                if (identifier.MungedValue != null)
                                {
                                    result.Append(identifier.MungedValue);
                                }
                                else
                                {
                                    result.Append(symbol);
                                }
                            }
                            else if (token.TokenType == Token.COMMA)
                            {
                                result.Append(',');
                            }
                        }

                        result.Append(')');
                        token = ConsumeToken();

                        if (token.TokenType != Token.LC)
                        {
                            throw new InvalidOperationException();
                        }

                        result.Append('{');
                        _braceNesting++;
                        token = GetToken(0);

                        if (token.TokenType == Token.STRING &&
                            GetToken(1).TokenType == Token.SEMI)
                        {
                            // This is a hint. Skip it!
                            ConsumeToken();
                            ConsumeToken();
                        }
                        break;

                    case Token.RETURN:
                    case Token.TYPEOF:
                        result.Append((string) Literals[token.TokenType]);
                        // No space needed after 'return' and 'typeof' when followed
                        // by '(', '[', '{', a string or a regexp.
                        if (_offset < length)
                        {
                            token = GetToken(0);
                            if (token.TokenType != Token.LP &&
                                token.TokenType != Token.LB &&
                                token.TokenType != Token.LC &&
                                token.TokenType != Token.STRING &&
                                token.TokenType != Token.REGEXP &&
                                token.TokenType != Token.SEMI)
                            {
                                result.Append(' ');
                            }
                        }
                        break;

                    case Token.CASE:
                    case Token.THROW:
                        result.Append((string) Literals[token.TokenType]);
                        // White-space needed after 'case' and 'throw' when not followed by a string.
                        if (_offset < length &&
                            GetToken(0).TokenType != Token.STRING)
                        {
                            result.Append(' ');
                        }
                        break;

                    case Token.BREAK:
                    case Token.CONTINUE:
                        result.Append((string) Literals[token.TokenType]);

                        if (_offset < length &&
                            GetToken(0).TokenType != Token.SEMI)
                        {
                            // If 'break' or 'continue' is not followed by a semi-colon, it must
                            // be followed by a label, hence the need for a white space.
                            result.Append(' ');
                        }
                        break;

                    case Token.LC:
                        result.Append('{');
                        _braceNesting++;
                        break;

                    case Token.RC:
                        result.Append('}');
                        _braceNesting--;
                        if (_braceNesting < currentScope.BraceNesting)
                        {
                            throw new InvalidOperationException();
                        }

                        if (_braceNesting == currentScope.BraceNesting)
                        {
                            LeaveCurrentScope();
                        }
                        break;

                    case Token.SEMI:
                        // No need to output a semi-colon if the next character is a right-curly...
                        if (preserveAllSemiColons ||
                            _offset < length &&
                            GetToken(0).TokenType != Token.RC)
                        {
                            result.Append(';');
                        }

                        if (linebreakpos >= 0 &&
                            result.Length - linestartpos > linebreakpos)
                        {
                            // Some source control tools don't like it when files containing lines longer
                            // than, say 8000 characters, are checked in. The linebreak option is used in
                            // that case to split long lines after a specific column.
                            result.Append('\n');
                            linestartpos = result.Length;
                        }
                        break;

                    case Token.CONDCOMMENT:
                    case Token.KEEPCOMMENT:
                        if (result.Length > 0 &&
                            result[result.Length - 1] != '\n')
                        {
                            result.Append("\n");
                        }

                        result.Append("/*");
                        result.Append(symbol);
                        result.Append("*/\n");
                        break;

                    default:
                        var literal = (string) Literals[token.TokenType];
                        if (literal != null)
                        {
                            result.Append(literal);
                        }
                        else
                        {
                            Warn("This symbol cannot be printed: " + symbol, true);
                        }
                        break;
                }
            }

            // Append a semi-colon at the end, even if unnecessary semi-colons are
            // supposed to be removed. This is especially useful when concatenating
            // several minified files (the absence of an ending semi-colon at the
            // end of one file may very likely cause a syntax error)
            if (!preserveAllSemiColons &&
                result.Length > 0 &&
                GetToken(-1).TokenType != Token.CONDCOMMENT &&
                GetToken(-1).TokenType != Token.KEEPCOMMENT)
            {
                if (result[result.Length - 1] == '\n')
                {
                    //result.Append(";", result.Length - 1, 1);
                    result[result.Length - 1] = ';';
                }
                else
                {
                    result.Append(';');
                }
            }

            return result;
        }

        #endregion

        #region Public Methods

        public static string Compress(string javaScript, string fileName)
        {
            return Compress(javaScript,
                            fileName,
                            true);
        }

        public static string Compress(string javaScript,
                                      string fileName,
                                      bool isVerboseLogging)
        {
            return Compress(javaScript,
                            fileName,
                            isVerboseLogging,
                            true,
                            false,
                            false,
                            -1);
        }

        public static string Compress(string javaScript,
                                      string fileName,
                                      bool isVerboseLogging,
                                      bool isObfuscateJavascript,
                                      bool preserveAllSemicolons,
                                      bool disableOptimizations,
                                      int lineBreakPosition)
        {
            return Compress(javaScript,
                            fileName,
                            isVerboseLogging,
                            isObfuscateJavascript,
                            preserveAllSemicolons,
                            disableOptimizations,
                            lineBreakPosition,
                            Encoding.Default,
                            CultureInfo.CreateSpecificCulture("en-GB"));
        }

        public static string Compress(string javaScript,
                                      string fileName,
                                      bool isVerboseLogging,
                                      bool isObfuscateJavascript,
                                      bool preserveAllSemicolons,
                                      bool disableOptimizations,
                                      int lineBreakPosition,
                                      Encoding encoding,
                                      CultureInfo threadCulture)
        {
            return Compress(javaScript,
                            fileName,
                            isVerboseLogging,
                            isObfuscateJavascript,
                            preserveAllSemicolons,
                            disableOptimizations,
                            lineBreakPosition,
                            encoding,
                            threadCulture,
                            false);
        }


        public static string Compress(string javaScript,
                                      string fileName,
                                      bool isVerboseLogging,
                                      bool isObfuscateJavascript,
                                      bool preserveAllSemicolons,
                                      bool disableOptimizations,
                                      int lineBreakPosition,
                                      Encoding encoding,
                                      CultureInfo threadCulture,
                                      bool isEvalIgnored)
        {
            if (string.IsNullOrEmpty(javaScript))
            {
                throw new ArgumentNullException("javaScript");
            }

            var javaScriptCompressor = new JavaScriptCompressor(javaScript,
                                                                fileName,
                                                                isVerboseLogging,
                                                                encoding,
                                                                threadCulture,
                                                                isEvalIgnored,
                                                                null);

            return javaScriptCompressor.Compress(isObfuscateJavascript,
                                                 preserveAllSemicolons,
                                                 disableOptimizations,
                                                 lineBreakPosition);
        }

        public string Compress()
        {
            return Compress(true,
                            false,
                            false,
                            -1);
        }

        public string Compress(bool isObfuscateJavascript,
                               bool preserveAllSemicolons,
                               bool disableOptimizations,
                               int lineBreakPosition)
        {
            _munge = isObfuscateJavascript;

            ProcessStringLiterals(_tokens,
                                  !disableOptimizations);

            if (!disableOptimizations)
            {
                OptimizeObjectMemberAccess(_tokens);
                OptimizeObjLitMemberDecl(_tokens);
            }

            BuildSymbolTree();
            MungeSymboltree();
            StringBuilder stringBuilder = PrintSymbolTree(lineBreakPosition, preserveAllSemicolons);

            return stringBuilder.ToString();
        }

        #endregion

        #endregion
    }
}