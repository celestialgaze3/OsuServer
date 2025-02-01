﻿namespace OsuServer.External.Database
{
    public abstract class DbColumn
    {
        private object _object;
        public object Object { 
            get
            {
                return _object;
            }
            protected set
            {
                _object = value;
            }
        }
        public string Name { get; }
        public bool CanModify { get; }
        public bool HasBeenModified { get; set; } = false;

        protected DbColumn(string name, object obj, bool canModify) 
        {
            Name = name;
            _object = obj;
            CanModify = canModify;
        }
    }

    public class DbColumn<T> : DbColumn where T : notnull
    {
        private T _value;
        public T Value { 
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                Object = _value;
                HasBeenModified = true;
            }
        }

        public DbColumn(string name, T value, bool canModify = true) : base(name, value, canModify)
        {
            _value = value;
        }
    }
}
