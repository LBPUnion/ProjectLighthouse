#nullable enable
using System;
using System.Linq;
using LBPUnion.ProjectLighthouse.Logging;

namespace LBPUnion.ProjectLighthouse.Configuration;

public static class ServerStatics
{
    public const string ServerName = "ProjectLighthouse";

    public const int PageSize = 20;

    public static bool DbConnected {
        get {
            try
            {
                return new Database().Database.CanConnect();
            }
            catch(Exception e)
            {
                Logger.LogError(e.ToString(), LogArea.Database);
                return false;
            }
        }
    }

    public static bool IsUnitTesting => AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.FullName!.StartsWith("xunit"));

    #if DEBUG
    public static readonly bool IsDebug = true;
    #else
    public static readonly bool IsDebug = false;
    #endif
}