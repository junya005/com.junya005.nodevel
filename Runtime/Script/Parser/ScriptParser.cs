using System;
using System.Collections.Generic;
using System.Text;
namespace Junya005.Nodevel
{
    /// <summary>
    /// Nodevel テキストDSLのパーサー。
    /// </summary>
    public static class ScriptParser
    {
        /// <summary>
        /// スクリプト文字列をパースして <see cref="ScriptAssetData"/> を生成します。
        /// </summary>
        /// <param name="scriptText">パース対象のテキスト。</param>
        /// <returns>パースされたASTコンテナ。</returns>
        public static ScriptAssetData Parse(string scriptText)
        {
            var result = new ScriptAssetData();
            if (string.IsNullOrEmpty(scriptText))
            {
                return result;
            }

            var lines = scriptText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            for (var i = 0; i < lines.Length; i++)
            {
                var lineNumber = i + 1;
                var rawLine = lines[i];
                var trimmed = rawLine.Trim();

                // 空行・コメント行のスキップ
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//"))
                {
                    continue;
                }

                if (trimmed.StartsWith("@"))
                {
                    var cmdLine = ParseCommandLine(trimmed, lineNumber, rawLine);
                    result.Lines.Add(cmdLine);
                }
                else
                {
                    var dialogueLine = ParseDialogueLine(trimmed, lineNumber, rawLine);
                    result.Lines.Add(dialogueLine);
                }
            }

            return result;
        }

        private static ScriptCommandLine ParseCommandLine(string trimmed, int lineNumber, string rawLine)
        {
            var line = new ScriptCommandLine
            {
                LineNumber = lineNumber,
                RawText = rawLine
            };

            // 先頭の '@' を除去
            var content = trimmed.Substring(1).TrimStart();
            var tokens = Tokenize(content, lineNumber, rawLine);

            if (tokens.Count == 0)
            {
                throw new NovelScriptException("コマンド名が指定されていません。", lineNumber, rawLine);
            }

            line.CommandName = tokens[0];

            for (var i = 1; i < tokens.Count; i++)
            {
                var token = tokens[i];
                var equalIndex = token.IndexOf('=');

                if (equalIndex > 0)
                {
                    var key = token.Substring(0, equalIndex).Trim();
                    var value = token.Substring(equalIndex + 1).Trim();
                    value = StripQuotes(value);
                    line.NamedArgs[key] = value;
                }
                else
                {
                    line.PositionalArgs.Add(StripQuotes(token));
                }
            }

            return line;
        }

        private static ScriptDialogueLine ParseDialogueLine(string trimmed, int lineNumber, string rawLine)
        {
            var line = new ScriptDialogueLine
            {
                LineNumber = lineNumber,
                RawText = rawLine
            };

            // 話者プレフィックスの判定 (コロン ':' の探索)
            var colonIndex = -1;
            for (var i = 0; i < trimmed.Length; i++)
            {
                if (trimmed[i] == ':' || trimmed[i] == '：')
                {
                    colonIndex = i;
                    break;
                }
            }

            string speaker = null;
            string dialogueText;

            if (colonIndex >= 0)
            {
                speaker = trimmed.Substring(0, colonIndex).Trim();
                dialogueText = trimmed.Substring(colonIndex + 1).TrimStart();
            }
            else
            {
                dialogueText = trimmed;
            }

            line.Speaker = string.IsNullOrEmpty(speaker) ? null : speaker;
            line.Text = dialogueText;

            // インラインタグの解析
            ParseInlineTags(dialogueText, line.InlineTags, out var cleanText, lineNumber, rawLine);
            line.CleanText = cleanText;

            return line;
        }

        private static void ParseInlineTags(string text, List<InlineTag> outTags, out string cleanText, int lineNumber, string rawLine)
        {
            var cleanSb = new StringBuilder();
            var i = 0;

            while (i < text.Length)
            {
                if (text[i] == '{')
                {
                    var closeIndex = text.IndexOf('}', i);
                    if (closeIndex < 0)
                    {
                        throw new NovelScriptException("閉じられていないインラインタグ '{' が存在します。", lineNumber, rawLine);
                    }

                    var tagContent = text.Substring(i + 1, closeIndex - i - 1).Trim();
                    var spaceIndex = tagContent.IndexOf(' ');

                    string tagName;
                    string tagArg = string.Empty;

                    if (spaceIndex >= 0)
                    {
                        tagName = tagContent.Substring(0, spaceIndex).Trim();
                        tagArg = tagContent.Substring(spaceIndex + 1).Trim();
                    }
                    else
                    {
                        var eqIndex = tagContent.IndexOf('=');
                        if (eqIndex >= 0)
                        {
                            tagName = tagContent.Substring(0, eqIndex).Trim();
                            tagArg = tagContent.Substring(eqIndex + 1).Trim();
                        }
                        else
                        {
                            tagName = tagContent;
                        }
                    }

                    outTags.Add(new InlineTag(tagName, StripQuotes(tagArg), cleanSb.Length));
                    i = closeIndex + 1;
                }
                else
                {
                    cleanSb.Append(text[i]);
                    i++;
                }
            }

            cleanText = cleanSb.ToString();
        }

        private static List<string> Tokenize(string text, int lineNumber, string rawLine)
        {
            var tokens = new List<string>();
            var sb = new StringBuilder();
            var inQuotes = false;
            char quoteChar = '\0';

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (inQuotes)
                {
                    if (c == quoteChar)
                    {
                        inQuotes = false;
                        sb.Append(c);
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    if (c == '"' || c == '\'')
                    {
                        inQuotes = true;
                        quoteChar = c;
                        sb.Append(c);
                    }
                    else if (char.IsWhiteSpace(c))
                    {
                        if (sb.Length > 0)
                        {
                            tokens.Add(sb.ToString());
                            sb.Clear();
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            if (inQuotes)
            {
                throw new NovelScriptException("クォート文字列が閉じられていません。", lineNumber, rawLine);
            }

            if (sb.Length > 0)
            {
                tokens.Add(sb.ToString());
            }

            return tokens;
        }

        private static string StripQuotes(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                (value.StartsWith("'") && value.EndsWith("'")))
            {
                if (value.Length >= 2)
                {
                    return value.Substring(1, value.Length - 2);
                }
            }
            return value;
        }
    }
}

