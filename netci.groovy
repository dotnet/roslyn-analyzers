// Groovy Script: http://www.groovy-lang.org/syntax.html
// Jenkins DSL: https://github.com/jenkinsci/job-dsl-plugin/wiki

import jobs.generation.Utilities;

static getJobName(def opsysName, def configName) {
  return "${opsysName}_${configName}"
}

static addArchival(def job, def filesToArchive, def filesToExclude) {
  def doNotFailIfNothingArchived = false
  def archiveOnlyIfSuccessful = false

  Utilities.addArchival(job, filesToArchive, filesToExclude, doNotFailIfNothingArchived, archiveOnlyIfSuccessful)
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

static addBuildSteps(def job, def projectName, def os, def configName, def isPR) {
  def buildJobName = getJobName(os, configName)
  def buildFullJobName = Utilities.getFullJobName(projectName, buildJobName, isPR)

  job.with {
    wrappers {
      credentialsBinding {
        string("CODECOV_TOKEN", "CODECOV_TOKEN_DOTNET_ROSLYN_ANALYZERS")
      }
    }
    steps {
      if (os == "Windows_NT") {
        batchFile(""".\\eng\\common\\CIBuild.cmd -configuration ${configName} -prepareMachine""")
      } else {
        shell("./eng/common/cibuild.sh --configuration ${configName} --prepareMachine")
      }
    }
  }
}

[true, false].each { isPR ->
  ['Windows_NT'].each { os ->
    ['Debug', 'Release'].each { configName ->
      def projectName = GithubProject

      def branchName = GithubBranchName

      def filesToArchive = "**/artifacts/${configName}/**"

      def jobName = getJobName(os, configName)
      def fullJobName = Utilities.getFullJobName(projectName, jobName, isPR)
      def myJob = job(fullJobName)

      Utilities.standardJobSetup(myJob, projectName, isPR, "*/${branchName}")

      if (isPR) {
        addGithubPRTriggerForBranch(myJob, branchName, jobName)
      } else {
        Utilities.addGithubPushTrigger(myJob)
      }
      
      addArchival(myJob, filesToArchive, "")
      addXUnitDotNETResults(myJob, configName)

      if (os == 'Windows_NT') {
        Utilities.setMachineAffinity(myJob, 'Windows.10.Amd64.ClientRS3.DevEx.Open')  
      } else {
        Utilities.setMachineAffinity(myJob, os, 'latest-or-auto')
      }

      addBuildSteps(myJob, projectName, os, configName, isPR)
    }
  }
}
