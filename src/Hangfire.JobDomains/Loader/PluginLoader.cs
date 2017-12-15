using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;

namespace Hangfire.JobDomains.Loader {

	/// <summary>
	/// When hosted in a separate AppDomain, provides a mechanism for loading 
	/// plugin assemblies and instantiating objects within them.
	/// </summary>
	[SecurityPermission(SecurityAction.Demand, Infrastructure = true)]
    public sealed class PluginLoader : MarshalByRefObject, IDisposable {

		private Sponsor<TextWriter> mLog;

		/// <summary>
		/// Gets or sets the directory containing the assemblies.
		/// </summary>
		private string PluginDir { get; set; }
		/// <summary>
		/// Gets or sets the collection of assemblies that have been loaded.
		/// </summary>
		private List<Assembly> Assemblies { get; set; }
		
		/// <summary>
		/// Gets or sets the TextWriter to use for logging.
		/// </summary>
		public TextWriter Log {
			get {
				return mLog?.Instance;
			}
			set {
				mLog = (value != null) ? new Sponsor<TextWriter>(value) : null;
			}
		}

		/// <summary>
		/// Initialises a new instance of the PluginLoader class.
		/// </summary>
		public PluginLoader() {
            Log = LogWriter.CreateWriter<PluginLoader>();
			Assemblies = new List<Assembly>();
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~PluginLoader() {
			Dispose(false);
		}

		/// <summary>
		/// Loads plugin assemblies into the application domain and populates the collection of plugins.
		/// </summary>
		/// <param name="pluginDir"></param>
		/// <param name="disabledPlugins"></param>
		public void Init(string pluginDir) {
			Uninit();
			PluginDir = pluginDir;
			foreach (string dllFile in Directory.GetFiles(PluginDir, "*.dll")) {
				try {
					Assembly asm = Assembly.LoadFile(dllFile);
					Log.WriteLine("Loaded assembly {0}.", asm.GetName().Name);
					Assemblies.Add(asm);
				}
				catch (ReflectionTypeLoadException rex) {
					Log.WriteLine("Plugin {0} failed to load.", Path.GetFileName(dllFile));
					foreach (Exception ex in rex.LoaderExceptions) {
						Log.WriteLine("\t{0}: {1}", ex.GetType().Name, ex.Message);
					}
				}
				catch (BadImageFormatException) {
					// ignore, this simply means the DLL was not a .NET assembly
					Log.WriteLine("Plugin {0} is not a valid assembly.", Path.GetFileName(dllFile));
				}
				catch (Exception ex) {
					Log.WriteLine("Plugin {0} failed to load.", Path.GetFileName(dllFile));
					Log.WriteLine("\t{0}: {1}", ex.GetType().Name, ex.Message);
				}
			}
		}

		/// <summary>
		/// Clears all plugin assemblies and type info.
		/// </summary>
		public void Uninit() {
			Assemblies.Clear();
		}

		/// <summary>
		/// Returns a sequence of instances of types that implement a 
		/// particular interface. Any instances that are MarshalByRefObject 
		/// must be sponsored to prevent disconnection.
		/// </summary>
		/// <typeparam name="TInterface"></typeparam>
		/// <returns></returns>
		public IEnumerable<TInterface> GetImplementations<TInterface>(object[]  paramers) {
			LinkedList<TInterface> instances = new LinkedList<TInterface>();
            Type[] types = paramers == null ? Type.EmptyTypes : paramers.Select(s => s.GetType()).ToArray();
            var constructors = GetConstructors<TInterface>(types);
            foreach (ConstructorInfo constructor in constructors) {
                instances.AddLast(CreateInstance<TInterface>(constructor, paramers));
			}
			return instances;
		}

		/// <summary>
		/// Returns the name of the assembly that owns the specified instance 
		/// of a particular interface. (If you try to obtain the assembly using 
		/// Object.GetType(), you will get MarshalByRefObject.)
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public AssemblyName GetOwningAssembly(object instance) {
			Type type = instance.GetType();
			return type.Assembly.GetName();
		}

		/// <summary>
		/// Returns the name of the type of the specified instance of a 
		/// particular interface. (If you try to obtain the type using 
		/// Object.GetType(), you will get MarshalByRefObject.)
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public string GetTypeName(object instance) {
			Type type = instance.GetType();
			return type.FullName;
		}

        public object GetInstance(string assemblyName, string typeName, object[] paramers)
        {
            var assembly = Assemblies.SingleOrDefault(s => s.FullName == assemblyName);
            if (assembly == null)
            {
                Log.WriteLine($"\t Assembly:{assemblyName}, 未被发现.");
                return null;
            }

            var type = assembly.GetTypes().SingleOrDefault(s => s.FullName == typeName);
            if (type == null)
            {
                Log.WriteLine($"\t Type:{typeName}: 未被发现.");
                return null;
            }

            Type[] types = paramers == null ? Type.EmptyTypes : paramers.Select(s => s.GetType()).ToArray();

            var constructor = type.GetConstructor(types);
            if (constructor != null) return CreateInstance(constructor, paramers);
            Log.WriteLine($"\t ConstructorInfo:{types.Length} 个参数的构造函数: 未被发现.");
            return null;
        }

        

        /// <summary>
        /// Returns the first implementation of a particular interface type. 
        /// Default implementations are not favoured.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        public TInterface GetImplementation<TInterface>(object[] paramers) {
			return GetImplementations<TInterface>(paramers).FirstOrDefault();
		}

