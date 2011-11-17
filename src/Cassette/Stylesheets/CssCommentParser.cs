using System.Collections.Generic;

namespace Cassette.Stylesheets
{
    class CssCommentParser : ICommentParser
    {
        enum State
        {
            Code, Comment
        }

        public IEnumerable<Comment> Parse(string code)
        {
            var state = State.Code;
            var commentStart = 0;
            var line = 1;
            for (var i = 0; i < code.Length; i++)
            {
                var c = code[i];

                if (c == '\r')
                {
                    i++;
                    if (i < code.Length && code[i] == '\n')
                    {
                        i++;
                    }
                    line++;
                    continue;
                }
                else if (c == '\n')
                {
                    i++;
                    line++;
                    continue;
                }

                switch (state)
                {
                    case State.Code:
                        if (c != '/') continue;
                        if (i >= code.Length - 2) yield break;
                        if (code[i + 1] == '*')
                        {
                            state = State.Comment;
                            commentStart = i + 2;
                            i++; // Skip the '*'
                        }
                        break;

                    case State.Comment:
                        // Scan forwards until "*/" or end of code.
                        while (i < code.Length - 1 && (code[i] != '*' || code[i + 1] != '/'))
                        {
                            // Track new lines within the comment.
                            if (code[i] == '\r')
                            {
                                yield return new Comment
                                {
                                    SourceLineNumber = line,
                                    Value = code.Substring(commentStart, i - commentStart)
                                };
                                i++;
                                if (i < code.Length && code[i] == '\n')
                                {
                                    i++;
                                }
                                commentStart = i;
                                line++;
                                continue;
                            }
                            else if (code[i] == '\n')
                            {
                                yield return new Comment
                                {
                                    SourceLineNumber = line,
                                    Value = code.Substring(commentStart, i - commentStart)
                                };
                                i++;
                                commentStart = i;
                                line++;
                                continue;
                            }
                            else
                            {
                                i++;
                            }
                        }
                        yield return new Comment
                        {
                            SourceLineNumber = line,
                            Value = code.Substring(commentStart, i - commentStart)
                        };
                        i++; // Skip the '/'
                        state = State.Code;
                        break;
                }
            }
        }
    }
}