using System;

namespace Omni.Servio
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : Attribute
    {
        public bool UseGlobal { get; set; } = false;
    }
}

