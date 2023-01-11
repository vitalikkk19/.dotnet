using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CustomArray
{
    public class CustomArray<T> : IEnumerable<T>
    {
        private int length;
        readonly T[] array;
        private int first;

        public int First
        {
            get
            {
                return first;
            }
            private set
            {
                first = value;
            }
        }

        public int Last
        {
            get => first + length -1;
        }

        public int Length
        {
            get => length;
            private set
            {
                if (value < 0)
                {
                    throw new ArgumentException("length");
                }
                length = value;
            }
        }

        public T[] Array
        {
            get => array;
        }

        private int Len(IEnumerable<T> list)
        {
            int res = 0;
            using (var enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    res++;
            }
            return res;
        }

        public CustomArray(int first, int length)
        {
            if (length <= 0)
                throw new ArgumentException("CustomArray");
            this.first = first;
            this.length = length;
            array = new T[length];
        }

        public CustomArray(int first, IEnumerable<T> list)
        {
            if (list == null)
                throw new NullReferenceException();
            if (Len(list) <= 0)
                throw new ArgumentException("CustomArray");
            this.first = first;
            this.length = Len(list);
            array = new T[length];
            array = list.ToArray<T>();
        }

        public CustomArray(int first, params T[] list)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (Len(list) <= 0)
                throw new ArgumentException("CustomArray");
            this.first = first;
            this.length = Len(list);
            array = new T[length];
            array = list;
        }

        public T this[int item]
        {
            get
            {
                if (item < First || item > Last) 
                    throw new ArgumentException("get");
                return array[item - first];
            }
            set
            {
                if (item < First || item > Last)
                    throw new ArgumentException("set");
                if (value == null)
                    throw new ArgumentNullException("item");
                array[item - first] = value;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < array.Length; i++)
            {
                yield return array[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
    }
}
