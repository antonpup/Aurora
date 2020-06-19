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
    /// Class to update pointers for applications with memory reading functionality.
    /// </summary>
    public static class PointerUpdateUtils
    {
        private static HashSet<string> appsToUpdate = new HashSet<string>();
        private static GitHubClient octokitClient = new GitHubClient(new ProductHeaderValue("aurora-pointer-updater"));
        private static readonly HttpClient pointerClient = new HttpClient()
        {
            // Just so if the thread decides to wait, it does not hang until the response times out
            Timeout = new TimeSpan(0, 0, 3)
        };

        /// <summary>
        /// Add application to pointer updater. Should be called in the constructor inheriting the Application class (e.g., Borderlands2 : Application).
        /// </summary>
        /// <param name="appPointerFile">The application's referenced pointer file.</param>
        public static void MarkAppForUpdate(string appPointerFile)
        {
            appsToUpdate.Add(appPointerFile);
        }

        /// <summary>
        /// Async task to fetch all pointer files contained in a specific branch from the Aurora repository.
        /// It will download files marked for update by MarkAppForUpdate(string).
        /// This will also retrieve deleted files.
        /// </summary>
        /// <param name="branch">The branch in antonpup/Aurora to pull the pointers from.</param>
        /// <param name="useOctokit">Get the pointers through GitHub's Content API using Octokit. Recommend this to be false to avoid rate limiting by GitHub's API.</param>
        public static async Task FetchPointers(string branch, bool useOctokit = false)
        {
            // If no games to update, return
            if (appsToUpdate.Count == 0) return;

            // Update pointer files in Aurora/Pointers/
            string pointerPath = Path.Combine(Global.ExecutingDirectory, "Pointers");
            string repoPath = "Project-Aurora/Project-Aurora/Pointers";

            foreach (string app in appsToUpdate)
            {
                if (useOctokit)
                {
                    // Use Octokit to get pointer files through API
                    try
                    {
                        IReadOnlyCollection<RepositoryContent> repoFile = await octokitClient.Repository.Content.GetAllContentsByRef("antonpup", "Aurora", repoPath + "/" + app + ".json", branch);
                        File.WriteAllText(Path.Combine(pointerPath, repoFile.ElementAt(0).Name), repoFile.ElementAt(0).Content);
                    }
                    catch (Exception e)
                    {
                        Global.logger.Error("FetchPointers Octokit exception, " + e);
                    }
                }
                else
                {
                    try
                    {
                        //TEMPLATE: https://github.com/antonpup/Aurora/raw/[BRANCH]/Project-Aurora/Project-Aurora/Pointers/[GAME].json
                        // This should redirect to raw.githubusercontent.com and comply with 301 redirect requests
                        string content = await pointerClient.GetStringAsync(@"https://github.com/antonpup/Aurora/raw/" + branch + @"/Project-Aurora/Project-Aurora/Pointers/" + app + ".json");
                        File.WriteAllText(Path.Combine(pointerPath, app) + ".json", content);
                    }
                    catch (Exception e)
                    {
                        Global.logger.Error("FetchPointers HTTP exception, " + e);
                    }
                }
            }
        }

        /// <summary>
        /// Async task to fetch all pointer files contained in a specific branch from the Aurora repository.
        /// It will download all files, including those for applications merged to the branch before a release.
        /// </summary>
        /// <param name="branch">The branch in antonpup/Aurora to pull the pointers from.</param>
        /// <param name="useOctokit">Get the pointers through GitHub's Content API using Octokit. Recommend this to be false to avoid rate limiting by GitHub's API.</param>
        public static async Task FetchDevPointers(string branch, bool useOctokit = false)
        {
            // Update pointer files in Aurora/Pointers/
            string pointerPath = Path.Combine(Global.ExecutingDirectory, "Pointers");
            string repoPath = "Project-Aurora/Project-Aurora/Pointers";

            // API call to get directory in repo where pointer jsons are held
            IReadOnlyCollection<RepositoryContent> content = await octokitClient.Repository.Content.GetAllContentsByRef("antonpup", "Aurora", repoPath, branch);

            foreach (RepositoryContent pointerRepoFiles in content)
            {
                if (useOctokit)
                {
                    // Use Octokit to get pointer files through API
                    try
                    {
                        IReadOnlyCollection<RepositoryContent> repoFile = await octokitClient.Repository.Content.GetAllContentsByRef("antonpup", "Aurora", repoPath + "/" + pointerRepoFiles.Name, branch);
                        File.WriteAllText(Path.Combine(pointerPath, repoFile.ElementAt(0).Name), repoFile.ElementAt(0).Content);
                    }
                    catch (Exception e)
                    {
                        Global.logger.Error("FetchDevPointers Octokit exception, " + e);
                    }
                }
                else
                {
                    // Make an HTTP request to the raw server
                    string app = pointerRepoFiles.Name;

                    try
                    {
                        //TEMPLATE: https://github.com/antonpup/Aurora/raw/[BRANCH]/Project-Aurora/Project-Aurora/Pointers/[GAME].json
                        // This should redirect to raw.githubusercontent.com and comply with 301 redirect requests
                        string fContent = await pointerClient.GetStringAsync(@"https://github.com/antonpup/Aurora/raw/" + branch + @"/Project-Aurora/Project-Aurora/Pointers/" + app);
                        File.WriteAllText(Path.Combine(pointerPath, app), fContent);
                    }
                    catch (Exception e)
                    {
                        Global.logger.Error("FetchDevPointers HTTP exception, " + e);
                    }
                }
            }
        }
    }
}