        /// <summary>
        /// Returns the constructors for implementations of a particular interface 
        /// type. Constructor info is cached after the initial crawl.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <returns></returns>
        private IEnumerable<ConstructorInfo> GetConstructors<TInterface>(Type[] paramerTypes) {

            LinkedList<ConstructorInfo> constructors = new LinkedList<ConstructorInfo>();
            foreach (Assembly asm in Assemblies) {
                foreach (Type type in asm.GetTypes()) {
                    if (type.IsClass && !type.IsAbstract) {
                        if (type.GetInterfaces().Contains(typeof(TInterface))) {
                            ConstructorInfo ctor= type.GetConstructor(paramerTypes);
                            constructors.AddLast(ctor);
                        }
                    }
                }
            }
            return constructors;
        }

        /// <summary>
        /// Returns instances of all implementations of a particular interface 
        /// type in the specified assembly.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private IEnumerable<TInterface> GetImplementations<TInterface>(Assembly assembly,object[] paramers) {
			List<TInterface> instances = new List<TInterface>();

			foreach (Type type in assembly.GetTypes()) {
				if (type.IsClass && !type.IsAbstract) {
					if (type.GetInterfaces().Contains(typeof(TInterface))) {
						TInterface instance = default(TInterface);
                        Type[] types = paramers == null ? Type.EmptyTypes : paramers.Select(s => s.GetType()).ToArray();
                        ConstructorInfo constructor = type.GetConstructor(types);
						instance = CreateInstance<TInterface>(constructor,paramers);
						if (instance != null) instances.Add(instance);
					}
				}
			}

			return instances;
		}

		/// <summary>
		/// Invokes the specified constructor to create an instance of an 
		/// interface type.
		/// </summary>
		/// <typeparam name="TInterface"></typeparam>
		/// <param name="constructor"></param>
		/// <returns></returns>
		private TInterface CreateInstance<TInterface>(ConstructorInfo constructor,object[] paramers) {
			TInterface instance = default(TInterface);

			try {
				instance = (TInterface)constructor.Invoke(paramers);
			}
			catch (Exception ex) {
				Log.WriteLine(
					"Unable to instantiate type {0} in plugin {1}",
					constructor.ReflectedType.FullName,
					Path.GetFileName(constructor.ReflectedType.Assembly.Location)
				);
				Log.WriteLine("\t{0}: {1}", ex.GetType().Name, ex.Message);
			}

			return instance;
		}

        private object CreateInstance(ConstructorInfo constructor, object[] paramers)
        {
            try
            {
                return constructor.Invoke(paramers);
            }
            catch (Exception ex)
            {
                Log.WriteLine(
                    "Unable to instantiate type {0} in plugin {1}",
                    constructor.ReflectedType.FullName,
                    Path.GetFileName(constructor.ReflectedType.Assembly.Location)
                );
                Log.WriteLine("\t{0}: {1}", ex.GetType().Name, ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Gets the first implementation of a particular interface type in 
        /// the specified assembly. Default implementations are not favoured.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private TInterface GetImplementation<TInterface>(Assembly assembly,object[] paramers) {
			return GetImplementations<TInterface>(assembly, paramers).FirstOrDefault();
		}

		#region IDisposable Members

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) {
			if (disposing) {
				Uninit();
				if (mLog != null) mLog.Dispose();
			}
		}

		#endregion
	}
}