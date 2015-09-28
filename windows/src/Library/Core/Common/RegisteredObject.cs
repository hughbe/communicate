using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

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

    internal static class RegisteredObjects
    {
        public static void RegisterObjects()
        {
            //Register all
            var types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .ToList()
                .Where(type => type.GetInterfaces().Contains(typeof(IUniqueNamedObject)));

            foreach (var type in types)
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(property => property.PropertyType.Equals(type));

                foreach (var property in properties)
                {
                    property.GetValue(null, null);
                }
            }
        }

        public static Dictionary<Type, IList> RegisteredTypes { get; } = new Dictionary<Type, IList>();
        
        public static void Register(IUniqueNamedObject uniqueObject)
        {
            if (uniqueObject == null)
            {
                throw new ArgumentNullException(nameof(uniqueObject));
            }

            var types = new Collection<IUniqueNamedObject>();

            if (RegisteredTypes.ContainsKey(uniqueObject.GetType()))
            {
                types = RegisteredTypes[uniqueObject.GetType()] as Collection<IUniqueNamedObject>;
            }
            if (!types.Contains(uniqueObject))
            {
                types.Add(uniqueObject);
            }
            RegisteredTypes[uniqueObject.GetType()] = types;
        }

        public static Collection<T> AllObjects<T>()
        {
            var allObjectsCollection = new Collection<T>();

            var allObjectsList = RegisteredTypes[typeof(T)];
            foreach (T registeredObject in allObjectsList)
            {
                allObjectsCollection.Add(registeredObject);
            }
            return allObjectsCollection;
        }
    }

    public abstract class RegisteredObject<T> : IEquatable<RegisteredObject<T>>, IUniqueNamedObject where T : class, IUniqueNamedObject
    {
        protected RegisteredObject(int identifier, string name)
        {
            Identifier = identifier;
            Name = name;
            if (string.IsNullOrEmpty(name))
            {
                Name = RegisteredVersion?.Name;
            }
        }

        public int Identifier { get; }
        public string Name { get; }

        public bool Is(params IUniqueObject[] list) => list.Contains(this);

        internal byte[] GetBytes() => BitConverter.GetBytes(Identifier);

        public virtual T Register()
        {
            var us = this as T;
            RegisteredObjects.Register(us);
            return us;
        }

        public T RegisteredVersion
        {
            get
            {
                foreach (T registeredObject in RegisteredObjects.AllObjects<T>())
                {
                    if (registeredObject.Identifier == Identifier)
                    {
                        return registeredObject;
                    }
                }
                return default(T);
            }
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

        public bool Equals(RegisteredObject<T> other) => Identifier == other?.Identifier;

        public override int GetHashCode() => Identifier.GetHashCode();

        public override string ToString() => Name;

        public static bool operator ==(RegisteredObject<T> rightHandSide, RegisteredObject<T> leftHandSide)
        {
            if ((object)rightHandSide == null && (object)leftHandSide == null)
            {
                return true;
            }
            if ((object)rightHandSide == null || (object)leftHandSide == null)
            {
                return false;
            }
            return rightHandSide.Equals(leftHandSide);
        }

        public static bool operator !=(RegisteredObject<T> rightHandSide, RegisteredObject<T> leftHandSide) => !(rightHandSide == leftHandSide);
    }
}
