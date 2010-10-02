using System;
using System.Linq;
using System.Collections.Generic;
using Rack.Hosting.AspNet;

namespace Rack
{
    public class Builder
    {
        private readonly List<dynamic> _appList;

        public Builder(Action<Builder> initializeWith = null)
        {
            _appList = new List<dynamic>();
            if (initializeWith != null)
            {
                initializeWith(this);
            }
        }

        public static dynamic App(Action<Builder> initializeWith)
        {
            return new Builder(initializeWith).ToApp();
        }

        public void Use<T>(params dynamic[] parameters)
        {
            Use(typeof (T), parameters);
        }

        public void Use(Type rackApplicationType, params dynamic[] parameters)
        {
            _appList.Add(
                new Proc((Func<dynamic, dynamic>)
                    (innerApp => InstantiateApplicationFromType(rackApplicationType, innerApp, parameters))));
        }

        public void Run(dynamic application)
        {
            _appList.Add(new MiddlewareWithEnumerableBodyAdapter(application));
        }
        
        public void Map(string url, Action<Builder> action)
        {
            if (_appList.Count == 0 || 
                _appList.Last() is Proc || 
                !_appList.Last() is Dictionary<string, dynamic>)
            {
                _appList.Add(new Dictionary<string, dynamic>());
            }
            ((Dictionary<string, dynamic>) _appList.Last())
                .Add(url, new Builder(action).ToApp());
        }

        public dynamic[] Call(IDictionary<string, string> environment)
        {
            return ToApp().Call(environment);
        }

        public dynamic ToApp()
        {
            var appList = _appList.ToArray();

            if (appList.Last() is Dictionary<string, dynamic>)
            {
                appList[appList.Length - 1] = new MiddlewareWithEnumerableBodyAdapter(new UrlMap(appList.Last()));
            }

            var innerApp = appList.Last();

            appList = appList.Reverse().ToArray();

            // Enumerable.Aggregate(appList, innerApp, (a, e) => e.Call(a));
            for (var i = 1; i < appList.Length; i++)
            {
                innerApp = appList[i].Call(innerApp);
            }

            return innerApp;
        }


        private dynamic InstantiateApplicationFromType(Type rackApplicationType,
            params dynamic[] constructorArguments)
        {
            if (constructorArguments.Length > 1)
            {
                if (constructorArguments[1] is dynamic[])
                {
                    var size = constructorArguments[1].Length + 1;
                    var tempArgs = new dynamic[size];
                    tempArgs[0] = constructorArguments[0];
                    
                    for (var i = 0; i < constructorArguments[1].Length; i++)
                    {
                        tempArgs[i + 1] = constructorArguments[1][i];
                    }

                    constructorArguments = tempArgs;
                }
            }

            dynamic rackApp;

            // TODO: Cleanup using reflection instead of catching the Missing Method Exception.
            try
            {
                rackApp = Activator.CreateInstance(rackApplicationType, constructorArguments);
            }
            catch (MissingMethodException)
            {
                rackApp = Activator.CreateInstance(rackApplicationType);
            }

            return new MiddlewareWithEnumerableBodyAdapter(rackApp);
        }
    }
}