using System;
using System.Collections.Generic;
using System.Text;

namespace RKSoftware.IPUtilities.AddOns
{
    public interface IAddon
    {
        string Name { get; }
        string Command { get; }
        string Description { get; }
        string Version { get; }
        void Initialize();
        void Execute(string[] args);
    }
}
