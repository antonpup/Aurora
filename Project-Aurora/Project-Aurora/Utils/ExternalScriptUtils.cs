using CSScriptLibrary;
using IronPython.Runtime.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Aurora.Utils {

    /// <summary>
    /// Utility methods that can help loading classes/types from external scripts or assemblies.
    /// </summary>
    public class ExternalScriptUtils {

        /// <summary>
        /// Dictionary that contains functions that will load the types from the given path for a particular kind of file.
        /// The function takes the full path to the file and should return a list of <see cref="LoadedType"/>s representing all types exported from that script/assembly.
        /// </summary>
        private static readonly Dictionary<string, Func<string, IEnumerable<LoadedType>>> fileLoaderDictionary = new Dictionary<string, Func<string, IEnumerable<LoadedType>>> {

            { ".dll", path => GetAssemblyTypes(Assembly.LoadFrom(path), path) },
            { ".cs", path => GetAssemblyTypes(CSScript.LoadFile(path), path) },

            { ".py", path => Global.PythonEngine.ExecuteFile(path).GetItems()
                .Where(kvp => kvp.Value is PythonType)
                .Select(kvp => new LoadedType(((PythonType)kvp.Value).__clrtype__(), @params => @params == null ? Global.PythonEngine.Operations.CreateInstance(kvp.Value) : Global.PythonEngine.Operations.CreateInstance(kvp.Value, @params), path))
            }
        };

        #region LoadTypes methods
        /// <summary>
        /// Loads and returns all types from supported files in the given directory, excluding sub-directories.
        /// </summary>
        /// <param name="dir">The directory to load files from.</param>
        public static IEnumerable<LoadedType> LoadTypes(string dir) {
            return Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly).SelectMany(path => {
                if (fileLoaderDictionary.TryGetValue(Path.GetExtension(path), out var loadFunc))
                    try {
                        return loadFunc(path);
                    } catch (Exception ex) {
                        Global.logger.Error("An error occured while trying to load game state plugin {0}. Exception: {1}", path, ex); // If an error occurs reading the cs file, log it to the console
                    }
                return new LoadedType[0];
            });
        }

        /// <summary>
        /// Loads and returns all types that extend the given type from supported files in the given directory, excluding sub-directories.
        /// Note that generic types are supported, but this will not check the generic type arguments, only the base generic type.
        /// </summary>
        /// <param name="dir">The directory to load files from.</param>
        /// <param name="type">The type filter that will be applied to loaded types. Loaded types must extend this type to be returned.</param>
        public static IEnumerable<LoadedType> LoadTypes(string dir, Type type) {
            return LoadTypes(dir).Where(ltype => !ltype.Type.IsInterface && (type.IsGenericType ? TypeUtils.ExtendsGenericType(ltype.Type, type) : type.IsAssignableFrom(ltype.Type)));
        }

        /// <summary>
        /// Loads and returns all types from supported files in the given directory that have a `TAttr` custom attribute applied to them.
        /// </summary>
        /// <typeparam name="TAttr">The attribute type that must be present on the loaded types for them to be returned.</typeparam>
        /// <param name="dir">The directory to load files from.</param>
        public static IEnumerable<LoadedTypeWithAttribute<TAttr>> LoadTypesWithAttribute<TAttr>(string dir) where TAttr : Attribute {
            return LoadTypes(dir).Where(ltype => ltype.Type.GetCustomAttribute<TAttr>() != null).Select(ltype => LoadedTypeWithAttribute<TAttr>.From(ltype));
        }

        /// <summary>
        /// Loads and returns all types from supported files in the given directory that extend the given type and also have the specified `TAttr` applied.
        /// Note that generic types are supported, but this will not check the generic type arguments, only the base generic type.
        /// </summary>
        /// <typeparam name="TAttr">The attribute type that must be present on the loaded types for them to be returned.</typeparam>
        /// <param name="dir">The directory to load files from.</param>
        /// <param name="type">The type filter that will be applied to loaded types. Loaded types must extend this type to be returned.</param>
        public static IEnumerable<LoadedTypeWithAttribute<TAttr>> LoadTypesWithAttribute<TAttr>(string dir, Type type) where TAttr : Attribute {
            return LoadTypes(dir, type).Where(ltype => ltype.Type.GetCustomAttribute<TAttr>() != null).Select(ltype => LoadedTypeWithAttribute<TAttr>.From(ltype));
        }
        #endregion

        /// <summary>
        /// Simple helper function for the above dictionary that will return the <see cref="LoadedType" />s for the given assembly after attempting to load
        /// the required assemblies referenced in the givven assembly.
        /// </summary>
        private static IEnumerable<LoadedType> GetAssemblyTypes(Assembly ass, string path) {
            // Attempt to load any required references that the plugin needs
            foreach (AssemblyName name in ass.GetReferencedAssemblies())
                AppDomain.CurrentDomain.Load(name);

            // If successful, return all the types from that file
            return ass.ExportedTypes.Select(type => new LoadedType(type, @params => Activator.CreateInstance(type, @params), path));
        }


        #region LoadedType storage classes
        /// <summary>
        /// Class that represents a type that has been loaded from a script/assembly.
        /// <para>Note that when instantiating the Type for this LoadedType, you should use the Create function and pass the parameters to that (or null for
        /// no parameters) rather than attempting to use <see cref="Activator.CreateInstance(Type)"/> as this may not work for all types. The IronPython types
        /// for example, are initialised using the IronPython library and will cause issues using the Activator.</para>
        /// </summary>
        public class LoadedType {
            /// <summary>The type that has been loaded from the script/assembly.</summary>
            public Type Type { get; set; }

            /// <summary>A function that creates a new instance of the loaded type with the given arguments (or null for no args).</summary>
            public Func<object[], object> Create { get; set; }

            /// <summary>The path of the script or assembly where this type was loaded from.</summary>
            public string Path { get; set; }

            public LoadedType(Type type, Func<object[], object> create, string path) {
                Type = type;
                Create = create;
                Path = path;
            }
        }

        /// <summary>
        /// Class that represents a type that has been loaded from a script/assembly and has been asserted that the given attribute type exists on it.
        /// <para>Like with <see cref="LoadedType"/>, you should use the Create function to instantiate the type, not <see cref="Activator.CreateInstance(Type)"/></para>
        /// </summary>
        /// <typeparam name="TAttr">The attribute type that is present on the given Type.</typeparam>
        public class LoadedTypeWithAttribute<TAttr> : LoadedType where TAttr : Attribute {
            /// <summary>The TAttr attribute that exists on the given type.</summary>
            public TAttr Attribute { get; set; }

            public LoadedTypeWithAttribute(Type type, Func<object[], object> create, string path) : base(type, create, path) {
                Attribute = type.GetCustomAttribute<TAttr>();
                if (Attribute == null)
                    throw new ArgumentException($"The attribute {typeof(TAttr).Name} does not exist on the given type.", "type");
            }

            /// <summary>Creates a new <see cref="LoadedTypeWithAttribute{TAttr}"/> from the given <see cref="LoadedType"/>.</summary>
            public static LoadedTypeWithAttribute<TAttr> From(LoadedType src) => new LoadedTypeWithAttribute<TAttr>(src.Type, src.Create, src.Path);
        }
#endregion
    }
}
