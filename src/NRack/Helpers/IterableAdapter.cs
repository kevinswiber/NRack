using System;
using System.Collections;
using System.Collections.Generic;

namespace NRack.Helpers
{
    public class IterableAdapter : IIterable
    {
        private readonly Action<Action<dynamic>> _eachAction;
        public dynamic Subject { get; private set; }

        public IterableAdapter(dynamic subject)
        {
            subject = EnsureIterable(subject);
            _eachAction = action => subject.Each(action);
        }

        public IterableAdapter(dynamic subject, Action<dynamic> eachAction)
        {
            subject = EnsureIterable(subject);
            _eachAction = action => eachAction(subject);
        }
        
        public void Each(Action<dynamic> action)
        {
            _eachAction(action);
        }

        private dynamic EnsureIterable(dynamic subject)
        {
            if (subject is IIterable)
            {
                Subject = subject;
                return subject;
            }

            if (subject is string)
            {
                subject = new string[] {subject};
            }

            if (subject is IEnumerable)
            {
                var iterable = new Iterable();

                foreach (var item in subject)
                {
                    iterable.Add(item);
                }

                subject = iterable;
            }

            Subject = subject;

            return Subject;
        }

        public override string ToString()
        {
            var returnList = new List<string>();

            Each(x => returnList.Add(x.ToString()));

            return string.Join(string.Empty, returnList.ToArray());
        }
    }
}