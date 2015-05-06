using System;
using System.ComponentModel;
using System.Resources;

namespace Common.WebDefinitions.Localization
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        readonly string resourceKey;
        readonly ResourceManager resource;

        public LocalizedDescriptionAttribute(Type resourceType, string resourceKey)
        {
            resource = new ResourceManager(resourceType);
            this.resourceKey = resourceKey;
        }

        public override string Description
        {
            get
            {
                string displayName = resource.GetString(resourceKey);

                if (!string.IsNullOrEmpty(displayName))
                {
                    return displayName;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
