using System;

namespace LBPUnion.ProjectLighthouse.Logging {
    public class NullScope : IDisposable{
        public static NullScope Instance { get; } = new();

        private NullScope() {}

        public void Dispose() {
            GC.SuppressFinalize(this);
        }
    }
}