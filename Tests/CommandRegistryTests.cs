using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Junya005.Nodevel;

namespace Junya005.Nodevel.Tests
{
    [TestFixture]
    public class CommandRegistryTests
    {
        private CommandRegistry _registry;

        public enum FadeType
        {
            None,
            CrossFade,
            Black
        }

        private class SampleCommandTarget
        {
            public string LastBg;
            public float LastFade;
            public FadeType LastFadeType;
            public bool WasCancelled;
            public bool SimpleCalled;

            [NovelCommand("bg")]
            public Awaitable OnBg(string name, float fade = 0.5f, FadeType type = FadeType.None, CancellationToken ct = default)
            {
                LastBg = name;
                LastFade = fade;
                LastFadeType = type;
                if (ct.IsCancellationRequested)
                {
                    WasCancelled = true;
                }
                var acs = new AwaitableCompletionSource();
                acs.SetResult();
                return acs.Awaitable;
            }

            [NovelCommand("simple")]
            public void OnSimple()
            {
                SimpleCalled = true;
            }
        }

        [SetUp]
        public void SetUp()
        {
            _registry = new CommandRegistry();
        }

        [Test]
        public void Register_InstanceMethods_AreRegistered()
        {
            var target = new SampleCommandTarget();
            _registry.RegisterTarget(target);

            Assert.IsTrue(_registry.ContainsCommand("bg"));
            Assert.IsTrue(_registry.ContainsCommand("simple"));
            Assert.IsFalse(_registry.ContainsCommand("unknown"));
        }

        [Test]
        public async Task DispatchAsync_ConvertsArgumentsAndExecutes()
        {
            var target = new SampleCommandTarget();
            _registry.RegisterTarget(target);

            var positional = new List<string> { "classroom" };
            var named = new Dictionary<string, string>
            {
                { "fade", "1.5" },
                { "type", "CrossFade" }
            };

            await _registry.DispatchAsync("bg", positional, named, CancellationToken.None);

            Assert.AreEqual("classroom", target.LastBg);
            Assert.AreEqual(1.5f, target.LastFade, 0.0001f);
            Assert.AreEqual(FadeType.CrossFade, target.LastFadeType);
        }

        [Test]
        public async Task DispatchAsync_WithDefaultArguments_UsesDefaults()
        {
            var target = new SampleCommandTarget();
            _registry.RegisterTarget(target);

            var positional = new List<string> { "corridor" };
            var named = new Dictionary<string, string>();

            await _registry.DispatchAsync("bg", positional, named, CancellationToken.None);

            Assert.AreEqual("corridor", target.LastBg);
            Assert.AreEqual(0.5f, target.LastFade, 0.0001f);
            Assert.AreEqual(FadeType.None, target.LastFadeType);
        }

        [Test]
        public async Task DispatchAsync_SyncVoidMethod_ExecutesImmediately()
        {
            var target = new SampleCommandTarget();
            _registry.RegisterTarget(target);

            await _registry.DispatchAsync("simple", new List<string>(), new Dictionary<string, string>(), CancellationToken.None);

            Assert.IsTrue(target.SimpleCalled);
        }

        [Test]
        public void DispatchAsync_UndefinedCommand_ThrowsNovelScriptException()
        {
            var target = new SampleCommandTarget();
            _registry.RegisterTarget(target);

            Assert.ThrowsAsync<NovelScriptException>(async () =>
            {
                await _registry.DispatchAsync("undefined_cmd", new List<string>(), new Dictionary<string, string>(), CancellationToken.None);
            });
        }
    }
}
