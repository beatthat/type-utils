using UnityEngine;

using System;
using System.Collections.Generic;

using System.Reflection;


namespace BeatThat
{
	public static class TypeUtils 
	{
		/// <summary>
		/// Searches assemblies in project for all static methods with a given attribute.
		/// Caches results for performance.
		/// </summary>
		public static MethodInfo[] FindStaticMethodsWithAttribute<T>(bool ignoreCache = false) where T : class
		{
			System.Type attrType = typeof(T);
			MethodInfo[] methods;
			if(ignoreCache || !m_staticMethodsByAttribute.TryGetValue(attrType, out methods)) {
				var methodList = new List<MethodInfo>();
				foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
					foreach(Type t in a.GetTypes()) {
						foreach(MethodInfo m in t.GetMethods(BindingFlags.Static | BindingFlags.Public)) {
							
							foreach(var attr in m.GetCustomAttributes(false)) {
								if(attr is T) {
#if BT_DEBUG_UNSTRIP
									Debug.Log ("[" + Time.time + "] TypeUtils::FindStaticMethodsWithAttribute '" + attrType.Name + "' found "
									           + m.DeclaringType.Name + "::" + m.Name);
#endif
									
									methodList.Add(m);
								}
							}
						}
					}
				}

				methods = methodList.ToArray();

				m_staticMethodsByAttribute[attrType] = methods;
			}

			return methods;
		}

		/// <summary>
		/// Searches assemblies in project for all static fields with a given attribute whose type is assignable from the given type
		/// Caches results for performance.
		/// </summary>
		public static FieldInfo[] FindStaticFieldsWithAttrAndValType<AttrType,ValType>(bool ignoreCache = false)
		{
			System.Type attrType = typeof(AttrType);
			System.Type valType = typeof(ValType);
			AttrAndType key = new AttrAndType(attrType, valType);

			FieldInfo[] fields;
			if(ignoreCache || !m_staticFieldsByAttrAndType.TryGetValue(key, out fields)) {

				var fieldList = new List<FieldInfo>();
				foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
					foreach(Type t in a.GetTypes()) {
						foreach(FieldInfo f in t.GetFields()) {
							if(valType.IsAssignableFrom(f.FieldType)) {
								foreach(var attr in f.GetCustomAttributes(false)) {
									if(attr is AttrType) {
#if BT_DEBUG_UNSTRIP
										Debug.Log ("[" + Time.time + "] TypeUtils::FindStaticFieldsWithAttrAndValType '" + attrType.Name + "' found "
										           + f.DeclaringType.Name + "::" + f.Name);
#endif

										fieldList.Add(f);
									}
								}
							}
						}
					}
				}

				fields = fieldList.ToArray();
				m_staticFieldsByAttrAndType[key] = fields;
			}

			return fields;
		}

		/// <summary>
		/// Searches assemblies in project for all static fields with a given attribute whose type is assignable from the given type
		/// Caches results for performance.
		/// </summary>
		public static ValType[] FindStaticValsWithAttrAndValType<AttrType,ValType>(bool ignoreCache = false)
		{
			FieldInfo[] fields = FindStaticFieldsWithAttrAndValType<AttrType, ValType>(ignoreCache);

			ValType[] vals = new ValType[fields.Length];
			for(int i = 0; i < fields.Length; i++) {
				vals[i] = (ValType)fields[i].GetRawConstantValue();
			}
			
			return vals;
		}
		
		/// <summary>
		/// Checks the existance of a list of type names and returns true if all exist.
		/// Useful if you've generated a class and are waiting to see if unity compiled those classes.
		/// </summary>
		public static bool AllTypesExist(params string[] typeNames)
		{
			foreach(string name in typeNames) {
				if(Type.GetType(name, false) == null) {
#if BT_DEBUG_UNSTRIP
					Debug.Log ("[" + Time.realtimeSinceStartup + "] ClassesExist " + name + " does NOT exist");
#endif
					return false;
				}
			}
			
			return true; // all exist
		}

		class AttrAndType
		{
			public AttrAndType(System.Type attrType, System.Type valType)
			{
				this.attrType = attrType;
				this.valType = valType;
			}

			public System.Type attrType
			{
				get; private set;
			}

			public System.Type valType
			{
				get; private set;
			}

			override public bool Equals(object o)
			{
				if(o == this) {
					return true;
				}
				else if(o == null) {
					return false;
				}
				else {
					AttrAndType thatObj = o as AttrAndType;
					if(thatObj == null) {
						return false;
					}
					else {
						return this.attrType == thatObj.attrType && this.valType == thatObj.valType;
					}
				}
			}

			override public int GetHashCode()
			{
				return this.attrType.GetHashCode() + (this.valType.GetHashCode() << 7);
			}
		}
		
		private static Dictionary<Type, MethodInfo[]> m_staticMethodsByAttribute = new Dictionary<Type, MethodInfo[]>();
		private static Dictionary<AttrAndType, FieldInfo[]> m_staticFieldsByAttrAndType = new Dictionary<AttrAndType, FieldInfo[]>();

	}
}
