// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EdFi.Admin.LearningStandards.CLI.Internal;
using EdFi.Admin.LearningStandards.Core;

[assembly: InternalsVisibleTo("EdFi.Admin.LearningStandards.Tests")]
namespace EdFi.Admin.LearningStandards.CLI
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            int appResult = 1;
            ILearningStandardsCLIApplicationBase app = null;

            try
            {
                IResponse appResponse;
                if (!args[0].ToLower().Equals("sync-from-csv"))
                {
                    app = new LearningStandardsCLIApplication();
                    appResponse = await app.Main(args).ConfigureAwait(false);
                }
                else
                {
                    app = new LearningStandardsCLICSVSyncApplication();
                    appResponse = await app.Main(args).ConfigureAwait(false);
                }

                appResult = appResponse != null && appResponse.IsSuccess ? 0 : 1;
            }
            catch (OperationCanceledException)
            {
                app?.CliWriter.Info("Operation cancelled");
            }
            catch (LearningStandardsCLIParserException)
            {
                Console.ReadLine();
                return 1;
            }
            catch (Exception e)
            {
                app?.CliWriter.Error(e.Message);
            }

            if (app != null && !app.Unattended)
            {
                Console.WriteLine("Finished. Press any key to exit.");
                Console.ReadLine();
            }

            return appResult;
        }
    }
}


