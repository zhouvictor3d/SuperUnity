using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuperUnity.Hub
{
    public class AppInfo : Singleton<AppInfo>
    {
        public int Target { get; set; }
        public string Group { get; set; }

        public int InstallVersion { get; set; }
    }
}