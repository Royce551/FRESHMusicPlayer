using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tmds.DBus;

[assembly: InternalsVisibleTo(Tmds.DBus.Connection.DynamicAssemblyName)]
namespace Desktop.DBus
{
    [DBusInterface("org.freedesktop.portal.ProxyResolver")]
    interface IProxyResolver : IDBusObject
    {
        Task<string[]> LookupAsync(string Uri);
        Task<T> GetAsync<T>(string prop);
        Task<ProxyResolverProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class ProxyResolverProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class ProxyResolverExtensions
    {
        public static Task<uint> GetVersionAsync(this IProxyResolver o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.FileChooser")]
    interface IFileChooser : IDBusObject
    {
        Task<ObjectPath> OpenFileAsync(string ParentWindow, string Title, IDictionary<string, object> Options);
        Task<ObjectPath> SaveFileAsync(string ParentWindow, string Title, IDictionary<string, object> Options);
        Task<ObjectPath> SaveFilesAsync(string ParentWindow, string Title, IDictionary<string, object> Options);
        Task<T> GetAsync<T>(string prop);
        Task<FileChooserProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class FileChooserProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class FileChooserExtensions
    {
        public static Task<uint> GetVersionAsync(this IFileChooser o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.Notification")]
    interface INotification : IDBusObject
    {
        Task AddNotificationAsync(string Id, IDictionary<string, object> Notification);
        Task RemoveNotificationAsync(string Id);
        Task<IDisposable> WatchActionInvokedAsync(Action<(string id, string action, object[] parameter)> handler, Action<Exception> onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<NotificationProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class NotificationProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class NotificationExtensions
    {
        public static Task<uint> GetVersionAsync(this INotification o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.Location")]
    interface ILocation : IDBusObject
    {
        Task<ObjectPath> CreateSessionAsync(IDictionary<string, object> Options);
        Task<ObjectPath> StartAsync(ObjectPath SessionHandle, string ParentWindow, IDictionary<string, object> Options);
        Task<IDisposable> WatchLocationUpdatedAsync(Action<(ObjectPath sessionHandle, IDictionary<string, object> location)> handler, Action<Exception> onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<LocationProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class LocationProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class LocationExtensions
    {
        public static Task<uint> GetVersionAsync(this ILocation o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.Screenshot")]
    interface IScreenshot : IDBusObject
    {
        Task<ObjectPath> ScreenshotAsync(string ParentWindow, IDictionary<string, object> Options);
        Task<ObjectPath> PickColorAsync(string ParentWindow, IDictionary<string, object> Options);
        Task<T> GetAsync<T>(string prop);
        Task<ScreenshotProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class ScreenshotProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class ScreenshotExtensions
    {
        public static Task<uint> GetVersionAsync(this IScreenshot o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.Account")]
    interface IAccount : IDBusObject
    {
        Task<ObjectPath> GetUserInformationAsync(string Window, IDictionary<string, object> Options);
        Task<T> GetAsync<T>(string prop);
        Task<AccountProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class AccountProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class AccountExtensions
    {
        public static Task<uint> GetVersionAsync(this IAccount o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.NetworkMonitor")]
    interface INetworkMonitor : IDBusObject
    {
        Task<bool> GetAvailableAsync();
        Task<bool> GetMeteredAsync();
        Task<uint> GetConnectivityAsync();
        Task<IDictionary<string, object>> GetStatusAsync();
        Task<bool> CanReachAsync(string Hostname, uint Port);
        Task<IDisposable> WatchchangedAsync(Action handler, Action<Exception> onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<NetworkMonitorProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class NetworkMonitorProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class NetworkMonitorExtensions
    {
        public static Task<uint> GetVersionAsync(this INetworkMonitor o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.Print")]
    interface IPrint : IDBusObject
    {
        Task<ObjectPath> PrintAsync(string ParentWindow, string Title, CloseSafeHandle Fd, IDictionary<string, object> Options);
        Task<ObjectPath> PreparePrintAsync(string ParentWindow, string Title, IDictionary<string, object> Settings, IDictionary<string, object> PageSetup, IDictionary<string, object> Options);
        Task<T> GetAsync<T>(string prop);
        Task<PrintProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class PrintProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class PrintExtensions
    {
        public static Task<uint> GetVersionAsync(this IPrint o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.Settings")]
    interface ISettings : IDBusObject
    {
        Task<IDictionary<string, IDictionary<string, object>>> ReadAllAsync(string[] Namespaces);
        Task<object> ReadAsync(string Namespace, string Key);
        Task<IDisposable> WatchSettingChangedAsync(Action<(string @namespace, string key, object value)> handler, Action<Exception> onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<SettingsProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class SettingsProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class SettingsExtensions
    {
        public static Task<uint> GetVersionAsync(this ISettings o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.GameMode")]
    interface IGameMode : IDBusObject
    {
        Task<int> QueryStatusAsync(int Pid);
        Task<int> RegisterGameAsync(int Pid);
        Task<int> UnregisterGameAsync(int Pid);
        Task<int> QueryStatusByPidAsync(int Target, int Requester);
        Task<int> RegisterGameByPidAsync(int Target, int Requester);
        Task<int> UnregisterGameByPidAsync(int Target, int Requester);
        Task<int> QueryStatusByPIDFdAsync(CloseSafeHandle Target, CloseSafeHandle Requester);
        Task<int> RegisterGameByPIDFdAsync(CloseSafeHandle Target, CloseSafeHandle Requester);
        Task<int> UnregisterGameByPIDFdAsync(CloseSafeHandle Target, CloseSafeHandle Requester);
        Task<T> GetAsync<T>(string prop);
        Task<GameModeProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class GameModeProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class GameModeExtensions
    {
        public static Task<uint> GetVersionAsync(this IGameMode o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.RemoteDesktop")]
    interface IRemoteDesktop : IDBusObject
    {
        Task<ObjectPath> CreateSessionAsync(IDictionary<string, object> Options);
        Task<ObjectPath> SelectDevicesAsync(ObjectPath SessionHandle, IDictionary<string, object> Options);
        Task<ObjectPath> StartAsync(ObjectPath SessionHandle, string ParentWindow, IDictionary<string, object> Options);
        Task NotifyPointerMotionAsync(ObjectPath SessionHandle, IDictionary<string, object> Options, double Dx, double Dy);
        Task NotifyPointerMotionAbsoluteAsync(ObjectPath SessionHandle, IDictionary<string, object> Options, uint Stream, double X, double Y);
        Task NotifyPointerButtonAsync(ObjectPath SessionHandle, IDictionary<string, object> Options, int Button, uint State);
        Task NotifyPointerAxisAsync(ObjectPath SessionHandle, IDictionary<string, object> Options, double Dx, double Dy);
        Task NotifyPointerAxisDiscreteAsync(ObjectPath SessionHandle, IDictionary<string, object> Options, uint Axis, int Steps);
        Task NotifyKeyboardKeycodeAsync(ObjectPath SessionHandle, IDictionary<string, object> Options, int Keycode, uint State);
        Task NotifyKeyboardKeysymAsync(ObjectPath SessionHandle, IDictionary<string, object> Options, int Keysym, uint State);
        Task NotifyTouchDownAsync(ObjectPath SessionHandle, IDictionary<string, object> Options, uint Stream, uint Slot, double X, double Y);
        Task NotifyTouchMotionAsync(ObjectPath SessionHandle, IDictionary<string, object> Options, uint Stream, uint Slot, double X, double Y);
        Task NotifyTouchUpAsync(ObjectPath SessionHandle, IDictionary<string, object> Options, uint Slot);
        Task<T> GetAsync<T>(string prop);
        Task<RemoteDesktopProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class RemoteDesktopProperties
    {
        private uint _AvailableDeviceTypes = default(uint);
        public uint AvailableDeviceTypes
        {
            get
            {
                return _AvailableDeviceTypes;
            }

            set
            {
                _AvailableDeviceTypes = (value);
            }
        }

        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class RemoteDesktopExtensions
    {
        public static Task<uint> GetAvailableDeviceTypesAsync(this IRemoteDesktop o) => o.GetAsync<uint>("AvailableDeviceTypes");
        public static Task<uint> GetVersionAsync(this IRemoteDesktop o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.MemoryMonitor")]
    interface IMemoryMonitor : IDBusObject
    {
        Task<IDisposable> WatchLowMemoryWarningAsync(Action<byte> handler, Action<Exception> onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<MemoryMonitorProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class MemoryMonitorProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class MemoryMonitorExtensions
    {
        public static Task<uint> GetVersionAsync(this IMemoryMonitor o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.OpenURI")]
    interface IOpenURI : IDBusObject
    {
        Task<ObjectPath> OpenURIAsync(string ParentWindow, string Uri, IDictionary<string, object> Options);
        Task<ObjectPath> OpenFileAsync(string ParentWindow, CloseSafeHandle Fd, IDictionary<string, object> Options);
        Task<ObjectPath> OpenDirectoryAsync(string ParentWindow, CloseSafeHandle Fd, IDictionary<string, object> Options);
        Task<T> GetAsync<T>(string prop);
        Task<OpenURIProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class OpenURIProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class OpenURIExtensions
    {
        public static Task<uint> GetVersionAsync(this IOpenURI o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.Secret")]
    interface ISecret : IDBusObject
    {
        Task<ObjectPath> RetrieveSecretAsync(CloseSafeHandle Fd, IDictionary<string, object> Options);
        Task<T> GetAsync<T>(string prop);
        Task<SecretProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class SecretProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class SecretExtensions
    {
        public static Task<uint> GetVersionAsync(this ISecret o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.Wallpaper")]
    interface IWallpaper : IDBusObject
    {
        Task<ObjectPath> SetWallpaperURIAsync(string ParentWindow, string Uri, IDictionary<string, object> Options);
        Task<ObjectPath> SetWallpaperFileAsync(string ParentWindow, CloseSafeHandle Fd, IDictionary<string, object> Options);
        Task<T> GetAsync<T>(string prop);
        Task<WallpaperProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class WallpaperProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class WallpaperExtensions
    {
        public static Task<uint> GetVersionAsync(this IWallpaper o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.Camera")]
    interface ICamera : IDBusObject
    {
        Task<ObjectPath> AccessCameraAsync(IDictionary<string, object> Options);
        Task<CloseSafeHandle> OpenPipeWireRemoteAsync(IDictionary<string, object> Options);
        Task<T> GetAsync<T>(string prop);
        Task<CameraProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class CameraProperties
    {
        private bool _IsCameraPresent = default(bool);
        public bool IsCameraPresent
        {
            get
            {
                return _IsCameraPresent;
            }

            set
            {
                _IsCameraPresent = (value);
            }
        }

        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class CameraExtensions
    {
        public static Task<bool> GetIsCameraPresentAsync(this ICamera o) => o.GetAsync<bool>("IsCameraPresent");
        public static Task<uint> GetVersionAsync(this ICamera o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.Device")]
    interface IDevice : IDBusObject
    {
        Task<ObjectPath> AccessDeviceAsync(uint Pid, string[] Devices, IDictionary<string, object> Options);
        Task<T> GetAsync<T>(string prop);
        Task<DeviceProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class DeviceProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class DeviceExtensions
    {
        public static Task<uint> GetVersionAsync(this IDevice o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.ScreenCast")]
    interface IScreenCast : IDBusObject
    {
        Task<ObjectPath> CreateSessionAsync(IDictionary<string, object> Options);
        Task<ObjectPath> SelectSourcesAsync(ObjectPath SessionHandle, IDictionary<string, object> Options);
        Task<ObjectPath> StartAsync(ObjectPath SessionHandle, string ParentWindow, IDictionary<string, object> Options);
        Task<CloseSafeHandle> OpenPipeWireRemoteAsync(ObjectPath SessionHandle, IDictionary<string, object> Options);
        Task<T> GetAsync<T>(string prop);
        Task<ScreenCastProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class ScreenCastProperties
    {
        private uint _AvailableSourceTypes = default(uint);
        public uint AvailableSourceTypes
        {
            get
            {
                return _AvailableSourceTypes;
            }

            set
            {
                _AvailableSourceTypes = (value);
            }
        }

        private uint _AvailableCursorModes = default(uint);
        public uint AvailableCursorModes
        {
            get
            {
                return _AvailableCursorModes;
            }

            set
            {
                _AvailableCursorModes = (value);
            }
        }

        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class ScreenCastExtensions
    {
        public static Task<uint> GetAvailableSourceTypesAsync(this IScreenCast o) => o.GetAsync<uint>("AvailableSourceTypes");
        public static Task<uint> GetAvailableCursorModesAsync(this IScreenCast o) => o.GetAsync<uint>("AvailableCursorModes");
        public static Task<uint> GetVersionAsync(this IScreenCast o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.Email")]
    interface IEmail : IDBusObject
    {
        Task<ObjectPath> ComposeEmailAsync(string ParentWindow, IDictionary<string, object> Options);
        Task<T> GetAsync<T>(string prop);
        Task<EmailProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class EmailProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class EmailExtensions
    {
        public static Task<uint> GetVersionAsync(this IEmail o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.Inhibit")]
    interface IInhibit : IDBusObject
    {
        Task<ObjectPath> InhibitAsync(string Window, uint Flags, IDictionary<string, object> Options);
        Task<ObjectPath> CreateMonitorAsync(string Window, IDictionary<string, object> Options);
        Task QueryEndResponseAsync(ObjectPath SessionHandle);
        Task<IDisposable> WatchStateChangedAsync(Action<(ObjectPath sessionHandle, IDictionary<string, object> state)> handler, Action<Exception> onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<InhibitProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class InhibitProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class InhibitExtensions
    {
        public static Task<uint> GetVersionAsync(this IInhibit o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.Trash")]
    interface ITrash : IDBusObject
    {
        Task<uint> TrashFileAsync(CloseSafeHandle Fd);
        Task<T> GetAsync<T>(string prop);
        Task<TrashProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class TrashProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class TrashExtensions
    {
        public static Task<uint> GetVersionAsync(this ITrash o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.Background")]
    interface IBackground : IDBusObject
    {
        Task<ObjectPath> RequestBackgroundAsync(string ParentWindow, IDictionary<string, object> Options);
        Task<T> GetAsync<T>(string prop);
        Task<BackgroundProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class BackgroundProperties
    {
        private uint _version = default(uint);
        public uint Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class BackgroundExtensions
    {
        public static Task<uint> GetVersionAsync(this IBackground o) => o.GetAsync<uint>("version");
    }

    [DBusInterface("org.freedesktop.portal.Request")]
    interface IRequest : IDBusObject
    {
        Task CloseAsync();
        Task<IDisposable> WatchResponseAsync(Action<(uint response, IDictionary<string, object> results)> handler, Action<Exception> onError = null);
    }
}