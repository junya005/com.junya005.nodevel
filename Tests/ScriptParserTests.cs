using System.Linq;
using NUnit.Framework;
using Junya005.Nodevel;

namespace Junya005.Nodevel.Tests
{
    [TestFixture]
    public class ScriptParserTests
    {
        [Test]
        public void Parse_CommandLine_ReturnsCommandNodeWithArgsAndNamedParams()
        {
            var text = "@bg \"classroom\" fade=0.5";
            var script = ScriptParser.Parse(text);

            Assert.AreEqual(1, script.Lines.Count);
            var line = script.Lines[0] as ScriptCommandLine;
            Assert.IsNotNull(line);
            Assert.AreEqual("bg", line.CommandName);
            Assert.AreEqual(1, line.PositionalArgs.Count);
            Assert.AreEqual("classroom", line.PositionalArgs[0]);
            Assert.AreEqual("0.5", line.NamedArgs["fade"]);
        }

        [Test]
        public void Parse_DialogueLine_ReturnsSpeakerAndText()
        {
            var text = "アリス: こんにちは！今日もいい天気だね。";
            var script = ScriptParser.Parse(text);

            Assert.AreEqual(1, script.Lines.Count);
            var line = script.Lines[0] as ScriptDialogueLine;
            Assert.IsNotNull(line);
            Assert.AreEqual("アリス", line.Speaker);
            Assert.AreEqual("こんにちは！今日もいい天気だね。", line.Text);
        }

        [Test]
        public void Parse_NarrationLine_ReturnsEmptySpeaker()
        {
            var text = "遠くからチャイムの音が響いてきた。";
            var script = ScriptParser.Parse(text);

            Assert.AreEqual(1, script.Lines.Count);
            var line = script.Lines[0] as ScriptDialogueLine;
            Assert.IsNotNull(line);
            Assert.IsNull(line.Speaker);
            Assert.AreEqual("遠くからチャイムの音が響いてきた。", line.Text);
        }

        [Test]
        public void Parse_InlineTags_SeparatesTextAndTags()
        {
            var text = "こんにちは！{wait 0.5}いい天気だね。";
            var script = ScriptParser.Parse(text);

            Assert.AreEqual(1, script.Lines.Count);
            var line = script.Lines[0] as ScriptDialogueLine;
            Assert.IsNotNull(line);
            Assert.AreEqual("こんにちは！いい天気だね。", line.CleanText);
            Assert.AreEqual(1, line.InlineTags.Count);
            Assert.AreEqual("wait", line.InlineTags[0].TagName);
            Assert.AreEqual("0.5", line.InlineTags[0].Argument);
            Assert.AreEqual(6, line.InlineTags[0].CharacterIndex);
        }

        [Test]
        public void Parse_CommentAndEmptyLines_AreIgnored()
        {
            var text = @"
// これはコメントです
@bg 'bg01'

アリス: テスト
// 終了コメント
";
            var script = ScriptParser.Parse(text);
            Assert.AreEqual(2, script.Lines.Count);
            Assert.IsInstanceOf<ScriptCommandLine>(script.Lines[0]);
            Assert.IsInstanceOf<ScriptDialogueLine>(script.Lines[1]);
        }

        [Test]
        public void Parse_UnclosedQuote_ThrowsNovelScriptException()
        {
            var text = "@bg \"classroom fade=0.5";
            Assert.Throws<NovelScriptException>(() => ScriptParser.Parse(text));
        }
    }
}

