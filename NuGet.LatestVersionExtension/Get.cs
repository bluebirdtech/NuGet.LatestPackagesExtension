using System;
using NuGet.Commands;

namespace NuGet.LatestVersionExtension.Commands
{
    [Command(typeof(GetResources), "get", "GetCommandDescription", MinArgs = 1)]
    public class Get : Command
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        public override void ExecuteCommand()
        {
            //Probably need some better return code logic here...
            if (Arguments[0] == null) return;
            try
            {

            }
            catch (Exception e)
            {
                //HACK big catch here, but if anything goes wrong we want to log it and ensure a non-zero exit code...
                throw new CommandLineException(String.Format("GET Failed: {0}",e.Message),e);
            }
        }

    }
}
