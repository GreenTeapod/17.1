using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Diagnostics.CodeAnalysis;
namespace First;
file class Program
{
    static void Main()
    {
        #pragma warning disable CA1416
        Console.WriteLine(new SocketInformation(803, "www.google.com").ToString());
        Console.WriteLine(new SocketInformation(80, "www.google.com").ToString());
        #pragma warning restore CA1416
    }
}
file ref struct SocketInformation
{
    private static readonly object _lock = new object();

    internal required int Port;
    [AllowNull] internal required string? Host;

    [SetsRequiredMembers]
        #if TRACE && NET7_0
    internal SocketInformation(int _port, [NotNull]string host) => (Port, Host) = (_port, host);

    [DllImport("wininet.dll")]
    private static extern bool InternetGetConnectedState(out int Description, int ReservedValue);

    [SupportedOSPlatform("windows")]
    public readonly bool IsPortAvaible()
    {
        try 
        { 
            if (Monitor.TryEnter(_lock) is false || RuntimeInformation.IsOSPlatform(OSPlatform.Windows) is false && Host != string.Empty && Port < 0 ||
                InternetGetConnectedState(out int _, 0) is false)
                return false;
            using Socket client = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(Host!, Port);
            return client.Connected;
        }
        catch
        {
            return false;
        }
        finally
        {
            Monitor.Exit(_lock);
        }
        #endif
    }
    public readonly override string ToString()
    {
        return $"Host = {Host}, Port = {Port}, подключен : {(IsPortAvaible() ? "подключен" : "не подключен")}";
    }
}
