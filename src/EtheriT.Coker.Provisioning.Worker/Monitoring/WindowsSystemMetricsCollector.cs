using System.Runtime.InteropServices;

namespace EtheriT.Coker.Provisioning.Worker;

public sealed class WindowsSystemMetricsCollector
{
    private ulong? previousIdle;
    private ulong? previousKernel;
    private ulong? previousUser;

    public SystemMetrics Collect(IReadOnlyList<IisApplicationPoolMetrics> applicationPools)
    {
        if (!OperatingSystem.IsWindows())
            return new SystemMetrics(null, 0, 0, null, [], applicationPools);

        var cpu = ReadCpuUsage();
        var disks = ReadDisks();
        var memory = new MemoryStatusEx
        {
            Length = (uint)Marshal.SizeOf<MemoryStatusEx>()
        };
        if (!GlobalMemoryStatusEx(ref memory))
            return new SystemMetrics(cpu, 0, 0, null, disks, applicationPools);

        var total = ToLong(memory.TotalPhysical);
        var available = ToLong(memory.AvailablePhysical);
        var used = Math.Max(0, total - available);
        double? usage = total > 0 ? Math.Clamp(used * 100d / total, 0, 100) : null;
        return new SystemMetrics(cpu, used, total, usage, disks, applicationPools);
    }

    private static IReadOnlyList<DiskMetrics> ReadDisks()
    {
        var output = new List<DiskMetrics>();
        foreach (var drive in DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Fixed))
        {
            try
            {
                if (!drive.IsReady || drive.TotalSize <= 0) continue;
                var used = Math.Max(0, drive.TotalSize - drive.AvailableFreeSpace);
                output.Add(new DiskMetrics(
                    drive.Name.TrimEnd('\\'),
                    drive.VolumeLabel,
                    used,
                    drive.TotalSize,
                    drive.AvailableFreeSpace,
                    Math.Round(Math.Clamp(used * 100d / drive.TotalSize, 0, 100), 1)));
            }
            catch (IOException)
            {
                // 磁碟可能在列舉後被卸載；略過本次取樣即可。
            }
            catch (UnauthorizedAccessException)
            {
                // 無權限讀取的磁碟不應中斷整台主機的心跳。
            }
        }
        return output;
    }

    private double? ReadCpuUsage()
    {
        if (!GetSystemTimes(out var idleTime, out var kernelTime, out var userTime))
            return null;

        var idle = idleTime.Value;
        var kernel = kernelTime.Value;
        var user = userTime.Value;
        if (previousIdle is null || previousKernel is null || previousUser is null)
        {
            previousIdle = idle;
            previousKernel = kernel;
            previousUser = user;
            return null;
        }

        var idleDelta = idle - previousIdle.Value;
        var kernelDelta = kernel - previousKernel.Value;
        var userDelta = user - previousUser.Value;
        previousIdle = idle;
        previousKernel = kernel;
        previousUser = user;
        var totalDelta = kernelDelta + userDelta;
        if (totalDelta == 0) return null;
        var busyDelta = totalDelta > idleDelta ? totalDelta - idleDelta : 0;
        return Math.Round(Math.Clamp(busyDelta * 100d / totalDelta, 0, 100), 1);
    }

    private static long ToLong(ulong value) => value > long.MaxValue ? long.MaxValue : (long)value;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemTimes(out FileTime idleTime, out FileTime kernelTime, out FileTime userTime);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);

    [StructLayout(LayoutKind.Sequential)]
    private struct FileTime
    {
        public uint Low;
        public uint High;
        public readonly ulong Value => ((ulong)High << 32) | Low;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhysical;
        public ulong AvailablePhysical;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;
    }
}
