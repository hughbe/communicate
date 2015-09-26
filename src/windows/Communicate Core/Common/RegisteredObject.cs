using System;
using System.Collections.Generic;
using System.Linq;

namespace Communicate
{
    public interface IUniqueObject
    {
        int Identifier { get; }
    }

    public interface IUniqueNamedObject : IUniqueObject
    {
        string Name { get; }
    }

    public abstract class RegisteredObject<T> : IEquatable<RegisteredObject<T>>, IUniqueNamedObject where T : class, IUniqueNamedObject
    {
        public int Identifier { get; }
        public string Name { get; private set; }

        protected RegisteredObject(int identifier, string name)
        {
            Identifier = identifier;
            Name = name;
            if (string.IsNullOrEmpty(name))
            {
                SetupFromRegistry(RegisteredObjects.First(obj => obj.Identifier == Identifier));
            }
        }

        public bool Is(params IUniqueObject[] list) => list.Contains(this);

        internal byte[] GetBytes() => BitConverter.GetBytes(Identifier);

        public static HashSet<T> RegisteredObjects { get; } = new HashSet<T>();

        public T Register()
        {
            var us = this as T;
            RegisteredObjects.Add(this as T);
            return us;
        }

        static RegisteredObject()
        {
            //Register all
            var objectType = typeof(T);

            objectType
                .GetProperties().Where(property => property.PropertyType.Equals(objectType)).ToList()
                .ForEach(property => property.GetValue(null, null));
        }

        protected virtual void SetupFromRegistry(T registeredObject)
        {
            Name = registeredObject.Name;
        }

        public override bool Equals(object obj)
        {
            var other = obj as IUniqueObject;
            if (other == null)
            {
                return false;
            }
            return Identifier == other.Identifier;
        }

        public bool Equals(RegisteredObject<T> other) => Identifier == other.Identifier;

        public override int GetHashCode() => Identifier.GetHashCode();

        public override string ToString() => Name;

        public static bool operator ==(RegisteredObject<T> a, RegisteredObject<T> b)
        {
            if ((object)a == null && (object)b == null)
            {
                return true;
            }
            if ((object)a == null || (object)b == null)
            {
                return false;
            }
            return a.Equals(b);
        }

        public static bool operator !=(RegisteredObject<T> a, RegisteredObject<T> b) => !(a == b);
    }
}
