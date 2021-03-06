using System;
using System.Collections.Generic;
using NRack.Helpers;
using NRack.Auth;
using NRack.Mock;
using NUnit.Framework;

namespace NRack.Specs
{
    [TestFixture]
    public class Builder_Specs
    {
        [Test]
        public void Should_Support_Mapping()
        {
            var app =
                new Builder(builder =>
                            builder.Map("/", innerBuilder =>
                                          innerBuilder.Run(env => 
                                              new dynamic[] {200, new Hash(), new[] {"root"}})))
                                    .Map("/sub", innerBuilder =>
                                             innerBuilder.Run(env => 
                                                 new dynamic[] {200, new Hash(), new[] {"sub"}})).ToApp();

            Assert.AreEqual("root", new MockRequest(app).Get("/").Body.ToString());
            Assert.AreEqual("sub", new MockRequest(app).Get("/sub").Body.ToString());
        }

        [Test]
        public void Should_Not_Dupe_Environment_Even_When_Mapping()
        {
            var app = new Builder(builder =>
                builder.Use<NothingMiddleware>()
                    .Map("/", innerBuilder =>
                            innerBuilder.Run(env =>
                                            {
                                                env["new_key"] = "new_value";
                                                return new dynamic[] {200, new Hash(), new[] {"root"}};
                                            }))).ToApp();

            Assert.AreEqual("root", new MockRequest(app).Get("/").Body.ToString());
            Assert.AreEqual("new_value", NothingMiddleware.Environment["new_key"]);
        }

        [Test]
        public void Should_Chain_Apps_By_Default()
        {
            var app = new Builder(builder =>
                                  builder.Use<ShowExceptions>()
                                      .Run(env => { throw new Exception("Bzzzt!"); })).ToApp();

            Assert.AreEqual(500, new MockRequest(app).Get("/").Status);
            Assert.AreEqual(500, new MockRequest(app).Get("/").Status);
            Assert.AreEqual(500, new MockRequest(app).Get("/").Status);
        }

        [Test]
        public void Should_Have_Implicit_To_App()
        {
            var app = new Builder(
                builder => builder.Use<ShowExceptions>()
                    .Run(env => { throw new Exception("Bzzzt!"); }));

            Assert.AreEqual(500, new MockRequest(app).Get("/").Status);
            Assert.AreEqual(500, new MockRequest(app).Get("/").Status);
            Assert.AreEqual(500, new MockRequest(app).Get("/").Status);
        }

        [Test]
        public void Should_Support_Blocks_On_Use()
        {
            var app = new Builder(
                builder =>
                builder.Use<ShowExceptions>()
                    .Use<BasicAuthHandler>((Func<string, string, bool>)((username, password) => password == "secret"))
                    .Run(env => new dynamic[] { 200, new Hash(), new[] { "Hi Boss" } }));

            var response = new MockRequest(app).Get("/");
            Assert.AreEqual(401, response.Status);
        }

        [Test]
        public void Should_Have_Explicit_To_App()
        {
            var app = Builder.App(
                builder =>
                builder.Use<ShowExceptions>()
                    .Run(env => { throw new Exception("Bzzzt!"); }));

            Assert.AreEqual(500, new MockRequest(app).Get("/").Status);
            Assert.AreEqual(500, new MockRequest(app).Get("/").Status);
            Assert.AreEqual(500, new MockRequest(app).Get("/").Status);
        }

        [Test]
        public void Should_Initialize_Apps_Once()
        {
            var app = new Builder(
                builder => builder.Use<ShowExceptions>()
                    .Run(new AppClass()));

            Assert.AreEqual(200, new MockRequest(app).Get("/").Status);
            Assert.AreEqual(500, new MockRequest(app).Get("/").Status);
        }

        public class AppClass : ICallable
        {
            private int _called;

            public AppClass()
            {
                _called = 0;
            }

            #region Implementation of ICallable

            public dynamic[] Call(IDictionary<string, object> environment)
            {
                if (_called > 0)
                {
                    throw new Exception("Bzzzt!");
                }

                _called++;

                return new dynamic[] {200, new Hash {{"Content-Type", "text/plain"}}, new[] {"OK"}};
            }

            #endregion
        }
    }

    public class NothingMiddleware : ICallable
    {
        private readonly dynamic _app;

        public NothingMiddleware(dynamic app)
        {
            _app = app;
        }

        #region Implementation of ICallable

        public dynamic[] Call(IDictionary<string, object> environment)
        {
            Environment = environment;
            var response = _app.Call(environment);
            return response;
        }

        #endregion

        public static IDictionary<string, object> Environment { get; private set; }
    }
}