using System;
using System.Linq;
using System.Collections.Generic;
using NRack.Helpers;

namespace NRack
{
    public class Builder
    {
        private readonly List<dynamic> _appList;

        public Builder(Action<Builder> initializeWith = null)
        {
            _appList = new List<object>();
            if (initializeWith != null)
            {
                initializeWith(this);
            }
        }

        public static dynamic App(Action<Builder> initializeWith)
        {
            return new Builder(initializeWith).ToApp();
        }

        public Builder Use<T>(params dynamic[] parameters)
        {
            return Use(typeof (T), parameters);
        }

        public Builder Use(Type rackApplicationType, params dynamic[] parameters)
        {
            _appList.Add(
                new Proc((Func<object, object>)
                    (innerApp => InstantiateApplicationFromType(rackApplicationType, innerApp, parameters))));

            return this;
        }

        public Builder Use(Func<IDictionary<string, dynamic>, dynamic[]> application)
        {
            _appList.Add(new CalledWithIterableResponseAdapter(new Proc(application)));

            return this;
        }

        public Builder Run(dynamic application)
        {
            _appList.Add(new CalledWithIterableResponseAdapter(application));

            return this;
        }

        public Builder Run(Func<IDictionary<string, dynamic>, dynamic[]> application)
        {
            _appList.Add(new CalledWithIterableResponseAdapter(new Proc(application)));

            return this;
        }
        
        public Builder Map(string url, Action<Builder> action)
        {
            if (_appList.Count == 0 || 
                _appList.Last() is Proc || 
                !(_appList.Last() is Dictionary<string, dynamic>))
            {
                _appList.Add(new Dictionary<string, dynamic>());
            }
            ((Dictionary<string, dynamic>) _appList.Last())
                .Add(url, new Builder(action).ToApp());

            return this;
        }

        public Builder Map(string url, Func<IDictionary<string, dynamic>, dynamic[]> application)
        {
            _appList.Add(new CalledWithIterableResponseAdapter(application));

            return this;
        }

        public dynamic[] Call(IDictionary<string, dynamic> environment)
        {
            return ToApp().Call(environment);
        }

        public dynamic ToApp()
        {
            var appList = _appList.ToArray();

            if (appList.Last() is Dictionary<string, dynamic>)
            {
                appList[appList.Length - 1] = new CalledWithIterableResponseAdapter(new UrlMap(appList.Last()));
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

            return new CalledWithIterableResponseAdapter(rackApp);
        }
    }
}