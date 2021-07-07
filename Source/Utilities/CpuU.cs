using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace RavUtilities {
	public class CpuU {
		private static readonly int cacheLineSize = CalculateCacheLineSize();
		private const int SC_LEVEL1_DCACHE_LINESIZE = 190;

		public static int GetCacheLineSize() {
			return cacheLineSize;
		}

		private static int CalculateCacheLineSize() {
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				SYSTEM_LOGICAL_PROCESSOR_INFORMATION[]? info = ManagedGetLogicalProcessorInformation();
				if (info == null) {
					throw new Exception("Could not retrieve the cache line indices.");
				}

				return info.First(x => x.Relationship == LOGICAL_PROCESSOR_RELATIONSHIP.RelationCache).ProcessorInformation.Cache.LineSize;
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
				return (int)sysconf(SC_LEVEL1_DCACHE_LINESIZE);
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
				IntPtr sizeOfLineSize = (IntPtr)IntPtr.Size;
				sysctlbyname("hw.cachelinesize", out IntPtr lineSize, ref sizeOfLineSize, IntPtr.Zero, IntPtr.Zero);
				return lineSize.ToInt32();
			}

			throw new Exception("Unrecognized OS platform.");
		}

		// http://stackoverflow.com/a/6972620/232574
		[StructLayout(LayoutKind.Sequential)]
		private struct PROCESSORCORE {
			public byte Flags;
		};

		[StructLayout(LayoutKind.Sequential)]
		private struct NUMANODE {
			public uint NodeNumber;
		}

		private enum PROCESSOR_CACHE_TYPE {
			CacheUnified,
			CacheInstruction,
			CacheData,
			CacheTrace
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct CACHE_DESCRIPTOR {
			public byte Level;
			public byte Associativity;
			public ushort LineSize;
			public uint Size;
			public PROCESSOR_CACHE_TYPE Type;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION_UNION {
			[FieldOffset(0)] public PROCESSORCORE ProcessorCore;
			[FieldOffset(0)] public NUMANODE NumaNode;
			[FieldOffset(0)] public CACHE_DESCRIPTOR Cache;
			[FieldOffset(0)] private ulong Reserved1;
			[FieldOffset(8)] private ulong Reserved2;
		}

		private enum LOGICAL_PROCESSOR_RELATIONSHIP {
			RelationProcessorCore,
			RelationNumaNode,
			RelationCache,
			RelationProcessorPackage,
			RelationGroup,
			RelationAll = 0xffff
		}

		private struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION {
			#pragma warning disable 0649

			public UIntPtr ProcessorMask;
			public LOGICAL_PROCESSOR_RELATIONSHIP Relationship;
			public SYSTEM_LOGICAL_PROCESSOR_INFORMATION_UNION ProcessorInformation;

			#pragma warning restore 0649
		}

		[DllImport(@"kernel32.dll", SetLastError = true)]
		private static extern bool GetLogicalProcessorInformation(IntPtr Buffer, ref uint ReturnLength);

		private const int ERROR_INSUFFICIENT_BUFFER = 122;

		private static SYSTEM_LOGICAL_PROCESSOR_INFORMATION[] ManagedGetLogicalProcessorInformation() {
			uint ReturnLength = 0;
			GetLogicalProcessorInformation(IntPtr.Zero, ref ReturnLength);
			if (Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER) {
				return null;
			}

			IntPtr Ptr = Marshal.AllocHGlobal((int)ReturnLength);
			try {
				if (GetLogicalProcessorInformation(Ptr, ref ReturnLength)) {
					int size = Marshal.SizeOf<SYSTEM_LOGICAL_PROCESSOR_INFORMATION>();
					int len = (int)ReturnLength / size;
					SYSTEM_LOGICAL_PROCESSOR_INFORMATION[] Buffer = new SYSTEM_LOGICAL_PROCESSOR_INFORMATION[len];
					IntPtr Item = Ptr;
					for (int i = 0; i < len; i++) {
						Buffer[i] = Marshal.PtrToStructure<SYSTEM_LOGICAL_PROCESSOR_INFORMATION>(Item);
						Item += size;
					}

					return Buffer;
				}
			} finally {
				Marshal.FreeHGlobal(Ptr);
			}

			return null;
		}

		[DllImport("libc")]
		private static extern int sysctlbyname(string name, out IntPtr oldp, ref IntPtr oldlenp, IntPtr newp, IntPtr newlen);

		[DllImport("libc")]
		private static extern long sysconf(int name);
	}
}
