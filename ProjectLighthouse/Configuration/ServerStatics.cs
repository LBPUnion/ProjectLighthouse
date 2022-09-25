#nullable enable
using System;
using System.Linq;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types;

namespace LBPUnion.ProjectLighthouse.Configuration;

public static class ServerStatics
{
    public const int PageSize = 20;

    public static bool DbConnected {
        get {
            try
            {
                return new Database().Database.CanConnect();
            }
            catch(Exception e)
            {
                Logger.Error(e.ToString(), LogArea.Database);
                return false;
            }
        }
    }

    // FIXME: This needs to go at some point.
    public static bool IsUnitTesting => AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.FullName!.StartsWith("xunit"));

    public static bool IsDebug()
    {
        #if DEBUG
        return true;
        #else
        return false;
        #endif
    }

    /// <summary>
    /// The servertype, determined on startup. Shouldn't be null unless very very early in startup.
    /// </summary>
    // The way of doing this is kinda weird, but it works.
    public static ServerType ServerType;
}