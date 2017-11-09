// Groovy Script: http://www.groovy-lang.org/syntax.html
// Jenkins DSL: https://github.com/jenkinsci/job-dsl-plugin/wiki

import jobs.generation.Utilities;
import jobs.generation.ArchivalSettings;

static getJobName(def opsysName, def configName) {
  return "${opsysName}_${configName}"
}

static addArchival(def job, def filesToArchive, def filesToExclude) {
  def archivalSettings = new ArchivalSettings()
  archivalSettings.addFiles(filesToArchive)
  archivalSettings.excludeFiles(filesToExclude)
  archivalSettings.setFailIfNothingArchived()
  archivalSettings.setArchiveOnFailure()

  Utilities.addArchival(job, archivalSettings)
}

static addGithubPRTriggerForBranch(def job, def branchName, def jobName) {
  def prContext = "prtest/${jobName.replace('_', '/')}"
  def triggerPhrase = "(?i)^\\s*(@?dotnet-bot\\s+)?(re)?test\\s+(${prContext})(\\s+please)?\\s*\$"
  def triggerOnPhraseOnly = false

  Utilities.addGithubPRTriggerForBranch(job, branchName, prContext, triggerPhrase, triggerOnPhraseOnly)
}

static addXUnitDotNETResults(def job, def configName) {
  def resultFilePattern = "**/artifacts/${configName}/TestResults/*.xml"
  def skipIfNoTestFiles = false
    
  Utilities.addXUnitDotNETResults(job, resultFilePattern, skipIfNoTestFiles)
}

static addBuildSteps(def job, def projectName, def opsysName, def configName, def isPR) {
  def buildJobName = getJobName(opsysName, configName)
  def buildFullJobName = Utilities.getFullJobName(projectName, buildJobName, isPR)

  job.with {
    steps {
      batchFile(""".\\build\\CIBuild.cmd -configuration ${configName} -prepareMachine""")
    }
  }
}

[true, false].each { isPR ->
  ['windows'].each { opsysName ->
    ['debug', 'release'].each { configName ->
      def projectName = GithubProject

      def branchName = GithubBranchName

      def filesToArchive = "**/artifacts/**"
      def filesToExclude = "**/artifacts/${configName}/obj/**"

      def jobName = getJobName(opsysName, configName)
      def fullJobName = Utilities.getFullJobName(projectName, jobName, isPR)
      def myJob = job(fullJobName)

      Utilities.standardJobSetup(myJob, projectName, isPR, "*/${branchName}")

      if (isPR) {
        addGithubPRTriggerForBranch(myJob, branchName, jobName)
      } else {
        Utilities.addGithubPushTrigger(myJob)
      }
      
      addArchival(myJob, filesToArchive, filesToExclude)
      addXUnitDotNETResults(myJob, configName)

      Utilities.setMachineAffinity(myJob, 'Windows_NT', 'latest-dev15-3')

      addBuildSteps(myJob, projectName, opsysName, configName, isPR)
    }
  }
}
