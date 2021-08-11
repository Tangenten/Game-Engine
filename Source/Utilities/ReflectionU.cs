using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp;

namespace RavUtilities {
	public static class ReflectionU {
		public static List<Type> GetAllTypesThatImplementCustomAttribute<T>() where T : Attribute {
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			List<Type> types = new List<Type>();
			Type customAttributeType = typeof(T);

			for (int i = 0; i < assemblies.Length; i++) {
				Assembly assembly = assemblies[i];
				Type[] assemblyTypes = assembly.GetTypes();

				for (int j = 0; j < assemblyTypes.Length; j++) {
					Type assemblyType = assemblyTypes[i];

					if (Attribute.IsDefined(assemblyType, customAttributeType)) {
						types.Add(assemblyType);
					}
				}
			}

			return types;
		}

		private static List<MemberInfo> alreadyVisited = new List<MemberInfo>(); // to avoid infinite recursion

		public static List<(object, MemberInfo)> GetAllObjectsThatImplementCustomAttribute<T>(ref object startingObject) where T : Attribute {
			alreadyVisited.Clear();
			List<(object, MemberInfo)> objects = new List<(object, MemberInfo)>();
			RecursiveGetAllObjectsThatImplementCustomAttribute<T>(ref startingObject, objects);
			return objects;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static void RecursiveGetAllObjectsThatImplementCustomAttribute<T>(ref object startingObject, List<(object, MemberInfo)> objectsCollected) where T : Attribute {
			Type startingType = startingObject.GetType();

			FieldInfo[] fieldInfos = startingType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			for (int i = 0; i < fieldInfos.Length; i++) {
				FieldInfo fieldInfo = fieldInfos[i];

				if (alreadyVisited.Contains(fieldInfo)) {
					return;
				}
				alreadyVisited.Add(fieldInfo);

				if (Attribute.IsDefined(fieldInfo, typeof(T))) {
					objectsCollected.Add((startingObject, fieldInfo));
				}

				object? member = null;
				try {
					member = fieldInfo.GetValue(startingObject);
				} catch (Exception e) { }

				Type fieldType = fieldInfo.FieldType;
				if (fieldType.Namespace.StartsWith("Rav") && !fieldType.IsPrimitive && member != null) {
					RecursiveGetAllObjectsThatImplementCustomAttribute<T>(ref member, objectsCollected);
				}
			}

			PropertyInfo[] propertyInfos = startingType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			for (int i = 0; i < propertyInfos.Length; i++) {
				PropertyInfo propertyInfo = propertyInfos[i];

				if (alreadyVisited.Contains(propertyInfo)) {
					return;
				}
				alreadyVisited.Add(propertyInfo);

				if (Attribute.IsDefined(propertyInfo, typeof(T))) {
					objectsCollected.Add((startingObject, propertyInfo));
				}

				object? member = null;
				try {
					member = propertyInfo.GetValue(startingObject);
				} catch (Exception e) { }

				Type propertyType = propertyInfo.PropertyType;
				if (propertyType.Namespace.StartsWith("Rav") && !propertyType.IsPrimitive && member != null) {
					RecursiveGetAllObjectsThatImplementCustomAttribute<T>(ref member, objectsCollected);
				}
			}

			MethodInfo[] methodInfos = startingType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			for (int i = 0; i < methodInfos.Length; i++) {
				MethodInfo methodInfo = methodInfos[i];

				if (Attribute.IsDefined(methodInfo, typeof(T))) {
					objectsCollected.Add((startingObject, methodInfo));
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static object GetUnderlyingValue(this MemberInfo memberInfo, object obj, object val = null) {
			switch (memberInfo.MemberType) {
				case MemberTypes.Field:    return ((FieldInfo) memberInfo).GetValue(obj);
				case MemberTypes.Property: return ((PropertyInfo) memberInfo).GetValue(obj);
				case MemberTypes.Method:   return ((MethodInfo) memberInfo).Invoke(obj, (object?[]?) val);
				default:                   throw new NotImplementedException();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static void SetUnderlyingValue(this MemberInfo memberInfo, object obj, object val) {
			switch (memberInfo.MemberType) {
				case MemberTypes.Field:
					((FieldInfo) memberInfo).SetValue(obj, val);
					break;
				case MemberTypes.Property:
					((PropertyInfo) memberInfo).SetValue(obj, val);
					break;
				case MemberTypes.Method:
					((MethodInfo) memberInfo).Invoke(obj, (object?[]?) val);
					break;
				default: throw new NotImplementedException();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Type UnderlyingType(this MemberInfo memberInfo) {
			switch (memberInfo.MemberType) {
				case MemberTypes.Field:    return ((FieldInfo) memberInfo).FieldType;
				case MemberTypes.Property: return ((PropertyInfo) memberInfo).PropertyType;
				case MemberTypes.Method:   return ((MethodInfo) memberInfo).ReturnType;
				default:                   throw new NotImplementedException();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Type ParameterType(this MemberInfo memberInfo, int index) { return ((MethodInfo) memberInfo).GetParameters()[index].ParameterType; }

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Type ParameterType(this MethodInfo methodInfo, int index) { return methodInfo.GetParameters()[index].ParameterType; }

		public static T Convert<T>(string value) {
			if (typeof(T).GetTypeInfo().IsEnum) return (T) Enum.Parse(typeof(T), value);

			return (T) System.Convert.ChangeType(value, typeof(T));
		}

		public static object Convert(string value, Type type) {
			if (type.GetTypeInfo().IsEnum) return Enum.Parse(type, value);

			return System.Convert.ChangeType(value, type);
		}

		public static unsafe int SizeOf<T>() where T : struct {
			Type type = typeof(T);
			TypeCode typeCode = Type.GetTypeCode(type);
			switch (typeCode) {
				case TypeCode.Boolean:  return sizeof(bool);
				case TypeCode.Char:     return sizeof(char);
				case TypeCode.SByte:    return sizeof(sbyte);
				case TypeCode.Byte:     return sizeof(byte);
				case TypeCode.Int16:    return sizeof(short);
				case TypeCode.UInt16:   return sizeof(ushort);
				case TypeCode.Int32:    return sizeof(int);
				case TypeCode.UInt32:   return sizeof(uint);
				case TypeCode.Int64:    return sizeof(long);
				case TypeCode.UInt64:   return sizeof(ulong);
				case TypeCode.Single:   return sizeof(float);
				case TypeCode.Double:   return sizeof(double);
				case TypeCode.Decimal:  return sizeof(decimal);
				case TypeCode.DateTime: return sizeof(DateTime);
				default:                return Unsafe.SizeOf<T>();
			}
		}

		public static Type[] TypesImplementing<T>(this Assembly assembly) { return assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(T))).ToArray(); }

		private static readonly Type[] Numbers = { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) };
		private static readonly Type[] Integers = { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong) };
		private static readonly Type[] SignedIntegers = { typeof(sbyte), typeof(short), typeof(int), typeof(long) };
		private static readonly Type[] UnsignedIntegers = { typeof(byte), typeof(ushort), typeof(uint), typeof(ulong) };
		private static readonly Type[] FloatingPoints = { typeof(float), typeof(double), typeof(decimal) };

		public static bool IsNumber(this Type t) { return Numbers.Contains(t); }

		public static bool IsInteger(this Type t) { return Integers.Contains(t); }

		public static bool IsSignedInteger(this Type t) { return SignedIntegers.Contains(t); }

		public static bool IsUnsignedInteger(this Type t) { return UnsignedIntegers.Contains(t); }

		public static bool IsFloatingPoint(this Type t) { return FloatingPoints.Contains(t); }

		public static string NameWithoutNamespace(this Type type) { return type.Name.Split('.')[^1]; }

		public static (float, float) NumberClamp(Type type) {
			TypeCode typeCode = Type.GetTypeCode(type);
			switch (typeCode) {
				case TypeCode.SByte:   return (sbyte.MinValue, sbyte.MaxValue);
				case TypeCode.Byte:    return (byte.MinValue, byte.MaxValue);
				case TypeCode.Int16:   return (short.MinValue, short.MaxValue);
				case TypeCode.UInt16:  return (ushort.MinValue, ushort.MaxValue);
				case TypeCode.Int32:   return (int.MinValue, int.MaxValue);
				case TypeCode.UInt32:  return (uint.MinValue, uint.MaxValue);
				case TypeCode.Int64:   return (long.MinValue, long.MaxValue);
				case TypeCode.UInt64:  return (ulong.MinValue, ulong.MaxValue);
				case TypeCode.Single:  return (float.MinValue, float.MaxValue);
				case TypeCode.Double:  return ((float, float)) (double.MinValue, double.MaxValue);
				case TypeCode.Decimal: return ((float, float)) (decimal.MinValue, decimal.MaxValue);
				default:               return (0f, 0f);
			}
		}

		public static unsafe void* GetObjectAddress(this object obj) { return *(void**) Unsafe.AsPointer(ref obj); }

		public static unsafe void TransmuteTo(this object target, object source) {
			if (target.GetType() == source.GetType()) return; // no need to act

			void** s = (void**) source.GetObjectAddress();
			void** t = (void**) target.GetObjectAddress();
			*t = *s;

			if (target.GetType() != source.GetType()) {
				// something happened and we failed, so the entire program is in an invalid state now
				throw new AccessViolationException();
			}
		}

		/// <summary>
		///     Redirects all calls from method 'from' to method 'to'.
		/// </summary>
		public static void OverrideMethod(MethodInfo from, MethodInfo to) {
			// GetFunctionPointer enforces compilation of the method.

			RuntimeHelpers.PrepareMethod(from.MethodHandle);
			RuntimeHelpers.PrepareMethod(to.MethodHandle);

			IntPtr fptr1 = from.MethodHandle.GetFunctionPointer();
			IntPtr fptr2 = to.MethodHandle.GetFunctionPointer();

			// Primitive patching. Inserts a jump to 'target' at 'site'. Works even if both methods'
			// callers have already been compiled.
			// R11 is volatile.
			unsafe {
				byte* sitePtr = (byte*) fptr1.ToPointer();
				*sitePtr = 0x49; // mov r11, target
				*(sitePtr + 1) = 0xBB;
				*(ulong*) (sitePtr + 2) = (ulong) fptr2.ToInt64();
				*(sitePtr + 10) = 0x41; // jmp r11
				*(sitePtr + 11) = 0xFF;
				*(sitePtr + 12) = 0xE3;
			}

			/*
			    Note: For a x86/32 bit version, you can drop the REX prefixes (0x49, 0x41) of the opcodes.
			    You will also need to change ulong to uint. This yields opcodes for
			    mov ebx, target
			    jmp ebx
			    (which just happens to work since the REX prefix turns ebx into R11).
			*/
		}

		public static Assembly CreateAssemblyFromSource(string source, string[] assemblyLocations = null) {
			CSharpCodeProvider provider = new CSharpCodeProvider();
			CompilerParameters cp = new CompilerParameters();

			cp.OutputAssembly = "gen" + Guid.NewGuid().ToString().Replace("-", "");
			cp.GenerateExecutable = false;
			cp.GenerateInMemory = true;
			cp.IncludeDebugInformation = true;

			cp.ReferencedAssemblies.Add("System.dll");
			cp.ReferencedAssemblies.Add(typeof(object).Assembly.Location);

			if (assemblyLocations != null) {
				foreach (string assemblyLocation in assemblyLocations) {
					cp.ReferencedAssemblies.Add(assemblyLocation);
				}
			}

			CompilerResults cr = provider.CompileAssemblyFromSource(cp, source);

			if (cr.Errors.Count > 0) {
				throw new Exception(cr.Errors.ToString());
			}

			return cr.CompiledAssembly;
		}

		public static Assembly CreateAssemblyFromFile(string filePath, string[] assemblyLocations = null) {
			CSharpCodeProvider provider = new CSharpCodeProvider();
			CompilerParameters cp = new CompilerParameters();

			cp.OutputAssembly = "gen" + Guid.NewGuid().ToString().Replace("-", "");
			cp.GenerateExecutable = false;
			cp.GenerateInMemory = true;
			cp.IncludeDebugInformation = true;

			cp.ReferencedAssemblies.Add("System.dll");
			cp.ReferencedAssemblies.Add(typeof(object).Assembly.Location);

			if (assemblyLocations != null) {
				foreach (string assemblyLocation in assemblyLocations) {
					cp.ReferencedAssemblies.Add(assemblyLocation);
				}
			}

			CompilerResults cr = provider.CompileAssemblyFromFile(cp, filePath);

			if (cr.Errors.Count > 0) {
				throw new Exception(cr.Errors.ToString());
			}

			return cr.CompiledAssembly;
		}

		private static Assembly CompileSourceRoslyn(string fooSource, string[] assemblyLocations = null) {
			using MemoryStream? ms = new MemoryStream();
			using MemoryStream? ms2 = new MemoryStream();
			string assemblyFileName = "gen" + Guid.NewGuid().ToString().Replace("-", "") + ".dll";

			List<PortableExecutableReference> portableExecutableReferences = new List<PortableExecutableReference>();
			portableExecutableReferences.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

			if (assemblyLocations != null) {
				foreach (string assemblyLocation in assemblyLocations) {
					portableExecutableReferences.Add(MetadataReference.CreateFromFile(assemblyLocation));
				}
			}

			CSharpCompilation.Create(assemblyFileName,
									 new[] { CSharpSyntaxTree.ParseText(fooSource) },
									 portableExecutableReferences,
									 new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
			).Emit(ms, ms2);

			Assembly assembly = Assembly.Load(ms.GetBuffer(), ms2.GetBuffer());
			return assembly;
		}
	}
}