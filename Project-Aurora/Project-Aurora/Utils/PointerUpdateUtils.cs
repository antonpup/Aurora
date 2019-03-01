using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit;

namespace Aurora.Utils
{
    /// <summary>
    /// Class to update pointers for games with memory reading functionality.
    /// </summary>
    public static class PointerUpdateUtils
    {
        private static GitHubClient gClient = new GitHubClient(new ProductHeaderValue("aurora-pointer-updater"));

        /// <summary>
        /// Updates the Pointers directory with the most recent from a specified branch.
        /// </summary>
        /// <param name="branch">The branch in antonpup/Aurora to pull the pointers from.</param>
        /// <param name="useOctokit">Get the pointers through GitHub's Content API using Octokit. Recommend this to be false to avoid rate limiting by GitHub's API.</param>
        /// <param name="backgroundTask">Set updating pointer files as a background task.</param>
        /// <returns></returns>
        public static void UpdatePointers(string branch, bool useOctokit = false, bool backgroundTask = false)
        {
            if (!backgroundTask) Task.Run(() => FetchAllPointers(branch, useOctokit)).Wait();
            else Task.Run(() => FetchAllPointers(branch, useOctokit));
        }

        public static void UpdateLocalPointers(string branch, bool backgroundTask = false)
        {
            if (!backgroundTask) Task.Run(() => FetchLocalPointers(branch)).Wait();
            else Task.Run(() => FetchLocalPointers(branch));
        }

        private static async Task FetchAllPointers(string branch, bool useOctokit)
        {
            // Update pointer files in Aurora/Pointers/
            string pointerPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            pointerPath += @"\Pointers\";

            string repoPath = "Project-Aurora/Project-Aurora/Pointers";

            // API call to get directory in repo where pointer jsons are held
            IReadOnlyCollection<RepositoryContent> content = await gClient.Repository.Content.GetAllContentsByRef("antonpup", "Aurora", repoPath, branch);

            foreach (RepositoryContent pointerRepoFiles in content)
            {
                if (useOctokit)
                {
                    // Use Octokit to get pointer files through API
                    try
                    {
                        IReadOnlyCollection<RepositoryContent> repoFile = await gClient.Repository.Content.GetAllContentsByRef("antonpup", "Aurora", repoPath + "/" + pointerRepoFiles.Name, branch);
                        File.WriteAllText(pointerPath + repoFile.ElementAt(0).Name, repoFile.ElementAt(0).Content);
                    }
                    catch (Exception e)
                    {
                        Global.logger.Error("FetchAllPointers exception, " + e);
                    }
                }
                else
                {
                    // Make an HTTP request to the raw server
                    string game = pointerRepoFiles.Name;

                    using (HttpClient client = new HttpClient())
                    {
                        try
                        {
                            //TEMPLATE: https://github.com/antonpup/Aurora/raw/[BRANCH]/Project-Aurora/Project-Aurora/Pointers/[GAME].json
                            // This should redirect to raw.githubusercontent.com and comply with 301 redirect requests
                            string fContent = await client.GetStringAsync(@"https://github.com/antonpup/Aurora/raw/" + branch + @"/Project-Aurora/Project-Aurora/Pointers/" + game);
                            File.WriteAllText(pointerPath + game, fContent);
                        }
                        catch (Exception e)
                        {
                            Global.logger.Error("FetchAllPointers exception, " + e);
                        }
                    }
                }
            }
        }

        static private async Task FetchLocalPointers(string branch)
        {
            // This method will only update the pointer files found locally from the repo (as in if a file is deleted, it won't be pulled from the repo).
            // Truly an edge case, as every update will rewrite this folder.
            // Not sure if this will hit some rate limit.

            // Update pointer files in Aurora/Pointers/
            string pointerPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            pointerPath += @"\Pointers";
            string[] pointerFiles = Directory.GetFiles(pointerPath);

            foreach (string pFile in pointerFiles)
            {
                string[] splitPath = pFile.Split(new[] { "\\" }, StringSplitOptions.None);
                string game = splitPath[splitPath.Length - 1];

                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        //TEMPLATE: https://github.com/antonpup/Aurora/raw/[BRANCH]/Project-Aurora/Project-Aurora/Pointers/[GAME].json
                        // This should redirect to raw.githubusercontent.com and comply with 301 redirect requests
                        string content = await client.GetStringAsync(@"https://github.com/antonpup/Aurora/raw/" + branch + @"/Project-Aurora/Project-Aurora/Pointers/" + game);
                        File.WriteAllText(pFile, content);
                    }
                    catch (Exception e)
                    {
                        Global.logger.Error("FetchLocalPointers exception, " + e);
                    }
                }
            }
        }
    }
}
