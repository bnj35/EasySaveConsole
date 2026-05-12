namespace EasySaveConsole
{
    public static class PriorityFileManager
    {
        public static bool HasPendingPriorityFiles(Joblist joblist, List<string> priorityExtensions)
        {
            if (priorityExtensions == null || priorityExtensions.Count == 0)
                return false;

            var allJobs = joblist.GetAllJobs();
            
            foreach (var job in allJobs)
            {
                if (job.IsRunning)
                {
                    if (HasPriorityFilesInJob(job, priorityExtensions))
                        return true;
                }
            }

            return false;
        }
        private static bool HasPriorityFilesInJob(BackupJob job, List<string> priorityExtensions)
        {
            if (string.IsNullOrWhiteSpace(job.SourceDir))
                return false;

            try
            {
                var copyPlan = CopyPlanner.Build(job.SourceDir, job.TargetDir);
                var priorityFiles = copyPlan.GetPriorityFiles(priorityExtensions);
                return priorityFiles.Count > 0;
            }
            catch
            {
                return false;
            }
        }
        public static bool CanExecuteNonPriorityJob(
            BackupJob jobToExecute, 
            Joblist joblist, 
            List<string> priorityExtensions)
        {
            if (ContainsOnlyNonPriorityFiles(jobToExecute, priorityExtensions))
            {
                var allJobs = joblist.GetAllJobs();
                var otherJobs = new Joblist();
                
                foreach (var job in allJobs)
                {
                    if (job != jobToExecute)
                    {
                        otherJobs.AddJob(job);
                    }
                }
                
                return !HasPendingPriorityFiles(otherJobs, priorityExtensions);
            }
            return true;
        }
        private static bool ContainsOnlyNonPriorityFiles(BackupJob job, List<string> priorityExtensions)
        {
            if (string.IsNullOrWhiteSpace(job.SourceDir))
                return false;

            try
            {
                var copyPlan = CopyPlanner.Build(job.SourceDir, job.TargetDir);
                var nonPriorityFiles = copyPlan.GetNonPriorityFiles(priorityExtensions);
                var priorityFiles = copyPlan.GetPriorityFiles(priorityExtensions);
                
                return priorityFiles.Count == 0 && nonPriorityFiles.Count > 0;
            }
            catch
            {
                return false;
            }
        }
        public static List<BackupJob> GetJobsWithPriorityFiles(Joblist joblist, List<string> priorityExtensions)
        {
            var result = new List<BackupJob>();

            if (priorityExtensions == null || priorityExtensions.Count == 0)
                return result;

            var allJobs = joblist.GetAllJobs();

            foreach (var job in allJobs)
            {
                if (HasPriorityFilesInJob(job, priorityExtensions))
                {
                    result.Add(job);
                }
            }

            return result;
        }
    }
}