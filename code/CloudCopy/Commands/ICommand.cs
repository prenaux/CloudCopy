namespace CloudCopy.Commands
{
    using System;

    public interface ICommand
    {
        bool Execute();
    }
}
