namespace CloudCopy
{
    using System;

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    sealed class FlagAttribute : Attribute
    {
        readonly string acronim;
        readonly string description;

        public FlagAttribute(string acronim, string description)
        {
            this.acronim = acronim;
            this.description = description;
        }

        public string Acronim
        {
            get { return acronim; }
        }

        public string Description
        {
            get { return description; }
        }

        public bool IsRequired { get; set; }

        public string Sample { get; set; }
    }
}
