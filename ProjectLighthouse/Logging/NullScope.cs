using System;

namespace LBPUnion.ProjectLighthouse.Logging
{
    public class NullScope : IDisposable
    {

        private NullScope()
        {}
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}