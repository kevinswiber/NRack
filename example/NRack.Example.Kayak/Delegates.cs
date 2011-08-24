using System;
using System.Collections.Generic;
public delegate void AppDelegate(
    IDictionary<string, object> env,
    ResultDelegate result,
    Action<Exception> fault);

public delegate void ResultDelegate(
    string status,
    IDictionary<string, string> headers,
    BodyDelegate body);

public delegate Action /* cancel */ BodyDelegate(
    Func<
        ArraySegment<byte>, // data
        Action, // continuation
        bool> // continuation was or will be invoked
        next,
    Action<Exception> error,
    Action complete);

namespace NRack.Example.Kayak
{
    public static class Delegates
    {
        public static Action<IDictionary<string, object>, Action<string, IDictionary<string, string>, Func<Func<ArraySegment<byte>, Action, bool>, Action<Exception>, Action, Action>>, Action<Exception>> ToAction(this AppDelegate app)
        {
            return
                (env, result, fault) =>
                app(
                    env,
                    (status, headers, body) =>
                    result(
                        status,
                        headers,
                        (next, error, complete) =>
                        body(next, error, complete)),
                    fault);
        }

        public static AppDelegate ToDelegate(this Action<IDictionary<string, object>, Action<string, IDictionary<string, string>, Func<Func<ArraySegment<byte>, Action, bool>, Action<Exception>, Action, Action>>, Action<Exception>> app) {
            return
                (env, result, fault) =>
                app(
                    env,
                    (status, headers, body) =>
                    result(
                        status,
                        headers,
                        (next, error, complete) =>
                        body(next, error, complete)),
                    fault);
        }

        public static Func<Func<ArraySegment<byte>, Action, bool>, Action<Exception>, Action, Action> ToAction(this BodyDelegate body) {
            return (next, error, complete) => body(next, error, complete);
        }

        public static BodyDelegate ToDelegate(this Func<Func<ArraySegment<byte>, Action, bool>, Action<Exception>, Action, Action> body) {
            return (next, error, complete) => body(next, error, complete);
        }
    }
}